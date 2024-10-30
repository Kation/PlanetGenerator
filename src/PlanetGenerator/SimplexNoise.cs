using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public sealed class SimplexNoise : INoise
    {
        private static readonly int _VectorLength;
        private static readonly Vector<int> _Int0Vector, _Int1Vector, _Int2Vector, _Int3Vector;
        private static readonly Vector<float> _HalfVector = new Vector<float>(0.5f);

        static SimplexNoise()
        {
            if (Vector.IsHardwareAccelerated)
            {
                _VectorLength = Vector<float>.Count;
                _Int0Vector = Vector<int>.Zero;
                _Int1Vector = Vector<int>.One;
                _Int2Vector = new Vector<int>(2);
                _Int3Vector = new Vector<int>(3);
            }
        }

        private byte[] _perm;
        private float[] _permFloat;
        private INoiseSeed _seed;

        public SimplexNoise(int seed) : this(new NoiseSeed(seed)) { }

        public SimplexNoise(INoiseSeed seed)
        {
            _perm = new byte[256];
            Random r = new Random(0);
            r.NextBytes(_perm);
            _permFloat = new float[256];
            for (int i = 0; i < 256; i++)
                _permFloat[i] = (_perm[i] / 255f - 0.5f) * 2f;
            _seed = seed;
        }

        public INoiseSeed Seed => _seed;

        public float Get(float x, int cellOffsetX = 0)
        {
            var floorX = MathF.Floor(x);
            var offset0 = x - floorX;
            var offset1 = offset0 - 1;
            var cellX = (int)floorX + cellOffsetX;
            var h0 = _seed.Hash(cellX);
            var h1 = _seed.Hash(cellX + 1);
            var g1 = 1f - offset0 * offset0;
            g1 = g1 * g1;
            g1 = g1 * g1 * _seed.GetGrad(h0, offset0);
            var g2 = 1f - offset1 * offset1;
            g2 = g2 * g2;
            g2 = g2 * g2 * _seed.GetGrad(h1, offset1);
            return (g1 + g2) * 0.395f;
        }

        public float Get(float x, float y, int cellOffsetX = 0, int cellOffsetY = 0)
        {
            float sum = x + y;
            float skewToCell, skewFromCell, sample;
            skewToCell = SkewValue<float[,]>.SkewToCell;
            skewFromCell = SkewValue<float[,]>.SkewFromCell;
            sample = SkewValue<float[,]>.Sample;
            float skewFromOrigin = skewFromCell;
            //变形至单元格的差值
            skewToCell *= sum;
            //单元格原点总和
            var cellPositionX = x + skewToCell;
            var cellPositionY = y + skewToCell;
            var cellFloorX = MathF.Floor(cellPositionX);
            var cellFloorY = MathF.Floor(cellPositionY);
            //单元格起点坐标
            var cellX = (int)cellFloorX + cellOffsetX;
            var cellY = (int)cellFloorY + cellOffsetY;
            sum = cellFloorX + cellFloorY;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            //单元格原点在单型里的坐标
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;

            //输入点到单元格起点坐标差值
            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;

            var hx0 = _seed.Hash(cellX);
            var hx1 = _seed.Hash(cellX + 1);

            int h0, h1, h2;
            h0 = _seed.Hash(hx0 + cellY);

            //计算单元格第二个点的位置
            int cellOffset1x, cellOffset1y;
            if (cellPositionX - cellFloorX >= cellPositionY - cellFloorY)
            {
                cellOffset1x = 1;
                cellOffset1y = 0;
                h1 = _seed.Hash(hx1 + cellY);
            }
            else
            {
                cellOffset1x = 0;
                cellOffset1y = 1;
                h1 = _seed.Hash(hx0 + cellY + 1);
            }
            //输入点到单型第二个点坐标的差值
            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            //输入点到单型终点坐标的差值
            var simplexOffset2x = simplexOffset0x - 1 + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - 1 + 2 * skewFromCell;

            float total = 0f;

            h2 = _seed.Hash(hx1 + cellY + 1);

            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h0, simplexOffset0x, simplexOffset0y);
                total += grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h1, simplexOffset1x, simplexOffset1y);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset2x * simplexOffset2x + simplexOffset2y * simplexOffset2y;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h2, simplexOffset2x, simplexOffset2y);
                total += grad * sum;
            }
            total = total * sample;
            return total;
        }

        public float Get(float x, float y, float z, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0)
        {
            float sum = x + y + z;
            float skewToCell, skewFromCell, sample;
            skewToCell = SkewValue<float[,,]>.SkewToCell;
            skewFromCell = SkewValue<float[,,]>.SkewFromCell;
            sample = SkewValue<float[,,]>.Sample;
            float skewFromOrigin = skewFromCell;
            skewToCell *= sum;
            //单元格原点总和
            var cellPositionX = x + skewToCell;
            var cellPositionY = y + skewToCell;
            var cellPositionZ = z + skewToCell;
            var cellFloorX = MathF.Floor(cellPositionX);
            var cellFloorY = MathF.Floor(cellPositionY);
            var cellFloorZ = MathF.Floor(cellPositionZ);
            var cellX = (int)cellFloorX + cellOffsetX;
            var cellY = (int)cellFloorY + cellOffsetY;
            var cellZ = (int)cellFloorZ + cellOffsetZ;
            sum = cellFloorX + cellFloorY + cellFloorZ;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;
            var simplexFloorZ = cellFloorZ - skewFromOrigin;

            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;
            var simplexOffset0z = z - simplexFloorZ;

            var hx0 = _seed.Hash(cellX);
            var hx1 = _seed.Hash(cellX + 1);

            var h0 = _seed.Hash(_seed.Hash(hx0 + cellY) + cellZ);

            var ox = cellPositionX - cellFloorX;
            var oy = cellPositionY - cellFloorY;
            var oz = cellPositionZ - cellFloorZ;
            int rankx = 0,
                ranky = 0,
                rankz = 0;
            if (ox > oy) rankx++; else ranky++;
            if (ox > oz) rankx++; else rankz++;
            if (oy > oz) ranky++; else rankz++;

            int cellOffset1x = rankx >= 2 ? 1 : 0,
                cellOffset1y = ranky >= 2 ? 1 : 0,
                cellOffset1z = rankz >= 2 ? 1 : 0,
                cellOffset2x = rankx >= 1 ? 1 : 0,
                cellOffset2y = ranky >= 1 ? 1 : 0,
                cellOffset2z = rankz >= 1 ? 1 : 0;

            int h1 = _seed.Hash(_seed.Hash(_seed.Hash(cellX + cellOffset1x) + cellY + cellOffset1y) + cellZ + cellOffset1z);
            int h2 = _seed.Hash(_seed.Hash(_seed.Hash(cellX + cellOffset2x) + cellY + cellOffset2y) + cellZ + cellOffset2z);
            int h3 = _seed.Hash(_seed.Hash(hx1 + cellY + 1) + cellZ + 1);

            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            var simplexOffset1z = simplexOffset0z - cellOffset1z + skewFromCell;
            var simplexOffset2x = simplexOffset0x - cellOffset2x + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - cellOffset2y + 2 * skewFromCell;
            var simplexOffset2z = simplexOffset0z - cellOffset2z + 2 * skewFromCell;
            var simplexOffset3x = simplexOffset0x - 1 + 3 * skewFromCell;
            var simplexOffset3y = simplexOffset0y - 1 + 3 * skewFromCell;
            var simplexOffset3z = simplexOffset0z - 1 + 3 * skewFromCell;

            float total;

            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y + simplexOffset0z * simplexOffset0z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                //计算单元格的梯度值
                sum *= sum;
                sum *= sum;
                var grad = _seed.GetGrad(h0, simplexOffset0x, simplexOffset0y, simplexOffset0z);
                total = grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y + simplexOffset1z * simplexOffset1z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h1, simplexOffset1x, simplexOffset1y, simplexOffset1z);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset2x * simplexOffset2x + simplexOffset2y * simplexOffset2y + simplexOffset2z * simplexOffset2z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h2, simplexOffset2x, simplexOffset2y, simplexOffset2z);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset3x * simplexOffset3x + simplexOffset3y * simplexOffset3y + simplexOffset3z * simplexOffset3z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h3, simplexOffset3x, simplexOffset3y, simplexOffset3z);
                total += grad * sum;
            }
            total = total * sample;
            return total;
        }

        public float Get(float x, float y, float z, float w, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0)
        {
            float sum = x + y + z + w;
            float skewToCell, skewFromCell, sample;
            skewToCell = SkewValue<float[,,,]>.SkewToCell;
            skewFromCell = SkewValue<float[,,,]>.SkewFromCell;
            sample = SkewValue<float[,,,]>.Sample;
            float skewFromOrigin = skewFromCell;
            skewToCell *= sum;
            //单元格原点总和
            var cellPositionX = x + skewToCell;
            var cellPositionY = y + skewToCell;
            var cellPositionZ = z + skewToCell;
            var cellPositionW = w + skewToCell;
            var cellFloorX = MathF.Floor(cellPositionX);
            var cellFloorY = MathF.Floor(cellPositionY);
            var cellFloorZ = MathF.Floor(cellPositionZ);
            var cellFloorW = MathF.Floor(cellPositionW);
            var cellX = (int)cellFloorX;
            var cellY = (int)cellFloorY;
            var cellZ = (int)cellFloorZ;
            var cellW = (int)cellFloorW;
            sum = cellFloorX + cellFloorY + cellFloorZ + cellFloorW;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;
            var simplexFloorZ = cellFloorZ - skewFromOrigin;
            var simplexFloorW = cellFloorW - skewFromOrigin;

            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;
            var simplexOffset0z = z - simplexFloorZ;
            var simplexOffset0w = w - simplexFloorW;

            var hx0 = _seed.Hash(cellX);
            var hx1 = _seed.Hash(cellX + 1);

            var h0 = _seed.Hash(_seed.Hash(_seed.Hash(hx0 + cellY) + cellZ) + cellW);

            var ox = cellPositionX - cellFloorX;
            var oy = cellPositionY - cellFloorY;
            var oz = cellPositionZ - cellFloorZ;
            var ow = cellPositionW - cellFloorW;
            int rankx = 0,
                ranky = 0,
                rankz = 0,
                rankw = 0;
            if (ox > oy) rankx++; else ranky++;
            if (ox > oz) rankx++; else rankz++;
            if (ox > ow) rankx++; else rankw++;
            if (oy > oz) ranky++; else rankz++;
            if (oy > ow) ranky++; else rankw++;
            if (oz > ow) rankz++; else rankw++;

            int cellOffset1x = rankx >= 3 ? 1 : 0, cellOffset1y = ranky >= 3 ? 1 : 0, cellOffset1z = rankz >= 3 ? 1 : 0, cellOffset1w = rankw >= 3 ? 1 : 0,
                cellOffset2x = rankx >= 2 ? 1 : 0, cellOffset2y = ranky >= 2 ? 1 : 0, cellOffset2z = rankz >= 2 ? 1 : 0, cellOffset2w = rankw >= 2 ? 1 : 0,
                cellOffset3x = rankx >= 1 ? 1 : 0, cellOffset3y = ranky >= 1 ? 1 : 0, cellOffset3z = rankz >= 1 ? 1 : 0, cellOffset3w = rankw >= 1 ? 1 : 0;

            int h1 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellX + cellOffset1x) + cellY + cellOffset1y) + cellZ + cellOffset1z) + cellW + cellOffset1w);
            int h2 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellX + cellOffset2x) + cellY + cellOffset2y) + cellZ + cellOffset2z) + cellW + cellOffset2w);
            int h3 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellX + cellOffset3x) + cellY + cellOffset3y) + cellZ + cellOffset3z) + cellW + cellOffset3w);
            int h4 = _seed.Hash(_seed.Hash(_seed.Hash(hx1 + cellY + 1) + cellZ + 1) + cellW + 1);

            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            var simplexOffset1z = simplexOffset0z - cellOffset1z + skewFromCell;
            var simplexOffset1w = simplexOffset0w - cellOffset1w + skewFromCell;
            var simplexOffset2x = simplexOffset0x - cellOffset2x + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - cellOffset2y + 2 * skewFromCell;
            var simplexOffset2z = simplexOffset0z - cellOffset2z + 2 * skewFromCell;
            var simplexOffset2w = simplexOffset0w - cellOffset2w + 2 * skewFromCell;
            var simplexOffset3x = simplexOffset0x - cellOffset3x + 3 * skewFromCell;
            var simplexOffset3y = simplexOffset0y - cellOffset3y + 3 * skewFromCell;
            var simplexOffset3z = simplexOffset0z - cellOffset3z + 3 * skewFromCell;
            var simplexOffset3w = simplexOffset0w - cellOffset3w + 3 * skewFromCell;
            var simplexOffset4x = simplexOffset0x - 1 + 4 * skewFromCell;
            var simplexOffset4y = simplexOffset0y - 1 + 4 * skewFromCell;
            var simplexOffset4z = simplexOffset0z - 1 + 4 * skewFromCell;
            var simplexOffset4w = simplexOffset0w - 1 + 4 * skewFromCell;

            float total;

            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y + simplexOffset0z * simplexOffset0z + simplexOffset0w * simplexOffset0w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                //计算单元格的梯度值
                sum *= sum;
                sum *= sum;
                var grad = _seed.GetGrad(h0, simplexOffset0x, simplexOffset0y, simplexOffset0z, simplexOffset0w);
                total = grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y + simplexOffset1z * simplexOffset1z + simplexOffset1w * simplexOffset1w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h1, simplexOffset1x, simplexOffset1y, simplexOffset1z, simplexOffset1w);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset2x * simplexOffset2x + simplexOffset2y * simplexOffset2y + simplexOffset2z * simplexOffset2z + simplexOffset2w * simplexOffset2w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h2, simplexOffset2x, simplexOffset2y, simplexOffset2z, simplexOffset2w);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset3x * simplexOffset3x + simplexOffset3y * simplexOffset3y + simplexOffset3z * simplexOffset3z + simplexOffset3w * simplexOffset3w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h3, simplexOffset3x, simplexOffset3y, simplexOffset3z, simplexOffset3w);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset4x * simplexOffset4x + simplexOffset4y * simplexOffset4y + simplexOffset4z * simplexOffset4z + simplexOffset4w * simplexOffset4w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = _seed.GetGrad(h4, simplexOffset4y, simplexOffset4z, simplexOffset4w);
                total += grad * sum;
            }
            total = total * sample;
            return total;
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> values, int cellOffsetX = 0, bool aligned = false)
        {
            if (x.Length == 0)
                throw new ArgumentException("数组不能为空。");
            if (x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            using (var px = x.Pin())
            using (var pv = values.Pin())
            {
                if (!Vector.IsHardwareAccelerated)
                {
                    Parallel.For(0, values.Length, i =>
                    {
                        ((float*)pv.Pointer)[i] = Get(((float*)px.Pointer)[i]);
                    });
                    return;
                }

                var count = x.Length;
                if (count % _VectorLength != 0)
                    throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
                var vectorCount = count / _VectorLength;
                var sample = new Vector<float>(SkewValue<float[]>.Sample);

                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Parallel.For(0, vectorCount, c =>
                {
                    var index = c * _VectorLength;
                    Vector<float> vx, sumValueVector;
                    if (aligned)
                        vx = Vector.LoadAlignedNonTemporal((float*)px.Pointer + index);
                    else
                        vx = Vector.Load((float*)px.Pointer + index);

                    Vector<float> cellXFloor = Vector.Floor(vx);
                    Vector<int> cellX = Vector.ConvertToInt32(cellXFloor) + vcellOffsetX;

                    var ox1 = vx - cellXFloor;
                    var ox2 = ox1 - Vector<float>.One;

                    var h0 = _seed.Hash(cellX);
                    var h1 = _seed.Hash(cellX + Vector<int>.One);

                    {
                        var sum = ox1 * ox1;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h0, ox1);
                        sumValueVector = sum;
                    }
                    {
                        var sum = ox2 * ox2;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h1, ox2);
                        sumValueVector += sum;
                    }
                    sumValueVector *= sample;
                    if (aligned)
                        Vector.StoreAlignedNonTemporal(sumValueVector, (float*)pv.Pointer + index);
                    else
                        Vector.Store(sumValueVector, (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, bool aligned = false)
        {
            if (x.Length == 0)
                throw new ArgumentException("数组不能为空。");
            if (x.Length != y.Length || y.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pv = values.Pin())
            {
                if (!Vector.IsHardwareAccelerated)
                {
                    Parallel.For(0, values.Length, i =>
                    {
                        ((float*)pv.Pointer)[i] = Get(((float*)px.Pointer)[i], ((float*)py.Pointer)[i]);
                    });
                    return;
                }

                var count = x.Length;
                if (count % _VectorLength != 0)
                    throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
                //向量个数
                var vectorCount = count / _VectorLength;
                float skewToCell, skewFromCell;
                //原始变形值
                skewToCell = SkewValue<float[,]>.SkewToCell;
                skewFromCell = SkewValue<float[,]>.SkewFromCell;
                //初始化通用向量
                var skewToCellVector = new Vector<float>(skewToCell);
                var sample = new Vector<float>(SkewValue<float[,]>.Sample);
                var vskewFromCell1 = new Vector<float>(skewFromCell);
                var vskewFromCell2 = new Vector<float>(skewFromCell * 2) + Vector<float>.One;

                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);
                //float[] 
                Parallel.For(0, vectorCount, c =>
                {
                    //当前向量组对应的数组索引
                    var index = c * _VectorLength;
                    //将数组中对应的数据转为向量
                    Vector<float> vx, vy, sumValueVector;
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
                    //计算单型到单元格变形值
                    var skew = (skewToCellVector * (vx + vy));
                    //单元格起点坐标
                    var cellX = vx + skew;
                    var cellY = vy + skew;
                    Vector<float> cellXFloor, cellYFloor;
                    Vector<int> cellXFloorInt, cellYFloorInt;
                    //单元格原点坐标

                    cellXFloor = Vector.Floor(cellX);
                    cellYFloor = Vector.Floor(cellY);
                    cellXFloorInt = Vector.ConvertToInt32(cellXFloor) + vcellOffsetX;
                    cellYFloorInt = Vector.ConvertToInt32(cellYFloor) + vcellOffsetY;
                    //计算单元格第二个点的位置
                    var xOffset = cellX - cellXFloor;
                    var yOffset = cellY - cellYFloor;
                    var compareResult = Vector.GreaterThanOrEqual(xOffset, yOffset);
                    var xOffsetInt = Vector.ConditionalSelect(compareResult, Vector<int>.One, Vector<int>.Zero);
                    var yOffsetInt = Vector.ConditionalSelect(compareResult, Vector<int>.Zero, Vector<int>.One);

                    //单元格原点转换为单型原点差值
                    skew = vskewFromCell1 * (cellXFloor + cellYFloor);
                    //单元格原点在单型里的坐标
                    var simplexXFloor = cellXFloor - skew;
                    var simplexYFloor = cellYFloor - skew;
                    //输入点到单元格起点坐标差值
                    var ox1 = vx - simplexXFloor;
                    var oy1 = vy - simplexYFloor;
                    //输入点到单元格第二个坐标点差值
                    var ox2 = ox1 - Vector.ConvertToSingle(xOffsetInt) + vskewFromCell1;
                    var oy2 = oy1 - Vector.ConvertToSingle(yOffsetInt) + vskewFromCell1;
                    //输入点到单元格终点坐标差值
                    var ox3 = ox1 - vskewFromCell2;
                    var oy3 = oy1 - vskewFromCell2;

                    //计算梯度
                    var h0 = _seed.Hash(_seed.Hash(cellXFloorInt) + cellYFloorInt);
                    var h1 = _seed.Hash(_seed.Hash(cellXFloorInt + xOffsetInt) + cellYFloorInt + yOffsetInt);
                    var h2 = _seed.Hash(_seed.Hash(cellXFloorInt + Vector<int>.One) + cellYFloorInt + Vector<int>.One);

                    //顶点0
                    {
                        //x
                        var sum = ox1 * ox1;
                        //y
                        sum += oy1 * oy1;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h0, ox1, oy1);
                        sumValueVector = sum;
                    }
                    {
                        //x
                        var sum = ox2 * ox2;
                        //y
                        sum += oy2 * oy2;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h1, ox2, oy2);
                        sumValueVector += sum;
                    }
                    {
                        //x
                        var sum = ox3 * ox3;
                        //y
                        sum += oy3 * oy3;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h2, ox3, oy3);
                        sumValueVector += sum;
                    }
                    sumValueVector *= sample;
                    if (aligned)
                        Vector.StoreAlignedNonTemporal(sumValueVector, (float*)pv.Pointer + index);
                    else
                        Vector.Store(sumValueVector, (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, bool aligned = false)
        {
            if (x.Length == 0)
                throw new ArgumentException("数组不能为空。");
            if (x.Length != y.Length || x.Length != z.Length || x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pz = y.Pin())
            using (var pv = values.Pin())
            {
                if (!Vector.IsHardwareAccelerated)
                {
                    Parallel.For(0, values.Length, i =>
                    {
                        ((float*)pv.Pointer)[i] = Get(((float*)px.Pointer)[i], ((float*)py.Pointer)[i], ((float*)pz.Pointer)[i]);
                    });
                    return;
                }

                var count = x.Length;
                if (count % _VectorLength != 0)
                    throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
                //向量个数
                var vectorCount = count / _VectorLength;
                float skewToCell, skewFromCell;
                skewToCell = SkewValue<float[,,]>.SkewToCell;
                skewFromCell = SkewValue<float[,,]>.SkewFromCell;
                var skewToCellVector = new Vector<float>(skewToCell);
                var sample = new Vector<float>(SkewValue<float[,,]>.Sample);
                var v2 = new Vector<float>(2f);
                var v1 = Vector<float>.One;
                var vskewFromCell1 = new Vector<float>(skewFromCell);
                var vskewFromCell2 = new Vector<float>(skewFromCell * 2);
                var vskewFromCell3 = new Vector<float>(skewFromCell * 3);

                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);
                Vector<int> vcellOffsetZ = new Vector<int>(cellOffsetZ);
                //float[] 
                Parallel.For(0, vectorCount, c =>
                {
                    var index = c * _VectorLength;
                    //将数组中对应的数据转为向量
                    Vector<float> vx, vy, vz, sumValueVector;
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
                    //计算单型到单元格变形值
                    var skew = (skewToCellVector * (vx + vy + vz));
                    //单元格坐标
                    var cellX = vx + skew;
                    var cellY = vy + skew;
                    var cellZ = vz + skew;
                    Vector<float> cellXFloor, cellYFloor, cellZFloor;
                    Vector<int> cellXFloorInt, cellYFloorInt, cellZFloorInt;
                    //单元格原点坐标
                    cellXFloor = Vector.Floor(cellX);
                    cellYFloor = Vector.Floor(cellY);
                    cellZFloor = Vector.Floor(cellZ);
                    cellXFloorInt = Vector.ConvertToInt32(cellXFloor) + vcellOffsetX;
                    cellYFloorInt = Vector.ConvertToInt32(cellYFloor) + vcellOffsetY;
                    cellZFloorInt = Vector.ConvertToInt32(cellZFloor) + vcellOffsetZ;
                    var xOffset = cellX - cellXFloor;
                    var yOffset = cellY - cellYFloor;
                    var zOffset = cellZ - cellZFloor;
                    Vector<int> xRange, yRange, zRange;
                    var compareResult = Vector.GreaterThanOrEqual(xOffset, yOffset);
                    xRange = Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    yRange = Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(xOffset, zOffset);
                    xRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    zRange = Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(yOffset, zOffset);
                    yRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    zRange += Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    var xOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(xRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var yOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(yRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var zOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(zRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var xOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(xRange, _Int1Vector), _Int1Vector, _Int0Vector);
                    var yOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(yRange, _Int1Vector), _Int1Vector, _Int0Vector);
                    var zOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(zRange, _Int1Vector), _Int1Vector, _Int0Vector);

                    skew = vskewFromCell1 * (cellXFloor + cellYFloor + cellZFloor);
                    var simplexXFloor = cellXFloor - skew;
                    var simplexYFloor = cellYFloor - skew;
                    var simplexZFloor = cellZFloor - skew;
                    var ox1 = vx - simplexXFloor;
                    var oy1 = vy - simplexYFloor;
                    var oz1 = vz - simplexZFloor;
                    var ox2 = ox1 - Vector.ConvertToSingle(xOffset1Int) + vskewFromCell1;
                    var oy2 = oy1 - Vector.ConvertToSingle(yOffset1Int) + vskewFromCell1;
                    var oz2 = oz1 - Vector.ConvertToSingle(zOffset1Int) + vskewFromCell1;
                    var ox3 = ox1 - Vector.ConvertToSingle(xOffset2Int) + vskewFromCell2;
                    var oy3 = oy1 - Vector.ConvertToSingle(yOffset2Int) + vskewFromCell2;
                    var oz3 = oz1 - Vector.ConvertToSingle(zOffset2Int) + vskewFromCell2;
                    var ox4 = ox1 - v1 + vskewFromCell3;
                    var oy4 = oy1 - v1 + vskewFromCell3;
                    var oz4 = oz1 - v1 + vskewFromCell3;

                    var h0 = _seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt) + cellYFloorInt) + cellZFloorInt);
                    var h1 = _seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + xOffset1Int) + cellYFloorInt + yOffset1Int) + cellZFloorInt + zOffset1Int);
                    var h2 = _seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + xOffset2Int) + cellYFloorInt + yOffset2Int) + cellZFloorInt + zOffset2Int);
                    var h3 = _seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + Vector<int>.One) + cellYFloorInt + Vector<int>.One) + cellZFloorInt + Vector<int>.One);

                    //顶点0
                    {
                        var sum = ox1 * ox1;
                        sum += oy1 * oy1;
                        sum += oz1 * oz1;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h0, ox1, oy1, oz1);
                        sumValueVector = sum;
                    }
                    {
                        var sum = ox2 * ox2;
                        sum += oy2 * oy2;
                        sum += oz2 * oz2;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h1, ox2, oy2, oz2);
                        sumValueVector += sum;
                    }
                    {
                        var sum = ox3 * ox3;
                        sum += oy3 * oy3;
                        sum += oz3 * oz3;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h2, ox3, oy3, oz3);
                        sumValueVector += sum;
                    }
                    {
                        var sum = ox4 * ox4;
                        sum += oy4 * oy4;
                        sum += oz4 * oz4;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h3, ox4, oy4, oz4);
                        sumValueVector += sum;
                    }
                    sumValueVector *= sample;
                    if (aligned)
                        Vector.StoreAlignedNonTemporal(sumValueVector, (float*)pv.Pointer + index);
                    else
                        Vector.Store(sumValueVector, (float*)pv.Pointer + index);
                });
            }
        }

        public unsafe void GetRange(Memory<float> x, Memory<float> y, Memory<float> z, Memory<float> w, Memory<float> values, int cellOffsetX = 0, int cellOffsetY = 0, int cellOffsetZ = 0, int cellOffsetW = 0, bool aligned = false)
        {
            if (x.Length == 0)
                throw new ArgumentException("数组不能为空。");
            if (x.Length != y.Length || x.Length != z.Length || x.Length != w.Length || x.Length != values.Length)
                throw new ArgumentException("数组长度必须一致。");

            using (var px = x.Pin())
            using (var py = y.Pin())
            using (var pz = y.Pin())
            using (var pw = w.Pin())
            using (var pv = values.Pin())
            {
                if (!Vector.IsHardwareAccelerated)
                {
                    Parallel.For(0, values.Length, i =>
                    {
                        ((float*)pv.Pointer)[i] = Get(((float*)px.Pointer)[i], ((float*)py.Pointer)[i], ((float*)pz.Pointer)[i], ((float*)pw.Pointer)[i]);
                    });
                    return;
                }

                var count = x.Length;
                if (count % _VectorLength != 0)
                    throw new ArgumentException("数组长度必须是Vector<float>.Count的倍数。");
                var vectorCount = (int)Math.Ceiling(count / (double)_VectorLength);
                float skewToCell, skewFromCell;
                skewToCell = SkewValue<float[,,,]>.SkewToCell;
                skewFromCell = SkewValue<float[,,,]>.SkewFromCell;
                var skewToCellVector = new Vector<float>(skewToCell);
                var sample = new Vector<float>(SkewValue<float[,,,]>.Sample);
                var v2 = new Vector<float>(2f);
                var v1 = Vector<float>.One;
                var vskewFromCell1 = new Vector<float>(skewFromCell);
                var vskewFromCell2 = new Vector<float>(skewFromCell * 2);
                var vskewFromCell3 = new Vector<float>(skewFromCell * 3);
                var vskewFromCell4 = new Vector<float>(skewFromCell * 4);

                Vector<int> vcellOffsetX = new Vector<int>(cellOffsetX);
                Vector<int> vcellOffsetY = new Vector<int>(cellOffsetY);
                Vector<int> vcellOffsetZ = new Vector<int>(cellOffsetZ);
                Vector<int> vcellOffsetW = new Vector<int>(cellOffsetW);
                //float[]
                Parallel.For(0, vectorCount, c =>
                {
                    var index = c * _VectorLength;
                    Vector<float> vx, vy, vz, vw, sumValueVector;
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
                    //计算单型到单元格变形值
                    var skew = (skewToCellVector * (vx + vy + vz + vw));
                    //单元格坐标
                    var cellX = vx + skew;
                    var cellY = vy + skew;
                    var cellZ = vz + skew;
                    var cellW = vw + skew;
                    Vector<float> cellXFloor, cellYFloor, cellZFloor, cellWFloor;
                    Vector<int> cellXFloorInt, cellYFloorInt, cellZFloorInt, cellWFloorInt;
                    //单元格原点坐标
                    cellXFloor = Vector.Floor(cellX);
                    cellYFloor = Vector.Floor(cellY);
                    cellZFloor = Vector.Floor(cellZ);
                    cellWFloor = Vector.Floor(cellW);
                    cellXFloorInt = Vector.ConvertToInt32(cellXFloor) + vcellOffsetX;
                    cellYFloorInt = Vector.ConvertToInt32(cellYFloor) + vcellOffsetY;
                    cellZFloorInt = Vector.ConvertToInt32(cellZFloor) + vcellOffsetZ;
                    cellWFloorInt = Vector.ConvertToInt32(cellWFloor) + vcellOffsetW;
                    var xOffset = cellX - cellXFloor;
                    var yOffset = cellY - cellYFloor;
                    var zOffset = cellZ - cellZFloor;
                    var wOffset = cellW - cellWFloor;
                    Vector<int> xRange, yRange, zRange, wRange;
                    var compareResult = Vector.GreaterThanOrEqual(xOffset, yOffset);
                    xRange = Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    yRange = Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(xOffset, zOffset);
                    xRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    zRange = Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(xOffset, wOffset);
                    xRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    wRange = Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(yOffset, zOffset);
                    yRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    zRange += Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(yOffset, wOffset);
                    yRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    wRange += Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    compareResult = Vector.GreaterThanOrEqual(zOffset, wOffset);
                    zRange += Vector.ConditionalSelect(compareResult, _Int1Vector, _Int0Vector);
                    wRange += Vector.ConditionalSelect(compareResult, _Int0Vector, _Int1Vector);
                    var xOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(xRange, _Int3Vector), _Int1Vector, _Int0Vector);
                    var yOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(yRange, _Int3Vector), _Int1Vector, _Int0Vector);
                    var zOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(zRange, _Int3Vector), _Int1Vector, _Int0Vector);
                    var wOffset1Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(wRange, _Int3Vector), _Int1Vector, _Int0Vector);
                    var xOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(xRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var yOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(yRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var zOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(zRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var wOffset2Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(wRange, _Int2Vector), _Int1Vector, _Int0Vector);
                    var xOffset3Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(xRange, _Int1Vector), _Int1Vector, _Int0Vector);
                    var yOffset3Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(yRange, _Int1Vector), _Int1Vector, _Int0Vector);
                    var zOffset3Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(zRange, _Int1Vector), _Int1Vector, _Int0Vector);
                    var wOffset3Int = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(wRange, _Int1Vector), _Int1Vector, _Int0Vector);

                    skew = vskewFromCell1 * (cellXFloor + cellYFloor + cellZFloor + cellWFloor);
                    var simplexXFloor = cellXFloor - skew;
                    var simplexYFloor = cellYFloor - skew;
                    var simplexZFloor = cellZFloor - skew;
                    var simplexWFloor = cellWFloor - skew;
                    var ox1 = vx - simplexXFloor;
                    var oy1 = vy - simplexYFloor;
                    var oz1 = vz - simplexZFloor;
                    var ow1 = vw - simplexWFloor;
                    var ox2 = ox1 - Vector.ConvertToSingle(xOffset1Int) + vskewFromCell1;
                    var oy2 = oy1 - Vector.ConvertToSingle(yOffset1Int) + vskewFromCell1;
                    var oz2 = oz1 - Vector.ConvertToSingle(zOffset1Int) + vskewFromCell1;
                    var ow2 = ow1 - Vector.ConvertToSingle(wOffset1Int) + vskewFromCell1;
                    var ox3 = ox1 - Vector.ConvertToSingle(xOffset2Int) + vskewFromCell2;
                    var oy3 = oy1 - Vector.ConvertToSingle(yOffset2Int) + vskewFromCell2;
                    var oz3 = oz1 - Vector.ConvertToSingle(zOffset2Int) + vskewFromCell2;
                    var ow3 = ow1 - Vector.ConvertToSingle(wOffset2Int) + vskewFromCell2;
                    var ox4 = ox1 - Vector.ConvertToSingle(xOffset3Int) + vskewFromCell3;
                    var oy4 = oy1 - Vector.ConvertToSingle(yOffset3Int) + vskewFromCell3;
                    var oz4 = oz1 - Vector.ConvertToSingle(zOffset3Int) + vskewFromCell3;
                    var ow4 = ow1 - Vector.ConvertToSingle(wOffset3Int) + vskewFromCell3;
                    var ox5 = ox1 - v1 + vskewFromCell4;
                    var oy5 = oy1 - v1 + vskewFromCell4;
                    var oz5 = oz1 - v1 + vskewFromCell4;
                    var ow5 = ow1 - v1 + vskewFromCell4;

                    var h0 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt) + cellYFloorInt) + cellZFloorInt) + cellWFloorInt);
                    var h1 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + xOffset1Int) + cellYFloorInt + yOffset1Int) + cellZFloorInt + zOffset1Int) + cellWFloorInt + wOffset1Int);
                    var h2 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + xOffset2Int) + cellYFloorInt + yOffset2Int) + cellZFloorInt + zOffset2Int) + cellWFloorInt + wOffset2Int);
                    var h3 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + xOffset3Int) + cellYFloorInt + yOffset3Int) + cellZFloorInt + zOffset3Int) + cellWFloorInt + wOffset3Int);
                    var h4 = _seed.Hash(_seed.Hash(_seed.Hash(_seed.Hash(cellXFloorInt + Vector<int>.One) + cellYFloorInt + Vector<int>.One) + cellZFloorInt + Vector<int>.One) + cellWFloorInt + Vector<int>.One);
                    //顶点0
                    {
                        var sum = ox1 * ox1;
                        sum += oy1 * oy1;
                        sum += oz1 * oz1;
                        sum += ow1 * ow1;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h0, ox1, oy1, oz1, ow1);
                        sumValueVector = sum;
                    }
                    {
                        var sum = ox2 * ox2;
                        sum += oy2 * oy2;
                        sum += oz2 * oz2;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h1, ox2, oy2, oz2, ow2);
                        sumValueVector += sum;
                    }
                    {
                        var sum = ox3 * ox3;
                        sum += oy3 * oy3;
                        sum += oz3 * oz3;
                        sum += ow3 * ow3;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h2, ox3, oy3, oz3, ow3);
                        sumValueVector += sum;
                    }
                    {
                        var sum = ox4 * ox4;
                        sum += oy4 * oy4;
                        sum += oz4 * oz4;
                        sum += ow4 * ow4;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h3, ox4, oy4, oz4, ow4);
                        sumValueVector += sum;
                    }
                    {
                        var sum = ox5 * ox5;
                        sum += oy5 * oy5;
                        sum += oz5 * oz5;
                        sum += ow5 * ow5;
                        sum = _HalfVector - sum;
                        sum = Vector.Max(Vector<float>.Zero, sum);
                        sum *= sum;
                        sum *= sum;
                        sum *= _seed.GetGrad(h4, ox5, oy5, oz5, ow5);
                        sumValueVector += sum;
                    }
                    sumValueVector *= sample;
                    if (aligned)
                        Vector.StoreAlignedNonTemporal(sumValueVector, (float*)pv.Pointer + index);
                    else
                        Vector.Store(sumValueVector, (float*)pv.Pointer + index);
                });
            }
        }
        public class SkewValue<T>
        {
            static SkewValue()
            {
                var rank = typeof(T).GetArrayRank();
                SkewToCell = (MathF.Sqrt(rank + 1) - 1) / rank;
                SkewFromCell = (1 - 1 / MathF.Sqrt(rank + 1)) / rank;
                //                float l = 0;
                //                for (int i = 0; i < rank; i++)
                //                {
                //                    var v = (i == 0 ? 1 : 0) - SkewFromCell;
                //                    l += v * v;
                //                }
                //#if NETSTANDARD2_0
                //                var length = (float)Math.Sqrt(l);
                //#else
                //                var length = MathF.Sqrt(l);
                //#endif
                float[] cellPosition = new float[rank];
                //float[] cell
                cellPosition[0] = 0.5f;
                //计算最大值坐标
                //for (int i = 1; i < rank; i++)
                //{
                //    for (int ii = 0; ii < i; ii++)
                //    {
                //        cellPosition[ii] = cellPosition[ii] + (1 - cellPosition[ii]) / (i + 1);
                //    }
                //}
                //for (int i = 0; i < rank; i++)
                //{
                //    for (int ii = 0; ii <= i; ii++)
                //    {
                //        cellPosition[ii] = cellPosition[ii] + (1 - cellPosition[ii]) / (i + 2);
                //    }
                //}
                var skewFromCell = SkewFromCell * cellPosition.Sum();
                float[] position = new float[rank];
                var sum = 0f;
                for (int i = 0; i < rank; i++)
                {
                    position[i] = cellPosition[i] - skewFromCell;
                    sum += position[i];
                }
                var skewFromOrigin = SkewToCell * sum;
                int[,] cellGradOffsets = new int[rank, rank];
                //计算各个顶点的单元格坐标向量
                for (int i = 0; i < rank; i++)
                {
                    for (int ii = 0; ii < rank; ii++)
                    {
                        cellGradOffsets[i, ii] = ii <= i ? 1 : 0;
                    }
                }
                int[,] gradPositions = new int[rank + 1, rank];
                float[,] simplexOffset = new float[rank + 1, rank];
                for (int i = 0; i < rank; i++)
                {
                    simplexOffset[0, i] = position[i];
                    gradPositions[0, i] = (int)MathF.Floor(cellPosition[i]);
                }
                for (int i = 1; i <= rank; i++)
                {
                    for (int ii = 0; ii < rank; ii++)
                    {
                        simplexOffset[i, ii] = position[ii] - cellGradOffsets[i - 1, ii] + i * SkewFromCell;
                        gradPositions[i, ii] = gradPositions[0, ii] + cellGradOffsets[i - 1, ii];
                    }
                }
                float sqrt = MathF.Sqrt(rank);
                float max = 0f;
                float l;
                for (int i = 0; i <= rank; i++)
                {
                    //距离的平方
                    sum = 0f;
                    //var g = 0f;
                    for (var ii = 0; ii < rank; ii++)
                    {
                        var offset = simplexOffset[i, ii];
                        sum += offset * offset;
                    }
                    l = sum;
                    sum = 0.5f - sum;
                    //if (sum < 0)
                    //    sum = 0;
                    sum = sum * sum * sum * sum * (float)Math.Sqrt(l) * sqrt;
                    max += sum;
                    //if (sum > max)
                    //    max = sum;
                }
                Sample = 1 / max;
            }

            public static readonly float SkewToCell;
            public static readonly float SkewFromCell;
            public static readonly float Sample;
        }
    }
}
