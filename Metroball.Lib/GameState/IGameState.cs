using Microsoft.Xna.Framework;

namespace Metroball.Lib.GameState
{
    public interface IGameState
    {
        void LoadContent();
        void UnloadContent();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
