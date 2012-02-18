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


namespace Metroball.Lib.Components
{
    public class ResultsScreen : DrawableGameComponent
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;
        private SpriteFont _scoreFont;
        private SpriteFont _selectedScoreFont;
        private Button _startButton;
        private SpriteBatch _spriteBatch;

        public Results Results { get; set; }
        public IEnumerable<HighScore> HighScores { get; set; }

        public EventHandler PlayGame;
        public EventHandler ExitResultsScreen;

        public ResultsScreen(Game game)
            : base(game)
        {
        }

        protected override void LoadContent()
        {
            _logoFont = Game.Content.Load<SpriteFont>("LogoFont");
            _menuFont = Game.Content.Load<SpriteFont>("MenuFont");
            _scoreFont = Game.Content.Load<SpriteFont>("ScoreFont");
            _selectedScoreFont = Game.Content.Load<SpriteFont>("SelectedScoreFont");

            _startButton = new Button(_menuFont, "play again", Color.White);
            var xPos = Game.GraphicsDevice.Viewport.Width / 4 - (_startButton.Rectangle.Width / 2);
            var yPos = Game.GraphicsDevice.Viewport.Height / 2 - _startButton.Rectangle.Height;
            _startButton.Position = new Vector2(xPos, yPos);

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ExitResultsScreen.Invoke(this, new EventArgs());
            }

            var press = TouchPanel.GetState().FirstOrDefault(t => t.State == TouchLocationState.Released);
            if (press != null && press.State == TouchLocationState.Released)
            {
                if (_startButton.Rectangle.Intersects(press.Position))
                {
                    PlayGame.Invoke(this, new EventArgs());
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "game over", new Vector2(0, 0), Color.White);

            if (String.IsNullOrEmpty(Results.Name))
            {
                var message = String.Format("Your score: {0}", Results.Score);
                _spriteBatch.DrawString(_selectedScoreFont, message, new Vector2(200 - (_selectedScoreFont.MeasureString(message).X / 2), _startButton.Rectangle.Y + 100), Color.White);
            }

            if (HighScores != null)
            {
                int counter = 1;
                foreach (var highScore in HighScores)
                {
                    var color = Color.Gray;

                    if (highScore.Current)
                    {
                        color = Color.White;
                    }

                    var name = highScore.Name;
                    var nameSize = _scoreFont.MeasureString(name);

                    while(nameSize.X > 120)
                    {
                        name = name.Substring(0, name.Length - 1);
                        nameSize = _scoreFont.MeasureString(name);
                    }

                    var scoreSize = _scoreFont.MeasureString(highScore.Score);

                    var rank = highScore.Rank.HasValue ? highScore.Rank.Value : counter;

                    var rankSize = _scoreFont.MeasureString(String.Format("#{0}", counter));

                    var row = ((counter - 1) * nameSize.Y) + 40;

                    _spriteBatch.DrawString(_scoreFont, highScore.Score, new Vector2(Game.GraphicsDevice.Viewport.Width - 20 - scoreSize.X, row), color);

                    _spriteBatch.DrawString(_scoreFont, name, new Vector2(Game.GraphicsDevice.Viewport.Width - 140 - nameSize.X, row), color);

                    _spriteBatch.DrawString(_scoreFont, String.Format("#{0}", rank), new Vector2(Game.GraphicsDevice.Viewport.Width - 260 - rankSize.X, row), color);

                    counter++;
                }

                _spriteBatch.DrawString(_selectedScoreFont, "high scores", new Vector2(460, 300), Color.Gray, -MathHelper.PiOver2, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
            }

            _startButton.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
