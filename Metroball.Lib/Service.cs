using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using Metroball.Lib.Settings;
using Newtonsoft.Json.Linq;

namespace Metroball.Lib
{
    public delegate void RankAvailable(int? rank);
    public delegate void HighScoresAvailable(HighScore[] highScores);

    public class ServiceRequest
    {
        public string Salt { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public EventHandler Completed { get; set; }
        
        public void SendRequest()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(Url);
            webRequest.Method = "POST";
            webRequest.Accept = "*/*";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Credentials = new NetworkCredential();

            webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest);   
        }

        protected virtual void MessageSent()
        {
            
        }

        private string GetRequestData()
        {
            var sortedData = Data.OrderBy(d => d.Key).Select(d => d.Key + "=" + d.Value);
            var body = String.Join("&", sortedData);

            string signature = CalculateHash(body);
            return String.Format("{0}&signature={1}", body, signature);
        }

        private string CalculateHash(string content)
        {
            var data = Encoding.UTF8.GetBytes(content + Salt);
            SHA256 shaM = new SHA256Managed();
            byte[] result = shaM.ComputeHash(data);
            return Convert.ToBase64String(result);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            var webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            var postStream = webRequest.EndGetRequestStream(asynchronousResult);
            var body = GetRequestData();
            var postData = Encoding.UTF8.GetBytes(body);
            postStream.Write(postData, 0, postData.Length);
            postStream.Close();
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
            MessageSent();
        }

        protected virtual void HandleResponse(string response)
        {
            
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                var webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                var response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                var streamResponse = response.GetResponseStream();
                var streamReader = new StreamReader(streamResponse);
                var responseData = streamReader.ReadToEnd();
                HandleResponse(responseData);
                if(Completed != null)
                {
                    Completed.Invoke(this, new EventArgs());
                }
                streamResponse.Close();
                streamReader.Close();
                response.Close();

            }
            catch (WebException e)
            {
                HandleResponse("");
            }
        }
    }

    public class SessionEndRequest : ServiceRequest
    {
        private EventHandler _sessionEndCallback;

        public SessionEndRequest(EventHandler callback)
        {
            _sessionEndCallback = callback;
        }

        protected override void MessageSent()
        {
            _sessionEndCallback.Invoke(this, new EventArgs());
        }
    }

    public class RankRequest : ServiceRequest
    {
        private readonly RankAvailable _rankCallback;

        public RankRequest(RankAvailable callback)
        {
            _rankCallback = callback;
        }

        protected override void HandleResponse(string response)
        {
            int rank;
            if (Int32.TryParse(response, out rank))
            {
                _rankCallback.Invoke(rank);
            }
            else
            {
                _rankCallback.Invoke(null);
            }

        }
    }

    public class HighScore
    {
        public string GameId { get; set; }
        public int? Rank { get; set; }
        public string Name { get; set; }
        public string Score { get; set; }
        public bool Current { get; set; }

        public HighScore()
        {
            Current = false;
        }
    }

    public class HighScoreRequest : ServiceRequest
    {
        private readonly HighScoresAvailable _highScoreCallback;

        public HighScoreRequest(HighScoresAvailable callback)
        {
            _highScoreCallback = callback;
        }

        protected override void HandleResponse(string response)
        {
            try
            {
                var jsonObject = JArray.Parse(response);
                var highScores = jsonObject.Children().Select(hs =>
                                                        new HighScore()
                                                        {
                                                            GameId = hs["_id"].ToString(),
                                                            Name = hs["name"].ToString(),
                                                            Score = hs["score"].ToString()
                                                        }).ToArray();
                _highScoreCallback.Invoke(highScores);
                return;
            }
            catch (Exception)
            {
                _highScoreCallback.Invoke(null);
                throw;
            }
            
        }
    }

    public static class Service
    {
        public const string ServiceUrl = "https://hax.io/mb/";
        public static string SecretKey { get { return SettingsManager.ApplicationId; }}

        public static void SessionStarted(string userId, string sessionId, int started)
        {
            var data = new Dictionary<string, string>()
                           {
                               {"_id", sessionId},
                               {"userId", userId},
                               {"started", started.ToString(CultureInfo.InvariantCulture)},
                               {"ended", ""}
                           };
            var request = new ServiceRequest()
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}SessionStarted/", ServiceUrl)
            };

            request.SendRequest();
        }

        public static void SessionEnded(string userId, string sessionId, int started, int ended, EventHandler callback)
        {
            var data = new Dictionary<string, string>()
                           {
                               {"_id", sessionId},
                               {"userId", userId},
                               {"started", started.ToString(CultureInfo.InvariantCulture)},
                               {"ended", ended.ToString(CultureInfo.InvariantCulture)}
                           };
            var request = new SessionEndRequest(callback)
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}SessionEnded/", ServiceUrl)
            };

            request.SendRequest();
        }

        public static void UpdateGameStatus(string userId, string sessionId, Results results)
        {
            UpdateGameStatus(userId, sessionId, results, null);
        }

        public static void UpdateGameStatus(string userId, string sessionId, Results results, EventHandler callback)
        {
            var data = new Dictionary<string, string>()
                           {
                               {"_id", results.GameId},    
                               {"userId", userId},
                               {"sessionId", sessionId},
                               {"name", results.Name},
                               {"score", results.Score.ToString(CultureInfo.InvariantCulture)},
                               {"level", results.Level.ToString(CultureInfo.InvariantCulture)},
                               {"status", results.GameStatus.ToString()},
                               {"started", results.Started.ToString(CultureInfo.InvariantCulture)},
                               {"ended", results.Ended.ToString(CultureInfo.InvariantCulture)}
                           };
            var request = new ServiceRequest()
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}UpdateGameStatus/", ServiceUrl),
                Completed = callback
            };

            request.SendRequest();
        }

        public static void LogError(string userId, string sessionId, string errorMessage)
        {
            var data = new Dictionary<string, string>()
                           {
                               {"userId", userId},
                               {"sessionId", sessionId},
                               {"error", errorMessage}
                           };
            var request = new ServiceRequest()
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}LogError/", ServiceUrl)
            };

            request.SendRequest();
        }

        public static void GetRank(int score, RankAvailable rankCallback)
        {
            var data = new Dictionary<string, string>() {{"score", score.ToString(CultureInfo.InvariantCulture)}};
            var request = new RankRequest(rankCallback)
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}GetRank/", ServiceUrl)
            };

            request.SendRequest();
        }

        public static void GetHighScores(HighScoresAvailable highScoresCallback)
        {
            var data = new Dictionary<string, string>() { { "score", "10" } };
            var request = new HighScoreRequest(highScoresCallback)
            {
                Data = data,
                Salt = SecretKey,
                Url = String.Format("{0}GetHighScores/", ServiceUrl)
            };

            request.SendRequest();

        }
    }
}
