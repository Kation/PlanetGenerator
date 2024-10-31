using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Benchmark
{
    public unsafe class SeedBenchmark
    {
        private INoiseSeed _noiseSeed, _hashSeed;
        private int* _px, _py;
        private float* _ox, _oy;

        [GlobalSetup]
        public unsafe void GlobalSetup()
        {
            _noiseSeed = new NoiseSeed(0);
            _hashSeed = new HashSeed();
            var length = 1600 * 1600;
            _px = (int*)NativeMemory.AlignedAlloc((nuint)length * sizeof(int), (nuint)sizeof(Vector<int>));
            _py = (int*)NativeMemory.AlignedAlloc((nuint)length * sizeof(int), (nuint)sizeof(Vector<int>));
            _ox = (float*)NativeMemory.AlignedAlloc((nuint)length * sizeof(float), (nuint)sizeof(Vector<float>));
            _oy = (float*)NativeMemory.AlignedAlloc((nuint)length * sizeof(float), (nuint)sizeof(Vector<float>));
            for (int x = 0; x < 1600; x++)
            {
                for (int y = 0; y < 1600; y++)
                {
                    var index = x + y * 1600;
                    var x0 = (x / 80f);
                    var y0 = (y / 80f);
                    var floorx = MathF.Floor(x0);
                    var floory = MathF.Floor(y0);
                    _px[index] = (int)floorx;
                    _px[index] = (int)floory;
                    _ox[index] = x0 - floorx;
                    _oy[index] = y0 - floory;
                }
            }
        }

        [Benchmark]
        public void NoiseSeedFor()
        {
            for (int i = 0; i < 1600 * 1600; i += 1)
            {
                _noiseSeed.GetGrad(_noiseSeed.Hash(_noiseSeed.Hash(_px[i]) + _py[i]), _ox[i], _oy[i]);
            }
        }

        [Benchmark]
        public void NoiseSeedParallel()
        {
            Parallel.For(0, 1600 * 1600, i =>
            {
                _noiseSeed.GetGrad(_noiseSeed.Hash(_noiseSeed.Hash(_px[i]) + _py[i]), _ox[i], _oy[i]);
            });
        }


        [Benchmark(Baseline = true)]
        public void NoiseSeedSIMD()
        {
            var count = 1600 * 1600 / Vector<float>.Count;
            Parallel.For(0, count, c =>
            {
                var index = c * Vector<float>.Count;
                var px = Vector.LoadAlignedNonTemporal<int>(_px + index);
                var py = Vector.LoadAlignedNonTemporal<int>(_py + index);
                var ox = Vector.LoadAlignedNonTemporal<float>(_ox + index);
                var oy = Vector.LoadAlignedNonTemporal<float>(_oy + index);
                _noiseSeed.GetGrad(_noiseSeed.Hash(_noiseSeed.Hash(px) + py), ox, oy);
            });
        }

        [Benchmark]
        public void NoiseSeedSIMDFor()
        {
            for (int i = 0; i < 1600 * 1600; i += Vector<float>.Count)
            {
                var px = Vector.LoadAlignedNonTemporal<int>(_px + i);
                var py = Vector.LoadAlignedNonTemporal<int>(_py + i);
                var ox = Vector.LoadAlignedNonTemporal<float>(_ox + i);
                var oy = Vector.LoadAlignedNonTemporal<float>(_oy + i);
                _noiseSeed.GetGrad(_noiseSeed.Hash(_noiseSeed.Hash(px) + py), ox, oy);
            }
        }

        [Benchmark]
        public void HashSeedFor()
        {
            for (int i = 0; i < 1600 * 1600; i += 1)
            {
                _hashSeed.GetGrad(_px[i], _py[i], _ox[i], _oy[i]);
            }
        }

        [Benchmark]
        public void HashSeedParallel()
        {
            Parallel.For(0, 1600 * 1600, i =>
            {
                _hashSeed.GetGrad(_px[i], _py[i], _ox[i], _oy[i]);
            });
        }

        [Benchmark]
        public void HashSeedSIMD()
        {
            var count = 1600 * 1600 / Vector<float>.Count;
            Parallel.For(0, count, c =>
            {
                var index = c * Vector<float>.Count;
                var px = Vector.LoadAlignedNonTemporal<int>(_px + index);
                var py = Vector.LoadAlignedNonTemporal<int>(_py + index);
                var ox = Vector.LoadAlignedNonTemporal<float>(_ox + index);
                var oy = Vector.LoadAlignedNonTemporal<float>(_oy + index);
                _hashSeed.GetGrad(px, py, ox, oy);
            });
        }

        [Benchmark]
        public void HashSeedSIMDFor()
        {
            for (int i = 0; i < 1600 * 1600; i += Vector<float>.Count)
            {
                var px = Vector.LoadAlignedNonTemporal<int>(_px + i);
                var py = Vector.LoadAlignedNonTemporal<int>(_py + i);
                var ox = Vector.LoadAlignedNonTemporal<float>(_ox + i);
                var oy = Vector.LoadAlignedNonTemporal<float>(_oy + i);
                _hashSeed.GetGrad(px, py, ox, oy);
            }
        }
    }
}
