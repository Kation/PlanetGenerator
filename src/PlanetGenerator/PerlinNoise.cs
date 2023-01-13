using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PlanetGenerator
{
    public abstract class PerlinNoise
    {
        public virtual float Get(params float[] position)
        {
            if (position.Length == 0)
                return 0f;
            float[] grads = Grads(position, out var floors);
            return Lerp(grads, position, floors);
        }

        public virtual float[] Get(float[] positions, int rank)
        {
            if (positions.Length % rank != 0)
                throw new ArgumentException();
            return new float[positions.Length / rank];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Lerp(float min, float max, float offset)
        {
            return min - (min - max) * Fade(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Lerp(float[] grads, float[] position, int[] floors)
        {
            List<Tuple<float, float>> values = new List<Tuple<float, float>>();
            for (int n = 0; n < grads.Length; n += 2)
            {
                var min = grads[n];
                var max = grads[n + 1];
                values.Add(new Tuple<float, float>(min, max));
            }
            List<float> lerps = new List<float>();
            //计算插值
            for (int i = 0; i < position.Length; i++)
            {
                if (lerps.Count > 0)
                {
                    for (int n = 0; n < lerps.Count; n += 2)
                        values.Add(new Tuple<float, float>(lerps[n], lerps[n + 1]));
                    lerps.Clear();
                }
                var o = position[i] - floors[i];
                for (int n = 0; n < values.Count; n++)
                {
                    lerps.Add(Lerp(values[n].Item1, values[n].Item2, o));
                }
                values.Clear();
            }
            return lerps[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int[] Floors(float[] position, out float[] offsets)
        {
            int[] vecs = new int[position.Length];
            offsets = new float[position.Length];
            for (int i = 0; i < position.Length; i++)
            {
                vecs[i] = FloorToInt(position[i]);
                offsets[i] = position[i] - vecs[i];
            }
            return vecs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float[] Grads(float[] position, out int[] floors)
        {
            int p = GetGradsCount(position.Length);
            floors = Floors(position, out var offsets);
            float[] grads = new float[p];
            for (int i = 0; i < p; i++)
            {
                int[] gradP = new int[position.Length];
                float[] gradO = new float[position.Length];
                for (int v = 0; v < position.Length; v++)
                {
                    if (((1 << v) & i) == 0)
                    {
                        gradP[v] = floors[v];
                        gradO[v] = offsets[v];
                    }
                    else
                    {
                        gradP[v] = floors[v] + 1;
                        gradO[v] = offsets[v] - 1;
                    }
                }
                //grads[i] = Grad(hashs[i], gradO);
                grads[i] = Grad(gradP, gradO);
            }
            return grads;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int GetGradsCount(int dimension)
        {
            return 2 << (dimension - 1);
        }

        protected abstract int Hash(int value);

        protected virtual int Hash(int[] position)
        {
            int hash = 0;
            for (int v = 0; v < position.Length; v++)
            {
                hash = Hash(hash + position[v]);
            }
            return hash;
        }

        protected virtual float Grad(int[] position, float[] offsets)
        {
            var hash = Hash(position);
            hash = hash & ((2 << (offsets.Length - 1)) - 1);
            float f = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                var h = 1 << i;
                bool flip = (hash & h) == 0;
                if (flip)
                {
                    f -= offsets[i];
                }
                else
                {
                    f += offsets[i];
                }
            }
            return f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Fade(float value)
        {
            return value * value * value * (value * (value * 6 - 15) + 10);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Floor(float value)
        {
#if NETSTANDARD2_0
            return (float)Math.Floor(value);
#else
            return MathF.Floor(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int FloorToInt(float value)
        {
#if NETSTANDARD2_0
            return (int)Math.Floor(value);
#else
            return (int)MathF.Floor(value);
#endif
        }
    }
}
