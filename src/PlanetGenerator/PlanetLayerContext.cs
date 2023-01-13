using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public class PlanetLayerContext
    {
        private Dictionary<string, float[]> _layers;
        private int _length;

        public PlanetLayerContext(float[] positionX, float[] positionY, float[] positionZ, float[] longitudes, float[] latitudes, INoise noise, PlanetSettings settings)
        {
            PositionX = positionX ?? throw new ArgumentNullException(nameof(positionX));
            PositionY = positionY ?? throw new ArgumentNullException(nameof(positionY));
            PositionZ = positionZ ?? throw new ArgumentNullException(nameof(positionZ));
            Longitudes = longitudes ?? throw new ArgumentNullException(nameof(longitudes));
            Latitudes = latitudes ?? throw new ArgumentNullException(nameof(latitudes));
            Noise = noise;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _layers = new Dictionary<string, float[]>();
            _length = settings.TileResolution * settings.TileResolution;
        }

        public float[] PositionX { get; }

        public float[] PositionY { get; }

        public float[] PositionZ { get; }

        public float[] Longitudes { get; }

        public float[] Latitudes { get; }

        public INoise Noise { get; }

        public PlanetSettings Settings { get; }

        public Memory<float> GetLayer(string name)
        {
            if (_layers.TryGetValue(name, out var data))
                return data;
            throw new InvalidOperationException($"不存在\"{name}\"层。");
        }

        public Memory<float> CreateLayer(string name)
        {
            if (_layers.ContainsKey(name))
                throw new InvalidOperationException($"已存在\"{name}\"层。");
            var data = new float[_length];
            _layers[name] = data;
            return data;
        }
        public void CreateLayer(string name, float[] data)
        {
            if (_layers.ContainsKey(name))
                throw new InvalidOperationException($"已存在\"{name}\"层。");
            _layers[name] = data;
        }

        public ReadOnlyMemory<float> BuildTerrain()
        {
            if (_layers.Count == 0)
                throw new InvalidOperationException();
            if (_layers.Count == 1)
                return new ReadOnlyMemory<float>(_layers.First().Value);
            var terrain = new float[_length];
            if (Vector.IsHardwareAccelerated)
            {
                var count = Vector<float>.Count;
                var p = _length / count;
                Parallel.For(0, p, i =>
                {
                    var terrainSpan = MemoryMarshal.Cast<float, Vector<float>>(terrain);
                    bool first = true;
                    foreach (var item in _layers)
                    {
                        var span = MemoryMarshal.Cast<float, Vector<float>>(item.Value);
                        if (first)
                        {
                            terrain[i] = item.Value[i];
                            first = false;
                        }
                        else
                            terrainSpan[i] += span[i];
                    }
                });
                var m = _length % count;
                if (m != 0)
                {
                    for (int i = _length - m; i < _length; i++)
                    {
                        bool first = true;
                        foreach (var item in _layers)
                        {
                            if (first)
                            {
                                terrain[i] = item.Value[i];
                                first = false;
                            }
                            else
                                terrain[i] += item.Value[i];
                        }
                    }
                }
            }
            else
            {
                Parallel.For(0, _length, i =>
                {
                    bool first = true;
                    foreach (var item in _layers)
                    {
                        if (first)
                            terrain[i] = item.Value[i];
                        else
                            terrain[i] += item.Value[i];
                    }
                });
            }
            return terrain;
        }
    }
}
