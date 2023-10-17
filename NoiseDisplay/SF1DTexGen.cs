using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NoiseDisplay
{
    public static class SF1DTexGen
    {
        public static Texture2D CreateGraph(this Texture2D texIn, IScalarField field, Func<float, float, bool> drawCondition, float sampleY = 0f, float yScale = 1f)
        {
            var w = texIn.Width;
            var h = texIn.Height;

            Color[] pixels = new Color[w * h];

            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    float xN = ((float)x / (w - 1)) * 2f - 1f;
                    float yN = ((float)y / (h - 1)) * 2f - 1f;

                    yN *= yScale;

                    var v = field.GetValue(new Vector2(xN, sampleY));

                    if (drawCondition(yN, v))
                    {
                        pixels[(h - y - 1) * w + x] = Color.White;
                    }
                    else
                    {
                        pixels[(h - y - 1) * w + x] = Color.Black;
                    }
                }

            texIn.SetData(pixels);
            return texIn;
        }

        public static Texture2D CreateGraphFill(this Texture2D texIn, IScalarField field, float sampleY = 0f, float yScale = 1f)
        {
            return texIn.CreateGraph(field, (yN, v) => yN < v, sampleY, yScale);
        }

        public static Texture2D CreateGraphLine(this Texture2D texIn, IScalarField field, float sampleY = 0f, float yScale = 1f, float lineThickness = 0.05f)
        {
            var c = lineThickness * yScale;
            return texIn.CreateGraph(field, (yN, v) => MathF.Abs(yN - v) < c, sampleY, yScale);
        }
    }
}
