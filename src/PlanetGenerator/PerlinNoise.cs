using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public sealed class PerlinNoise : INoise
    {
        private readonly INoiseSeed _seed;

        public PerlinNoise(INoiseSeed seed)
        {
            _seed = seed;
        }

        public PerlinNoise(int seed) : this(new NoiseSeed(seed)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Lerp(float min, float max, float offset)
        {
            return min - (min - max) * offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector<float> Lerp(Vector<float> min, Vector<float> max, Vector<float> offset)
        {
            return min - (min - max) * offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Fade(float value)
        {
            return value * value * value * (value * (value * 6 - 15) + 10);
        }

        private static readonly Vector<float> _Fade6 = new Vector<float>(6f);
        private static readonly Vector<float> _Fade15 = new Vector<float>(15f);
        private static readonly Vector<float> _Fade10 = new Vector<float>(10f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector<float> Fade(Vector<float> value)
        {
            return value * value * value * (value * (value * _Fade6 - _Fade15) + _Fade10);
        }

        public INoiseSeed Seed => _seed;

        public float Get(float x, int cellOffsetX = 0)
        {
            int cellX = (int)MathF.Floor(x);
            float ox1 = x - cellX;
            float ox2 = ox1 - 1f;
            cellX += cellOffsetX;

            var gx1 = _seed.GetGrad(cellX, ox1);
            var gx2 = _seed.GetGrad(cellX + 1, ox2);

            return Lerp(gx1, gx2, Fade(ox1));
        }

        public float Get(float x, float y, int cellOffsetX = 0, int cellOffsetY = 0)
        {
            int cellX = (int)MathF.Floor(x);
            int cellY = (int)MathF.Floor(y);
            float ox1 = x - cellX;
            float ox2 = ox1 - 1f;
            float oy1 = y - cellY;
            float oy2 = oy1 - 1f;
            cellX += cellOffsetX;
            cellY += cellOffsetY;

            var gx1y1 = _seed.GetGrad(cellX, cellY, ox1, oy1);
            var gx2y1 = _seed.GetGrad(cellX + 1, cellY, ox2, oy1);
            var gx1y2 = _seed.GetGrad(cellX, cellY + 1, ox1, oy2);
            var gx2y2 = _seed.GetGrad(cellX + 1, cellY + 1, ox2, oy2);

            var fx = Fade(ox1);

            var lh1 = Lerp(gx1y1, gx2y1, fx);
            var lh2 = Lerp(gx1y2, gx2y2, fx);
            return Lerp(lh1, lh2, Fade(oy1));
        }

        public float Get(float x, float y, float z, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0)
        {
            int cellX = (int)MathF.Floor(x);
            int cellY = (int)MathF.Floor(y);
            int cellZ = (int)MathF.Floor(z);
            float ox1 = x - cellX;
            float ox2 = ox1 - 1f;
            float oy1 = y - cellY;
            float oy2 = oy1 - 1f;
            float oz1 = z - cellZ;
            float oz2 = oz1 - 1f;
            cellX += cellOffsetX;
            cellY += cellOffsetY;
            cellZ += cellOffsetZ;

            var gx1y1z1 = _seed.GetGrad(cellX, cellY, cellZ, ox1, oy1, oz1);
            var gx2y1z1 = _seed.GetGrad(cellX + 1, cellY, cellZ, ox2, oy1, oz1);
            var gx1y2z1 = _seed.GetGrad(cellX, cellY + 1, cellZ, ox1, oy2, oz1);
            var gx2y2z1 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ, ox2, oy2, oz1);
            var gx1y1z2 = _seed.GetGrad(cellX, cellY, cellZ + 1, ox1, oy1, oz2);
            var gx2y1z2 = _seed.GetGrad(cellX + 1, cellY, cellZ + 1, ox2, oy1, oz2);
            var gx1y2z2 = _seed.GetGrad(cellX, cellY + 1, cellZ + 1, ox1, oy2, oz2);
            var gx2y2z2 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ + 1, ox2, oy2, oz2);

            var fx = Fade(ox1);
            var fy = Fade(oy1);

            var lx1 = Lerp(gx1y1z1, gx2y1z1, fx);
            var lx2 = Lerp(gx1y2z1, gx2y2z1, fx);
            var lx3 = Lerp(gx1y1z2, gx2y1z2, fx);
            var lx4 = Lerp(gx1y2z2, gx2y2z2, fx);

            var ly1 = Lerp(lx1, lx2, fy);
            var ly2 = Lerp(lx3, lx4, fy);

            return Lerp(ly1, ly2, Fade(oz1));
        }

        public float Get(float x, float y, float z, float w, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0)
        {
            int cellX = (int)MathF.Floor(x);
            int cellY = (int)MathF.Floor(y);
            int cellZ = (int)MathF.Floor(z);
            int cellW = (int)MathF.Floor(w);
            float ox1 = x - cellX;
            float ox2 = ox1 - 1f;
            float oy1 = y - cellY;
            float oy2 = oy1 - 1f;
            float oz1 = z - cellZ;
            float oz2 = oz1 - 1f;
            float ow1 = w - cellW;
            float ow2 = ow1 - 1f;
            cellX += cellOffsetX;
            cellY += cellOffsetY;
            cellZ += cellOffsetZ;
            cellW += cellOffsetW;

            var gx1y1z1w1 = _seed.GetGrad(cellX, cellY, cellZ, cellW, ox1, oy1, oz1, ow1);
            var gx2y1z1w1 = _seed.GetGrad(cellX + 1, cellY, cellZ, cellW, ox2, oy1, oz1, ow1);
            var gx1y2z1w1 = _seed.GetGrad(cellX, cellY + 1, cellZ, cellW, ox1, oy2, oz1, ow1);
            var gx2y2z1w1 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ, cellW, ox2, oy2, oz1, ow1);
            var gx1y1z2w1 = _seed.GetGrad(cellX, cellY, cellZ + 1, cellW, ox1, oy1, oz2, ow1);
            var gx2y1z2w1 = _seed.GetGrad(cellX + 1, cellY, cellZ + 1, cellW, ox2, oy1, oz2, ow1);
            var gx1y2z2w1 = _seed.GetGrad(cellX, cellY + 1, cellZ + 1, cellW, ox1, oy2, oz2, ow1);
            var gx2y2z2w1 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ + 1, cellW, ox2, oy2, oz2, ow1);
            var gx1y1z1w2 = _seed.GetGrad(cellX, cellY, cellZ, cellW + 1, ox1, oy1, oz1, ow2);
            var gx2y1z1w2 = _seed.GetGrad(cellX + 1, cellY, cellZ, cellW + 1, ox2, oy1, oz1, ow2);
            var gx1y2z1w2 = _seed.GetGrad(cellX, cellY + 1, cellZ, cellW + 1, ox1, oy2, oz1, ow2);
            var gx2y2z1w2 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ, cellW + 1, ox2, oy2, oz1, ow2);
            var gx1y1z2w2 = _seed.GetGrad(cellX, cellY, cellZ + 1, cellW + 1, ox1, oy1, oz2, ow2);
            var gx2y1z2w2 = _seed.GetGrad(cellX + 1, cellY, cellZ + 1, cellW + 1, ox2, oy1, oz2, ow2);
            var gx1y2z2w2 = _seed.GetGrad(cellX, cellY + 1, cellZ + 1, cellW + 1, ox1, oy2, oz2, ow2);
            var gx2y2z2w2 = _seed.GetGrad(cellX + 1, cellY + 1, cellZ + 1, cellW + 1, ox2, oy2, oz2, ow2);

            var fx = Fade(ox1);
            var fy = Fade(oy1);
            var fz = Fade(oz1);

            var lx1 = Lerp(gx1y1z1w1, gx2y1z1w1, fx);
            var lx2 = Lerp(gx1y2z1w1, gx2y2z1w1, fx);
            var lx3 = Lerp(gx1y1z2w1, gx2y1z2w1, fx);
            var lx4 = Lerp(gx1y2z2w1, gx2y2z2w1, fx);
            var lx5 = Lerp(gx1y1z1w2, gx2y1z1w2, fx);
            var lx6 = Lerp(gx1y2z1w2, gx2y2z1w2, fx);
            var lx7 = Lerp(gx1y1z2w2, gx2y1z2w2, fx);
            var lx8 = Lerp(gx1y2z2w2, gx2y2z2w2, fx);

            var ly1 = Lerp(lx1, lx2, fy);
            var ly2 = Lerp(lx3, lx4, fy);
            var ly3 = Lerp(lx5, lx6, fy);
            var ly4 = Lerp(lx7, lx8, fy);

            var lz1 = Lerp(ly1, ly2, fz);
            var lz2 = Lerp(ly3, ly4, fz);

            return Lerp(lz1, lz2, Fade(ow1));
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> values, int cellOffsetX = 0, bool aligned = false)
        {
            if (x.Length % Vector<float>.Count != 0)
                throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
            if (x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            var count = x.Length / Vector<float>.Count;
            using (var px = x.Pin())
            using (var pv = values.Pin())
            {
                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);

                Parallel.For(0, count, i =>
                {
                    var index = i * Vector<float>.Count;
                    Vector<float> vx;
                    if (aligned)
                        vx = Vector.LoadAlignedNonTemporal((float*)px.Pointer + index);
                    else
                        vx = Vector.Load((float*)px.Pointer + index);
                    Vector<float> floorX = Vector.Floor(vx);
                    Vector<int> cellX = Vector.ConvertToInt32(floorX) + vcellOffsetX;
                    Vector<float> ox1 = vx - floorX;
                    Vector<float> ox2 = ox1 - Vector<float>.One;

                    var gx1 = _seed.GetGrad(cellX, ox1);
                    var gx2 = _seed.GetGrad(cellX + Vector<int>.One, ox2);

                    if (aligned)
                        Vector.StoreAlignedNonTemporal(Lerp(gx1, gx2, Fade(ox1)), (float*)pv.Pointer + index);
                    else
                        Vector.Store(Lerp(gx1, gx2, Fade(ox1)), (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, bool aligned = false)
        {
            if (x.Length % Vector<float>.Count != 0)
                throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
            if (x.Length != y.Length || x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            var count = x.Length / Vector<float>.Count;
            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pv = values.Pin())
            {
                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);

                Parallel.For(0, count, i =>
                {
                    var index = i * Vector<float>.Count;
                    Vector<float> vx, vy;
                    if (aligned)
                    {
                        vx = Vector.LoadAlignedNonTemporal((float*)px.Pointer + index);
                        vy = Vector.LoadAlignedNonTemporal((float*)py.Pointer + index);
                    }
                    else
                    {
                        vx = Vector.Load((float*)px.Pointer + index);
                        vy = Vector.Load((float*)py.Pointer + index);
                    }
                    Vector<float> floorX = Vector.Floor(vx);
                    Vector<float> floorY = Vector.Floor(vy);
                    Vector<int> cellX = Vector.ConvertToInt32(floorX) + vcellOffsetX;
                    Vector<int> cellY = Vector.ConvertToInt32(floorY) + vcellOffsetY;
                    Vector<float> ox1 = vx - floorX;
                    Vector<float> ox2 = ox1 - Vector<float>.One;
                    Vector<float> oy1 = vy - floorY;
                    Vector<float> oy2 = oy1 - Vector<float>.One;

                    Vector<float> gx1y1 = _seed.GetGrad(cellX, cellY, ox1, oy1);
                    Vector<float> gx2y1 = _seed.GetGrad(cellX + Vector<int>.One, cellY, ox2, oy1);
                    Vector<float> gx1y2 = _seed.GetGrad(cellX, cellY + Vector<int>.One, ox1, oy2);
                    Vector<float> gx2y2 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, ox2, oy2);

                    Vector<float> fx = Fade(ox1);

                    Vector<float> lx1 = Lerp(gx1y1, gx2y1, fx);
                    Vector<float> lx2 = Lerp(gx1y2, gx2y2, fx);

                    if (aligned)
                        Vector.StoreAlignedNonTemporal(Lerp(lx1, lx2, Fade(oy1)), (float*)pv.Pointer + index);
                    else
                        Vector.Store(Lerp(lx1, lx2, Fade(oy1)), (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, bool aligned = false)
        {
            if (x.Length % Vector<float>.Count != 0)
                throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
            if (x.Length != y.Length || x.Length != z.Length || x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            var count = x.Length / Vector<float>.Count;
            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pz = z.Pin())
            using (var pv = values.Pin())
            {
                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);
                Vector<int> vcellOffsetZ = new Vector<int>(cellOffsetZ);

                Parallel.For(0, count, i =>
                {
                    var index = i * Vector<float>.Count;
                    Vector<float> vx, vy, vz;
                    if (aligned)
                    {
                        vx = Vector.LoadAlignedNonTemporal((float*)px.Pointer + index);
                        vy = Vector.LoadAlignedNonTemporal((float*)py.Pointer + index);
                        vz = Vector.LoadAlignedNonTemporal((float*)pz.Pointer + index);
                    }
                    else
                    {
                        vx = Vector.Load((float*)px.Pointer + index);
                        vy = Vector.Load((float*)py.Pointer + index);
                        vz = Vector.Load((float*)pz.Pointer + index);
                    }
                    Vector<float> floorX = Vector.Floor(vx);
                    Vector<float> floorY = Vector.Floor(vy);
                    Vector<float> floorZ = Vector.Floor(vz);
                    Vector<int> cellX = Vector.ConvertToInt32(floorX) + vcellOffsetX;
                    Vector<int> cellY = Vector.ConvertToInt32(floorY) + vcellOffsetY;
                    Vector<int> cellZ = Vector.ConvertToInt32(floorZ) + vcellOffsetZ;
                    Vector<float> ox1 = vx - floorX;
                    Vector<float> ox2 = ox1 - Vector<float>.One;
                    Vector<float> oy1 = vy - floorY;
                    Vector<float> oy2 = oy1 - Vector<float>.One;
                    Vector<float> oz1 = vz - floorZ;
                    Vector<float> oz2 = oz1 - Vector<float>.One;

                    Vector<float> gx1y1z1 = _seed.GetGrad(cellX, cellY, cellZ, ox1, oy1, oz1);
                    Vector<float> gx2y1z1 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ, ox2, oy1, oz1);
                    Vector<float> gx1y2z1 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ, ox1, oy2, oz1);
                    Vector<float> gx2y2z1 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ, ox2, oy2, oz1);
                    Vector<float> gx1y1z2 = _seed.GetGrad(cellX, cellY, cellZ + Vector<int>.One, ox1, oy1, oz2);
                    Vector<float> gx2y1z2 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ + Vector<int>.One, ox2, oy1, oz2);
                    Vector<float> gx1y2z2 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ + Vector<int>.One, ox1, oy2, oz2);
                    Vector<float> gx2y2z2 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ + Vector<int>.One, ox2, oy2, oz2);

                    Vector<float> fx = Fade(ox1);
                    Vector<float> fy = Fade(oy1);

                    Vector<float> lx1 = Lerp(gx1y1z1, gx2y1z1, fx);
                    Vector<float> lx2 = Lerp(gx1y2z1, gx2y2z1, fx);
                    Vector<float> lx3 = Lerp(gx1y1z2, gx2y1z2, fx);
                    Vector<float> lx4 = Lerp(gx1y2z2, gx2y2z2, fx);

                    var ly1 = Lerp(lx1, lx2, fy);
                    var ly2 = Lerp(lx3, lx4, fy);

                    if (aligned)
                        Vector.StoreAlignedNonTemporal(Lerp(ly1, ly2, Fade(oz1)), (float*)pv.Pointer + index);
                    else
                        Vector.Store(Lerp(ly1, ly2, Fade(oz1)), (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> w, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0, bool aligned = false)
        {
            if (x.Length % Vector<float>.Count != 0)
                throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
            if (x.Length != y.Length || x.Length != z.Length || x.Length != w.Length || x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            var count = x.Length / Vector<float>.Count;
            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pz = z.Pin())
            using (var pw = w.Pin())
            using (var pv = values.Pin())
            {
                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);
                Vector<int> vcellOffsetZ = new Vector<int>(cellOffsetZ);
                Vector<int> vcellOffsetW = new Vector<int>(cellOffsetW);

                Parallel.For(0, count, i =>
                {
                    var index = i * Vector<float>.Count;
                    Vector<float> vx, vy, vz, vw;
                    if (aligned)
                    {
                        vx = Vector.LoadAlignedNonTemporal((float*)px.Pointer + index);
                        vy = Vector.LoadAlignedNonTemporal((float*)py.Pointer + index);
                        vz = Vector.LoadAlignedNonTemporal((float*)pz.Pointer + index);
                        vw = Vector.LoadAlignedNonTemporal((float*)pw.Pointer + index);
                    }
                    else
                    {
                        vx = Vector.Load((float*)px.Pointer + index);
                        vy = Vector.Load((float*)py.Pointer + index);
                        vz = Vector.Load((float*)pz.Pointer + index);
                        vw = Vector.Load((float*)pw.Pointer + index);
                    }
                    Vector<float> floorX = Vector.Floor(vx);
                    Vector<float> floorY = Vector.Floor(vy);
                    Vector<float> floorZ = Vector.Floor(vz);
                    Vector<float> floorW = Vector.Floor(vw);
                    Vector<int> cellX = Vector.ConvertToInt32(floorX) + vcellOffsetX;
                    Vector<int> cellY = Vector.ConvertToInt32(floorY) + vcellOffsetY;
                    Vector<int> cellZ = Vector.ConvertToInt32(floorZ) + vcellOffsetZ;
                    Vector<int> cellW = Vector.ConvertToInt32(floorW) + vcellOffsetW;
                    Vector<float> ox1 = vx - floorX;
                    Vector<float> ox2 = ox1 - Vector<float>.One;
                    Vector<float> oy1 = vy - floorY;
                    Vector<float> oy2 = oy1 - Vector<float>.One;
                    Vector<float> oz1 = vz - floorZ;
                    Vector<float> oz2 = oz1 - Vector<float>.One;
                    Vector<float> ow1 = vw - floorW;
                    Vector<float> ow2 = ow1 - Vector<float>.One;

                    Vector<float> gx1y1z1w1 = _seed.GetGrad(cellX, cellY, cellZ, cellW, ox1, oy1, oz1, ow1);
                    Vector<float> gx2y1z1w1 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ, cellW, ox2, oy1, oz1, ow1);
                    Vector<float> gx1y2z1w1 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ, cellW, ox1, oy2, oz1, ow1);
                    Vector<float> gx2y2z1w1 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ, cellW, ox2, oy2, oz1, ow1);
                    Vector<float> gx1y1z2w1 = _seed.GetGrad(cellX, cellY, cellZ + Vector<int>.One, cellW, ox1, oy1, oz2, ow1);
                    Vector<float> gx2y1z2w1 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ + Vector<int>.One, cellW, ox2, oy1, oz2, ow1);
                    Vector<float> gx1y2z2w1 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ + Vector<int>.One, cellW, ox1, oy2, oz2, ow1);
                    Vector<float> gx2y2z2w1 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ + Vector<int>.One, cellW, ox2, oy2, oz2, ow1);
                    Vector<float> gx1y1z1w2 = _seed.GetGrad(cellX, cellY, cellZ, cellW + Vector<int>.One, ox1, oy1, oz1, ow2);
                    Vector<float> gx2y1z1w2 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ, cellW + Vector<int>.One, ox2, oy1, oz1, ow2);
                    Vector<float> gx1y2z1w2 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ, cellW + Vector<int>.One, ox1, oy2, oz1, ow2);
                    Vector<float> gx2y2z1w2 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ, cellW + Vector<int>.One, ox2, oy2, oz1, ow2);
                    Vector<float> gx1y1z2w2 = _seed.GetGrad(cellX, cellY, cellZ + Vector<int>.One, cellW + Vector<int>.One, ox1, oy1, oz2, ow2);
                    Vector<float> gx2y1z2w2 = _seed.GetGrad(cellX + Vector<int>.One, cellY, cellZ + Vector<int>.One, cellW + Vector<int>.One, ox2, oy1, oz2, ow2);
                    Vector<float> gx1y2z2w2 = _seed.GetGrad(cellX, cellY + Vector<int>.One, cellZ + Vector<int>.One, cellW + Vector<int>.One, ox1, oy2, oz2, ow2);
                    Vector<float> gx2y2z2w2 = _seed.GetGrad(cellX + Vector<int>.One, cellY + Vector<int>.One, cellZ + Vector<int>.One, cellW + Vector<int>.One, ox2, oy2, oz2, ow2);

                    Vector<float> fx = Fade(ox1);
                    Vector<float> fy = Fade(oy1);
                    Vector<float> fz = Fade(oz1);

                    Vector<float> lx1 = Lerp(gx1y1z1w1, gx2y1z1w1, fx);
                    Vector<float> lx2 = Lerp(gx1y2z1w1, gx2y2z1w1, fx);
                    Vector<float> lx3 = Lerp(gx1y1z2w1, gx2y1z2w1, fx);
                    Vector<float> lx4 = Lerp(gx1y2z2w1, gx2y2z2w1, fx);
                    Vector<float> lx5 = Lerp(gx1y1z1w2, gx2y1z1w2, fx);
                    Vector<float> lx6 = Lerp(gx1y2z1w2, gx2y2z1w2, fx);
                    Vector<float> lx7 = Lerp(gx1y1z2w2, gx2y1z2w2, fx);
                    Vector<float> lx8 = Lerp(gx1y2z2w2, gx2y2z2w2, fx);

                    var ly1 = Lerp(lx1, lx2, fy);
                    var ly2 = Lerp(lx3, lx4, fy);
                    var ly3 = Lerp(lx5, lx6, fy);
                    var ly4 = Lerp(lx7, lx8, fy);

                    var lz1 = Lerp(ly1, ly2, fz);
                    var lz2 = Lerp(ly3, ly4, fz);

                    if (aligned)
                        Vector.StoreAlignedNonTemporal(Lerp(lz1, lz2, Fade(ow1)), (float*)pv.Pointer + index);
                    else
                        Vector.Store(Lerp(lz1, lz2, Fade(ow1)), (float*)pv.Pointer + index);
                });
            }
        }
    }
}
