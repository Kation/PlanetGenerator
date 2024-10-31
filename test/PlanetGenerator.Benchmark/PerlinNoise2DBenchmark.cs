using BenchmarkDotNet.Attributes;
using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Benchmark
{
    //[SimpleJob]
    public unsafe class PerlinNoise2DBenchmark
    {
        private const int _Width = 8192, _Height = 8192;
        private const int _Length = _Width * _Height;

        [Benchmark(Baseline = true)]
        public void PerlinParallel()
        {
            float[,] data = new float[_Width, _Height];
            Parallel.For(0, _Width, x =>
            {
                Parallel.For(0, _Height, y =>
                {
                    var x0 = (x / 40f);
                    var y0 = (y / 40f);
                    _noise.Get(x0, y0);
                    data[x, y] = _noise.Get(x0, y0);
                });
            });
        }

        private float* _px, _py, _pvalues;
        private PerlinNoise _noise;
        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            _px = (float*)NativeMemory.AlignedAlloc(_Length * sizeof(float), (nuint)sizeof(Vector<float>));
            _py = (float*)NativeMemory.AlignedAlloc(_Length * sizeof(float), (nuint)sizeof(Vector<float>));
            _pvalues = (float*)NativeMemory.AlignedAlloc(_Length * sizeof(float), (nuint)sizeof(Vector<float>));
            var px = new Span<float>(_px, _Length);//new float[_Width * _Height];
            var py = new Span<float>(_py, _Length);//new float[_px.Length];
            for (int x = 0; x < _Width; x++)
            {
                for (int y = 0; y < _Height; y++)
                {
                    var i = x + y * _Width;
                    px[i] = (x / 40f);
                    py[i] = (y / 40f);
                }
            }
            _noise = new PerlinNoise(new HashSeed());
        }

        [Benchmark]
        public void PerlinSIMD()
        {
            var xm = new NativeMemoryManager(_px, _Length);
            var ym = new NativeMemoryManager(_py, _Length);
            var vm = new NativeMemoryManager(_pvalues, _Length);
            _noise.GetRange(xm.Memory, ym.Memory, vm.Memory, aligned: true);
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

        private class NativeMemoryManager : MemoryManager<float>
        {
            private readonly float* _p;
            private readonly int _length;

            public NativeMemoryManager(float* p, int length)
            {
                _p = p;
                _length = length;
            }

            public override Span<float> GetSpan()
            {
                return new Span<float>(_p, _length);
            }

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                return new MemoryHandle(_p + elementIndex);
            }

            public override void Unpin()
            {

            }

            protected override void Dispose(bool disposing)
            {

            }
        }
    }
}
