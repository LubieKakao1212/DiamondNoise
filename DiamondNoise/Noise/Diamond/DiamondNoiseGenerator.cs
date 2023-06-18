using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Intrinsics;

namespace DiamondNoise.Noise.Diamond
{
    public class DiamondNoiseGenerator
    {
        private float randomStrength;
        private float randomDecay;
        private bool useNegativeRandom;
        private EdgeValueSource edgeValueSource;
        private float edgeConstant;

        public DiamondNoiseGenerator(float randomStrength = 1f, float randomDecay = 0.5f, bool useNegativeRandom = true, EdgeValueSource edgeValueSource = EdgeValueSource.Loop, float edgeConstant = 0f)
        {
            this.randomStrength = randomStrength;
            this.randomDecay = randomDecay;
            this.useNegativeRandom = useNegativeRandom;
            this.edgeValueSource = edgeValueSource;
            this.edgeConstant = edgeConstant;
        }

        public Result Generate(int iterations, int seed = 1337)
        {
            var random = new Random(seed);
            var iteration0 = new Result()
            {
                Content = new float[4]
                {
                    GetRandom(random, 0),
                    GetRandom(random, 0),
                    GetRandom(random, 0),
                    GetRandom(random, 0),
                },
                Iteration = 0,
                Seed = random.Next(),
                Size = 2
            };

            return ContinueGeneration(iteration0, iterations);
        }

        public Result ContinueGeneration(Result checkpoint, int iterations)
        {
            if (!checkpoint.Validate())
            {
                throw new Exception("Invalid Checkpoint");
            }

            for (int i = 0; i < iterations; i++)
            {
                checkpoint = Iteration(checkpoint);
            }

            return checkpoint;
        }

        public Result Iteration(in Result lastIteration)
        {
            if (lastIteration.Staggered)
            {
                return IterationEven(lastIteration);
            }
            else
            {
                return IterationOdd(lastIteration);
            }
        }

        //Staggered -> NotStaggered
        public Result IterationEven(in Result lastIteration)
        {
            var size = lastIteration.Size;
            var dataSize = size * size;
            var newData = new float[dataSize];
            var oldData = lastIteration.Content;
            
            var random = new Random(lastIteration.Seed);
            var iteration = lastIteration.Iteration + 1;

            for (int i = 0; i < dataSize; i += 1)
            {
                if ((i & 1) == 0)
                {
                    newData[i] = oldData[i];
                    continue;
                }
                var x = i % size;
                var y = i / size;
                float v1 = GetValue(oldData, size, x - 1, y, random, iteration);
                float v2 = GetValue(oldData, size, x + 1, y, random, iteration);
                float v3 = GetValue(oldData, size, x, y - 1, random, iteration);
                float v4 = GetValue(oldData, size, x, y + 1, random, iteration);
                newData[i] = GetNewValue(v1, v2, v3, v4, random, iteration);
            }

            return new Result()
            {
                Content = newData,
                Iteration = iteration,
                Seed = random.Next(),
                Size = size
            };

        }

        //NotStaggered -> Staggered
        public Result IterationOdd(Result lastIteration)
        {
            var oldSize = lastIteration.Size;
            var size = oldSize * 2 - 1;
            var dataSize = size * size;
            var newData = new float[dataSize];
            var oldData = lastIteration.Content;

            var random = new Random(lastIteration.Seed);
            var iteration = lastIteration.Iteration + 1;

            for (int i = 0; i < dataSize; i += 2)
            {
                var x = i % size;
                var y = i / size;
                var oldX = x / 2;
                var oldY = y / 2;
                //new value row
                if ((y & 1) == 1)
                {
                    if ((x & 1) == 1) 
                    {
                        var v1 = oldData[(oldY    ) * oldSize + oldX    ];
                        var v2 = oldData[(oldY    ) * oldSize + oldX + 1];
                        var v3 = oldData[(oldY + 1) * oldSize + oldX + 1];
                        var v4 = oldData[(oldY + 1) * oldSize + oldX    ];
                        newData[i] = GetNewValue(v1, v2, v3, v4, random, iteration);
                    }
                    else
                    {
                        throw new Exception("Wrong iteration");
                    }
                }
                //old value row
                else
                {
                    if ((x & 1) == 0)
                    {
                        newData[i] = oldData[oldY * oldSize + oldX];
                    }
                    else
                    { 
                        throw new Exception("Wrong iteration");
                    }
                }
            }

            return new Result()
            {
                Content = newData,
                Iteration = iteration,
                Seed = random.Next(),
                Size = size
            };
        }

        private float GetRandom(Random state, float iteration)
        {
            var value = state.NextSingle();
            if (useNegativeRandom)
            {
                value *= 2f;
                value -= 1f;
            }

            value *= randomStrength * MathF.Pow(randomDecay, iteration);

            return value;
        }

        //00X, 10 , 20X
        //01 , 11X, 21
        //02X, 12 , 22X

        private float GetValue(float[] data, int size, int x, int y, Random state, int iteration)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                return data[y * size + x];
            }
            return GetOBBValue(data, size, x, y, state, iteration);
        }

        private float GetOBBValue(float[] data, int size, int x, int y, Random state, int iteration)
        {
            switch (edgeValueSource) 
            {
                case EdgeValueSource.Random:
                    {
                        return GetRandom(state, iteration);
                    }
                case EdgeValueSource.Constant:
                    {
                        return edgeConstant;
                    }
                case EdgeValueSource.Loop:
                    break;
                default:
                    throw new Exception("Invalid EdgeValueSource setting");
            }

            if (x < 0)
            {
                x = size - 2;
            }
            else if (x >= size)
            {
                x = size - x;
            }

            if (y < 0)
            {
                y = size - 2;
            }
            else if (y >= size)
            {
                y = size - y;
            }

            return data[y * size + x];
        }

        private float GetNewValue(float v1, float v2, float v3, float v4, Random state, int iteration)
        {
            return ((v1 + v2 + v3 + v4) / 4f) + GetRandom(state, iteration);
        }

        public struct Result
        {
            public bool Staggered => (Iteration & 1) == 1;

            public int Seed { get; init; }
            public int Size { get; init; }
            public int Iteration { get; init; }
            public float[] Content { get; init; }

            public bool Validate()
            {
                bool flag = true;

                flag &= (Size * Size) == Content.Length;

                var calculatedSize = 2;

                var expansion = Iteration / 2;

                for (int i=0; i<Iteration; i++)
                {
                    calculatedSize *= 2;
                    calculatedSize -= 1;
                }

                flag &= Size == calculatedSize;

                return flag;
            }
        }

        public enum EdgeValueSource 
        { 
            Random = 0,
            Constant = 1,
            Loop = 2
        }
    }
}
