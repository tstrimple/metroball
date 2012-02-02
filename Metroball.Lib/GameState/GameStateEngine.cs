using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameState
{
    public class GameStateEngine
    {
        private readonly Game _game;
        private readonly IList<IGameState> _gameStates;
        private readonly Stack<IGameState> _currentGameState;  

        public GameStateEngine(Game game)
        {
            _game = game;
            _gameStates = new List<IGameState>();
            _currentGameState = new Stack<IGameState>();

            var mainMenuGameState = new MainMenuGameState(game);

            _gameStates.Add(mainMenuGameState);
            _currentGameState.Push(mainMenuGameState);
        }

        public void LoadContent()
        {
            foreach (var gameState in _gameStates)
            {
                gameState.LoadContent();
            }
        }

        public void UnloadContent()
        {
        }

        public bool ExitState()
        {
            _currentGameState.Pop();
            if(_currentGameState.Count == 0)
            {
                return true;
            }

            return false;
        }

        public void Update(GameTime gameTime)
        {
            _currentGameState.Peek().Update(gameTime);
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
