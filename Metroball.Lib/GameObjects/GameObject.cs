using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public class GameObject
    {
        protected VertexPositionColor[][] Meshes { get; set; }
        public virtual Vector3 Position { get; set; }
        public virtual Vector3 Velocity { get; set; }
        
        public BoundingBox BoundingBox 
        { 
            get { return BuildBoundingBox(); }
        }

        public GameObject()
        {
        }

        public virtual void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            basicEffect.World = Matrix.CreateTranslation(Position);

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in Meshes)
                {
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, mesh, 0, mesh.Length - 1);
                }
            }
        }

        public virtual void Update()
        {
            Position += Velocity;
        }

        protected virtual BoundingBox BuildBoundingBox()
        {
            throw new NotImplementedException();
        }
    }
}
