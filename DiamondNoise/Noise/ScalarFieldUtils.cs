using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise
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

        public static float[] Remap11To01(this float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (values[i] + 1f) / 2f;
            }
            return values;
        }

        public static Texture2D AsScalarFieldTexture(this float[] values, int size, GraphicsDevice graphicsDevice)
        {
            if (values.Length != (size * size))
            {
                throw new Exception();
            }

            var texture = new Texture2D(graphicsDevice, size, size, false, SurfaceFormat.Single);

            texture.SetData(values);
            return texture;
        }

    }
}
