using DiamondNoise.Noise.Scalar;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Diamond
{
    public static class DiamondNoise1D
    {
        public static Checkpoint Create(IScalarField offsetProvider, EdgeGenerationType vertivalEdgeGeneration = EdgeGenerationType.Loop, float verticalEdgeConstant = 0f, EdgeGenerationType horizontalEdgeGeneration = EdgeGenerationType.Loop, float horizontalEdgeConstant = 0f)
        {
            var checkpoint = new Checkpoint()
            {
                HorizontalEdgeGeneration = horizontalEdgeGeneration,
                VerticalEdgeGeneration = vertivalEdgeGeneration,
                HorizontalEdgeConstant = horizontalEdgeConstant,
                VerticalEdgeConstant = verticalEdgeConstant,
                Content = new float[] 
                {
                    offsetProvider.GetValue(new Vector2(0f, 0f), 0),
                    offsetProvider.GetValue(new Vector2(1f, 0f), 0),
                    offsetProvider.GetValue(new Vector2(0f, 1f), 0),
                    offsetProvider.GetValue(new Vector2(1f, 1f), 0),
                },
                HorizontalIteration = 0,
                VerticalIteration = 0,
                Width = 2,
                Height = 2,
                OffsetProvider = offsetProvider
            };

            return checkpoint;
        }

        public static Checkpoint Generate(this Checkpoint checkpoint, int iterations, GenerationAxis axis)
        {
            if (axis == GenerationAxis.Horizontal)
            {
                for (int i = 0; i < iterations; i++) 
                {
                    checkpoint = GenerateHorizontal(checkpoint);
                }
                return checkpoint;
            }
            else
            {
                for (int i = 0; i < iterations; i++)
                {
                    checkpoint = GenerateVertical(checkpoint);
                }
                return checkpoint;
            }
        }

        public static Checkpoint GenerateHorizontal(this in Checkpoint checkpoint)
        {
            var checkpointOut = checkpoint.NewState(GenerationAxis.Horizontal);

            var newWidth = checkpointOut.Width;
            var oldWidth = checkpoint.Width;

            for (int y = 0; y < checkpoint.Height; y++)
                for (int newX = 0; newX < checkpointOut.Width; newX++)
                {
                    var oldX = newX / 2;
                    if ((newX & 1) == 0)
                    {
                        checkpointOut.Content[GetIdx(newX, y, newWidth)] = checkpoint.Content[GetIdx(oldX, y, oldWidth)];
                    }
                    else
                    {
                        checkpointOut.Content[GetIdx(newX, y, newWidth)] =
                            NewValue(
                                checkpoint.Content[GetIdx(oldX, y, oldWidth)],
                                GetValue(checkpoint, oldX + 1, y,
                                checkpoint.HorizontalEdgeGeneration,
                                checkpoint.HorizontalEdgeConstant,
                                GenerationAxis.Horizontal),
                                FieldPos(newX, y, 
                                    checkpointOut.Width, 
                                    checkpointOut.Height),
                                checkpointOut.OffsetProvider,
                                checkpointOut.HorizontalIteration);
                    }
                }

            return checkpointOut;
        }

        public static Checkpoint GenerateVertical(this in Checkpoint checkpoint)
        {
            var checkpointOut = checkpoint.NewState(GenerationAxis.Vertical);

            var newWidth = checkpointOut.Width;
            var oldWidth = checkpoint.Width;

            for (int newY = 0; newY < checkpointOut.Height; newY++)
                for (int x = 0; x < checkpoint.Width; x++)
                {
                    var oldY = newY / 2;
                    if ((newY & 1) == 0)
                    {
                        checkpointOut.Content[GetIdx(x, newY, newWidth)] = checkpoint.Content[GetIdx(x, oldY, oldWidth)];
                    }
                    else
                    {
                        checkpointOut.Content[GetIdx(x, newY, newWidth)] =
                            NewValue(
                                checkpoint.Content[GetIdx(x, oldY, oldWidth)],
                                GetValue(checkpoint, x, oldY + 1,
                                checkpoint.VerticalEdgeGeneration,
                                checkpoint.VerticalEdgeConstant,
                                GenerationAxis.Vertical),
                                FieldPos(x, newY,
                                    checkpointOut.Width,
                                    checkpointOut.Height),
                                checkpointOut.OffsetProvider,
                                checkpointOut.VerticalIteration);
                    }
                }

            return checkpointOut;
        }

        public static int CalculateSizeForIteration(int iteration, EdgeGenerationType edgeGenerationType)
        {
            var calculatedSize = 2;
            if (edgeGenerationType != EdgeGenerationType.NoEdge)
            {
                for (int i = 0; i < iteration; i++)
                {
                    calculatedSize *= 2;
                }
            }
            else
            {
                for (int i = 0; i < iteration; i++)
                {
                    calculatedSize *= 2;
                    calculatedSize -= 1;
                }
            }

            return calculatedSize;
        }

        /// <summary>
        /// Does not check for underflow and overflow greater than 1
        /// </summary>
        /// <param name="data"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="edgeGenerationType"></param>
        /// <param name="edgeConstant"></param>
        /// <param name="field"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static float GetValue(Checkpoint source, int x, int y, EdgeGenerationType edgeGenerationType, float edgeConstant, GenerationAxis axis)
        {
            var data = source.Content;
            var width = source.Width;
            var height = source.Height;
            var field = source.OffsetProvider;
            if (x == width)
            {
                switch (edgeGenerationType) 
                {
                    case EdgeGenerationType.Constant:
                        return edgeConstant;
                    case EdgeGenerationType.Loop:
                        return data[GetIdx(0, y, width)];
                    case EdgeGenerationType.Field:
                        return field.GetValue(FieldPos(x, y, width, height));
                    default:
                        throw new Exception("invalid State");
                }
            }
            else if(y == height)
            {
                switch (edgeGenerationType)
                {
                    case EdgeGenerationType.Constant:
                        return edgeConstant;
                    case EdgeGenerationType.Loop:
                        return data[GetIdx(x, 0, width)];
                    case EdgeGenerationType.Field:
                        return field.GetValue(FieldPos(x, y, width, height));
                    default:
                        throw new Exception("invalid State");
                }
            }

            return data[GetIdx(x, y, width)];
        }
        
        private static float NewValue(float v1, float v2, Vector2 pos, IScalarField field, int iteration)
        {
            return (v1 + v2) / 2f + field.GetValue(pos, iteration);
        }

        private static Vector2 FieldPos(int x, int y, int width, int height)
        {
            return new Vector2((float) x / (width - 1), (float)y / (height - 1));
        }

        public static int GetIdx(int x, int y, int width)
        {
            return y * width + x;
        }

        public struct Checkpoint
        {
            public int Width { get; init; }

            public int Height { get; init; }

            public EdgeGenerationType VerticalEdgeGeneration { get; init; }

            public EdgeGenerationType HorizontalEdgeGeneration { get; init; }

            public float VerticalEdgeConstant { get; init; }
            public float HorizontalEdgeConstant { get; init; }

            public float[] Content { get; init; }

            public IScalarField OffsetProvider { get; init; }

            public int HorizontalIteration { get; init; }

            public int VerticalIteration { get; init; }

            public bool Validate()
            {
                bool flag = true;

                flag &= ValidateSize(VerticalEdgeGeneration, VerticalIteration, Height);

                flag &= ValidateSize(HorizontalEdgeGeneration, HorizontalIteration, Width);

                flag &= (Width * Height) == Content.Length;

                return flag;
            }

            internal Checkpoint NewState(GenerationAxis axis)
            {
                if(!Validate()) {
                    throw new Exception("Invalid checkpoint");
                }

                var hIt = HorizontalIteration + (axis == GenerationAxis.Horizontal ? 1 : 0);
                var vIt = VerticalIteration + (axis == GenerationAxis.Vertical ? 1 : 0);

                return new Checkpoint()
                {
                    HorizontalEdgeGeneration = HorizontalEdgeGeneration,
                    VerticalEdgeGeneration = VerticalEdgeGeneration,
                    HorizontalIteration = hIt,
                    VerticalIteration = vIt,
                    HorizontalEdgeConstant = HorizontalEdgeConstant,
                    VerticalEdgeConstant = VerticalEdgeConstant,
                    Width = CalculateSizeForIteration(hIt, HorizontalEdgeGeneration),
                    Height = CalculateSizeForIteration(vIt, VerticalEdgeGeneration),
                    Content = new float[GetNextSize(axis)],
                    OffsetProvider = OffsetProvider.NewState()
                };
            }

            private bool ValidateSize(EdgeGenerationType edgeGeneration, int iteration, int size)
            {
                return CalculateSizeForIteration(iteration, edgeGeneration) == size;
            }

            private int GetNextSize(GenerationAxis axis)
            {
                var w = HorizontalIteration;
                var h = VerticalIteration;

                if (axis == GenerationAxis.Horizontal)
                {
                    w++;
                }
                else
                {
                    h++;
                }

                return 
                    CalculateSizeForIteration(w, HorizontalEdgeGeneration) *
                    CalculateSizeForIteration(h, VerticalEdgeGeneration);
            }
        }

        public enum GenerationAxis 
        { 
            Horizontal,
            Vertical
        }

        public enum EdgeGenerationType
        {
            NoEdge = 0,
            Constant = 1,
            Loop = 2,
            Field = 3
        }
    }
}
