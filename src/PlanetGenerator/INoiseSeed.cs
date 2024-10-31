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
        int Hash(int value);

        Vector<int> Hash(Vector<int> value);

        float GetGrad(int hash, float offsetX);

        float GetGrad(int hash, float offsetX, float offsetY);

        float GetGrad(int hash, float offsetX, float offsetY, float offsetZ);

        float GetGrad(int hash, float offsetX, float offsetY, float offsetZ, float offsetW);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW);


        //float GetHashGrad(int x, float offsetX);

        float GetHashGrad(int x, int y, float offsetX, float offsetY);

        //float GetHashGrad(int x, int y, int w, float offsetX, float offsetY, float offsetW);

        Vector<float> GetHashGrad(Vector<int> x, Vector<int> y, Vector<float> offsetX, Vector<float> offsetY);
    }
}
