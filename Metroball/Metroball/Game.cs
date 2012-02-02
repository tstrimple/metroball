using System;
using System.Collections.Generic;
using System.Linq;
using Metroball.Lib;
using Metroball.Lib.GameState;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
 
namespace Metroball
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GameStateEngine _gameStateEngine;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private BasicEffect _primaryEffect;

        public Game()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _gameStateEngine = new GameStateEngine(this);
            Content.RootDirectory = "Content";

            TargetElapsedTime = TimeSpan.FromTicks(333333);
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width * 0.5f,
                GraphicsDevice.Viewport.Height * 0.5f);

            _primaryEffect = new BasicEffect(GraphicsDevice)
            {
                LightingEnabled = false,
                World = Matrix.Identity,
                View =
                   Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0)),
                Projection =
                    Matrix.CreateOrthographic(800.0f, 480.0f, -0.5f, 1.0f),
                VertexColorEnabled = true
            };

            _gameStateEngine.LoadContent();
        }

        protected override void UnloadContent()
        {
            _gameStateEngine.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if(_gameStateEngine.ExitState())
                {
                    Exit();   
                }
            }

            _gameStateEngine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _gameStateEngine.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
