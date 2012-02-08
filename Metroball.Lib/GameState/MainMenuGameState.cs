using System;
using System.Collections.Generic;
using System.Linq;
using Metroball.Lib.UI;
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
        private Button _startButton;
        private Button _settingsButton;

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

            var xCenter = _game.GraphicsDevice.Viewport.Width / 2;
            var yCenter = _game.GraphicsDevice.Viewport.Height / 2;

            _startButton = new Button(_menuFont, "start game", Color.White);
            _startButton.Position = new Vector2(xCenter - (_startButton.Rectangle.Width / 2) - 10, yCenter - _startButton.Rectangle.Height);

            _settingsButton = new Button(_menuFont, "settings", Color.White);
            _settingsButton.Position = new Vector2(xCenter - (_settingsButton.Rectangle.Width / 2) - 10, yCenter - _settingsButton.Rectangle.Height + 100);
            
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
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_logoFont, "Metroball", new Vector2(-10, -30), Color.White);
            _startButton.Draw(_spriteBatch);
            _settingsButton.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
