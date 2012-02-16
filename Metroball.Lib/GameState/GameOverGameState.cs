using System;
using System.Collections.Generic;
using System.Linq;
using Metroball.Lib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace Metroball.Lib.GameState
{
    public class GameOverGameState : GameComponent, IGameState
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;

        private Button _startButton;
        
        private SpriteBatch _spriteBatch;

        private readonly Game _game;

        public string Name { get; set; }

        public int Score { get; set; }

        public EventHandler PlayGame;

        public GameOverGameState(Game game, int score)
            : base(game)
        {
            _game = game;
            Score = score;
        }

        public void Closing()
        {
        }

        public void LoadContent()
        {
            _logoFont = _game.Content.Load<SpriteFont>("LogoFont");
            _menuFont = _game.Content.Load<SpriteFont>("MenuFont");

            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            var yCenter = _game.GraphicsDevice.Viewport.Height / 2;

            _startButton = new Button(_menuFont, "play again", Color.White);
            _startButton.Position = new Vector2(xCenter - (_startButton.Rectangle.Width / 2), (yCenter - _startButton.Rectangle.Height / 2) + 40);

            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
        }

        public void UnloadContent()
        {

        }

        public void Update(GameTime gameTime, TouchCollection touches)
        {
            var press = touches.FirstOrDefault(t => t.State == TouchLocationState.Released);
            if(press != null)
            {
                CheckForButtonPress(press.Position);
            }
        }

        private void CheckForButtonPress(Vector2 point)
        {
            if(_startButton.Rectangle.Intersects(point))
            {
                PlayGame.Invoke(this, new EventArgs());
            }
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "game over", new Vector2(0, 0), Color.White);
            var scoreMessage = String.Format("score: {0}", Score);
            if(!String.IsNullOrEmpty(Name))
            {
                scoreMessage = String.Format("{0}'s score: {1}", Name, Score);
            }
            var size = _menuFont.MeasureString(scoreMessage);
            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            _spriteBatch.DrawString(_menuFont, scoreMessage, new Vector2(xCenter - size.X / 2, _startButton.Position.Y - 80), Color.White);
            _startButton.Draw(_spriteBatch);
            
            _spriteBatch.End();
        }
    }
}
