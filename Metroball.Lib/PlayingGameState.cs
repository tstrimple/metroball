using System;
using System.Collections.Generic;
using System.Linq;
using Metroball.Lib.GameObjects;
using Metroball.Lib.UI;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using Microsoft.Advertising.Mobile.Xna;

namespace Metroball.Lib
{
    public class GameOverEventArgs : EventArgs
    {
        public int Score { get; set; }
    }

    public class PlayingGameState : GameComponent
    {
        private string _userId;
        private string _sessionId;
        public string GameId { get; set; }
        private static AdGameComponent _adGameComponent;
        private static DrawableAd _ad;

        public bool SoundEnabled { get; set; }
        private SpriteBatch _spriteBatch;
        private readonly Game _game;

        private bool _holdingPaddle;

        private Ball _ball;
        private Arena _arena;
        private DepthLine _depthLine;
        private AutoPaddle _cpuPaddle;
        private Paddle _playerPaddle;
        private BasicEffect _basicEffect;
        private List<Vector3> _averageVelocity;
        private SpriteFont _gameFont;
        private int _cpuLives;
        private int _playerLives;
        public int Level { get; set; }
        private SoundEffect _miss;
        private SoundEffect _paddle;
        private SoundEffect _wall;
        public int Score { get; set; }
        private int _volleyCount;
        private bool _curveBonus;

        public string Rank { get; set; }

        public DateTime Started { get; set; }

        public EventHandler<GameOverEventArgs> GameOver;

        public PlayingGameState(Game game, bool soundEnabled, string userId, string sessionId)
            : base(game)
        {
            Rank = "N/A";
            Started = DateTime.UtcNow;
            _userId = userId;
            _sessionId = sessionId;
            GameId = Guid.NewGuid().ToString();
            _adGameComponent = AdGameComponent.Current;

            _averageVelocity = new List<Vector3>();
            _holdingPaddle = false;

            _game = game;
            SoundEnabled = soundEnabled;
        }

        public void Reset()
        {
            _cpuLives = 3;
            _playerLives = 3;
            Level = 1;
            Score = 0;
            _volleyCount = 0;
            _curveBonus = false;
        }

        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);

            var model = _game.Content.Load<Model>("Sphere");
            _gameFont = _game.Content.Load<SpriteFont>("GameFont");
            _miss = _game.Content.Load<SoundEffect>("Miss");
            _paddle = _game.Content.Load<SoundEffect>("Paddle");
            _wall = _game.Content.Load<SoundEffect>("Wall");

            _ball = new Ball(model);
            _arena = new Arena();
            _depthLine = new DepthLine(_ball, _arena);
            _cpuPaddle = new AutoPaddle(_ball, _arena, _arena.Far);
            _playerPaddle = new Paddle(_arena, _arena.Near);

            _ad = _adGameComponent.CreateAd("81995", new Rectangle(320, 0, 480, 80));

            Service.UpdateGameStatus(GameId, _userId, _sessionId, new GameResults() { Level = Level, Score = Score, Started = Started, Status = GameStatus.InProgress, Ended = null});

            _basicEffect = new BasicEffect(_game.GraphicsDevice)
            {
                LightingEnabled = false,
                World = Matrix.Identity,
                View =
                    Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f)),
                Projection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, _game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 5.5f),
                VertexColorEnabled = true
            };
        }

        public void UpdateRank(int? rank)
        {
            if(rank.HasValue)
            {
                Rank = rank.Value.ToString();
            }
        }

        public void Closing()
        {
            Service.UpdateGameStatus(GameId, _userId, _sessionId, new GameResults() { Level = Level, Score = Score, Started = Started, Status = GameStatus.Abandoned, Ended = DateTime.UtcNow });
        }

        public void UnloadContent()
        {
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

            Vector3 nearPoint = _game.GraphicsDevice.Viewport.Unproject(nearSource,
                _basicEffect.Projection, _basicEffect.View, _basicEffect.World);

            Vector3 farPoint = _game.GraphicsDevice.Viewport.Unproject(farSource,
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
                    _arena.Color = Color.DarkSlateGray;
                }
                else
                {
                    _volleyCount = 0;
                    PlayMiss();
                    _cpuLives -= 1;

                    Service.GetRank(Score, UpdateRank);

                    if (_cpuLives <= 0)
                    {
                        _cpuLives = 3;
                        Level += 1;
                    }
                    _ball.Reset();
                    _arena.Color = Color.Red;
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

                    _arena.Color = Color.DarkSlateGray;

                    int points = (int)(1000.0f + (1000.0f * ((Level - 1) / 10.0f)));
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

                        Score += points;
                    }
                }
                else
                {
                    PlayMiss();
                    _playerLives -= 1;
                    if (_playerLives <= 0)
                    {
                        Service.GetRank(Score, UpdateRank);
                        GameOver.Invoke(this, new GameOverEventArgs() {Score = Score});
                    }
                    else
                    {
                        Service.GetRank(Score, UpdateRank);
                        Service.UpdateGameStatus(GameId, _userId, _sessionId, new GameResults() { Level = Level, Score = Score, Started = Started, Status = GameStatus.InProgress, Ended = null });
                    }

                    _ball.Reset();
                    _arena.Color = Color.Red;
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
            if(SoundEnabled)
            {
                _paddle.Play();   
            }
        }

        private void PlayPaddleHit()
        {
            if (SoundEnabled)
            {
                _paddle.Play();
            }
        }

        private void PlayMiss()
        {
            if (SoundEnabled)
            {
                _miss.Play();
            }
        }

        public void Update(GameTime gameTime, TouchCollection touches)
        {
            _adGameComponent.Update(gameTime);
            _ball.Level = Level;
            _cpuPaddle.Level = Level;

            UpdateTouch(touches);
            _ball.Update();
            _depthLine.Update();
            _cpuPaddle.Update();
            _playerPaddle.Update();
            CheckForBallCollission();
        }

        private void UpdateTouch(TouchCollection touches)
        {
            if (touches.Count > 0)
            {
                var press = touches[0];

                if (!_holdingPaddle & press.State == TouchLocationState.Pressed)
                {
                    _holdingPaddle = true;
                }
                else if (_holdingPaddle && press.State == TouchLocationState.Released)
                {
                    _holdingPaddle = false;
                    _playerPaddle.Velocity = Vector3.Zero;
                }
                else if (_holdingPaddle && press.State == TouchLocationState.Moved)
                {
                    TouchLocation previousPress;
                    if (press.TryGetPreviousLocation(out previousPress))
                    {
                        _playerPaddle.Velocity = GetCursorPosition(press.Position) - GetCursorPosition(previousPress.Position);
                    }
                }
            }

            _averageVelocity.Add(_playerPaddle.Velocity);
            while (_averageVelocity.Count > 5)
            {
                _averageVelocity.RemoveAt(0);
            }
        }

        public void Draw(GameTime gameTime)
        {
            var vp = _game.GraphicsDevice.Viewport;
            var fullHeight = vp.Height;
            var fullWidth = vp.Width;
            vp.Height = fullHeight - 80;
            vp.Width = vp.Width * (fullHeight / vp.Height);
            vp.Y = 80;
            vp.X = (fullWidth - vp.Width) / 2;
            _game.GraphicsDevice.Viewport = vp;

            _arena.Draw(_game.GraphicsDevice, _basicEffect);
            _ball.Draw(_game.GraphicsDevice, _basicEffect);
            _depthLine.Draw(_game.GraphicsDevice, _basicEffect);
            _cpuPaddle.Draw(_game.GraphicsDevice, _basicEffect);
            _playerPaddle.Draw(_game.GraphicsDevice, _basicEffect);

            vp = _game.GraphicsDevice.Viewport;
            vp.X = 0;
            vp.Y = 0;
            vp.Width = 800;
            vp.Height = 480;
            _game.GraphicsDevice.Viewport = vp;

            
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_gameFont, _cpuLives.ToString(),
                                    new Vector2(20, 90), Color.White);
            _spriteBatch.DrawString(_gameFont, _playerLives.ToString(),
                                    new Vector2(_game.GraphicsDevice.Viewport.Width - 40, 90), Color.White);
            _spriteBatch.DrawString(_gameFont, String.Format("SCORE: {0}  LEVEL: {1}\nRANK: {2}", Score, Level, Rank),
                                    new Vector2(20, 10), Color.White);
            _spriteBatch.End();

            _adGameComponent.Draw(gameTime);

            _game.GraphicsDevice.BlendState = BlendState.Opaque;
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
