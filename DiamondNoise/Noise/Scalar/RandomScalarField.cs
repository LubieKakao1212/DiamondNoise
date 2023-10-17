using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public class RandomScalarField : IScalarField
    {
        private int seed;
        private Random random;
        private bool normalizeOutput;

        public RandomScalarField(int seed, bool normalizeOutput)
        {
            this.seed = seed;
            this.normalizeOutput = normalizeOutput;
            this.random = new Random(seed);
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            var value = random.NextSingle();
            if (normalizeOutput)
            {
                value *= 2f;
                value -= 1f;
            }
            return value;
        }

        public IScalarField NewState()
        {
            return new RandomScalarField(random.Next(), normalizeOutput);
        }
    }
}
