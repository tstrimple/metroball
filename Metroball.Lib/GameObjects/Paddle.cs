using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class Paddle : GameObject
    {
        private Arena _arena;
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom{ get; set; }
        public bool Disabled { get; set; }

        private Vector3 _position;
        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                var x = value.X;
                var y = value.Y;

                float xMax = _arena.Right - Right;
                float xMin = _arena.Left - Left;
                if (x < xMin)
                {
                    x = xMin;
                }
                else if (x > xMax)
                {
                    x = xMax;
                }

                float yMax = _arena.Top - Top;
                float yMin = _arena.Bottom - Bottom;
                if (y < yMin)
                {
                    y = yMin;
                }
                else if (y > yMax)
                {
                    y = yMax;
                }

                _position = new Vector3(x, y, value.Z);
            }
        }

        public Paddle(Arena arena, float depth)
        {
            _arena = arena;
            Left = -0.3f;
            Right = 0.3f;
            Top = 0.25f;
            Bottom = -0.25f;

            Position = new Vector3(0.0f, 0.0f, depth);

            Meshes =
                new[]
                    {
                        new[]
                            {
                                new VertexPositionColor(
                                    new Vector3(Right,
                                                Top,
                                                0.0f), Color.Blue),
                                new VertexPositionColor(
                                    new Vector3(Left,
                                               Top,
                                                0.0f),
                                    Color.Blue),
                                new VertexPositionColor(
                                    new Vector3(Left,
                                                Bottom,
                                                0.0f),
                                    Color.Blue),
                                new VertexPositionColor(
                                    new Vector3(Right,
                                                Bottom,
                                                0.0f),
                                    Color.Blue),
                                new VertexPositionColor(
                                    new Vector3(Right,
                                                Top,
                                                0.0f), Color.Blue)
                            }
                    };
        }

        protected override BoundingBox BuildBoundingBox()
        {
            var min = new Vector3
                          {
                              X = Position.X + Left,
                              Y = Position.Y + Bottom,
                              Z = Position.Z
                          };

            var max = new Vector3
                          {
                              X = Position.X + Right,
                              Y = Position.Y + Top,
                              Z = Position.Z + 0.1f
                          };

            return new BoundingBox(min, max);
        }
    }
}
