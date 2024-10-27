using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class Tile
    {
        private const float _TileAngleX = 1f / 10f;
        private const float _TileAngleY = 2f / 3f;

        public Tile(int index, int zoomLevel, ReadOnlyMemory<float> terrain, PlanetSettings settings, List<LayerTexture> textures)
        {
            Index = index;
            ZoomLevel = zoomLevel;
            Terrain = terrain;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Textures = textures;
        }

        public int Index { get; }

        public int ZoomLevel { get; }

        public ReadOnlyMemory<float> Terrain { get; }

        public PlanetSettings Settings { get; }

        public List<LayerTexture> Textures { get; }
    }
}
