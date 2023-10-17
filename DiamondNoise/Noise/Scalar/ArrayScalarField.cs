using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public class ArrayScalarField : IScalarField
    {
        private float[] data;

        private int width, height;

        public ArrayScalarField(float[] data, int width, int height)
        {
            this.data = data;
            this.width = width;
            this.height = height;
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            var xT = pos.X / 2.0f + 0.5f;
            var x = (int)MathF.Round(xT * width);
            
            var yT = pos.X / 2.0f + 0.5f;
            var y = (int)MathF.Round(yT * height);

            x = MathHelper.Clamp(x, 0, width - 1);
            y = MathHelper.Clamp(y, 0, height - 1);

            return data[y * width + x];
        }

        public IScalarField NewState()
        {
            var d = new float[data.Length];

            data.CopyTo(d, 0);

            return new ArrayScalarField(d, width, height);
        }
    }
}
