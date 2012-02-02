using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Metroball.Lib.GameState
{
    public class MainMenuGameState : GameComponent, IGameState
    {
        private SpriteFont _logoFont;
        private SpriteFont _menuFont;

        private SpriteBatch _spriteBatch;

        private readonly Game _game;

        public MainMenuGameState(Game game)
            : base(game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            _logoFont = _game.Content.Load<SpriteFont>("LogoFont");
            _menuFont = _game.Content.Load<SpriteFont>("MenuFont");
            _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
        }

        public void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            var yCenter = _game.GraphicsDevice.Viewport.Height / 2;
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "Metroball", new Vector2(-10, -30), Color.White);
            var startMeasurements = _menuFont.MeasureString("start game");
            _spriteBatch.DrawString(_menuFont, "start game", new Vector2(xCenter - (startMeasurements.X / 2) - 10, yCenter - startMeasurements.Y), Color.White);
            var settingsMeasurements = _menuFont.MeasureString("settings");
            _spriteBatch.DrawString(_menuFont, "settings", new Vector2(xCenter - (settingsMeasurements.X / 2) - 10, yCenter - settingsMeasurements.Y + 100), Color.White);
            _spriteBatch.End();
        }
    }
}
