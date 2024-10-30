using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Benchmark
{
    public class Noise3DBenchmark
    {
        //[Benchmark(Baseline = true)]
        //public void DefaultPerlin()
        //{
        //    var noise = new DefaultPerlinNoise(1);
        //    var width = 80;
        //    var height = 80;
        //    var length = 80;
        //    float[,,] data = new float[width, height, length];
        //    for (int x = 0; x < width; x++)
        //    {
        //        for (int y = 0; y < height; y++)
        //        {
        //            for (int z = 0; z < length; z++)
        //            {
        //                var x0 = (x / 40f);
        //                var y0 = (y / 40f);
        //                var z0 = (z / 40f);
        //                var value = noise.Get(x0, y0, z0);
        //                data[x, y, z] = value;
        //            }
        //        }
        //    }
        //}

        //[Benchmark]
        //public void DefaultPerlinParallel()
        //{
        //    var noise = new DefaultPerlinNoise(1);
        //    var width = 80;
        //    var height = 80;
        //    var length = 80;
        //    float[,,] data = new float[width, height, length];
        //    Parallel.For(0, width, x =>
        //    {
        //        Parallel.For(0, height, y =>
        //        {
        //            Parallel.For(0, length, z =>
        //            {
        //                var x0 = (x / 40f);
        //                var y0 = (y / 40f);
        //                var z0 = (z / 40f);
        //                var value = noise.Get(x0, y0, z0);
        //                data[x, y, z] = value;

        //            });
        //        });
        //    });
        //}

        private const int _Width = 120, _Height = 120, _Length = 120;

        [Benchmark(Baseline = true)]
        public void SimplexParallel()
        {
            var noise = new SimplexNoise(1);
            float[,,] data = new float[_Width, _Height, _Length];
            Parallel.For(0, _Width, x =>
            {
                var x0 = (x / 40f);
                Parallel.For(0, _Height, y =>
                {
                    var y0 = (y / 40f);
                    Parallel.For(0, _Length, z =>
                    {
                        var z0 = (z / 40f);
                        var value = noise.Get(x0, y0, z0);
                        _values[x + y * _Width + z * _Width * _Height] = value;
                    });
                });
            });
        }

        private float[] _px, _py, _pz, _values;
        [GlobalSetup]
        public void GlobalSetup()
        {
            _px = new float[_Width * _Height * _Length];
            _py = new float[_px.Length];
            _pz = new float[_px.Length];
            _values = new float[_px.Length];
            Parallel.For(0, _Width, x =>
            {
                Parallel.For(0, _Height, y =>
                {
                    Parallel.For(0, _Length, z =>
                    {
                        var i = x + y * _Width + z * _Height;
                        _px[i] = (x / 40f);
                        _py[i] = (y / 40f);
                        _pz[i] = (z / 40f);
                    });
                });
            });
        }

        [Benchmark]
        public void SimplexSIMD()
        {
            var noise = new SimplexNoise(1);
            noise.GetRange(_px, _py, _pz, _values);
        }
    }
}
