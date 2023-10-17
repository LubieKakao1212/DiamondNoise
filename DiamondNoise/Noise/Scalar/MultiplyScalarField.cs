using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondNoise.Noise.Scalar
{
    public class MultiplyScalarField : IScalarField
    {
        private IScalarField[] fields;

        public MultiplyScalarField(params IScalarField[] fields)
        {
            this.fields = fields;
        }

        public float GetValue(Vector2 pos, int iteration)
        {
            float val = 1f;
            foreach (var field in fields)
            {
                val *= field.GetValue(pos, iteration);
            }
            return val;
        }

        public IScalarField NewState()
        {
            IScalarField[] newFields = new IScalarField[fields.Length];
            for (int i = 0; i < fields.Length; i++) 
            {
                newFields[i] = fields[i].NewState();
            }
            return new MultiplyScalarField(newFields);
        }
    }
}
