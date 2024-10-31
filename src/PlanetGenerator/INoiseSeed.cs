using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public interface INoiseSeed
    {
        float GetGrad(int x, float offsetX);

        float GetGrad(int x, int y, float offsetX, float offsetY);

        float GetGrad(int x, int y, int z, float offsetX, float offsetY, float offsetZ);

        float GetGrad(int x, int y, int z, int w, float offsetX, float offsetY, float offsetZ, float offsetW);

        Vector<float> GetGrad(Vector<int> x, Vector<float> offsetX);

        Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<float> offsetX, Vector<float> offsetY);

        Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<int> z, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ);

        Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<int> z, Vector<int> w, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW);

        float GetHashValue(int x);

        Vector2 GetHashValue(int x, int y);

        Vector3 GetHashValue(int x, int y, int z);

        Vector4 GetHashValue(int x, int y, int z, int w);
    }
}
