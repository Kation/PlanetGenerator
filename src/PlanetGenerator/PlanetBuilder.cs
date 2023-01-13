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
            var length = Settings.TileResolution;
            //根块
            int rootTile;
            int baseX = 0, baseY = 0;
            float rootTileX, rootTileY;
            float rootLength;
            if (zoomLevel == 0)
            {
                rootTile = index;
                rootLength = length - 1;
            }
            else
            {
                int n = 9, sn = 3;
                for (int ii = 1; ii < zoomLevel; ii++)
                {
                    n *= 9;
                    sn *= 3;
                }
                rootTile = index / n;
                var subTitle = index % n;
                baseX = length * (subTitle % sn);
                baseY = length * (subTitle / sn);
                rootLength = sn * length - 1;
            }
            bool up = rootTile % 2 == 0;
            rootTileX = rootTile * _TileAngleX;
            rootTileY = up ? _TileAngleY / 4 : -_TileAngleY / 4;
            var rootTileXAngle = rootTileX * MathF.PI * 2;
            var rootTileYAngle = rootTileY * MathF.PI;

            float[] positionX = new float[length * length];
            float[] positionY = new float[length * length];
            float[] positionZ = new float[length * length];
            float[] longitudes = new float[length * length];
            float[] latitudes = new float[length * length];

            int i = 0;
            for (int b = 0; b < length; b++)
            {
                for (int a = 0; a < length; a++)
                {
                    var x = (baseX + a) / rootLength;
                    var y = (baseY + b) / rootLength;
                    var p = new Vector2(x, x) + new Vector2(y, -y);
                    float lng;
                    if ((x == 1 && y == 0) || (x == 0 && y == 1))
                        lng = rootTileXAngle + _TileAngleX2 * MathF.PI;
                    else
                    {
                        if (up)
                        {
                            if (p.Y >= 0)
                                lng = rootTileXAngle + (p.X - p.Y) / (2 - p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }
                        else
                        {
                            if (p.Y <= 0)
                                lng = rootTileXAngle + (p.X + p.Y) / (2 + p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }
                    }
                    var lat = rootTileYAngle + p.Y * _TileAngleY * MathF.PI / 2;

                    var px = Settings.PlanetRadius * MathF.Cos(lat) * MathF.Cos(lng);
                    var pz = Settings.PlanetRadius * MathF.Cos(lat) * MathF.Sin(lng);
                    var py = Settings.PlanetRadius * MathF.Sin(lat);
                    longitudes[i] = lng;
                    latitudes[i] = lat;
                    positionX[i] = px;
                    positionY[i] = py;
                    positionZ[i] = -pz;
                    i++;
                }
            }
            var context = new PlanetLayerContext(positionX, positionY, positionZ, longitudes, latitudes, Noise, Settings);
            foreach (var layer in Layers)
                layer.Handle(context, index, zoomLevel);
            var terrain = context.BuildTerrain();
            //var terrain = Noise.GetRange(positionX, positionY, positionZ);
            return new Tile(index, zoomLevel, terrain, Settings);
        }
    }
}
