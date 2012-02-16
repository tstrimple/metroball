using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class DepthLine : GameObject
    {
        private Ball _ball;
        private Arena _arena;

        public DepthLine(Ball ball, Arena arena)
        {
            _ball = ball;
            _arena = arena;

            var depthLineColor = Color.LightSlateGray;
            Meshes = new[]
                         {
                             new[]
                                 {
                                     new VertexPositionColor(
                                         new Vector3(_arena.Right,
                                                     _arena.Top,
                                                     0.0f), depthLineColor),
                                     new VertexPositionColor(
                                         new Vector3(_arena.Left,
                                                     _arena.Top,
                                                     0.0f),
                                         depthLineColor),
                                     new VertexPositionColor(
                                         new Vector3(_arena.Left,
                                                     _arena.Bottom,
                                                     0.0f),
                                         depthLineColor),
                                     new VertexPositionColor(
                                         new Vector3(_arena.Right,
                                                     _arena.Bottom,
                                                     0.0f),
                                         depthLineColor),
                                     new VertexPositionColor(
                                         new Vector3(_arena.Right,
                                                     _arena.Top,
                                                     0.0f), depthLineColor)
                                 }
                         };
        }

        public override void Update()
        {
            Position = new Vector3(Position.X, Position.Y, _ball.Position.Z);
        }
    }
}
