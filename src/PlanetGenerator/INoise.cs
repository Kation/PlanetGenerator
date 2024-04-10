using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public interface INoise
    {
        float Get(params float[] position);

        float Get(float x);

        float Get(float x, float y);

        float Get(float x, float y, float z);

        float Get(float x, float y, float z, float w);

        float[] GetRange(params float[][] positions);

        float[] GetRange(float[] x);

        void GetRange(IntPtr x, IntPtr y, IntPtr values, int length);

        float[] GetRange(float[] x, float[] y, float[] z);

        float[] GetRange(float[] x, float[] y, float[] z, float[] w);

        float GetHashFloat(int x);
    }
}
