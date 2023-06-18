using DiamondNoise.Noise;
using DiamondNoise.Noise.Diamond;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondNoise
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D scalarField;
        private DiamondNoiseGenerator noiseGenerator;
        private DiamondNoiseGenerator.Result checkpoint;
       
        private int counter = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 512;
            _graphics.PreferredBackBufferHeight = 512;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            noiseGenerator = new DiamondNoiseGenerator(1f, randomDecay: 0.9f, edgeValueSource: DiamondNoiseGenerator.EdgeValueSource.Random);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            checkpoint = noiseGenerator.Generate(16); 
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((counter++ & 128) == 0)
            {

                UpdateField();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(scalarField, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
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
            var field = checkpoint.Content;
            scalarField = field.Normalize().Remap11To01().AsScalarFieldTexture(checkpoint.Size, GraphicsDevice);
        }
    }
}