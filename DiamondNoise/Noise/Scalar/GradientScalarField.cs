using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public class GradientScalarField : IScalarField
    {
        private Vector2 gradient;
        private float offset;

        public GradientScalarField(Vector2 gradient, float offset)
        {
            this.gradient = gradient;
            this.offset = offset;
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            var value = Vector2.Dot(gradient, pos) + offset;

            return value;
        }

        public IScalarField NewState()
        {
            return new GradientScalarField(gradient, offset);
        }
    }
}
