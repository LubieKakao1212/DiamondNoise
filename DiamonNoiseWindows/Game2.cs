using DiamondNoise.Noise.Diamond;
using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace DiamondNoise
{
    public class Game2 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D scalarField;
        private DiamondNoiseGenerator2D noiseGenerator;
        private DiamondNoiseGenerator2D.Checkpoint checkpoint;
        private DiamondNoise1D.Checkpoint checkpoint2;

        private int imageSize = 512;

        private int counter = 0;

        private int imageCount = 2;

        private int maxIteration = 20;
        private int maxIterationHorizontal = 10;
        private int maxIterationVertical = 10;

        private GameWindow otherWindow;

        private int i = 0;

        public Game2()
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
            //var gradient = new GradientScalarField(Vector2.UnitX * 2f, -1f);

            var multiply = new MultiplyScalarField(random);

            var damping = new DampingScalarFieldWrapper(multiply, decay: 0.7f);

            //checkpoint = noiseGenerator.Generate(damping, 0);
            checkpoint2 = DiamondNoise1D.Create(damping, vertivalEdgeGeneration: DiamondNoise1D.EdgeGenerationType.NoEdge);

            UpdateField();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((counter++ & 63) == 0)
            {
                /*if (checkpoint.Iteration < maxIteration)
                {
                    checkpoint = noiseGenerator.ContinueGeneration(checkpoint, 2);
                    UpdateField();
                }*/

                if ((i++ & 1) == 0)
                {
                    if (checkpoint2.HorizontalIteration < maxIterationHorizontal)
                    {
                        checkpoint2 = checkpoint2.GenerateHorizontal();
                        UpdateField();
                    }
                }
                else
                {
                    if (checkpoint2.VerticalIteration < maxIterationVertical)
                    {
                        checkpoint2 = checkpoint2.GenerateVertical();
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

            for (int x = 0; x < imageCount; x++)
                for (int y = 0; y < imageCount; y++)
                {
                    _spriteBatch.Draw(scalarField, new Rectangle(x * imageSize, y * imageSize,
                        imageSize,
                        imageSize), Color.White);
                }
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void UpdateField()
        {
            if (scalarField != null)
            {
                scalarField.Dispose();
            }
            var field = new List<float>(checkpoint2.Content).ToArray().Normalize();
            field.MapNormalizedTo01();
            scalarField = field.AsScalarFieldTexture(checkpoint2.Width, GraphicsDevice);
        }
    }
}