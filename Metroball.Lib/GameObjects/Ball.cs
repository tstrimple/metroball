using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class Ball : GameObject
    {
        private readonly Model _model;
        public Vector3 Curve;
        private Vector3 _velocity;
        public int Level { get; set; }

        private readonly float _curveDegredation;
        private readonly float _velocityCap;

        public override Vector3 Velocity
        {
            get { return _velocity; }
            set
            {
                var cap = _velocityCap + (Level / 100.0f);
                _velocity.X = value.X.Clip(-cap, cap);
                _velocity.Y = value.Y.Clip(-cap, cap);
                _velocity.Z = value.Z;
            }
        }

        public Ball(Model model)
        {
            _velocityCap = 0.15f;
            _curveDegredation = 0.95f;
            _model = model;
            Level = 1;
            Reset();
        }

        public void Reset()
        {
            Curve = Vector3.Zero;
            Position = new Vector3(0.0f, 0.0f, 1.1f);
            Velocity = new Vector3(0.0f, 0.0f, 0.06f + Level / 100.0f);
        }

        protected override BoundingBox BuildBoundingBox()
        {
            var min = new Vector3(Position.X - 0.1f, Position.Y - 0.1f, 0.00f);
            var max = new Vector3(Position.X + 0.1f, Position.Y + 0.1f, 0.00f);
            return new BoundingBox(min, max);
        }

        public override void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            var transforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Position);
                    effect.View = basicEffect.View;
                    effect.Projection = basicEffect.Projection;
                }

                mesh.Draw();
            }
        }

        public void DrawBoundingBox(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            var boundingBox = BoundingBox;
            var mesh = new[]
                             {
                                 new VertexPositionColor(
                                     new Vector3(boundingBox.Max.X,
                                                 boundingBox.Max.Y,
                                                 1.5f), Color.Pink),
                                 new VertexPositionColor(
                                     new Vector3(boundingBox.Max.X,
                                                 boundingBox.Min.Y,
                                                 1.5f),
                                     Color.Pink),
                                 new VertexPositionColor(
                                     new Vector3(boundingBox.Min.X,
                                                 boundingBox.Min.Y,
                                                 1.5f),
                                     Color.Pink),
                                 new VertexPositionColor(
                                     new Vector3(boundingBox.Min.X,
                                                 boundingBox.Max.Y,
                                                 1.5f),
                                     Color.Pink),
                                 new VertexPositionColor(
                                     new Vector3(boundingBox.Max.X,
                                                 boundingBox.Max.Y,
                                                 1.5f), Color.Pink)
                             };

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, mesh, 0, mesh.Length - 1);
            }
        }

        private void DegradeCurve()
        {
            Curve = Curve * _curveDegredation;
        }

        public override void Update()
        {
            Velocity = Velocity + Curve;
            Position = Position + Velocity;
            DegradeCurve();
        }
    }
}
