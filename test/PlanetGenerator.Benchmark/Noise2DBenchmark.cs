using BenchmarkDotNet.Attributes;
using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Benchmark
{
    //[SimpleJob]
    public unsafe class Noise2DBenchmark
    {
        private const int _Width = 8192, _Height = 8192;

        [Benchmark(Baseline = true)]
        public void SimplexParallel()
        {
            float[,] data = new float[_Width, _Height];
            Parallel.For(0, _Width, x =>
            {
                Parallel.For(0, _Height, y =>
                {
                    var x0 = (x / 40f);
                    var y0 = (y / 40f);
                    var value = _noise.Get(x0, y0);
                    data[x, y] = value;
                });
            });
        }

        private void* _px, _py, _pvalues;
        private SimplexNoise _noise;
        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            _px = NativeMemory.AlignedAlloc(_Width * _Height * sizeof(float), (nuint)sizeof(Vector<float>));
            _py = NativeMemory.AlignedAlloc(_Width * _Height * sizeof(float), (nuint)sizeof(Vector<float>));
            _pvalues = NativeMemory.AlignedAlloc(_Width * _Height * sizeof(float), (nuint)sizeof(Vector<float>));
            var px = new Span<float>(_px, _Width * _Height);//new float[_Width * _Height];
            var py = new Span<float>(_py, _Width * _Height);//new float[_px.Length];
            for (int x = 0; x < _Width; x++)
            {
                for (int y = 0; y < _Height; y++)
                {
                    var i = x + y * _Width;
                    px[i] = (x / 40f);
                    py[i] = (y / 40f);
                }
            }
            _noise = new SimplexNoise(1);
        }

        [Benchmark]
        public void SimplexSIMD()
        {
            _noise.GetRange(new IntPtr(_px), new IntPtr(_py), new IntPtr(_pvalues), _Width * _Height);
        }

        //[Benchmark]
        //public void SimplexOpenCL()
        //{
        //    using var context = Context.Create(builder => builder.OpenCL());
        //    var noise = new GPUSimplexNoise(1, context, context.GetCLDevice(0));
        //    var values = noise.GetRange(_px, _py, _Width * _Height);
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
