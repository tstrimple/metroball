using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metroball.Lib.GameObjects;
using Metroball.Lib.Settings;
using Microsoft.Advertising.Mobile.Xna;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Metroball.Lib.Components
{
    public class GameScreen : DrawableGameComponent
    {
        private static AdGameComponent _adGameComponent;
        private static DrawableAd _ad;

        private SpriteBatch _spriteBatch;
        
        private Ball _ball;
        private Arena _arena;
        private DepthLine _depthLine;
        private AutoPaddle _cpuPaddle;
        private Paddle _playerPaddle;
        private BasicEffect _basicEffect;
        private List<Vector3> _averageVelocity;
        private SpriteFont _gameFont;

        public Results Results { get; set; }
        public bool Alert { get;set; }
        
        private SoundEffect _miss;
        private SoundEffect _paddle;
        private SoundEffect _wall;
        
        private int _volleyCount;
        private bool _curveBonus;

        public EventHandler PlayerScored;
        public EventHandler ComputerScored;
        public EventHandler ExitGameScreen;

        public GameScreen(Game game)
            : base(game)
        {
            Alert = false;
            _adGameComponent = AdGameComponent.Current;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            var model = Game.Content.Load<Model>("Sphere");
            _gameFont = Game.Content.Load<SpriteFont>("GameFont");
            _miss = Game.Content.Load<SoundEffect>("Miss");
            _paddle = Game.Content.Load<SoundEffect>("Paddle");
            _wall = Game.Content.Load<SoundEffect>("Wall");

            _ball = new Ball(model);
            _arena = new Arena();
            _depthLine = new DepthLine(_ball, _arena);
            _cpuPaddle = new AutoPaddle(_ball, _arena, _arena.Far);
            _playerPaddle = new Paddle(_arena, _arena.Near);

            _ad = _adGameComponent.CreateAd(SettingsManager.SmallAdId, new Rectangle(320, 0, 480, 80));

            Reset();
            
            _basicEffect = new BasicEffect(Game.GraphicsDevice)
            {
                LightingEnabled = false,
                World = Matrix.Identity,
                View =
                    Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f)),
                Projection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 5.5f),
                VertexColorEnabled = true
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ExitGameScreen.Invoke(this, new EventArgs());
            }

            _adGameComponent.Update(gameTime);
            _ball.Level = Results.Level;
            _cpuPaddle.Level = Results.Level;

            UpdateTouch(TouchPanel.GetState());
            _ball.Update();
            _depthLine.Update();
            _cpuPaddle.Update();
            _playerPaddle.Update();
            CheckForBallCollission();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var vp = Game.GraphicsDevice.Viewport;
            var fullHeight = vp.Height;
            var fullWidth = vp.Width;
            vp.Height = fullHeight - 80;
            vp.Width = vp.Width * (fullHeight / vp.Height);
            vp.Y = 80;
            vp.X = (fullWidth - vp.Width) / 2;
            Game.GraphicsDevice.Viewport = vp;

            if(Alert)
            {
                _arena.Color = Color.Red;
            }
            else
            {
                _arena.Color = Color.DarkSlateGray;    
            }

            _arena.Draw(Game.GraphicsDevice, _basicEffect);
            _ball.Draw(Game.GraphicsDevice, _basicEffect);
            _depthLine.Draw(Game.GraphicsDevice, _basicEffect);
            _cpuPaddle.Draw(Game.GraphicsDevice, _basicEffect);
            _playerPaddle.Draw(Game.GraphicsDevice, _basicEffect);

            vp = Game.GraphicsDevice.Viewport;
            vp.X = 0;
            vp.Y = 0;
            vp.Width = 800;
            vp.Height = 480;
            Game.GraphicsDevice.Viewport = vp;

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_gameFont, Results.ComputerLives.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(20, 90), Color.White);
            _spriteBatch.DrawString(_gameFont, Results.PlayerLives.ToString(CultureInfo.InvariantCulture),
                                    new Vector2(Game.GraphicsDevice.Viewport.Width - 40, 90), Color.White);

            if (Results.Rank.HasValue)
            {
                _spriteBatch.DrawString(_gameFont, String.Format("SCORE: {0}  LEVEL: {1}\nRANK: {2}", Results.Score, Results.Level, Results.Rank), new Vector2(20, 10), Color.White);
            }
            else
            {
                _spriteBatch.DrawString(_gameFont, String.Format("SCORE: {0}  LEVEL: {1}", Results.Score, Results.Level), new Vector2(20, 10), Color.White);
            }

            _spriteBatch.End();

            _adGameComponent.Draw(gameTime);

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }

        private Vector3 GetCursorPosition(Vector2 touchPosition)
        {
            Ray r = CalculateCursorRay(touchPosition);
            var translatedPosition = r.Direction;
            var multiplier = translatedPosition.Z / _playerPaddle.Position.Z;

            return new Vector3(translatedPosition.X / multiplier, translatedPosition.Y / multiplier, _playerPaddle.Position.Z);
        }

        private Ray CalculateCursorRay(Vector2 point)
        {
            Vector3 nearSource = new Vector3(point, 0.1f);
            Vector3 farSource = new Vector3(point, 5.5f);

            Vector3 nearPoint = Game.GraphicsDevice.Viewport.Unproject(nearSource,
                _basicEffect.Projection, _basicEffect.View, _basicEffect.World);

            Vector3 farPoint = Game.GraphicsDevice.Viewport.Unproject(farSource,
                _basicEffect.Projection, _basicEffect.View, _basicEffect.World);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            return new Ray(nearPoint, Vector3.Negate(direction));
        }

        private void CheckForBallCollission()
        {
            if (_ball.Position.X > _arena.Right - 0.05f || _ball.Position.X < _arena.Left + 0.05)
            {
                PlayWallHit();
                _ball.Position = new Vector3(_ball.Position.X.Clip(_arena.Left + 0.05f, _arena.Right - 0.05f), _ball.Position.Y, _ball.Position.Z);
                _ball.Velocity = _ball.Velocity * new Vector3(-1.0f, 1.0f, 1.0f);
            }

            if (_ball.Position.Y > _arena.Top - 0.05f || _ball.Position.Y < _arena.Bottom + 0.05f)
            {
                PlayWallHit();
                _ball.Position = new Vector3(_ball.Position.X, _ball.Position.Y.Clip(_arena.Bottom + 0.05f, _arena.Top - 0.05f), _ball.Position.Z);
                _ball.Velocity = _ball.Velocity * new Vector3(1.0f, -1.0f, 1.0f);
            }

            if (_ball.Position.Z >= _arena.Far - 0.05)
            {
                _ball.Position = new Vector3(_ball.Position.X, _ball.Position.Y, _arena.Far - 0.05f);
                _ball.Velocity = _ball.Velocity * new Vector3(1.0f, 1.0f, -1.0f);
                if (CheckForPaddleCollission(_cpuPaddle))
                {
                    PlayPaddleHit();
                    _ball.Curve = new Vector3(-_cpuPaddle.Velocity.X, -_cpuPaddle.Velocity.Y, 0.0f) / 6.5f;
                }
                else
                {
                    _volleyCount = 0;
                    PlayMiss();
                    PlayerScored.Invoke(this, new EventArgs());
                }
            }

            if (_ball.Position.Z <= _arena.Near + 0.05)
            {
                _ball.Position = new Vector3(_ball.Position.X, _ball.Position.Y, _arena.Near + 0.05f);
                _ball.Velocity = _ball.Velocity * new Vector3(1.0f, 1.0f, -1.0f);
                if (CheckForPaddleCollission(_playerPaddle))
                {
                    VibrateController vibrate = VibrateController.Default;
                    vibrate.Start(TimeSpan.FromMilliseconds(50));
                    _volleyCount += 1;
                    PlayPaddleHit();
                    _ball.Curve = new Vector3(-_averageVelocity.Average(v => v.X), -_averageVelocity.Average(v => v.Y), 0.0f) / 6.5f;
                    _curveBonus = _ball.Curve.Length() * 100.0f > 1;

                    int points = (int)(1000.0f + (1000.0f * ((Results.Level - 1) / 10.0f)));
                    points -= _volleyCount * 100;

                    if (points > 0)
                    {
                        if (_curveBonus)
                        {
                            points += 250;
                        }
                        else if (_ball.Curve == Vector3.Zero)
                        {
                            points = 0;
                        }

                        Results.Score += points;
                    }
                }
                else
                {
                    PlayMiss();
                    ComputerScored.Invoke(this, new EventArgs());
                }
            }
        }

        private bool CheckForPaddleCollission(Paddle paddle)
        {
            var ballBox = _ball.BoundingBox;

            var paddleBox = paddle.BoundingBox;
            paddleBox.Min.Z = 0.0f;
            paddleBox.Max.Z = 0.0f;

            var contains = paddleBox.Contains(ballBox);

            return contains == ContainmentType.Intersects || contains == ContainmentType.Contains;
        }

        private void PlayWallHit()
        {
            if (SettingsManager.SoundEnabled)
            {
                _paddle.Play();
            }
        }

        private void PlayPaddleHit()
        {
            if (SettingsManager.SoundEnabled)
            {
                _paddle.Play();
            }
        }

        private void PlayMiss()
        {
            if (SettingsManager.SoundEnabled)
            {
                _miss.Play();
            }
        }

        private void UpdateTouch(TouchCollection touches)
        {
            if (touches.Count > 0)
            {
                var press = touches[0];

                if (press.State == TouchLocationState.Moved)
                {
                    TouchLocation previousPress;
                    if (press.TryGetPreviousLocation(out previousPress))
                    {
                        _playerPaddle.Velocity = GetCursorPosition(press.Position) - GetCursorPosition(previousPress.Position);
                    }
                }
                else if (press.State == TouchLocationState.Released)
                {
                    _playerPaddle.Velocity = Vector3.Zero;
                }
            }

            _averageVelocity.Add(_playerPaddle.Velocity);
            while (_averageVelocity.Count > 5)
            {
                _averageVelocity.RemoveAt(0);
            }
        }

        public void Reset()
        {
            _averageVelocity = new List<Vector3>();

            _playerPaddle.Velocity = Vector3.Zero;
            _ball.Reset();
            _volleyCount = 0;
            _curveBonus = false;
        }
    }
}
