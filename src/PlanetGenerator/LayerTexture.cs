using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public class LayerTexture
    {
        public LayerTexture(string name, ReadOnlyMemory<float> textureData, int size)
        {
            Name = name;
            TextureData = textureData;
            Size = size;
        }

        public string Name { get; }

        public ReadOnlyMemory<float> TextureData { get; }

        public int Size { get; }
    }
}
