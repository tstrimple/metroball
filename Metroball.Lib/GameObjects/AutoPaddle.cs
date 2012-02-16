using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class AutoPaddle : Paddle
    {
        public int Level { get; set; }
        private Ball _ball;

        public AutoPaddle(Ball ball, Arena arena, float depth) : base(arena, depth)
        {
            Level = 1;
            _ball = ball;

            Left = -0.3f;
            Right = 0.3f;
            Top = 0.25f;
            Bottom = -0.25f;

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

        public override void Update()
        {
            if(Disabled)
            {
                return;
            }

            float cap = 0.03f + (Level / 100.0f);
            Vector3 vector = _ball.Position - Position;
            vector.X = vector.X.Clip(-cap, cap);
            vector.Y = vector.Y.Clip(-cap, cap);
            vector.Z = 0.0f;

            Position = Position + vector;

            base.Update();
        }
    }
}
