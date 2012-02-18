using System;
using Microsoft.Xna.Framework;

namespace Metroball.Lib.Components
{
    public class DelayedCallback : GameComponent
    {
        private TimeSpan _elapsed;
        private readonly TimeSpan _delay;
        private readonly EventHandler _callback;

        public DelayedCallback(Game game, TimeSpan delay, EventHandler callback)
            : base(game)
        {
            _elapsed = TimeSpan.Zero;
            _delay = delay;
            _callback = callback;
        }

        public override void Update(GameTime gameTime)
        {
            _elapsed += gameTime.ElapsedGameTime;

            if(_elapsed >= _delay)
            {
                if(_callback != null)
                {
                    _callback.Invoke(this, new EventArgs());
                }
            }

            base.Update(gameTime);
        }
    }
}
