using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PlanetGenerator
{
    public class Planet
    {
        private PlanetSettings _settings;
        private PlanetBuilder _generator;
        private string _savePath;
        private int _tileFileLength;

        public Planet(PlanetOptions options)
        {
            _settings = options.Settings;
            _generator = new PlanetBuilder(_settings, options.Noise);
            _savePath = options.SavePath;
            _tileFileLength = _settings.TileResolution * _settings.TileResolution * sizeof(float);
        }

        public Tile GetTile(int index, int zoomLevel)
        {
            var path = Path.Combine(_savePath, $"{zoomLevel}_{index}.data");
            if (File.Exists(path))
            {
                var stream = File.OpenRead(path);
                if (stream.Length != _tileFileLength)
                {
                    stream.Dispose();
                    File.Delete(path);
                }
                else
                {
                    var data = new byte[_tileFileLength];
                    stream.Read(data);
                    var dataMemory = data.AsMemory();
                    ref var terrain = ref Unsafe.As<Memory<byte>, Memory<float>>(ref dataMemory);
                    return new Tile(index, zoomLevel, terrain, _settings);
                }
            }
            {
                var tile = _generator.GenerateTile(index, zoomLevel);
                var stream = File.OpenWrite(path);
                stream.Write(MemoryMarshal.Cast<float, byte>(tile.Terrain.Span));
                stream.Flush();
                stream.Dispose();
                return tile;
            }
        }
    }
}
