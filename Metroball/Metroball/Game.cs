using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO.IsolatedStorage;
using Metroball.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using Microsoft.Advertising.Mobile.Xna;
namespace Metroball
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private BasicEffect _primaryEffect;
        private string _sessionStarted;

        public GameState GameState { get; set; }

        private const string UserAppIdKey = "UserAppId";
        private const string SoundEnabledKey = "SoundEnabled";
        private const string NicknameKey = "Nickname";
        private const string ApplicationId = "280959f5-c67a-4b4f-a882-3baea4edc0d2";
        private readonly string _sessionId;
        private readonly string _userId;
        private readonly Playing _playingGameState;
        private readonly MainMenu _mainMenuGameState;
        private readonly GameOver _gameOverGameState;
        private readonly Flash _flash;

        public string AppId { get { return ApplicationId; } }
        public string SessionId { get { return _sessionId; } }
        public string UserId { get { return _userId; } }

        public Game()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this) {IsFullScreen = true};
            Content.RootDirectory = "Content";

            AdGameComponent.Initialize(this, ApplicationId);

            if (!IsolatedStorageSettings.ApplicationSettings.Contains(UserAppIdKey))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(UserAppIdKey, Guid.NewGuid().ToString());
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            _userId = IsolatedStorageSettings.ApplicationSettings[UserAppIdKey].ToString();
            _sessionId = Guid.NewGuid().ToString();

            _flash = new Flash(this);

            _mainMenuGameState = new MainMenu(this);
            _mainMenuGameState.PlayGame += PlayGame;
            _mainMenuGameState.EnableSound += EnableSound;
            _mainMenuGameState.DisableSound += DisableSound;

            _gameOverGameState = new GameOver(this);
            _gameOverGameState.PlayGame += PlayGame;

            _playingGameState = new Playing(this, _userId, _sessionId, _flash);
            _playingGameState.GameOver += GameOver;


            if (!IsolatedStorageSettings.ApplicationSettings.Contains(SoundEnabledKey))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(SoundEnabledKey, true);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            var soundEnabled = (bool) IsolatedStorageSettings.ApplicationSettings[SoundEnabledKey];
            _playingGameState.SoundEnabled = soundEnabled;
            _mainMenuGameState.SoundEnabled = soundEnabled;

            GameState = GameState.MainMenu;

            _sessionStarted = DateTime.UtcNow.ToUnixTime().ToString(CultureInfo.InvariantCulture);

            Service.SecretKey = AppId;
            Service.SessionStarted(UserId, SessionId, _sessionStarted);

            TargetElapsedTime = TimeSpan.FromTicks(333333);
            InactiveSleepTime = TimeSpan.FromSeconds(0);
        }

        protected override void LoadContent()
        {
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f,
                GraphicsDevice.Viewport.Height * 0.5f);

            _primaryEffect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = false,
                World = Matrix.Identity,
                View =
                   Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0)),
                Projection =
                    Matrix.CreateOrthographic(800.0f, 480.0f, -0.5f, 1.0f),
                VertexColorEnabled = true
            };

            _playingGameState.LoadContent();
            _mainMenuGameState.LoadContent();
            _gameOverGameState.LoadContent();
            _flash.LoadContent();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Guide.IsVisible || !IsActive)
            {
                base.Update(gameTime);
                return;
            }

            _flash.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                switch (GameState)
                {
                    case GameState.MainMenu:
                        GameState = GameState.Exiting;
                        Service.SessionEnded(UserId, SessionId, _sessionStarted, DateTime.UtcNow.ToUnixTime().ToString(CultureInfo.InvariantCulture), SessionEndedComplete);
                        break;
                    case GameState.Playing:
                        GameState = GameState.MainMenu;
                        break;
                    case GameState.GameOver:
                        GameState = GameState.MainMenu;
                        break;
                    case GameState.Exiting:
                    default:
                        base.Update(gameTime);
                        return;
                }
            }

            //try
            //{
            TouchCollection touches = TouchPanel.GetState();

            switch (GameState)
            {
                case GameState.MainMenu:
                    _mainMenuGameState.Update(gameTime, touches);
                    break;
                case GameState.Playing:
                    _playingGameState.Update(gameTime, touches);
                    break;
                case GameState.GameOver:
                    _gameOverGameState.Update(gameTime, touches);
                    break;
                case GameState.Exiting:
                default:
                    break;
            }

            base.Update(gameTime);
            //}
            //catch (Exception ex)
            //{
            //    Service.LogError(UserId, SessionId, ex.Message);
            //    throw;
            //}
        }

        protected override void Draw(GameTime gameTime)
        {
            //try
            //{
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            _flash.Draw(gameTime);

            switch (GameState)
            {
                case GameState.MainMenu:
                    _mainMenuGameState.Draw(gameTime);
                    break;
                case GameState.Playing:
                    _playingGameState.Draw(gameTime);
                    break;
                case GameState.GameOver:
                    _gameOverGameState.Draw(gameTime);
                    break;
                case GameState.Exiting:
                default:
                    break;
            }

            base.Draw(gameTime);
            //}
            //catch (Exception ex)
            //{
            //    Service.LogError(UserId, SessionId, ex.Message);
            //    throw;
            //}
        }

        private void DisableSound(object sender, EventArgs eventArgs)
        {
            IsolatedStorageSettings.ApplicationSettings[SoundEnabledKey] = false;
            IsolatedStorageSettings.ApplicationSettings.Save();
            _playingGameState.SoundEnabled = false;
        }

        private void EnableSound(object sender, EventArgs eventArgs)
        {
            IsolatedStorageSettings.ApplicationSettings[SoundEnabledKey] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();
            _playingGameState.SoundEnabled = true;
        }

        private void GameOver(object sender, GameResults results)
        {
            _gameOverGameState.Rank = _playingGameState.Rank;
            _gameOverGameState.GameId = _playingGameState.GameId;
            _gameOverGameState.GameResults = results;

            if (IsolatedStorageSettings.ApplicationSettings.Contains(NicknameKey))
            {
                _gameOverGameState.GameResults.Nickname = IsolatedStorageSettings.ApplicationSettings[NicknameKey].ToString();
            }

            GameState = GameState.GameOver;


            Guide.BeginShowKeyboardInput(
                PlayerIndex.One,
                "Save your high score!",
                "Type in your name to save your score on the high score list.",
                _gameOverGameState.GameResults.Nickname,
                delegate(IAsyncResult ar)
                    {
                        _gameOverGameState.GameResults.Nickname = Guide.EndShowKeyboardInput(ar);
                        if (!String.IsNullOrEmpty(_gameOverGameState.GameResults.Nickname))
                        {
                            if (IsolatedStorageSettings.ApplicationSettings.Contains(NicknameKey))
                            {
                                IsolatedStorageSettings.ApplicationSettings[NicknameKey] =
                                    _gameOverGameState.GameResults.Nickname;
                                IsolatedStorageSettings.ApplicationSettings.Save();
                            }
                            else
                            {
                                IsolatedStorageSettings.ApplicationSettings.Add(NicknameKey,
                                                                                _gameOverGameState.GameResults.Nickname);
                                IsolatedStorageSettings.ApplicationSettings.Save();
                            }

                            Service.UpdateGameStatus(_playingGameState.GameId, _userId, _sessionId,
                                                     new GameResults()
                                                         {
                                                             Level = _playingGameState.Level,
                                                             Score = _playingGameState.Score,
                                                             Started = _playingGameState.Started,
                                                             Status = GameStatus.Completed,
                                                             Ended = DateTime.UtcNow,
                                                             Nickname = _gameOverGameState.GameResults.Nickname
                                                         }, (o, args) => Service.GetHighScores(HighScoresCallback));
                        }
                        else
                        {
                            Service.GetHighScores(HighScoresCallback);
                        }
                    }, null);
        }

        private void HighScoresCallback(HighScore[] highScores)
        {
            _gameOverGameState.HighScores = highScores;
        }

        private void PlayGame(object sender, EventArgs eventArgs)
        {
            _playingGameState.Reset();
            GameState = GameState.Playing;
        }

        private void SessionEndedComplete(object sender, EventArgs eventArgs)
        {
            Exit();
        }
    }
}
