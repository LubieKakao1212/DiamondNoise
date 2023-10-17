using Microsoft.Xna.Framework;

namespace DiamondNoise.Noise.Scalar
{
    /// <summary>
    /// Represents a semi-deterministic 2d scalar field
    /// </summary>
    //TODO remap 0 <> 1 to -1 <> 1
    public interface IScalarField
    {
        float GetValue(Vector2 pos)
        {
            return GetValue(pos, 0);
        }

        float GetValue(Vector2 pos, int iteration);
        /// <summary>
        /// Returns a Scalar Provider of the same type, if called twice on same objects with state results will match
        /// </summary>
        /// <returns></returns>
        IScalarField NewState();
    }
}
