using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public class DampingScalarFieldWrapper : IScalarField
    {
        private float scale;
        private float decay;

        private IScalarField inner;

        public DampingScalarFieldWrapper(IScalarField inner, float scale = 1f, float decay = 0.5f)
        {
            this.scale = scale;
            this.decay = decay;
            this.inner = inner;
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            return inner.GetValue(pos, iteration) * scale * MathF.Pow(decay, iteration);
        }

        public IScalarField NewState()
        {
            return new DampingScalarFieldWrapper(inner.NewState(), scale, decay);
        }
    }
}
