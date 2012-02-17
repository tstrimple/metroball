using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Metroball.Lib
{
    public class GameOver : GameComponent
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;
        private SpriteFont _scoreFont;
        private SpriteFont _selectedScoreFont;
        private Button _startButton;
        private SpriteBatch _spriteBatch;

        private readonly Game _game;

        public string GameId { get; set; }
        public string Rank { get; set; }
        public GameResults GameResults { get; set; }
        public HighScore[] HighScores { get; set; }

        public EventHandler PlayGame;

        public GameOver(Game game)
            : base(game)
        {
            _game = game;
        }

        public void Closing()
        {
        }

        public void LoadContent()
        {
            _logoFont = _game.Content.Load<SpriteFont>("LogoFont");
            _menuFont = _game.Content.Load<SpriteFont>("MenuFont");
            _scoreFont = _game.Content.Load<SpriteFont>("ScoreFont");
            _selectedScoreFont = _game.Content.Load<SpriteFont>("SelectedScoreFont");
            

            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            var yCenter = _game.GraphicsDevice.Viewport.Height / 2;

            _startButton = new Button(_menuFont, "play again", Color.White);
            _startButton.Position = new Vector2((xCenter / 2) - (_startButton.Rectangle.Width / 2), (_game.GraphicsDevice.Viewport.Height / 2 - _startButton.Rectangle.Height));

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

            if(HighScores != null)
            {
                int rank = 1;
                foreach (var highScore in HighScores)
                {
                    var color = Color.Gray;

                    if(highScore.GameId == GameId)
                    {
                        color = Color.White;
                    }

                    var nameSize = _scoreFont.MeasureString(highScore.Name);
                    var scoreSize = _scoreFont.MeasureString(highScore.Score);
                    var rankSize = _scoreFont.MeasureString(String.Format("#{0}", rank));

                    var row = ((rank - 1) * nameSize.Y) + 40;

                    _spriteBatch.DrawString(_scoreFont, highScore.Score, new Vector2(_game.GraphicsDevice.Viewport.Width - 20 - scoreSize.X, row), color);
                    _spriteBatch.DrawString(_scoreFont, highScore.Name, new Vector2(_game.GraphicsDevice.Viewport.Width - 140 - nameSize.X, row), color);
                    _spriteBatch.DrawString(_scoreFont, String.Format("#{0}", rank), new Vector2(_game.GraphicsDevice.Viewport.Width - 260 - rankSize.X, row), color);
            
                    rank++;
                }

                if(!HighScores.AsQueryable().Any(hs => hs.GameId == GameId))
                {
                    var nameSize = _scoreFont.MeasureString(GameResults.Nickname);
                    var scoreSize = _scoreFont.MeasureString(GameResults.Score.ToString(CultureInfo.InvariantCulture));
                    var rankSize = _scoreFont.MeasureString(String.Format("#{0}", Rank));

                    var row = (10 * nameSize.Y) + 40;

                    _spriteBatch.DrawString(_scoreFont, GameResults.Score.ToString(CultureInfo.InvariantCulture), new Vector2(_game.GraphicsDevice.Viewport.Width - 20 - scoreSize.X, row), Color.White);
                    _spriteBatch.DrawString(_scoreFont, GameResults.Nickname, new Vector2(_game.GraphicsDevice.Viewport.Width - 140 - nameSize.X, row), Color.White);
                    _spriteBatch.DrawString(_scoreFont, String.Format("#{0}", Rank), new Vector2(_game.GraphicsDevice.Viewport.Width - 260 - rankSize.X, row), Color.White);
                }

                var hsSize = _selectedScoreFont.MeasureString("high score");
                _spriteBatch.DrawString(_selectedScoreFont, "high scores", new Vector2(460, 300), Color.Gray, -MathHelper.PiOver2, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
            }

            _startButton.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
