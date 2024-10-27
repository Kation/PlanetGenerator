using PlanetGenerator.Layers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetBuilder
    {
        private const float _TileAngleX = 1f / 10f;
        private const float _TileAngleX2 = _TileAngleX * 2;
        private const float _TileAngleY = 2f / 3f;

        public PlanetBuilder(PlanetSettings planetSettings, INoise noise)
        {
            Settings = planetSettings ?? throw new ArgumentNullException(nameof(planetSettings));
            Noise = noise ?? throw new ArgumentNullException(nameof(noise));
            Layers = new List<IPlanetLayer>();
            Layers.Add(new BaseLayer());
        }

        public PlanetSettings Settings { get; }

        public INoise Noise { get; }

        public IList<IPlanetLayer> Layers { get; }

        public Tile GenerateTile(int index, int zoomLevel)
        {
            PlanetHelper.GetLocationAndPositions(index, zoomLevel, Settings.TileResolution, Settings.PlanetRadius, out var positionX, out var positionY, out var positionZ, out var longitudes, out var latitudes);
            var context = new PlanetLayerContext(positionX, positionY, positionZ, longitudes, latitudes, Noise, Settings);
            foreach (var layer in Layers)
                layer.Handle(context, index, zoomLevel);
            var terrain = context.BuildTerrain();
            return new Tile(index, zoomLevel, terrain, Settings, context.Textures);
        }
    }
}
