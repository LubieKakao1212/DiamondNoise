using DiamondNoise.Noise.Coherent.D1;
using DiamondNoise.Noise.Diamond;
using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseDisplay
{
    public class NoiseDisplay : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D tex;

        private IScalarField[] fields;

        private float cooldown = 0;

        private int i = 0;

        public NoiseDisplay()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            tex = new Texture2D(GraphicsDevice,
                 GraphicsDevice.Viewport.Width,
                 GraphicsDevice.Viewport.Height);

            IScalarField r = new RandomScalarField(1337, true);
            IScalarField d = new DampingScalarFieldWrapper(r, scale: 1f, decay: 0.5f);

            var noise = DiamondNoise1D.Create(d,
                vertivalEdgeGeneration: DiamondNoise1D.EdgeGenerationType.NoEdge);

            noise = noise.Generate(10, DiamondNoise1D.GenerationAxis.Horizontal);

            fields = new IScalarField[]
            {
                new ArrayScalarField(noise.Content, noise.Width, 1),
                new Noise1DSF(1337, 4f, Noise1D.Cubic),
                new Noise1DSF(1337, 4f, Noise1D.Value),
                new Noise1DSF(1337, 4f, Noise1D.SimplexValue),
                new Noise1DSF(1337, 4f, Noise1D.SimplexGradient),
                new Noise1DSF(1337, 4f, Noise1D.Perlin),
                new Noise1DSF(1337, 4f, Noise1D.Voronoii)
            };

            tex.CreateGraphLine(fields[0], yScale: 1f, lineThickness: 0.01f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (cooldown > 0)
            {
                cooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            var r = Keyboard.GetState().IsKeyDown(Keys.Right);
            var l = Keyboard.GetState().IsKeyDown(Keys.Left);

            if (cooldown <= 0 && (r || l))
            {
                cooldown = 0.25f;

                i = r ? (++i) % fields.Length : (--i);

                if (i < 0)
                {
                    i = fields.Length - 1;
                }

                tex.CreateGraphLine(fields[i], yScale: 1f, lineThickness: 0.01f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(tex, GraphicsDevice.Viewport.Bounds, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}