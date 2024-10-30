using PlanetGenerator.Layers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetBuilder
    {
        private List<IPlanetLayer> _layers;
        private IReadOnlyList<IPlanetLayer> _readOnlylayers;

        public PlanetBuilder(PlanetSettings planetSettings, INoise noise)
        {
            Settings = planetSettings ?? throw new ArgumentNullException(nameof(planetSettings));
            Noise = noise ?? throw new ArgumentNullException(nameof(noise));
            _layers = [new BaseLayer()];
            _readOnlylayers = _layers.AsReadOnly();
        }

        public PlanetSettings Settings { get; }

        public INoise Noise { get; }

        public IList<IPlanetLayer> Layers => _layers;

        public void GenerateBase()
        {
            var context = new PlanetLayerContext(Noise, Settings, _readOnlylayers);
            foreach (var layer in Layers)
                layer.HandleBase(context);
        }

        public Tile GenerateTile(int index, int zoomLevel)
        {
            PlanetHelper.GetLocationAndPositions(index, zoomLevel, Settings.TileResolution, Settings.PlanetRadius, out var positionX, out var positionY, out var positionZ, out var longitudes, out var latitudes);
            var context = new PlanetLayerTileContext(positionX, positionY, positionZ, longitudes, latitudes, Noise, Settings, _readOnlylayers);
            foreach (var layer in Layers)
                layer.HandleTile(context, index, zoomLevel);
            var terrain = context.BuildTerrain();
            return new Tile(index, zoomLevel, terrain, Settings, context.Textures);
        }
    }
}
