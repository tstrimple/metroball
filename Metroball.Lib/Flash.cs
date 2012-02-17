using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Metroball.Lib
{
    public class Flash : GameComponent
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set 
            {
                _message = value;
                StartFlash();
            }
        }

        private SpriteFont _menuFont;
        private SpriteBatch _spriteBatch;

        private float _currentPosition;

        private float _startingPosition;
        private float _targetPosition;

        private TimeSpan _accumulator;
        private TimeSpan _fadeIn;
        private TimeSpan _display;
        private TimeSpan _fadeOut;
        private Game _game;

        private enum FlashState
        {
            Disabled,
            FadingIn,
            Displayed,
            FadingOut
        }

        private FlashState CurrentState { get; set; }

        public Flash(Game game)
            : base(game)
        {
            _game = game;
            _fadeIn = TimeSpan.FromMilliseconds(500);
            _display = TimeSpan.FromMilliseconds(1500);
            _fadeOut = TimeSpan.FromMilliseconds(500);
            _targetPosition = 400;
            _startingPosition = 500;
        }

        private void StartFlash()
        {
            _accumulator = TimeSpan.Zero;
            CurrentState = FlashState.FadingIn;
        }

        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            _menuFont = _game.Content.Load<SpriteFont>("MenuFont");    
        }

        public override void Update(GameTime gameTime)
        {
            if(CurrentState == FlashState.Disabled)
            {
                base.Update(gameTime);
                return;
            }

            _accumulator += gameTime.ElapsedGameTime;

            switch (CurrentState)
            {
                case FlashState.FadingIn:
                    if(_accumulator >= _fadeIn)
                    {
                        _accumulator = TimeSpan.Zero;
                        CurrentState = FlashState.Displayed;
                        _currentPosition = _targetPosition;
                    }
                    else
                    {
                        float percent = _accumulator.Milliseconds/(float) _fadeIn.Milliseconds;
                        _currentPosition = MathHelper.Lerp(_startingPosition, _targetPosition, percent);
                    }
                    break;
                case FlashState.Displayed:
                    if (_accumulator >= _display)
                    {
                        _accumulator = TimeSpan.Zero;
                        CurrentState = FlashState.FadingOut;
                    }
                    break;
                case FlashState.FadingOut:
                    if (_accumulator >= _fadeOut)
                    {
                        _accumulator = TimeSpan.Zero;
                        CurrentState = FlashState.Disabled;
                        _currentPosition = _startingPosition;
                    }
                    else
                    {
                        float percent = _accumulator.Milliseconds / (float)_fadeOut.Milliseconds;
                        _currentPosition = MathHelper.Lerp(_targetPosition, _startingPosition, percent);
                    }
                    break;
            }
            base.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime)
        {
            if (CurrentState == FlashState.Disabled)
            {
                return;
            }

            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_menuFont, _message, new Vector2(xCenter - (_menuFont.MeasureString(_message).X / 2), _currentPosition), Color.White);
            _spriteBatch.End();
        }
    }
}
