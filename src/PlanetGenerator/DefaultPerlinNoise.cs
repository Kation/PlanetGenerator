using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlanetGenerator
{
    public class DefaultPerlinNoise : PerlinNoise
    {
        private byte[] _perm;

        public DefaultPerlinNoise(int seed)
        {
            _perm = new byte[256];
            Random r = new Random(seed);
            r.NextBytes(_perm);
        }

        public DefaultPerlinNoise(byte[] seeds)
        {
            if (seeds == null)
                throw new ArgumentNullException(nameof(seeds));
            if (seeds.Length == 0)
                throw new ArgumentException("Seeds must have value.");
            _perm = seeds;
        }

        protected override int Hash(int value)
        {
            return _perm[value & 0xff];
        }

        private static readonly int[] _Prime = new int[] { 13283693, 62374087, 18303827, 53344667, 26854063, 89862779, 30319481, 80638853, 35568517, 95418593, 44407843, 71470727 };
        protected override float Grad(int[] position, float[] offsets)
        {
            float value = 0;
            for (int i = 0; i < position.Length; i++)
            {
                int hash = 0;
                for (int o = 0; o < offsets.Length; o++)
                {
                    int n = i + o;
                    if (n >= offsets.Length)
                        n -= offsets.Length;
                    hash = Hash(hash + position[n] * _Prime[o + i]);
                }
                float v;
                v = (MathF.Sin(hash) * 43758.5453123f);
                v -= MathF.Floor(v);
                v = (v * 2 - 1) * offsets[i];
                value += v;
            }

            return value;
        }
    }
}
