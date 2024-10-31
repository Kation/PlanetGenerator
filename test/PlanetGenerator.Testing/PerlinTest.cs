using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Buffers;

namespace PlanetGenerator.Testing
{
    public class PerlinTest
    {
        [Fact]
        public void D2()
        {
            int min = 255, max = 0;
            //var noise = new PerlinNoise(1);
            var noise = new PerlinNoise(new HashSeed());

            //noise.Get(6.25f, 6.25f);

            var v1 = noise.Seed.GetHashGrad(12, 12, 0.25f, 0.25f);
            var v2 = noise.Seed.GetHashGrad(new Vector<int>(12), new Vector<int>(12), new Vector<float>(0.25f), new Vector<float>(0.25f));

            Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var x0 = (x / 80f);
                    var y0 = (y / 80f);
                    var value = noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.5f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.25f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.125f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.0625f * noise.Get(x0, y0);
                    //value /= 2;
                    //var value = MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.5f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.25f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.125f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.0625f * MathF.Abs(noise.Get(x0, y0));
                    //value /= 2;
                    int c = (int)((value * 255) + 255) / 2;
                    if (c < min)
                        min = c;
                    if (c > max)
                        max = c;
                    if (c > 255 || c < 0)
                    {

                    }
                    bitmap.SetPixel(x, y, Color.FromArgb(c, c, c));
                }
            }
            bitmap.Save("d2.png", ImageFormat.Png);
            bitmap.Dispose();
        }

        [Fact]
        public unsafe void D2SIMD()
        {
            int min = 255, max = 0;
            //DefaultPerlinNoise noise = new DefaultPerlinNoise(perm);
            //DefaultPerlinNoise noise = new DefaultPerlinNoise(1);
            //SIMDPerlinNoise noise = new SIMDPerlinNoise(1);
            var noise = new PerlinNoise(new HashSeed());
            //noise.Get2D(new float[] { 0, 0 });
            //var v1 = noise.Get(0.5f, 0.5f);
            //var v3 = noise.Get(0.5f, 0f);
            //var v2 = noise.Get(new float[] { 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f }, 2);
            //var v1 = noise.Get(60f / 80f, 5f / 80f);
            //var v2 = noise.Get(new float[] { 60f / 80f, 5f / 80f }, 2);
            //noise.Get(0.5f, 0);
            //noise.Get(0.5f, 0, 0);
            //noise.Get(120f / 200f, 113f / 200f);
            //float[,] values = new float[100, 100];
            Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var length = bitmap.Width * bitmap.Height;
            var alignment = (nuint)(sizeof(float) * Vector<float>.Count);
            float* px = (float*)NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
            float* py = (float*)NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
            float* pvalues = (float*)NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
            var xSpan = new Span<float>(px, length);
            var ySpan = new Span<float>(py, length);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var i = x + y * bitmap.Width;
                    xSpan[i] = (x / 80f);
                    ySpan[i] = (y / 80f);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.5f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.25f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.125f * noise.Get(x0, y0);
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.0625f * noise.Get(x0, y0);
                    //value /= 2;
                    //var value = MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.5f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.25f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.125f * MathF.Abs(noise.Get(x0, y0));
                    //x0 *= 2;
                    //y0 *= 2;
                    //value += 0.0625f * MathF.Abs(noise.Get(x0, y0));
                    //value /= 2;
                }
            }
            var xm = new NativeMemoryManager(px, length);
            var ym = new NativeMemoryManager(py, length);
            var vm = new NativeMemoryManager(pvalues, length);
            noise.GetRange(xm.Memory, ym.Memory, vm.Memory, aligned: true);
            var valuesSpan = new Span<float>(pvalues, length);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var value = valuesSpan[x + y * bitmap.Width];
                    int c = (int)((value * 255) + 255) / 2;
                    if (c < min)
                        min = c;
                    if (c > max)
                        max = c;
                    if (c > 255 || c < 0)
                    {

                    }
                    bitmap.SetPixel(x, y, Color.FromArgb(c, c, c));
                }
            }
            bitmap.Save("d2simd.png", ImageFormat.Png);
            bitmap.Dispose();
            NativeMemory.AlignedFree(px);
            NativeMemory.AlignedFree(py);
            NativeMemory.AlignedFree(pvalues);
        }

        private unsafe class NativeMemoryManager : MemoryManager<float>
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
