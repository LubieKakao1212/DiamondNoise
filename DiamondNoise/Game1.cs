using DiamondNoise.Noise.Diamond;
using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace DiamondNoise
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D scalarField;
        private DiamondNoiseGenerator2D noiseGenerator;
        private DiamondNoiseGenerator2D.Checkpoint checkpoint;
        private DiamondNoise1D.Checkpoint checkpointR;
        private DiamondNoise1D.Checkpoint checkpointG;
        private DiamondNoise1D.Checkpoint checkpointB;

        private int imageSize = 256;

        private int counter = 0;

        private int imageCount = 1;

        private int maxIteration = 10;
        private int maxIterationHorizontal = 5;
        private int maxIterationVertical = 5;

        //private GameWindow otherWindow;

        private int i = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = imageSize * imageCount;
            _graphics.PreferredBackBufferHeight = imageSize * imageCount;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            noiseGenerator = new DiamondNoiseGenerator2D(DiamondNoiseGenerator2D.EdgeValueSource.Constant, 0f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var random = new RandomScalarField(1338, true);
            var gradient = new GradientScalarField(Vector2.UnitX + Vector2.UnitY, 0f);

            var multiply = new MultiplyScalarField(random);

            var damping = new DampingScalarFieldWrapper(multiply, decay: 0.5f);

            Window.AllowUserResizing = true;

            //checkpoint = noiseGenerator.Generate(damping, 0);
            checkpointR = DiamondNoise1D.Create(damping, vertivalEdgeGeneration: DiamondNoise1D.EdgeGenerationType.Loop, verticalEdgeConstant: 0f);

            var next = damping.NewState().NewState();

            checkpointG = DiamondNoise1D.Create(next, vertivalEdgeGeneration: DiamondNoise1D.EdgeGenerationType.Loop, verticalEdgeConstant: 0f);

            next = next.NewState();

            checkpointB = DiamondNoise1D.Create(next, vertivalEdgeGeneration: DiamondNoise1D.EdgeGenerationType.Loop, verticalEdgeConstant: 0f);

            for (int i = 0; i < 4; i++)
            {
                checkpointG = checkpointG.GenerateHorizontal();
                checkpointG = checkpointG.GenerateVertical();
            }

            var g = new List<float>(checkpointG.Content).ToArray().Normalize().MapNormalizedTo01();
            var tex = ScalarFieldUtils.AsScalarFieldsTexture(g, g, g, checkpointR.Width, GraphicsDevice);

            using var file = File.Create("g.png");

            tex.SaveAsPng(file, checkpointG.Width, checkpointG.Height);

            UpdateField();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((counter++ & 15) == 0)
            {
                /*if (checkpoint.Iteration < maxIteration)
                {
                    checkpoint = noiseGenerator.ContinueGeneration(checkpoint, 2);
                    UpdateField();
                }*/
                if ((i++ & 1) == 0)
                {
                    if (checkpointR.HorizontalIteration < maxIterationHorizontal)
                    {
                        checkpointR = checkpointR.GenerateHorizontal();
                        //checkpointG = checkpointG.GenerateHorizontal();
                        //checkpointB = checkpointB.GenerateHorizontal();
                        UpdateField();
                    }
                }
                else
                {
                    if (checkpointR.VerticalIteration < maxIterationVertical)
                    {
                        checkpointR = checkpointR.GenerateVertical();
                        //checkpointG = checkpointG.GenerateVertical();
                        //checkpointB = checkpointB.GenerateVertical();
                        UpdateField();
                    }
                }
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var ar = (float)scalarField.Width / scalarField.Height;

            var width = (int)(imageSize * ar);

            for (int x = 0; x < imageCount; x++)
                for (int y = 0; y < imageCount; y++)
                {
                    _spriteBatch.Draw(scalarField, new Rectangle(x * width, y * imageSize,
                        width,
                        imageSize), Color.White);
                }
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void UpdateField()
        {
            if(scalarField != null)
            {
                scalarField.Dispose();
            }
            //var field = new List<float>(checkpoint.Content).ToArray().Normalize()
            //    //.Select((v) => (float)MathF.Sign(v + 0.2f))
            //    .MapNormalizedTo01();
            //;

            var r = new List<float>(checkpointR.Content).ToArray().Normalize().MapNormalizedTo01();
            //var g = new List<float>(checkpointG.Content).ToArray().Normalize().MapNormalizedTo01();
            //var b = new List<float>(checkpointB.Content).ToArray().Normalize().MapNormalizedTo01();

            //scalarField = field.ToArray()
            //    .MapNormalizedTo01()
            //    .AsScalarFieldTexture(checkpoint2.Width, GraphicsDevice);
            scalarField = ScalarFieldUtils.AsScalarFieldsTexture(r, r, r, checkpointR.Width, GraphicsDevice);
        }
    }
}