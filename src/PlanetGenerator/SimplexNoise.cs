using System;
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
    public class SimplexNoise : INoise
    {
        private static readonly int _VectorLength, _VectorLength2, _VectorLength3, _VectorLength4;
        private static readonly Vector<int> _Int0Vector, _Int1Vector, _Int2Vector, _Int3Vector;
        static SimplexNoise()
        {
            if (Vector.IsHardwareAccelerated)
            {
                _VectorLength = Vector<float>.Count;
                _VectorLength2 = _VectorLength * 2;
                _VectorLength3 = _VectorLength * 3;
                _VectorLength4 = _VectorLength * 4;
                _Int0Vector = Vector<int>.Zero;
                _Int1Vector = Vector<int>.One;
                _Int2Vector = new Vector<int>(2);
                _Int3Vector = new Vector<int>(3);
            }
        }

        private byte[] _perm;
        private float[] _permFloat;

        public SimplexNoise(int seed)
        {
            _perm = new byte[256];
            Random r = new Random(seed);
            r.NextBytes(_perm);
            _permFloat = new float[256];
            for (int i = 0; i < 256; i++)
                _permFloat[i] = (_perm[i] / 255f - 0.5f) * 2f;
        }

        public SimplexNoise(byte[] seeds)
        {
            if (seeds == null)
                throw new ArgumentNullException(nameof(seeds));
            if (seeds.Length == 0)
                throw new ArgumentException("Seeds must have value.");
            _perm = seeds;
            _permFloat = new float[256];
            for (int i = 0; i < 256; i++)
                _permFloat[i] = (_perm[i] / 255f - 0.5f) * 2f;
        }

        protected float[] GetPermFloat() => _permFloat;
        protected byte[] GetPerm() => _perm;

        public virtual float Get(params float[] positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));
            if (positions.Length == 0)
                throw new ArgumentException("Dimension must large than zero.");
            float sum = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                sum += positions[i];
            }
            float skewToCell, skewFromCell, sample;
            switch (positions.Length)
            {
                case 1:
                    skewToCell = SkewValue<float[]>.SkewToCell;
                    skewFromCell = SkewValue<float[]>.SkewFromCell;
                    sample = SkewValue<float[]>.Sample;
                    break;
                case 2:
                    skewToCell = SkewValue<float[,]>.SkewToCell;
                    skewFromCell = SkewValue<float[,]>.SkewFromCell;
                    sample = SkewValue<float[,]>.Sample;
                    break;
                case 3:
                    skewToCell = SkewValue<float[,,]>.SkewToCell;
                    skewFromCell = SkewValue<float[,,]>.SkewFromCell;
                    sample = SkewValue<float[,,]>.Sample;
                    break;
                case 4:
                    skewToCell = SkewValue<float[,,,]>.SkewToCell;
                    skewFromCell = SkewValue<float[,,,]>.SkewFromCell;
                    sample = SkewValue<float[,,,]>.Sample;
                    break;
                case 5:
                    skewToCell = SkewValue<float[,,,,]>.SkewToCell;
                    skewFromCell = SkewValue<float[,,,,]>.SkewFromCell;
                    sample = SkewValue<float[,,,,]>.Sample;
                    break;
                case 6:
                    skewToCell = SkewValue<float[,,,,,]>.SkewToCell;
                    skewFromCell = SkewValue<float[,,,,,]>.SkewFromCell;
                    sample = SkewValue<float[,,,,,]>.Sample;
                    break;
                default:
                    var type = typeof(SkewValue<>).MakeGenericType(typeof(float).MakeArrayType(positions.Length));
                    skewToCell = (float)type.GetField("SkewTo", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                    skewFromCell = (float)type.GetField("SkewFrom", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                    sample = (float)type.GetField("Sample", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                    break;
            }
            float skewToOrigin = skewToCell;
            float skewFromOrigin = skewFromCell;
            skewToCell *= sum;
            //变形后的单元格坐标
            float[] cellPosition = new float[positions.Length];
            //变形后的单元格坐标原点
            int[] cellFloor = new int[positions.Length];
            //未变形的单元格坐标原点
            float[] simplexFloors = new float[positions.Length];
            //单元格原点总和
            sum = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                //单元格坐标
                cellPosition[i] = positions[i] + skewToCell;
                //单元格坐标原点
                cellFloor[i] = (int)MathF.Floor(cellPosition[i]);
                //单元格原点总和
                sum += cellFloor[i];
            }
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            for (int i = 0; i < positions.Length; i++)
            {
                //单元格原点还原为单型坐标
                simplexFloors[i] = cellFloor[i] - skewFromOrigin;
            }
            //单元格偏差向量
            int[,] cellGradOffsets = new int[positions.Length, positions.Length];
            //单元格坐标与单元格原点的差值
            float[] offsetOrigin = new float[positions.Length];
            //单型坐标与各个顶点的距离
            float[,] simplexOffset = new float[positions.Length + 1, positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                //计算单型坐标与单型原点的距离
                simplexOffset[0, i] = positions[i] - simplexFloors[i];
                //计算与单元格原点的距离
                offsetOrigin[i] = cellPosition[i] - cellFloor[i];
            }
            //计算各个顶点的单元格坐标向量
            for (int i = 0; i < positions.Length; i++)
            {
                var max = -1f;
                var index = -1;
                for (int ii = 0; ii < offsetOrigin.Length; ii++)
                {
                    if (offsetOrigin[ii] > max)
                    {
                        max = offsetOrigin[ii];
                        index = ii;
                    }
                }
                cellGradOffsets[i, index] = 1;
                for (int ii = i + 1; ii < positions.Length; ii++)
                {
                    cellGradOffsets[ii, index] = 1;
                }
                offsetOrigin[index] = -1;
            }
            //计算单型各个顶点坐标
            for (int i = 1; i <= positions.Length; i++)
            {
                for (int ii = 0; ii < positions.Length; ii++)
                {
                    simplexOffset[i, ii] = simplexOffset[0, ii] - cellGradOffsets[i - 1, ii] + i * skewFromCell;
                }
            }
            float total = 0f;
            //计算各个顶点梯度值
            for (int i = 0; i <= positions.Length; i++)
            {
                //单型顶点坐标
                var p = new int[positions.Length];
                var o = new float[positions.Length];
                //距离的平方
                sum = 0f;
                for (var ii = 0; ii < positions.Length; ii++)
                {
                    //单元格顶点坐标
                    p[ii] = cellFloor[ii] + (i == 0 ? 0 : cellGradOffsets[i - 1, ii]);
                    //单型坐标与顶点的距离
                    var offset = simplexOffset[i, ii];
                    //与单元格顶点坐标的距离
                    o[ii] = offset;
                    sum += offset * offset;
                }
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                //计算单元格的梯度值
                var grad = Grad(p, o);
                total += grad * sum * sum * sum * sum;
            }
            total = total * sample;
            if (total < -1)
                total = -1;
            else if (total > 1)
                total = 1;
            return total;
        }

        public virtual float Get(float x)
        {
            var floorX = Floor(x);
            var offset1 = x - floorX;
            var offset2 = offset1 - 1;
            var floorXInt = (int)floorX * PrimeX;
            var g1 = 1f - offset1 * offset1;
            g1 = g1 * g1;
            g1 = g1 * g1 * Grad(floorXInt, offset1);
            var g2 = 1f - offset2 * offset2;
            g2 = g2 * g2;
            g2 = g2 * g2 * Grad(floorXInt + PrimeX, offset2);
            return (g1 + g2) * 0.395f;
        }

        public virtual float Get(float x, float y)
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
            var cellFloorX = Floor(cellPositionX);
            var cellFloorY = Floor(cellPositionY);
            //单元格起点坐标
            var cellFloorXInt = (int)cellFloorX;
            var cellFloorYInt = (int)cellFloorY;
            sum = cellFloorX + cellFloorY;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            //单元格原点在单型里的坐标
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;

            //输入点到单元格起点坐标差值
            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;

            //计算单元格第二个点的位置
            int cellOffset1x, cellOffset1y;
            if (cellPositionX - cellFloorX >= cellPositionY - cellFloorY)
            {
                cellOffset1x = 1;
                cellOffset1y = 0;
            }
            else
            {
                cellOffset1x = 0;
                cellOffset1y = 1;
            }
            //输入点到单型第二个点坐标的差值
            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            //输入点到单型终点坐标的差值
            var simplexOffset2x = simplexOffset0x - 1 + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - 1 + 2 * skewFromCell;

            float total = 0f;

            cellFloorXInt *= PrimeX;
            cellFloorYInt *= PrimeY;
            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(cellFloorXInt, cellFloorYInt, simplexOffset0x, simplexOffset0y);
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
                var grad = Grad(cellFloorXInt + cellOffset1x * PrimeX, cellFloorYInt + cellOffset1y * PrimeY, simplexOffset1x, simplexOffset1y);
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
                var grad = Grad(cellFloorXInt + PrimeX, cellFloorYInt + PrimeY, simplexOffset2x, simplexOffset2y);
                total += grad * sum;
            }
            total = total * sample;
            if (total < -1)
                total = -1;
            else if (total > 1)
                total = 1;
            return total;
        }

        public virtual float Get(float x, float y, float z)
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
            var cellFloorX = Floor(cellPositionX);
            var cellFloorY = Floor(cellPositionY);
            var cellFloorZ = Floor(cellPositionZ);
            var cellFloorXInt = (int)cellFloorX;
            var cellFloorYInt = (int)cellFloorY;
            var cellFloorZInt = (int)cellFloorZ;
            sum = cellFloorX + cellFloorY + cellFloorZ;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;
            var simplexFloorZ = cellFloorZ - skewFromOrigin;

            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;
            var simplexOffset0z = z - simplexFloorZ;

            var ox = cellPositionX - cellFloorX;
            var oy = cellPositionY - cellFloorY;
            var oz = cellPositionZ - cellFloorZ;
            int rankx = 0,
                ranky = 0,
                rankz = 0;
            if (ox > oy) rankx++; else ranky++;
            if (ox > oz) rankx++; else rankz++;
            if (oy > oz) ranky++; else rankz++;

            int cellOffset1x = rankx >= 2 ? 1 : 0, cellOffset1y = ranky >= 2 ? 1 : 0, cellOffset1z = rankz >= 2 ? 1 : 0,
                cellOffset2x = rankx >= 1 ? 1 : 0, cellOffset2y = ranky >= 1 ? 1 : 0, cellOffset2z = rankz >= 1 ? 1 : 0;

            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            var simplexOffset1z = simplexOffset0z - cellOffset1z + skewFromCell;
            var simplexOffset2x = simplexOffset0x - cellOffset2x + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - cellOffset2y + 2 * skewFromCell;
            var simplexOffset2z = simplexOffset0z - cellOffset2z + 2 * skewFromCell;
            var simplexOffset3x = simplexOffset0x - 1 + 3 * skewFromCell;
            var simplexOffset3y = simplexOffset0y - 1 + 3 * skewFromCell;
            var simplexOffset3z = simplexOffset0z - 1 + 3 * skewFromCell;

            float total = 0f;

            cellFloorXInt *= PrimeX;
            cellFloorYInt *= PrimeY;
            cellFloorZInt *= PrimeZ;
            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y + simplexOffset0z * simplexOffset0z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                //计算单元格的梯度值
                sum *= sum;
                sum *= sum;
                var grad = Grad(cellFloorXInt, cellFloorYInt, cellFloorZInt, simplexOffset0x, simplexOffset0y, simplexOffset0z);
                total += grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y + simplexOffset1z * simplexOffset1z;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(cellFloorXInt + cellOffset1x * PrimeX, cellFloorYInt + cellOffset1y * PrimeY, cellFloorZInt + cellOffset1z * PrimeZ, simplexOffset1x, simplexOffset1y, simplexOffset1z);
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
                var grad = Grad(cellFloorXInt + cellOffset2x * PrimeX, cellFloorYInt + cellOffset2y * PrimeY, cellFloorZInt + cellOffset2z * PrimeZ, simplexOffset2x, simplexOffset2y, simplexOffset2z);
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
                var grad = Grad(cellFloorXInt + PrimeX, cellFloorYInt + PrimeY, cellFloorZInt + PrimeZ, simplexOffset3x, simplexOffset3y, simplexOffset3z);
                total += grad * sum;
            }
            total = total * sample;
            if (total < -1)
                total = -1;
            else if (total > 1)
                total = 1;
            return total;
        }

        public virtual float Get(float x, float y, float z, float w)
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
            var cellFloorX = Floor(cellPositionX);
            var cellFloorY = Floor(cellPositionY);
            var cellFloorZ = Floor(cellPositionZ);
            var cellFloorW = Floor(cellPositionW);
            var cellFloorXInt = (int)cellFloorX;
            var cellFloorYInt = (int)cellFloorY;
            var cellFloorZInt = (int)cellFloorZ;
            var cellFloorWInt = (int)cellFloorW;
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

            float total = 0f;

            cellFloorXInt *= PrimeX;
            cellFloorYInt *= PrimeY;
            cellFloorZInt *= PrimeZ;
            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y + simplexOffset0z * simplexOffset0z + simplexOffset0w * simplexOffset0w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                //计算单元格的梯度值
                sum *= sum;
                sum *= sum;
                var grad = Grad(cellFloorXInt, cellFloorYInt, cellFloorZInt, cellFloorWInt, simplexOffset0x, simplexOffset0y, simplexOffset0z, simplexOffset0w);
                total += grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y + simplexOffset1z * simplexOffset1z + simplexOffset1w * simplexOffset1w;
                sum = 0.5f - sum;
                if (sum < 0)
                    sum = 0;
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(cellFloorXInt + cellOffset1x * PrimeX, cellFloorYInt + cellOffset1y * PrimeY, cellFloorZInt + cellOffset1z * PrimeZ, cellFloorWInt + cellOffset1w * PrimeW, simplexOffset1x, simplexOffset1y, simplexOffset1z, simplexOffset1w);
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
                var grad = Grad(cellFloorXInt + cellOffset2x * PrimeX, cellFloorYInt + cellOffset2y * PrimeY, cellFloorZInt + cellOffset2z * PrimeZ, cellFloorWInt + cellOffset2w * PrimeW, simplexOffset2x, simplexOffset2y, simplexOffset2z, simplexOffset2w);
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
                var grad = Grad(cellFloorXInt + cellOffset3x * PrimeX, cellFloorYInt + cellOffset3y * PrimeY, cellFloorZInt + cellOffset3z * PrimeZ, cellFloorWInt + cellOffset3w * PrimeW, simplexOffset3x, simplexOffset3y, simplexOffset3z, simplexOffset3w);
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
                var grad = Grad(cellFloorXInt + PrimeX, cellFloorYInt + PrimeY, cellFloorZInt + PrimeZ, cellFloorWInt + PrimeW, simplexOffset4x, simplexOffset4y, simplexOffset4z, simplexOffset4w);
                total += grad * sum;
            }
            total = total * sample;
            if (total < -1)
                total = -1;
            else if (total > 1)
                total = 1;
            return total;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual ref float HashFloat(in int hash)
        {
            return ref _permFloat[hash];
        }
        private const int _HashG = 0x27d4eb2d;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Grad(int positionX, float offsetX)
        {
            int hash = positionX * _HashG;
            hash ^= hash >> 15;
            hash &= 255;
            return HashFloat(hash) * offsetX;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Grad(int positionX, int positionY, float offsetX, float offsetY)
        {
            int hash = positionX ^ positionY;
            hash *= 0x27d4eb2d;
            hash ^= hash >> 15;
            hash &= 254;
            var fx = HashFloat(hash);
            var fy = HashFloat(hash | 1);
            return fx * offsetX + fy * offsetY;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Grad(int positionX, int positionY, int positionZ, float offsetX, float offsetY, float offsetZ)
        {
            int hash = positionX ^ positionY ^ positionZ;
            hash *= 0x27d4eb2d;
            hash ^= hash >> 15;
            hash &= 253;
            var fx = HashFloat(hash);
            var fy = HashFloat(hash | 1);
            var fz = HashFloat(hash | 2);
            return fx * offsetX + fy * offsetY + fz * offsetZ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Grad(int positionX, int positionY, int positionZ, int positionW, float offsetX, float offsetY, float offsetZ, float offsetW)
        {
            int hash = positionX ^ positionY ^ positionZ ^ positionW;
            hash *= 0x27d4eb2d;
            hash ^= hash >> 15;
            hash &= 252;
            var fx = HashFloat(hash);
            var fy = HashFloat(hash | 1);
            var fz = HashFloat(hash | 2);
            var fw = HashFloat(hash | 3);
            return fx * offsetX + fy * offsetY + fz * offsetZ + fw * offsetW;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Grad(int[] positions, float[] offsets)
        {
            int hash = positions[0];
            for (int i = 1; i < positions.Length; i++)
            {
                hash ^= positions[i];
            }
            hash *= 0x27d4eb2d;
            hash ^= hash >> 15;
            hash &= 256 - positions.Length;
            float grad = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                grad += HashFloat(hash | i) * offsets[i];
            }
            return grad;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float Floor(float value)
        {
            return MathF.Floor(value);
        }

        public virtual float[] GetRange(params float[][] positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));
            if (positions.Length == 0)
                throw new ArgumentException("Dimension must large than zero.");
            if (positions.Select(t => t.Length).Distinct().Count() > 1)
                throw new ArgumentException("All length of dimension positions array should be same.");
            if (positions[0].Length == 0)
                throw new ArgumentException("Positions could not be empty.");
            switch (positions.Length)
            {
                case 1: return GetRange(positions[0]);
                case 2: return GetRange(positions[0], positions[1]);
                case 3: return GetRange(positions[0], positions[1], positions[2]);
                case 4: return GetRange(positions[0], positions[1], positions[2], positions[3]);
            }
            if (!Vector.IsHardwareAccelerated)
            {
                var d = positions.Length;
                var values = new float[positions[0].Length];
                Parallel.For(0, values.Length, c =>
                {
                    var p = new float[d];
                    for (int i = 0; i < d; i++)
                        p[i] = positions[c][i];
                    values[c] = Get(p);
                });
                return values;
            }
            return null;
        }

        public virtual float[] GetRange(float[] x)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (x.Length == 0)
                throw new ArgumentException("Positions could not be empty.");
            if (!Vector.IsHardwareAccelerated)
            {
                var v = new float[x.Length];
                Parallel.For(0, v.Length, i =>
                {
                    v[i] = Get(x[i]);
                });
                return v;
            }

            var count = x.Length;
            var vectorCount = (int)Math.Ceiling(count / (double)_VectorLength);
            var adjust = count % _VectorLength;
            if (adjust > 0)
            {
                //对齐向量长度
                Array.Resize(ref x, count + _VectorLength - adjust);
                count = x.Length;
            }
            var values = new float[count];
            var sample = new Vector<float>(SkewValue<float[]>.Sample);
            Parallel.For(0, vectorCount, c =>
            {
                var index = c * _VectorLength;
                var pxSpan = MemoryMarshal.Cast<float, Vector<float>>(x);
                ref var px = ref pxSpan[c];
                var valuesSpan = MemoryMarshal.Cast<float, Vector<float>>(values);
                ref var sumValueVector = ref valuesSpan[c];

                Vector<float> cellXFloor;
                Vector<int> cellXFloorInt;
#if NET5_0
                cellXFloor = Vector.Floor(px);
                cellXFloorInt = Vector.ConvertToInt32(cellXFloor);
#else
                var cellXFloorData = new int[_VectorLength];
                for (var i = 0; i < _VectorLength; i++)
                {
                    cellXFloorData[i] = (int)MathF.Floor(x[i]);
                }
                cellXFloorInt = new Vector<int>(cellXFloorData);
                cellXFloor = Vector.ConvertToSingle(cellXFloorInt);
#endif
                var ox1 = px - cellXFloor;
                var ox2 = ox1 - Vector<float>.One;

                Vector<int> cellGradX = cellXFloorInt * _PrimeXVector;
                {
                    var sum = ox1 * ox1;
                    sum = _HalfVector - sum;
                    sum = Vector.Max(Vector<float>.Zero, sum);
                    sum *= sum;
                    sum *= sum;
                    sum *= GradRange(cellGradX, ox1);
                    sumValueVector = sum;
                }
                {
                    var sum = ox2 * ox2;
                    sum = _HalfVector - sum;
                    sum = Vector.Max(Vector<float>.Zero, sum);
                    sum *= sum;
                    sum *= sum;
                    sum *= GradRange(cellGradX + _PrimeXVector, ox2);
                    sumValueVector += sum;
                }
                sumValueVector *= sample;
            });
            return values;
        }

        public unsafe virtual void GetRange(IntPtr x, IntPtr y, IntPtr values, int length)
        {
            //if (x == null)
            //    throw new ArgumentNullException(nameof(x));
            //if (y == null)
            //    throw new ArgumentNullException(nameof(y));
            //if (x.Length != y.Length)
            //    throw new ArgumentException("All length of dimension positions array should be same.");
            //if (x.Length == 0)
            //    throw new ArgumentException("Positions could not be empty.");
            ////没有硬件加速则使用普通并行计算
            //if (!Vector.IsHardwareAccelerated)
            //{
            //    var v = new float[x.Length];
            //    Parallel.For(0, v.Length, i =>
            //    {
            //        v[i] = Get(x[i], y[i]);
            //    });
            //    return v;
            //}
            var count = length;
            ////计算不对齐SIMD长度的数据大小
            //var adjust = count % _VectorLength;
            //if (adjust > 0)
            //{
            //    count = count + _VectorLength - adjust;
            //    //对齐向量长度
            //    Array.Resize(ref x, count);
            //    Array.Resize(ref y, count);
            //}
            //向量个数
            var vectorCount = count / _VectorLength;
            //返回值数组
            //var values = new float[count];
            float skewToCell, skewFromCell;
            //原始变形值
            skewToCell = SkewValue<float[,]>.SkewToCell;
            skewFromCell = SkewValue<float[,]>.SkewFromCell;
            //初始化通用向量
            var skewToCellVector = new Vector<float>(skewToCell);
            var sample = new Vector<float>(SkewValue<float[,]>.Sample);
            var vskewFromCell1 = new Vector<float>(skewFromCell);
            var vskewFromCell2 = new Vector<float>(skewFromCell * 2) + Vector<float>.One;

            //float[] 
            Parallel.For(0, vectorCount, c =>
            {
                //当前向量组对应的数组索引
                var index = c * _VectorLength;
                //将数组中对应的数据转为向量
                //var pxSpan = new Span<Vector<float>>(x, vectorCount);// MemoryMarshal.Cast<float, Vector<float>>(x);
                //var pySpan = new Span<Vector<float>>(y, vectorCount);// MemoryMarshal.Cast<float, Vector<float>>(y);
                ref var px = ref Unsafe.AsRef<Vector<float>>((x + index).ToPointer());//  pxSpan[c];
                ref var py = ref Unsafe.AsRef<Vector<float>>((y + index).ToPointer());//pySpan[c];
                //var valuesSpan = MemoryMarshal.Cast<float, Vector<float>>(values);
                //对应的返回值数组向量
                ref var sumValueVector = ref Unsafe.AsRef<Vector<float>>((values + index).ToPointer());// ref valuesSpan[c];
                //计算单型到单元格变形值
                var skew = (skewToCellVector * (px + py));
                //单元格起点坐标
                var cellX = px + skew;
                var cellY = py + skew;
                Vector<float> cellXFloor, cellYFloor;
                Vector<int> cellXFloorInt, cellYFloorInt;
                //单元格原点坐标

                cellXFloor = Vector.Floor(cellX);
                cellYFloor = Vector.Floor(cellY);
                cellXFloorInt = Vector.ConvertToInt32(cellXFloor);
                cellYFloorInt = Vector.ConvertToInt32(cellYFloor);
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
                var ox1 = px - simplexXFloor;
                var oy1 = py - simplexYFloor;
                //输入点到单元格第二个坐标点差值
                var ox2 = ox1 - Vector.ConvertToSingle(xOffsetInt) + vskewFromCell1;
                var oy2 = oy1 - Vector.ConvertToSingle(yOffsetInt) + vskewFromCell1;
                //输入点到单元格终点坐标差值
                var ox3 = ox1 - vskewFromCell2;
                var oy3 = oy1 - vskewFromCell2;

                //计算梯度
                Vector<int> cellGradX = cellXFloorInt * PrimeX,
                            cellGradY = cellYFloorInt * PrimeY;
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
                    sum *= GradRange(cellGradX, cellGradY, ox1, oy1);
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
                    sum *= GradRange(cellGradX + xOffsetInt * _PrimeXVector, cellGradY + yOffsetInt * _PrimeYVector, ox2, oy2);
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
                    sum *= GradRange(cellGradX + _PrimeXVector, cellGradY + _PrimeYVector, ox3, oy3);
                    sumValueVector += sum;
                }
                sumValueVector *= sample;
            });
        }

        public virtual float[] GetRange(float[] x, float[] y, float[] z)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (z == null)
                throw new ArgumentNullException(nameof(z));
            if (x.Length != y.Length || y.Length != z.Length)
                throw new ArgumentException("All length of dimension positions array should be same.");
            if (x.Length == 0)
                throw new ArgumentException("Positions could not be empty.");
            if (!Vector.IsHardwareAccelerated)
            {
                var v = new float[x.Length];
                Parallel.For(0, v.Length, i =>
                {
                    v[i] = Get(x[i], y[i], z[i]);
                });
                return v;
            }
            var count = x.Length;
            var vectorCount = (int)Math.Ceiling(count / (double)_VectorLength);
            var adjust = count % _VectorLength;
            if (adjust > 0)
            {
                count = count + _VectorLength - adjust;
                //对齐向量长度
                Array.Resize(ref x, count);
                Array.Resize(ref y, count);
                Array.Resize(ref z, count);
            }
            var values = new float[count];
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

            //float[] 
            Parallel.For(0, vectorCount, c =>
            {
                var index = c * _VectorLength;
                var pxSpan = MemoryMarshal.Cast<float, Vector<float>>(x);
                var pySpan = MemoryMarshal.Cast<float, Vector<float>>(y);
                var pzSpan = MemoryMarshal.Cast<float, Vector<float>>(z);
                ref var px = ref pxSpan[c];
                ref var py = ref pySpan[c];
                ref var pz = ref pzSpan[c];
                var valuesSpan = MemoryMarshal.Cast<float, Vector<float>>(values);
                ref var sumValueVector = ref valuesSpan[c];
                //计算单型到单元格变形值
                var skew = (skewToCellVector * (px + py + pz));
                //单元格坐标
                var cellX = px + skew;
                var cellY = py + skew;
                var cellZ = pz + skew;
                Vector<float> cellXFloor, cellYFloor, cellZFloor;
                Vector<int> cellXFloorInt, cellYFloorInt, cellZFloorInt;
                //单元格原点坐标
#if NET5_0
                cellXFloor = Vector.Floor(cellX);
                cellYFloor = Vector.Floor(cellY);
                cellZFloor = Vector.Floor(cellZ);
                cellXFloorInt = Vector.ConvertToInt32(cellXFloor);
                cellYFloorInt = Vector.ConvertToInt32(cellYFloor);
                cellZFloorInt = Vector.ConvertToInt32(cellZFloor);
#else
                var cellXFloorData = new int[_VectorLength];
                var cellYFloorData = new int[_VectorLength];
                var cellZFloorData = new int[_VectorLength];
                for (var i = 0; i < _VectorLength; i++)
                {
                    cellXFloorData[i] = (int)MathF.Floor(cellX[i]);
                    cellYFloorData[i] = (int)MathF.Floor(cellY[i]);
                    cellZFloorData[i] = (int)MathF.Floor(cellZ[i]);
                }
                cellXFloorInt = new Vector<int>(cellXFloorData);
                cellYFloorInt = new Vector<int>(cellYFloorData);
                cellZFloorInt = new Vector<int>(cellZFloorData);
                cellXFloor = Vector.ConvertToSingle(cellXFloorInt);
                cellYFloor = Vector.ConvertToSingle(cellYFloorInt);
                cellZFloor = Vector.ConvertToSingle(cellZFloorInt);
#endif
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
                var ox1 = px - simplexXFloor;
                var oy1 = py - simplexYFloor;
                var oz1 = pz - simplexZFloor;
                var ox2 = ox1 - Vector.ConvertToSingle(xOffset1Int) + vskewFromCell1;
                var oy2 = oy1 - Vector.ConvertToSingle(yOffset1Int) + vskewFromCell1;
                var oz2 = oz1 - Vector.ConvertToSingle(zOffset1Int) + vskewFromCell1;
                var ox3 = ox1 - Vector.ConvertToSingle(xOffset2Int) + vskewFromCell2;
                var oy3 = oy1 - Vector.ConvertToSingle(yOffset2Int) + vskewFromCell2;
                var oz3 = oz1 - Vector.ConvertToSingle(zOffset2Int) + vskewFromCell2;
                var ox4 = ox1 - v1 + vskewFromCell3;
                var oy4 = oy1 - v1 + vskewFromCell3;
                var oz4 = oz1 - v1 + vskewFromCell3;

                Vector<int> cellGradX = cellXFloorInt * _PrimeXVector,
                            cellGradY = cellYFloorInt * _PrimeYVector,
                            cellGradZ = cellZFloorInt * _PrimeZVector;
                //顶点0
                {
                    var sum = ox1 * ox1;
                    sum += oy1 * oy1;
                    sum += oz1 * oz1;
                    sum = _HalfVector - sum;
                    sum = Vector.Max(Vector<float>.Zero, sum);
                    sum *= sum;
                    sum *= sum;
                    sum *= GradRange(cellGradX, cellGradY, cellGradZ, ox1, oy1, oz1);
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
                    sum *= GradRange(cellGradX + xOffset1Int * _PrimeXVector, cellGradY + yOffset1Int * _PrimeYVector, cellGradZ + zOffset1Int * _PrimeZVector, ox2, oy2, oz2);
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
                    sum *= GradRange(cellGradX + xOffset2Int * _PrimeXVector, cellGradY + yOffset2Int * _PrimeYVector, cellGradZ + zOffset2Int * _PrimeZVector, ox3, oy3, oz3);
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
                    sum *= GradRange(cellGradX + _PrimeXVector, cellGradY + _PrimeYVector, cellGradZ + _PrimeZVector, ox4, oy4, oz4);
                    sumValueVector += sum;
                }
                sumValueVector *= sample;
            });
            return values;
        }

        public virtual float[] GetRange(float[] x, float[] y, float[] z, float[] w)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (z == null)
                throw new ArgumentNullException(nameof(z));
            if (w == null)
                throw new ArgumentNullException(nameof(w));
            if (x.Length != y.Length || y.Length != z.Length || z.Length != w.Length)
                throw new ArgumentException("All length of dimension positions array should be same.");
            if (x.Length == 0)
                throw new ArgumentException("Positions could not be empty.");
            if (!Vector.IsHardwareAccelerated)
            {
                var v = new float[x.Length];
                Parallel.For(0, v.Length, i =>
                {
                    v[i] = Get(x[i], y[i], z[i], w[i]);
                });
                return v;
            }
            var count = x.Length;
            var vectorCount = (int)Math.Ceiling(count / (double)_VectorLength);
            var adjust = count % _VectorLength;
            if (adjust > 0)
            {
                count = count + _VectorLength - adjust;
                //对齐向量长度
                Array.Resize(ref x, count);
                Array.Resize(ref y, count);
                Array.Resize(ref z, count);
                Array.Resize(ref w, count);
            }
            var values = new float[count];
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

            //float[] 
            Parallel.For(0, vectorCount, c =>
            {
                var index = c * _VectorLength;
                var pxSpan = MemoryMarshal.Cast<float, Vector<float>>(x);
                var pySpan = MemoryMarshal.Cast<float, Vector<float>>(y);
                var pzSpan = MemoryMarshal.Cast<float, Vector<float>>(z);
                var pwSpan = MemoryMarshal.Cast<float, Vector<float>>(w);
                ref var px = ref pxSpan[c];
                ref var py = ref pySpan[c];
                ref var pz = ref pzSpan[c];
                ref var pw = ref pwSpan[c];
                var valuesSpan = MemoryMarshal.Cast<float, Vector<float>>(values);
                ref var sumValueVector = ref valuesSpan[c];
                //计算单型到单元格变形值
                var skew = (skewToCellVector * (px + py + pz + pw));
                //单元格坐标
                var cellX = px + skew;
                var cellY = py + skew;
                var cellZ = pz + skew;
                var cellW = pw + skew;
                Vector<float> cellXFloor, cellYFloor, cellZFloor, cellWFloor;
                Vector<int> cellXFloorInt, cellYFloorInt, cellZFloorInt, cellWFloorInt;
                //单元格原点坐标
#if NET5_0
                cellXFloor = Vector.Floor(cellX);
                cellYFloor = Vector.Floor(cellY);
                cellZFloor = Vector.Floor(cellZ);
                cellWFloor = Vector.Floor(cellW);
                cellXFloorInt = Vector.ConvertToInt32(cellXFloor);
                cellYFloorInt = Vector.ConvertToInt32(cellYFloor);
                cellZFloorInt = Vector.ConvertToInt32(cellZFloor);
                cellWFloorInt = Vector.ConvertToInt32(cellWFloor);
#else
                var cellXFloorData = new int[_VectorLength];
                var cellYFloorData = new int[_VectorLength];
                var cellZFloorData = new int[_VectorLength];
                var cellWFloorData = new int[_VectorLength];
                for (var i = 0; i < _VectorLength; i++)
                {
                    cellXFloorData[i] = (int)MathF.Floor(cellX[i]);
                    cellYFloorData[i] = (int)MathF.Floor(cellY[i]);
                    cellZFloorData[i] = (int)MathF.Floor(cellZ[i]);
                    cellWFloorData[i] = (int)MathF.Floor(cellW[i]);
                }
                cellXFloorInt = new Vector<int>(cellXFloorData);
                cellYFloorInt = new Vector<int>(cellYFloorData);
                cellZFloorInt = new Vector<int>(cellZFloorData);
                cellWFloorInt = new Vector<int>(cellWFloorData);
                cellXFloor = Vector.ConvertToSingle(cellXFloorInt);
                cellYFloor = Vector.ConvertToSingle(cellYFloorInt);
                cellZFloor = Vector.ConvertToSingle(cellZFloorInt);
                cellWFloor = Vector.ConvertToSingle(cellWFloorInt);
#endif
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
                var ox1 = px - simplexXFloor;
                var oy1 = py - simplexYFloor;
                var oz1 = pz - simplexZFloor;
                var ow1 = pw - simplexWFloor;
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

                Vector<int> cellGradX = cellXFloorInt * _PrimeXVector,
                            cellGradY = cellYFloorInt * _PrimeYVector,
                            cellGradZ = cellZFloorInt * _PrimeZVector,
                            cellGradW = cellWFloorInt * _PrimeZVector;
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
                    sum *= GradRange(cellGradX, cellGradY, cellGradZ, cellGradW, ox1, oy1, oz1, ow1);
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
                    sum *= GradRange(cellGradX + xOffset1Int * _PrimeXVector, cellGradY + yOffset1Int * _PrimeYVector, cellGradZ + zOffset1Int * _PrimeZVector, cellGradW + wOffset1Int * _PrimeWVector, ox2, oy2, oz2, ow2);
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
                    sum *= GradRange(cellGradX + xOffset2Int * _PrimeXVector, cellGradY + yOffset2Int * _PrimeYVector, cellGradZ + zOffset2Int * _PrimeZVector, cellGradW + wOffset2Int * _PrimeWVector, ox3, oy3, oz3, ow3);
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
                    sum *= GradRange(cellGradX + xOffset3Int * _PrimeXVector, cellGradY + yOffset3Int * _PrimeYVector, cellGradZ + zOffset3Int * _PrimeZVector, cellGradW + wOffset3Int * _PrimeWVector, ox4, oy4, oz4, ow4);
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
                    sum *= GradRange(cellGradX + _PrimeXVector, cellGradY + _PrimeYVector, cellGradZ + _PrimeZVector, cellGradW + _PrimeWVector, ox5, oy5, oz5, ow5);
                    sumValueVector += sum;
                }
                sumValueVector *= sample;
            });
            return values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector<int> ShiftRight(in Vector<int> vector, in int value)
        {
            return Vector.ShiftRightLogical(vector, value);
            //var hashData = new int[_VectorLength];
            //for (int i = 0; i < _VectorLength; i++)
            //    hashData[i] = vector[i] >> value;
            //return new Vector<int>(hashData);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector<float> GradRange(in Vector<int> positionX, in Vector<float> offsetX)
        {
            var hash = positionX * _PrimeGVector;
            hash ^= ShiftRight(hash, 15);
            hash = hash & _HashAnd1Vector;
            var permData = new float[_VectorLength];
            for (int i = 0; i < _VectorLength; i++)
            {
                permData[i] = HashFloat(hash[i]);
            }
            return new Vector<float>(permData) * offsetX;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe virtual Vector<float> GradRange(in Vector<int> positionX, in Vector<int> positionY, in Vector<float> offsetX, in Vector<float> offsetY)
        {
            var hash = positionX ^ positionY;
            hash *= _PrimeGVector;
            hash ^= hash >> 15;
            var hashX = hash & _HashAnd2Vector;
            var hashY = hashX | _Int1Vector;
            //var permDataPtr = NativeMemory.AlignedAlloc((nuint)_VectorLength2 * sizeof(float), (nuint)_VectorLength);
            //var permData = new Span<float>(permDataPtr, _VectorLength2);
            var permData = new float[_VectorLength * 2];
            for (int i = 0; i < _VectorLength; i++)
            {
                permData[i] = _permFloat[hashX[i]];
                permData[_VectorLength + i] = _permFloat[hashY[i]];
            }
            //return new Vector<float>(permData) * offsetX + new Vector<float>(permData, _VectorLength) * offsetY;
            var vector = MemoryMarshal.Cast<float, Vector<float>>(permData);
            var result = vector[0] * offsetX + vector[1] * offsetY;
            //NativeMemory.AlignedFree(permDataPtr);
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector<float> GradRange(in Vector<int> positionX, in Vector<int> positionY, in Vector<int> positionZ, in Vector<float> offsetX, in Vector<float> offsetY, in Vector<float> offsetZ)
        {
            var hash = positionX ^ positionY ^ positionZ;
            hash *= _PrimeGVector;
            hash ^= ShiftRight(hash, 15);
            var hashX = hash & _HashAnd3Vector;
            var hashY = hashX | _Int1Vector;
            var hashZ = hashX | _Int2Vector;
            var permData = new float[_VectorLength * 3];
            for (int i = 0; i < _VectorLength; i++)
            {
                permData[i] = HashFloat(hashX[i]);
                permData[_VectorLength + i] = HashFloat(hashY[i]);
                permData[_VectorLength2 + i] = HashFloat(hashZ[i]);
            }
            return new Vector<float>(permData) * offsetX + new Vector<float>(permData, _VectorLength) * offsetY + new Vector<float>(permData, _VectorLength2) * offsetZ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector<float> GradRange(in Vector<int> positionX, in Vector<int> positionY, in Vector<int> positionZ, in Vector<int> positionW, in Vector<float> offsetX, in Vector<float> offsetY, in Vector<float> offsetZ, in Vector<float> offsetW)
        {
            var hash = positionX ^ positionY ^ positionZ ^ positionW;
            hash *= _PrimeGVector;
            hash ^= ShiftRight(hash, 15);
            var hashX = hash & _HashAnd4Vector;
            var hashY = hashX | _Int1Vector;
            var hashZ = hashX | _Int2Vector;
            var hashW = hashX | _Int3Vector;
            var permData = new float[_VectorLength * 4];
            for (int i = 0; i < _VectorLength; i++)
            {
                permData[i] = HashFloat(hashX[i]);
                permData[_VectorLength + i] = HashFloat(hashY[i]);
                permData[_VectorLength2 + i] = HashFloat(hashZ[i]);
                permData[_VectorLength3 + i] = HashFloat(hashW[i]);
            }
            return new Vector<float>(permData) * offsetX + new Vector<float>(permData, _VectorLength) * offsetY + new Vector<float>(permData, _VectorLength2) * offsetZ + new Vector<float>(permData, _VectorLength3) * offsetW;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector<float> GradRange(Vector<int>[] positions, Vector<float> offsets)
        {
            var hash = positions[0];
            for (int i = 1; i < positions.Length; i++)
                hash ^= positions[i];
            hash ^= ShiftRight(hash, 15);
            var hashResult = new Vector<int>[positions.Length];
            hashResult[0] = hash & new Vector<int>(256 - positions.Length);
            for (int i = 1; i < positions.Length; i++)
                hashResult[i] = hashResult[0] | new Vector<int>(i);
            var permData = new float[_VectorLength * positions.Length];
            for (int i = 0; i < _VectorLength; i++)
            {
                for (int j = 0; j < positions.Length; j++)
                    permData[_VectorLength * j + i] = HashFloat(hashResult[j][i]);
            }
            var grad = new Vector<float>(permData) * offsets[0];
            for (int i = 1; i < positions.Length; i++)
                grad += new Vector<float>(permData, _VectorLength * i) * offsets[i];
            return grad;
        }

        public float GetHashFloat(int x)
        {
            int hash = x * _HashG;
            hash ^= hash >> 15;
            hash &= 255;
            return HashFloat(hash);
        }

        private static readonly Vector<float> _HalfVector = new Vector<float>(0.5f);
        private static readonly Vector<int> _ByteVector = new Vector<int>(0xff);
        private static readonly Vector<int>[] _PrimeVector = new Vector<int>[] { new Vector<int>(13283693), new Vector<int>(62374087), new Vector<int>(18303827), new Vector<int>(53344667), new Vector<int>(26854063), new Vector<int>(89862779), new Vector<int>(30319481), new Vector<int>(80638853), new Vector<int>(35568517), new Vector<int>(95418593), new Vector<int>(44407843), new Vector<int>(71470727) };

        protected const int PrimeX = 501125321;
        protected const int PrimeY = 1136930381;
        protected const int PrimeZ = 1720413743;
        protected const int PrimeW = 71470727;
        private static Vector<int> _PrimeXVector = new Vector<int>(PrimeX);
        private static Vector<int> _PrimeYVector = new Vector<int>(PrimeY);
        private static Vector<int> _PrimeZVector = new Vector<int>(PrimeZ);
        private static Vector<int> _PrimeWVector = new Vector<int>(PrimeW);
        private static Vector<int> _PrimeGVector = new Vector<int>(0x27d4eb2d);
        private static Vector<int> _HashAnd1Vector = new Vector<int>(255);
        private static Vector<int> _HashAnd2Vector = new Vector<int>(254);
        private static Vector<int> _HashAnd3Vector = new Vector<int>(253);
        private static Vector<int> _HashAnd4Vector = new Vector<int>(252);

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
