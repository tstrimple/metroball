using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Metroball.Lib.GameState
{
    public interface IGameState
    {
        void LoadContent();
        void UnloadContent();
        void Update(GameTime gameTime, TouchCollection touches);
        void Draw(GameTime gameTime);
        void Closing();
    }
}
