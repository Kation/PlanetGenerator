using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public interface INoise
    {
        INoiseSeed Seed { get; }

        float Get(float x, int cellOffsetX = 0);

        float Get(float x, float y, int cellOffsetX = 0, int cellOffsetY = 0);

        float Get(float x, float y, float z, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0);

        float Get(float x, float y, float z, float w, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0);

        void GetRange(Memory<float> x, Memory<float> values, int cellOffsetX = 0, bool aligned = false);

        void GetRange(Memory<float> x, Memory<float> y, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, bool aligned = false);

        void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, bool aligned = false);

        void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> w, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0, bool aligned = false);
    }
}
