using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public static class ScalarFieldUtils
    {
        public static float[] Normalize(this float[] values)
        {
            var max = 0f;
            for (int i = 0; i < values.Length; i++)
            {
                max = MathF.Max(MathF.Abs(values[i]), max);
            }


            for (int i = 0; i < values.Length; i++)
            {
                values[i] /= max;
            }

            return values;
        }

        public static float[] MapNormalizedTo01(this float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (values[i] + 1f) / 2f;
            }
            return values;
        }

        public static Texture2D AsSquareScalarFieldTexture(this float[] values, int size, GraphicsDevice graphicsDevice)
        {
            if (values.Length != size * size)
            {
                throw new Exception();
            }

            var texture = new Texture2D(graphicsDevice, size, size, false, SurfaceFormat.Single);

            texture.SetData(values);
            return texture;
        }

        public static Texture2D AsScalarFieldTexture(this float[] values, int width, GraphicsDevice graphicsDevice)
        {
            var height = values.Length / width;
            var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Single);

            texture.SetData(values);
            return texture;
        }

        public static Texture2D AsScalarFieldsTexture(float[] r, float[] g, float[] b, int width, GraphicsDevice graphicsDevice)
        {
            var pixels = new Microsoft.Xna.Framework.Color[r.Length];

            for (int i = 0; i < r.Length; i++)
            {
                pixels[i] = new Microsoft.Xna.Framework.Color(r[i], g[i], b[i], 1f);//new Vector4(r[i], g[i], b[i], 1f);
            }

            var height = r.Length / width;

            var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);

            texture.SetData(pixels);
            return texture;
        }

    }
}
