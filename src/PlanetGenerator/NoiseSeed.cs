using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public class NoiseSeed : INoiseSeed
    {
        private readonly int[] _perm;
        private readonly float[] _gradD1;
        private readonly float[] _gradD2x;
        private readonly float[] _gradD2y;
        private readonly float[] _gradD3x;
        private readonly float[] _gradD3y;
        private readonly float[] _gradD3z;
        private readonly float[] _gradD4x;
        private readonly float[] _gradD4y;
        private readonly float[] _gradD4z;
        private readonly float[] _gradD4w;
        private readonly int _mask;

        public NoiseSeed(int seed) : this(seed, 2) { }

        public NoiseSeed(int seed, int permMultiple)
        {
            if (permMultiple < 1)
                throw new ArgumentException("因子数至少为1。");
            int length = 1 << (permMultiple * 4);
            _perm = new int[length];
            Random rnd = new Random(seed);
            for (int i = 0; i < length; i++)
                _perm[i] = rnd.Next(0, length);
            _gradD1 = new float[length];
            _gradD2x = new float[length];
            _gradD2y = new float[length];
            _gradD3x = new float[length];
            _gradD3y = new float[length];
            _gradD3z = new float[length];
            _gradD4x = new float[length];
            _gradD4y = new float[length];
            _gradD4z = new float[length];
            _gradD4w = new float[length];
            for (int i = 0; i < length; i++)
            {
                _gradD1[i] = rnd.NextSingle() * 2f - 1f;
                var d2 = Vector2.Normalize(new Vector2(rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f));
                _gradD2x[i] = d2.X;
                _gradD2y[i] = d2.Y;
                var d3 = Vector3.Normalize(new Vector3(rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f));
                _gradD3x[i] = d3.X;
                _gradD3y[i] = d3.Y;
                _gradD3z[i] = d3.Z;
                var d4 = Vector4.Normalize(new Vector4(rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f, rnd.NextSingle() * 2f - 1f));
                _gradD4x[i] = d4.X;
                _gradD4y[i] = d4.Y;
                _gradD4z[i] = d4.Z;
                _gradD4w[i] = d4.W;
            }
            _mask = length - 1;
        }

        public float GetGrad(int hash, float offsetX)
        {
            return _gradD1[hash] * offsetX;
        }

        public float GetGrad(int hash, float offsetX, float offsetY)
        {
            return _gradD2x[hash] * offsetX + _gradD2y[hash] * offsetY;
        }

        public float GetGrad(int hash, float offsetX, float offsetY, float offsetZ)
        {
            return _gradD3x[hash] * offsetX + _gradD3y[hash] * offsetY + _gradD3z[hash] * offsetZ;
        }

        public float GetGrad(int hash, float offsetX, float offsetY, float offsetZ, float offsetW)
        {
            return _gradD4x[hash] * offsetX + _gradD4y[hash] * offsetY + _gradD4z[hash] * offsetZ + _gradD4w[hash] * offsetZ;
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX)
        {
            Span<float> gx = stackalloc float[Vector<float>.Count];
            ref float p = ref MemoryMarshal.GetArrayDataReference(_gradD1);
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                gx[i] = Unsafe.Add(ref p, hash[i]);
            }
            return new Vector<float>(gx) * offsetX;
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY)
        {
            Span<float> gx = stackalloc float[Vector<float>.Count];
            Span<float> gy = stackalloc float[Vector<float>.Count];
            ref float px = ref MemoryMarshal.GetArrayDataReference(_gradD2x);
            ref float py = ref MemoryMarshal.GetArrayDataReference(_gradD2y);
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                gx[i] = Unsafe.Add(ref px, hash[i]);
                gy[i] = Unsafe.Add(ref py, hash[i]);
            }
            return new Vector<float>(gx) * offsetX + new Vector<float>(gy) * offsetY;
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ)
        {
            Span<float> gx = stackalloc float[Vector<float>.Count];
            Span<float> gy = stackalloc float[Vector<float>.Count];
            Span<float> gz = stackalloc float[Vector<float>.Count];
            ref float px = ref MemoryMarshal.GetArrayDataReference(_gradD3x);
            ref float py = ref MemoryMarshal.GetArrayDataReference(_gradD3y);
            ref float pz = ref MemoryMarshal.GetArrayDataReference(_gradD3z);
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                gx[i] = Unsafe.Add(ref px, hash[i]);
                gy[i] = Unsafe.Add(ref py, hash[i]);
                gz[i] = Unsafe.Add(ref pz, hash[i]);
            }
            return new Vector<float>(gx) * offsetX + new Vector<float>(gy) * offsetY + new Vector<float>(gz) * offsetZ;
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW)
        {
            Span<float> gx = stackalloc float[Vector<float>.Count];
            Span<float> gy = stackalloc float[Vector<float>.Count];
            Span<float> gz = stackalloc float[Vector<float>.Count];
            Span<float> gw = stackalloc float[Vector<float>.Count];
            ref float px = ref MemoryMarshal.GetArrayDataReference(_gradD4x);
            ref float py = ref MemoryMarshal.GetArrayDataReference(_gradD4y);
            ref float pz = ref MemoryMarshal.GetArrayDataReference(_gradD4z);
            ref float pw = ref MemoryMarshal.GetArrayDataReference(_gradD4w);
            for (int i = 0; i < Vector<int>.Count; i++)
            {
                gx[i] = Unsafe.Add(ref px, hash[i]);
                gy[i] = Unsafe.Add(ref py, hash[i]);
                gz[i] = Unsafe.Add(ref pz, hash[i]);
                gw[i] = Unsafe.Add(ref pw, hash[i]);
            }
            return new Vector<float>(gx) * offsetX + new Vector<float>(gy) * offsetY + new Vector<float>(gz) * offsetZ + new Vector<float>(gw) * offsetW;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int Hash(int value)
        {
            ref int p = ref MemoryMarshal.GetArrayDataReference(_perm);
            return Unsafe.Add(ref p, value & _mask);
        }

        public unsafe Vector<int> Hash(Vector<int> value)
        {
            Span<int> p = stackalloc int[Vector<int>.Count];
            for (int i = 0; i < Vector<int>.Count; i++)
                p[i] = Hash(value[i]);
            return new Vector<int>(p);
        }
    }
}
