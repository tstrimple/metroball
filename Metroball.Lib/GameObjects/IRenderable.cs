using Microsoft.Xna.Framework.Graphics;

namespace Metroball.Lib.GameObjects
{
    public interface IRenderable
    {
        void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect);
    }
}
