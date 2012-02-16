using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Microsoft.Advertising.Mobile.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Metroball.Lib.GameState
{
    public class GameStateEngine
    {
        private const string UserAppIdKey = "UserAppId";
        private const string NicknameKey = "Nickname";
        private const string ApplicationId = "280959f5-c67a-4b4f-a882-3baea4edc0d2";
        private readonly string _sessionId;
        private readonly string _userApplicationId;

        public string AppId { get { return ApplicationId; } }

        public string SessionId
        {
            get { return _sessionId; }
        }

        public string UserId
        {
            get { return _userApplicationId; }
        }

        private readonly Game _game;
        private readonly Stack<IGameState> _currentGameState;
        private readonly MainMenuGameState _mainMenuGameState;
        private readonly GameOverGameState _gameOverGameState;

        private bool _soundEnabled;

        public GameStateEngine(Game game)
        {
            AdGameComponent.Initialize(game, ApplicationId);
            _game = game;
            _currentGameState = new Stack<IGameState>();

            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains(UserAppIdKey))
            {
                settings.Add(UserAppIdKey, Guid.NewGuid().ToString());
                settings.Save();
            }

            _userApplicationId = settings[UserAppIdKey].ToString();
            _sessionId = Guid.NewGuid().ToString();

            _soundEnabled = true;

            _mainMenuGameState = new MainMenuGameState(game, _soundEnabled);
            _mainMenuGameState.PlayGame += PlayGame;
            _mainMenuGameState.EnableSound += EnableSound;
            _mainMenuGameState.DisableSound += DisableSound;

            _gameOverGameState = new GameOverGameState(game, 0);
            _gameOverGameState.PlayGame += PlayAgain;

            _currentGameState.Push(_mainMenuGameState);
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

            var gameState = (PlayingGameState)_currentGameState.Pop();
            _currentGameState.Push(_gameOverGameState);

            Guide.BeginShowKeyboardInput(
                PlayerIndex.One,
                "Save your high score!",
                "Type in the name you would like to see on the high score list.",
                _gameOverGameState.Name,
                delegate(IAsyncResult ar) { _gameOverGameState.Name = Guide.EndShowKeyboardInput(ar); },
                null);

            if (!String.IsNullOrEmpty(_gameOverGameState.Name))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(NicknameKey, _gameOverGameState.Name);
            }

            Service.UpdateGameStatus(gameState.GameId, _userApplicationId, _sessionId, new GameResults() { Level = gameState.Level, Score = gameState.Score, Started = gameState.Started, Status = GameStatus.Completed, Ended = DateTime.UtcNow, Nickname = _gameOverGameState.Name});
        }

        private void PlayGame(object sender, EventArgs eventArgs)
        {
            var gameState = new PlayingGameState(_game, true, _userApplicationId, _sessionId);
            gameState.GameOver += GameOver;
            gameState.LoadContent();
            _currentGameState.Push(gameState);
        }

        private void PlayAgain(object sender, EventArgs eventArgs)
        {
            var gameState = new PlayingGameState(_game, _soundEnabled, _userApplicationId, _sessionId);
            gameState.GameOver += GameOver;
            gameState.LoadContent();
            _currentGameState.Pop();
            _currentGameState.Push(gameState);
        }

        public void LoadContent()
        {
            _mainMenuGameState.LoadContent();
            _gameOverGameState.LoadContent();
        }

        public void UnloadContent()
        {
        }

        public bool ExitState()
        {
            var oldState = _currentGameState.Pop();
            oldState.Closing();
            if (_currentGameState.Count == 0)
            {
                return true;
            }

            return false;
        }

        public void Update(GameTime gameTime)
        {
            TouchCollection touches = TouchPanel.GetState();
            _currentGameState.Peek().Update(gameTime, touches);
        }

        public void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            _currentGameState.Peek().Draw(gameTime);
        }

        private void ClearScene()
        {
        }
    }
}
