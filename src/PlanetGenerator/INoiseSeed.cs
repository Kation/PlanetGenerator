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

        float GetGrad(int hash, float offsetX);
        
        float GetGrad(int hash, float offsetX, float offsetY);

        float GetGrad(int hash, float offsetX, float offsetY, float offsetZ);

        float GetGrad(int hash, float offsetX, float offsetY, float offsetZ, float offsetW);

        Vector<int> Hash(Vector<int> value);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ);

        Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW);
    }
}
