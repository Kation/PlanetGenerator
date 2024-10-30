using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PlanetGenerator
{
    public class GPUSimplexNoise : SimplexNoise, IDisposable
    {
        private readonly Context _context;
        private readonly Device _device;
        private readonly Accelerator _accelerator;
        private MemoryBuffer1D<float, Stride1D.Dense> _permFloatBuffer;

        public GPUSimplexNoise(int seed, Context context, Device device) : base(seed)
        {
            _context = context;
            _device = device;
            _accelerator = device.CreateAccelerator(context);
            _permFloatBuffer = _accelerator.Allocate1D(GetPermFloat());

        }

        //public GPUSimplexNoise(byte[] seeds, Context context, Device device) : base(seeds)
        //{
        //    _context = context;
        //    _device = device;
        //    _accelerator = device.CreateAccelerator(context);
        //    _permFloatBuffer = _accelerator.Allocate1D(GetPermFloat());
        //}

        public void Dispose()
        {
            _permFloatBuffer.Dispose();
            _accelerator.Dispose();
        }

        public unsafe override void GetRange(IntPtr x, IntPtr y, IntPtr values, int length)
        {
            var kernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ArrayView<float>>(GetRange);
            var mx = _accelerator.Allocate1D<float>(length);
            var my = _accelerator.Allocate1D<float>(length);
            var mv = _accelerator.Allocate1D<float>(length);
            ref var xRef = ref Unsafe.AsRef<float>(x.ToPointer());
            ref var yRef = ref Unsafe.AsRef<float>(y.ToPointer());
            mx.View.CopyFromCPU(ref xRef, length);
            my.View.CopyFromCPU(ref yRef, length);
            kernel(length, _permFloatBuffer.View, mx.View, my.View, mv.View);
            mv.View.CopyToCPU(ref Unsafe.AsRef<float>(values.ToPointer()), length);
            mv.Dispose();
            mx.Dispose();
            my.Dispose();
        }

        private static void GetRange(Index1D index, ArrayView<float> permFloat, ArrayView<float> vx, ArrayView<float> vy, ArrayView<float> values)
        {
            ref var x = ref vx[index];
            ref var y = ref vy[index];

            float sum = x + y;
            float skewToCell, skewFromCell, sample;
            skewToCell = SkewValue<float[,]>.SkewToCell;
            skewFromCell = SkewValue<float[,]>.SkewFromCell;
            sample = SkewValue<float[,]>.Sample;
            float skewFromOrigin = skewFromCell;
            skewToCell *= sum;
            //单元格原点总和
            var cellPositionX = x + skewToCell;
            var cellPositionY = y + skewToCell;
            var cellFloorX = XMath.Floor(cellPositionX);
            var cellFloorY = XMath.Floor(cellPositionY);
            var cellFloorXInt = (int)cellFloorX;
            var cellFloorYInt = (int)cellFloorY;
            sum = cellFloorX + cellFloorY;
            //单元格原点转换为单型原点差值
            skewFromOrigin *= sum;
            var simplexFloorX = cellFloorX - skewFromOrigin;
            var simplexFloorY = cellFloorY - skewFromOrigin;

            var simplexOffset0x = x - simplexFloorX;
            var simplexOffset0y = y - simplexFloorY;

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

            var simplexOffset1x = simplexOffset0x - cellOffset1x + skewFromCell;
            var simplexOffset1y = simplexOffset0y - cellOffset1y + skewFromCell;
            var simplexOffset2x = simplexOffset0x - 1 + 2 * skewFromCell;
            var simplexOffset2y = simplexOffset0y - 1 + 2 * skewFromCell;

            float total = 0f;

            cellFloorXInt *= PrimeX;
            cellFloorYInt *= PrimeY;
            //计算各个顶点梯度值
            {
                sum = simplexOffset0x * simplexOffset0x + simplexOffset0y * simplexOffset0y;
                sum = 0.5f - sum;
                sum = XMath.Max(sum, 0f);
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(permFloat, cellFloorXInt, cellFloorYInt, simplexOffset0x, simplexOffset0y);
                total += grad * sum;
            }
            {
                sum = simplexOffset1x * simplexOffset1x + simplexOffset1y * simplexOffset1y;
                sum = 0.5f - sum;
                sum = XMath.Max(sum, 0f);
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(permFloat, cellFloorXInt + cellOffset1x * PrimeX, cellFloorYInt + cellOffset1y * PrimeY, simplexOffset1x, simplexOffset1y);
                total += grad * sum;
            }
            {
                //单型顶点坐标
                sum = simplexOffset2x * simplexOffset2x + simplexOffset2y * simplexOffset2y;
                sum = 0.5f - sum;
                sum = XMath.Max(sum, 0f);
                sum *= sum;
                sum *= sum;
                //计算单元格的梯度值
                var grad = Grad(permFloat, cellFloorXInt + PrimeX, cellFloorYInt + PrimeY, simplexOffset2x, simplexOffset2y);
                total += grad * sum;
            }
            total = total * sample;
            if (total < -1)
                total = -1;
            else if (total > 1)
                total = 1;
            values[index] = total;
        }

        private static float Grad(ArrayView<float> permFloat, int positionX, int positionY, float offsetX, float offsetY)
        {
            int hash = positionX ^ positionY;
            hash *= 0x27d4eb2d;
            hash ^= hash >> 15;
            hash &= 254;
            var fx = permFloat[hash];
            var fy = permFloat[hash | 1];
            return fx * offsetX + fy * offsetY;
        }
    }
}
