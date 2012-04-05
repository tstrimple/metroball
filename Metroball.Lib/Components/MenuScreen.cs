using System;
using System.Linq;
using Metroball.Lib.Settings;
using Metroball.Lib.UI;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Metroball.Lib.Components
{
    public class MenuScreen : DrawableGameComponent
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;
        private Button _startButton;
        private Button _feedbackButton;

        private Button _soundOnButton;
        private Button _soundOffButton;

        private SpriteBatch _spriteBatch;

        public EventHandler PlayGame;
        public EventHandler ExitMenuScreen;

        public MenuScreen(Game game)
            : base(game)
        {
        }

        protected override void LoadContent()
        {
            _logoFont = Game.Content.Load<SpriteFont>("LogoFont");
            _menuFont = Game.Content.Load<SpriteFont>("MenuFont");

            var xCenter = Game.GraphicsDevice.Viewport.Width / 2;
            var yCenter = Game.GraphicsDevice.Viewport.Height / 2;

            _startButton = new Button(_menuFont, "start game", Color.White);
            _startButton.Position = new Vector2(xCenter - (_startButton.Rectangle.Width / 2) - 10, yCenter - _startButton.Rectangle.Height);

            _soundOnButton = new Button(_menuFont, "sound disabled", Color.White);
            _soundOnButton.Position = new Vector2(xCenter - (_soundOnButton.Rectangle.Width / 2) - 10, yCenter);

            _soundOffButton = new Button(_menuFont, "sound enabled", Color.White);
            _soundOffButton.Position = new Vector2(xCenter - (_soundOffButton.Rectangle.Width / 2) - 10, yCenter);

            _feedbackButton = new Button(_menuFont, "leave feedback", Color.White);
            _feedbackButton.Position = new Vector2(xCenter - (_feedbackButton.Rectangle.Width / 2) - 10, yCenter + _feedbackButton.Rectangle.Height);

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ExitMenuScreen.Invoke(this, new EventArgs());
            }

            var touch = TouchPanel.GetState().FirstOrDefault(t => t.State == TouchLocationState.Released);

            if (touch != null)
            {
                if (_startButton.Rectangle.Intersects(touch.Position))
                {
                    PlayGame.Invoke(this, new EventArgs());
                }

                if (SettingsManager.SoundEnabled && _soundOffButton.Rectangle.Intersects(touch.Position))
                {
                    SettingsManager.SoundEnabled = false;
                }
                else if (!SettingsManager.SoundEnabled && _soundOnButton.Rectangle.Intersects(touch.Position))
                {
                    SettingsManager.SoundEnabled = true;
                }
                
                if (_feedbackButton.Rectangle.Intersects(touch.Position))
                {
                    MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                    marketplaceReviewTask.Show();
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "metroball", new Vector2(0, 0), Color.White);
            _startButton.Draw(_spriteBatch);

            if (SettingsManager.SoundEnabled)
            {
                _soundOffButton.Draw(_spriteBatch);
            }
            else
            {
                _soundOnButton.Draw(_spriteBatch);
            }

            _feedbackButton.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
