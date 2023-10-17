using DiamondNoise.Noise.Scalar;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;

namespace DiamondNoise.Noise.Diamond
{
    public class DiamondNoiseGenerator2D
    {
        private EdgeValueSource edgeValueSource;
        private float edgeConstant;

        public DiamondNoiseGenerator2D(EdgeValueSource edgeValueSource = EdgeValueSource.Loop, float edgeConstant = 0f)
        {
            this.edgeValueSource = edgeValueSource;
            this.edgeConstant = edgeConstant;
        }

        public Checkpoint Generate(IScalarField offsetProvider, int iterations = 0)
        {
            var iteration0 = new Checkpoint()
            {
                Content = new float[4]
                {
                    offsetProvider.GetValue(new Vector2(0f, 0f), 0),
                    offsetProvider.GetValue(new Vector2(1f, 0f), 0),
                    offsetProvider.GetValue(new Vector2(0f, 1f), 0),
                    offsetProvider.GetValue(new Vector2(1f, 1f), 0),
                },
                Iteration = 0,
                OffsetProvider = offsetProvider,
                Size = 2
            };

            return ContinueGeneration(iteration0, iterations);
        }

        public Checkpoint ContinueGeneration(Checkpoint checkpoint, int iterations)
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

        public Checkpoint Iteration(in Checkpoint lastIteration)
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
        public Checkpoint IterationEven(in Checkpoint lastIteration)
        {
            var size = lastIteration.Size;
            var dataSize = size * size;
            var newData = new float[dataSize];
            var oldData = lastIteration.Content;

            var offsetProvider = lastIteration.OffsetProvider.NewState();
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
                var pos = new Vector2(
                    (float)x / (size - 1),
                    (float)y / (size - 1));
                float v1 = GetValue(oldData, size, x - 1, y);
                float v2 = GetValue(oldData, size, x + 1, y);
                float v3 = GetValue(oldData, size, x, y - 1);
                float v4 = GetValue(oldData, size, x, y + 1);
                newData[i] = GetNewValue(v1, v2, v3, v4, pos, offsetProvider, iteration);
            }
            return new Checkpoint()
            {
                Content = newData,
                Iteration = iteration,
                OffsetProvider = offsetProvider,
                Size = size
            };

        }

        //NotStaggered -> Staggered
        public Checkpoint IterationOdd(Checkpoint lastIteration)
        {
            var oldSize = lastIteration.Size;
            var size = oldSize * 2 - 1;
            var dataSize = size * size;
            var newData = new float[dataSize];
            var oldData = lastIteration.Content;
            
            var offsetProvider = lastIteration.OffsetProvider.NewState();
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
                        var pos = new Vector2(
                            (float)x / (size - 1),
                            (float)y / (size - 1));
                        var v1 = oldData[(oldY    ) * oldSize + oldX    ];
                        var v2 = oldData[(oldY    ) * oldSize + oldX + 1];
                        var v3 = oldData[(oldY + 1) * oldSize + oldX + 1];
                        var v4 = oldData[(oldY + 1) * oldSize + oldX    ];
                        newData[i] = GetNewValue(v1, v2, v3, v4, pos, offsetProvider, iteration);
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

            return new Checkpoint()
            {
                Content = newData,
                Iteration = iteration,
                OffsetProvider = offsetProvider,
                Size = size
            };
        }

        //00X, 10 , 20X
        //01 , 11X, 21
        //02X, 12 , 22X

        private float GetValue(float[] data, int size, int x, int y)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                return data[y * size + x];
            }
            return GetOBBValue(data, size, x, y);
        }

        private float GetOBBValue(float[] data, int size, int x, int y)
        {
            switch (edgeValueSource) 
            {
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

        private float GetNewValue(float v1, float v2, float v3, float v4, Vector2 pos, IScalarField offsetProvider, int iteration)
        {
            return ((v1 + v2 + v3 + v4) / 4f) + offsetProvider.GetValue(pos, iteration);
        }

        public struct Checkpoint
        {
            public bool Staggered => (Iteration & 1) == 1;
            
            public IScalarField OffsetProvider { get; init; }

            public int Size { get; init; }
            public int Iteration { get; init; }
            public float[] Content { get; init; }

            public bool Validate()
            {
                bool flag = true;

                flag &= (Size * Size) == Content.Length;

                var calculatedSize = 2;

                var expansion = (Iteration + 1) / 2;

                for (int i = 0; i < expansion; i++)
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
            Constant = 0,
            Loop = 1,
            //Random = 2
        }
    }
}
