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
    public class MainMenuGameState : GameComponent, IGameState
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;
        private Button _startButton;

        private Button _soundOnButton;
        private Button _soundOffButton;

        public bool SoundEnabled { get; set; }

        private SpriteBatch _spriteBatch;

        private readonly Game _game;

        public EventHandler PlayGame;
        public EventHandler EnableSound;
        public EventHandler DisableSound;

        public MainMenuGameState(Game game, bool soundEnabled)
            : base(game)
        {
            _game = game;
            SoundEnabled = soundEnabled;
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

            _startButton = new Button(_menuFont, "start game", Color.White);
            _startButton.Position = new Vector2(xCenter - (_startButton.Rectangle.Width / 2) - 10, yCenter - _startButton.Rectangle.Height);

            _soundOnButton = new Button(_menuFont, "sound disabled", Color.White);
            _soundOnButton.Position = new Vector2(xCenter - (_soundOnButton.Rectangle.Width / 2) - 10, yCenter);

            _soundOffButton = new Button(_menuFont, "sound enabled", Color.White);
            _soundOffButton.Position = new Vector2(xCenter - (_soundOffButton.Rectangle.Width / 2) - 10, yCenter);

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

            if (SoundEnabled && _soundOffButton.Rectangle.Intersects(point))
            {
                DisableSound.Invoke(this, new EventArgs());
                SoundEnabled = false;
            }
            else if (!SoundEnabled && _soundOnButton.Rectangle.Intersects(point))
            {
                EnableSound.Invoke(this, new EventArgs());
                SoundEnabled = true;
            }
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "metroball", new Vector2(0, 0), Color.White);
            _startButton.Draw(_spriteBatch);
            
            if(SoundEnabled)
            {
                _soundOffButton.Draw(_spriteBatch);
            }
            else
            {
                _soundOnButton.Draw(_spriteBatch);   
            }
            
            _spriteBatch.End();
        }
    }
}
