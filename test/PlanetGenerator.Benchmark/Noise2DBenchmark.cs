using BenchmarkDotNet.Attributes;
using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Benchmark
{
    //[SimpleJob]
    public class Noise2DBenchmark
    {
        private const int _Width = 8192, _Height = 8192;

        [Benchmark(Baseline = true)]
        public void SimplexParallel()
        {
            var noise = new SimplexNoise(1);
            float[,] data = new float[_Width, _Height];
            Parallel.For(0, _Width, x =>
            {
                Parallel.For(0, _Height, y =>
                {
                    var x0 = (x / 40f);
                    var y0 = (y / 40f);
                    var value = noise.Get(x0, y0);
                    data[x, y] = value;
                });
            });
        }

        private float[] _px, _py;
        [GlobalSetup]
        public void GlobalSetup()
        {
            _px = new float[_Width * _Height];
            _py = new float[_px.Length];
            Parallel.For(0, _Width, x =>
            {
                Parallel.For(0, _Height, y =>
                {
                    var i = x + y * _Width;
                    _px[i] = (x / 40f);
                    _py[i] = (y / 40f);
                });
            });
        }

        [Benchmark]
        public void SimplexSIMD()
        {
            var noise = new SimplexNoise(1);
            var values = noise.GetRange(_px, _py);
        }

        //[Benchmark]
        //public void SimplexOpenCL()
        //{
        //    using var context = Context.Create(builder => builder.OpenCL());
        //    var noise = new GPUSimplexNoise(1, context, context.GetCLDevice(0));
        //    var values = noise.GetRange(_px, _py);
        //}

        //[Benchmark]
        //public void SimplexCUDA()
        //{
        //    using var context = Context.Create(builder => builder.Cuda());
        //    var noise = new GPUSimplexNoise(1, context, context.GetCudaDevice(0));
        //    var values = noise.GetRange(_px, _py);
        //    noise.Dispose();
        //}
    }
}
