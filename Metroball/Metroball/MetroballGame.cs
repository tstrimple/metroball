using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Metroball.Lib;
using Metroball.Lib.Components;
using Metroball.Lib.Settings;
using Microsoft.Phone.Info;
using Microsoft.Phone.Notification;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using Microsoft.Advertising.Mobile.Xna;

namespace Metroball
{
    public class MetroballGame : Game
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private GameData GameData { get; set; }
        private readonly FlashMessage _flashMessage;
        private readonly GameScreen _gameScreen;
        private readonly MenuScreen _menuScreen;
        private readonly ResultsScreen _resultsScreen;
        private HttpNotificationChannel _pushChannel;

        public MetroballGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this) { IsFullScreen = true };
            Content.RootDirectory = "Content";

            AdGameComponent.Initialize(this, SettingsManager.ApplicationId);

            GameData = new GameData
                           {
                               UserId = SettingsManager.UserId,
                               Session = new Session(),
                               Results = {Name = SettingsManager.Name}
                           };

            ConnectLiveTileService(GameData.UserId);
            
            _flashMessage = new FlashMessage(this) { DrawOrder = 1 };

            _menuScreen = new MenuScreen(this)
                              {
                                  PlayGame = PlayGame,
                                  ExitMenuScreen =
                                      (sender, args) =>
                                      Service.SessionEnded(GameData.UserId, GameData.Session.SessionId,
                                                           GameData.Session.StartTime,
                                                           DateTime.UtcNow.ToUnixTime(), (o, a) => Exit()),
                                  DrawOrder = 2
                              };

            _gameScreen = new GameScreen(this)
                              {
                                  ComputerScored = ComputerScored,
                                  PlayerScored = PlayerScored,
                                  Results = GameData.Results,
                                  ExitGameScreen = delegate
                                                       {
                                                           GameData.Results.GameStatus = GameStatus.Abandoned;
                                                           Service.UpdateGameStatus(GameData.UserId, GameData.Session.SessionId, GameData.Results);
                                                           ShowMenu();
                                                       },
                                  DrawOrder = 2
                              };

            _resultsScreen = new ResultsScreen(this)
                                 {PlayGame = PlayGame, ExitResultsScreen = (sender, args) => ShowMenu(), DrawOrder = 2};

            Components.Add(_flashMessage);
            Components.Add(_menuScreen);
            Components.Add(_gameScreen);
            Components.Add(_resultsScreen);

            ShowMenu();

            Service.SessionStarted(GameData.UserId, GameData.Session.SessionId, GameData.Session.StartTime);

            TargetElapsedTime = TimeSpan.FromTicks(333333);
            InactiveSleepTime = TimeSpan.FromSeconds(0);
        }

        private void ConnectLiveTileService(string channelName)
        {
            _pushChannel = HttpNotificationChannel.Find(channelName);

            if (_pushChannel != null)
            {
                _pushChannel.Close();
            }

            _pushChannel = new HttpNotificationChannel(channelName, "metroball.hax.io");
            _pushChannel.ChannelUriUpdated += PushChannelUriUpdated;
            _pushChannel.ErrorOccurred += PushChannelErrorOccurred;
            _pushChannel.Open();
            _pushChannel.BindToShellTile();

            if (!_pushChannel.IsShellTileBound)
            {
                _pushChannel.BindToShellTile();

                var allowedDomains = new Collection<Uri> { new Uri("https://hax.io") };
                _pushChannel.BindToShellTile(allowedDomains);
                _pushChannel.HttpNotificationReceived += NotificationReceived;
            }
        }

        private void NotificationReceived(object sender, HttpNotificationEventArgs httpNotificationEventArgs)
        {   
        }

        private void PushChannelErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
        }

        private void PushChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Guide.IsVisible || !IsActive)
            {
                base.Update(gameTime);
                return;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            base.Draw(gameTime);
        }

        private void Back()
        {
        }

        private void ShowMenu()
        {
            _menuScreen.Enabled = true;
            _menuScreen.Visible = true;

            _gameScreen.Enabled = false;
            _gameScreen.Visible = false;

            _resultsScreen.Enabled = false;
            _resultsScreen.Visible = false;
        }

        private void UnfreezeGame()
        {
            _gameScreen.Alert = false;
            _gameScreen.Reset();

            if(!_resultsScreen.Enabled)
            {
                _gameScreen.Enabled = true;
            }
        }

        private void FreezeGame()
        {
            Service.UpdateGameStatus(GameData.UserId, GameData.Session.SessionId, GameData.Results);
            _gameScreen.Alert = true;
            _gameScreen.Enabled = false;
        }

        private void ShowGame()
        {
            _menuScreen.Enabled = false;
            _menuScreen.Visible = false;

            _gameScreen.Enabled = true;
            _gameScreen.Visible = true;

            _resultsScreen.Enabled = false;
            _resultsScreen.Visible = false;
        }

        private void ShowResults()
        {
            _menuScreen.Enabled = false;
            _menuScreen.Visible = false;

            _gameScreen.Enabled = false;
            _gameScreen.Visible = false;

            _resultsScreen.Enabled = true;
            _resultsScreen.Visible = true;
            _resultsScreen.Results = GameData.Results;
        }

        private void PlayGame(object sender, EventArgs eventArgs)
        {
            _gameScreen.Reset();
            GameData.Results.NewGame();
            Service.UpdateGameStatus(GameData.UserId, GameData.Session.SessionId, GameData.Results);
            ShowGame();
        }

        private void PlayerScored(object sender, EventArgs eventArgs)
        {
            Service.GetRank(GameData.Results.Score, rank => GameData.Results.Rank = rank);
            GameData.Results.ComputerLives--;
            if (GameData.Results.ComputerLives <= 0)
            {
                GameData.Results.ComputerLives = 3;
                GameData.Results.Level++;
                _flashMessage.StartFlash(String.Format("Level {0}!", GameData.Results.Level));

                FreezeGame();
                Components.Add(new DelayedCallback(this, _flashMessage.Duration, delegate(object o, EventArgs args) { Components.Remove((GameComponent)o); UnfreezeGame(); }));    
            }
            else
            {
                FreezeGame();
                Components.Add(new DelayedCallback(this, TimeSpan.FromMilliseconds(750), delegate(object o, EventArgs args) { Components.Remove((GameComponent)o); UnfreezeGame(); }));    
            }
        }

        private void ComputerScored(object sender, EventArgs eventArgs)
        {
            Service.GetRank(GameData.Results.Score, rank => GameData.Results.Rank = rank);
            GameData.Results.PlayerLives--;
            if (GameData.Results.PlayerLives <= 0)
            {
                GameData.Results.GameStatus = GameStatus.Completed;
                ShowResults();
                GetName(
                    (o, args) =>
                    Service.UpdateGameStatus(GameData.UserId, GameData.Session.SessionId, GameData.Results, delegate(object sender1, EventArgs args1)
                                                                                                                {
                                                                                                                    Service.GetHighScores(UpdateHighScores);
                                                                                                                    Service.GetTopPlayers(UpdateTopPlayers);
                                                                                                                }));
            }
            else
            {
                FreezeGame();
                Components.Add(new DelayedCallback(this, TimeSpan.FromMilliseconds(750), delegate(object o, EventArgs args) { Components.Remove((GameComponent)o); UnfreezeGame();  }));
            }
        }

        private void UpdateTopPlayers(HighScore[] highScores)
        {
            if(highScores == null || highScores.Length == 0)
            {
                return;
            }

            var topPlayers = new List<HighScore>(highScores);
            var currentPlayer = topPlayers.FirstOrDefault(s => s.GameId == GameData.UserId);
            if (currentPlayer != null)
            {
                currentPlayer.Current = true;
            }

            _resultsScreen.TopPlayers = topPlayers;
        }

        private void UpdateHighScores(HighScore[] highScores)
        {
            var scoreList = new List<HighScore>(highScores);
            var currentScore = scoreList.FirstOrDefault(s => s.GameId == GameData.Results.GameId);
            if (currentScore != null)
            {
                currentScore.Current = true;
            }
            else
            {
                scoreList.Add(new HighScore()
                {
                    GameId = GameData.Results.GameId,
                    Name = GameData.Results.Name,
                    Score = GameData.Results.Score.ToString(CultureInfo.InvariantCulture),
                    Rank = GameData.Results.Rank,
                    Current = true
                });
            }

            _resultsScreen.HighScores = scoreList;
        }

        private void GetName(EventHandler callback)
        {
            if(Guide.IsVisible)
            {
                if (callback != null)
                {
                    callback.Invoke(this, new EventArgs());
                }

                return;
            }

            Guide.BeginShowKeyboardInput(
                PlayerIndex.One,
                "Save your high score!",
                "Type in your name to save your score on the high score list.",
                SettingsManager.Name,
                delegate(IAsyncResult ar)
                {
                    SettingsManager.Name = Guide.EndShowKeyboardInput(ar);
                    GameData.Results.Name = SettingsManager.Name;

                    if (callback != null)
                    {
                        callback.Invoke(this, new EventArgs());
                    }
                }, null);
        }
    }
}