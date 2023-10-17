using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Coherent.D1
{
    public static class Noise1D 
    {
        private static uint M1 = 1597334677U;     //1719413*929
        private static uint M2 = 3812015801U;     //140473*2467*11
        
        private static float hash_Tong(int seed, int v)
        {
            var q1 = unchecked((uint)seed);
            var q2 = unchecked((uint)v);
            q1 *= M1;
            q2 *= M2;
            uint n = q1 ^ q2;
            n = n * (n ^ (n >> 15));
            return n / (float)(0xffffffffU);
        }

        private static float Quintic(float t)
        {
             return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f); 
        }

        private static float CubicLerp(float a, float b, float c, float d, float x)
        {
            float p = (d - c) - (a - b);

            return x * (x * (x * p + ((a - b) - p)) + (c - a)) + b;
        }

        private static float Falloff(float x)
        {
            float f = 1f - x * x;
            return f * f * f;
        }

        public static float Perlin(int seed, float x)
        {
            var gx0 = (int) MathF.Floor(x);
            var gx1 = gx0 + 1;

            var gr0 = hash_Tong(seed, gx0) * 2f - 1f; 
            var gr1 = hash_Tong(seed, gx1) * 2f - 1f;

            var x0 = x - gx0;

            var t = Quintic(x0);

            return (gr0 * x0) * (1f - t) + (gr1 * (x0 - 1f)) * (t);
        }

        public static float Value(int seed, float x)
        {
            var gx0 = (int)MathF.Floor(x);
            var gx1 = gx0 + 1;

            var gr0 = hash_Tong(seed, gx0) * 2f - 1f;
            var gr1 = hash_Tong(seed, gx1) * 2f - 1f;

            var x0 = x - gx0;

            var t = Quintic(x0);

            return (gr0) * (1f - t) + (gr1) * (t);
        }

        public static float Cubic(int seed, float x)
        {
            int xi = (int)MathF.Floor(x);
            float lerp = x - xi;

            return CubicLerp(
                    hash_Tong(seed, xi - 1),
                    hash_Tong(seed, xi),
                    hash_Tong(seed, xi + 1),
                    hash_Tong(seed, xi + 2),
                    lerp) * 0.5f;// + 0.25f;
        }

        public static float SimplexValue(int seed, float x)
        {
            var gx0 = (int)MathF.Floor(x);
            var gx1 = gx0 + 1;

            var gr0 = hash_Tong(seed, gx0) * 2f - 1f;
            var gr1 = hash_Tong(seed, gx1) * 2f - 1f;

            var x0 = x - gx0;

            return gr0 * Falloff(x0) + gr1 * Falloff(x0 - 1f);
        }

        public static float SimplexGradient(int seed, float x)
        {
            var gx0 = (int)MathF.Floor(x);
            var gx1 = gx0 + 1;

            var gr0 = hash_Tong(seed, gx0) * 2f - 1f;
            var gr1 = hash_Tong(seed, gx1) * 2f - 1f;

            var x0 = x - gx0;

            return gr0 * x0 * Falloff(x0) + gr1 * (x0 - 1f) * Falloff(x0 - 1f);
        }

        public static float Voronoii(int seed, float x)
        {
            var gx0 = (int)MathF.Floor(x);
            var gx1 = gx0 + 1;

            var gr0 = hash_Tong(seed, gx0) - 0.5f;
            var gr1 = hash_Tong(seed, gx1) - 0.5f;

            var x0 = x - gx0;

            var o0 = gr0 + gx0;
            var o1 = gr1 + gx1;

            var d0 = x - o0;
            var d1 = x - o1;

            return MathHelper.Min(d0 * d0, d1 * d1);
        }

    }

    public class Noise1DSF : IScalarField
    {
        private int seed;
        private float scale;
        private Func<int, float, float> noise;

        public Noise1DSF(int seed, float scale, Func<int, float, float> noise)
        {
            this.seed = seed;
            this.scale = scale;
            this.noise = noise;
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            return noise(seed, pos.X * scale);
        }

        public IScalarField NewState()
        {
            return new Noise1DSF(seed, scale, noise);
        }
    }
}
