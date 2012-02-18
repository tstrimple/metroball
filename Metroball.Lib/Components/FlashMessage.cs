using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.Components
{
    public class FlashMessage : DrawableGameComponent
    {
        private SpriteFont _menuFont;
        private SpriteBatch _spriteBatch;
        private string _message;

        private float _currentPosition;
        private readonly float _startingPosition;
        private readonly float _targetPosition;

        private TimeSpan _accumulator;
        private readonly TimeSpan _fadeIn;
        private readonly TimeSpan _display;
        private readonly TimeSpan _fadeOut;

        public TimeSpan Duration {get { return _fadeIn + _display + _fadeOut; }}

        private FlashState CurrentState { get; set; }

        private enum FlashState
        {
            Disabled,
            FadingIn,
            Displayed,
            FadingOut
        }
        
        public FlashMessage(Game game)
            : base(game)
        {
            _fadeIn = TimeSpan.FromMilliseconds(500);
            _display = TimeSpan.FromMilliseconds(1500);
            _fadeOut = TimeSpan.FromMilliseconds(500);
            _targetPosition = 400;
            _startingPosition = 500;
            _currentPosition = _startingPosition;
            Enabled = false;
            Visible = false;
        }

        public void StartFlash(string message)
        {
            Enabled = true;
            Visible = true;
            _message = message;
            _accumulator = TimeSpan.Zero;
            CurrentState = FlashState.FadingIn;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            _menuFont = Game.Content.Load<SpriteFont>("MenuFont");
        }


        public override void Update(GameTime gameTime)
        {
            _accumulator += gameTime.ElapsedGameTime;

            switch (CurrentState)
            {
                case FlashState.FadingIn:
                    if (_accumulator >= _fadeIn)
                    {
                        _accumulator = TimeSpan.Zero;
                        CurrentState = FlashState.Displayed;
                        _currentPosition = _targetPosition;
                    }
                    else
                    {
                        float percent = _accumulator.Milliseconds / (float)_fadeIn.Milliseconds;
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

        public override void Draw(GameTime gameTime)
        {
            var xCenter = Game.GraphicsDevice.Viewport.Width / 2;
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_menuFont, _message, new Vector2(xCenter - (_menuFont.MeasureString(_message).X / 2), _currentPosition), Color.White);
            _spriteBatch.End();

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
