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
        private bool _exiting;
        private string _sessionStarted;

        private bool _getName;

        public GameState GameState { get; set; }

        private const string UserAppIdKey = "UserAppId";
        private const string NicknameKey = "Nickname";
        private const string ApplicationId = "280959f5-c67a-4b4f-a882-3baea4edc0d2";
        private readonly string _sessionId;
        private readonly string _userId;
        private readonly PlayingGameState _playingGameState;
        private readonly MainMenuGameState _mainMenuGameState;
        private readonly GameOverGameState _gameOverGameState;

        public string AppId { get { return ApplicationId; } }
        public string SessionId { get { return _sessionId; } }
        public string UserId { get { return _userId; } }
        private bool _soundEnabled;

        public Game()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.IsFullScreen = true;
            Content.RootDirectory = "Content";

            _getName = false;
            _exiting = false;
            AdGameComponent.Initialize(this, ApplicationId);

            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains(UserAppIdKey))
            {
                settings.Add(UserAppIdKey, Guid.NewGuid().ToString());
                settings.Save();
            }

            _userId = settings[UserAppIdKey].ToString();
            _sessionId = Guid.NewGuid().ToString();

            _soundEnabled = true;

            _mainMenuGameState = new MainMenuGameState(this, _soundEnabled);
            _mainMenuGameState.PlayGame += PlayGame;
            _mainMenuGameState.EnableSound += EnableSound;
            _mainMenuGameState.DisableSound += DisableSound;

            _gameOverGameState = new GameOverGameState(this, 0);
            _gameOverGameState.PlayGame += PlayGame;

            _playingGameState = new PlayingGameState(this, _soundEnabled, _userId, _sessionId);
            _playingGameState.GameOver += GameOver;

            GameState = Lib.GameState.MainMenu;

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

            _mainMenuGameState.LoadContent();
            _gameOverGameState.LoadContent();
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                switch (GameState)
                {
                    case Lib.GameState.MainMenu:
                        GameState = Lib.GameState.Exiting;
                        Service.SessionEnded(UserId, SessionId, _sessionStarted, DateTime.UtcNow.ToUnixTime().ToString(CultureInfo.InvariantCulture), SessionEndedComplete);
                        break;
                    case Lib.GameState.Playing:
                        GameState = Lib.GameState.MainMenu;
                        break;
                    case Lib.GameState.GameOver:
                        GameState = Lib.GameState.MainMenu;
                        break;
                    case Lib.GameState.Exiting:
                    default:
                        break;
                }
            }

            if (_getName)
            {
                Guide.BeginShowKeyboardInput(
                    PlayerIndex.One,
                    "Save your high score!",
                    "Type in the name you would like to see on the high score list.",
                    _gameOverGameState.Name,
                    delegate(IAsyncResult ar) { _getName = false; _gameOverGameState.Name = Guide.EndShowKeyboardInput(ar); },
                    null);

                base.Update(gameTime);
                return;
            }

            try
            {
                TouchCollection touches = TouchPanel.GetState();

                switch (GameState)
                {
                    case Lib.GameState.MainMenu:
                        _mainMenuGameState.Update(gameTime, touches);
                        break;
                    case Lib.GameState.Playing:
                        _playingGameState.Update(gameTime, touches);
                        break;
                    case Lib.GameState.GameOver:
                        _gameOverGameState.Update(gameTime, touches);
                        break;
                    case Lib.GameState.Exiting:
                    default:
                        break;
                }

                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Service.LogError(UserId, SessionId, ex.Message);
                throw;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_exiting)
            {
                return;
            }

            try
            {
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

                switch (GameState)
                {
                    case Lib.GameState.MainMenu:
                        _mainMenuGameState.Draw(gameTime);
                        break;
                    case Lib.GameState.Playing:
                        _playingGameState.Draw(gameTime);
                        break;
                    case Lib.GameState.GameOver:
                        _gameOverGameState.Draw(gameTime);
                        break;
                    case Lib.GameState.Exiting:
                    default:
                        break;
                }

                base.Draw(gameTime);
            }
            catch (Exception ex)
            {
                Service.LogError(UserId, SessionId, ex.Message);
                throw;
            }
        }

        private void DisableSound(object sender, EventArgs eventArgs)
        {
            _soundEnabled = false;
        }

        private void EnableSound(object sender, EventArgs eventArgs)
        {
            _soundEnabled = true;
        }

        private void GameOver(object sender, GameOverEventArgs eventArgs)
        {
            _gameOverGameState.Score = eventArgs.Score;

            _gameOverGameState.Name = "";
            if (IsolatedStorageSettings.ApplicationSettings.Contains(NicknameKey))
            {
                _gameOverGameState.Name = IsolatedStorageSettings.ApplicationSettings[NicknameKey].ToString();
            }

            GameState = Lib.GameState.GameOver;

            _getName = true;

            if (!String.IsNullOrEmpty(_gameOverGameState.Name))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(NicknameKey, _gameOverGameState.Name);
            }

            Service.UpdateGameStatus(_playingGameState.GameId, _userId, _sessionId, new GameResults() { Level = _playingGameState.Level, Score = _playingGameState.Score, Started = _playingGameState.Started, Status = GameStatus.Completed, Ended = DateTime.UtcNow, Nickname = _gameOverGameState.Name });
        }

        private void PlayGame(object sender, EventArgs eventArgs)
        {
            _playingGameState.Reset();
            GameState = Lib.GameState.Playing;
        }

        private void SessionEndedComplete(object sender, EventArgs eventArgs)
        {
            Exit();
        }
    }
}
