using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace PlanetGenerator.Testing
{
    public class SimplexTest
    {

        private static byte[] _Perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

        [Fact]
        public void D1()
        {
            SimplexNoise noise = new SimplexNoise(0);
            float[] values = new float[100];
            for (int i = 0; i < 100; i++)
            {
                values[i] = noise.Get(i / 100f) * 16f;
            }
        }

        [Fact]
        public void D2()
        {
            int min = 255, max = 0;
            var noise = new SimplexNoise(1);
            Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var x0 = (x / 80f);
                    var y0 = (y / 80f);
                    var value = noise.Get(x0, y0);
                    x0 *= 2;
                    y0 *= 2;
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

        //[Fact]
        //public unsafe void D2SIMD()
        //{
        //    int min = 255, max = 0;
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(perm);
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(1);
        //    //SIMDPerlinNoise noise = new SIMDPerlinNoise(1);
        //    var noise = new SimplexNoise(1);
        //    //noise.Get2D(new float[] { 0, 0 });
        //    //var v1 = noise.Get(0.5f, 0.5f);
        //    //var v3 = noise.Get(0.5f, 0f);
        //    //var v2 = noise.Get(new float[] { 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f }, 2);
        //    //var v1 = noise.Get(60f / 80f, 5f / 80f);
        //    //var v2 = noise.Get(new float[] { 60f / 80f, 5f / 80f }, 2);
        //    //noise.Get(0.5f, 0);
        //    //noise.Get(0.5f, 0, 0);
        //    //noise.Get(120f / 200f, 113f / 200f);
        //    //float[,] values = new float[100, 100];
        //    Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    var length = bitmap.Width * bitmap.Height;
        //    var alignment = (nuint)(sizeof(float) * Vector<float>.Count);
        //    var px = NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
        //    var py = NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
        //    var pvalues = NativeMemory.AlignedAlloc((nuint)length * sizeof(float), alignment);
        //    var xSpan = new Span<float>(px, length);
        //    var ySpan = new Span<float>(py, length);
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var i = x + y * bitmap.Width;
        //            xSpan[i] = (x / 80f);
        //            ySpan[i] = (y / 80f);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * noise.Get(x0, y0);
        //            //value /= 2;
        //            //var value = MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * MathF.Abs(noise.Get(x0, y0));
        //            //value /= 2;
        //        }
        //    }
        //    noise.GetRange(new IntPtr(px), new IntPtr(py), new IntPtr(pvalues), length);
        //    var valuesSpan = new Span<float>(pvalues, length);
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var value = valuesSpan[x + y * bitmap.Width];
        //            int c = (int)((value * 255) + 255) / 2;
        //            if (c < min)
        //                min = c;
        //            if (c > max)
        //                max = c;
        //            if (c > 255 || c < 0)
        //            {

        //            }
        //            bitmap.SetPixel(x, y, Color.FromArgb((255 - c), 0, 0, 0));
        //        }
        //    }
        //    bitmap.Save("d2simd.png", ImageFormat.Png);
        //    bitmap.Dispose();
        //    NativeMemory.AlignedFree(px);
        //    NativeMemory.AlignedFree(py);
        //    NativeMemory.AlignedFree(pvalues);
        //}

        //[Fact]
        //public void D2OpenCL()
        //{
        //    int min = 255, max = 0;
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(perm);
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(1);
        //    //SIMDPerlinNoise noise = new SIMDPerlinNoise(1);
        //    using var context = Context.Create(builder => builder.OpenCL());
        //    var noise = new GPUSimplexNoise(1, context, context.GetCLDevice(0));
        //    //noise.Get2D(new float[] { 0, 0 });
        //    //var v1 = noise.Get(0.5f, 0.5f);
        //    //var v3 = noise.Get(0.5f, 0f);
        //    //var v2 = noise.Get(new float[] { 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f }, 2);
        //    //var v1 = noise.Get(60f / 80f, 5f / 80f);
        //    //var v2 = noise.Get(new float[] { 60f / 80f, 5f / 80f }, 2);
        //    //noise.Get(0.5f, 0);
        //    //noise.Get(0.5f, 0, 0);
        //    //noise.Get(120f / 200f, 113f / 200f);
        //    //float[,] values = new float[100, 100];
        //    Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    float[] px = new float[bitmap.Width * bitmap.Height];
        //    float[] py = new float[px.Length];
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var i = x + y * bitmap.Width;
        //            px[i] = (x / 80f);
        //            py[i] = (y / 80f);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * noise.Get(x0, y0);
        //            //value /= 2;
        //            //var value = MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * MathF.Abs(noise.Get(x0, y0));
        //            //value /= 2;
        //        }
        //    }
        //    var values = noise.GetRange(px, py);
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var value = values[x + y * bitmap.Width];
        //            int c = (int)((value * 255) + 255) / 2;
        //            if (c < min)
        //                min = c;
        //            if (c > max)
        //                max = c;
        //            if (c > 255 || c < 0)
        //            {

        //            }
        //            bitmap.SetPixel(x, y, Color.FromArgb((255 - c), 0, 0, 0));
        //        }
        //    }
        //    bitmap.Save("d2opencl.png", ImageFormat.Png);
        //    bitmap.Dispose();
        //}

        //[Fact]
        //public void D2CUDA()
        //{
        //    int min = 255, max = 0;
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(perm);
        //    //DefaultPerlinNoise noise = new DefaultPerlinNoise(1);
        //    //SIMDPerlinNoise noise = new SIMDPerlinNoise(1);
        //    using var context = Context.Create(builder => builder.Cuda());
        //    var noise = new GPUSimplexNoise(1, context, context.GetCudaDevice(0));
        //    //noise.Get2D(new float[] { 0, 0 });
        //    //var v1 = noise.Get(0.5f, 0.5f);
        //    //var v3 = noise.Get(0.5f, 0f);
        //    //var v2 = noise.Get(new float[] { 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f, 0.5f, 0.5f, 0.5f, 0f }, 2);
        //    //var v1 = noise.Get(60f / 80f, 5f / 80f);
        //    //var v2 = noise.Get(new float[] { 60f / 80f, 5f / 80f }, 2);
        //    //noise.Get(0.5f, 0);
        //    //noise.Get(0.5f, 0, 0);
        //    //noise.Get(120f / 200f, 113f / 200f);
        //    //float[,] values = new float[100, 100];
        //    Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    float[] px = new float[bitmap.Width * bitmap.Height];
        //    float[] py = new float[px.Length];
        //    float[] values = new float[px.Length];
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var i = x + y * bitmap.Width;
        //            px[i] = (x / 80f);
        //            py[i] = (y / 80f);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * noise.Get(x0, y0);
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * noise.Get(x0, y0);
        //            //value /= 2;
        //            //var value = MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.5f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.25f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.125f * MathF.Abs(noise.Get(x0, y0));
        //            //x0 *= 2;
        //            //y0 *= 2;
        //            //value += 0.0625f * MathF.Abs(noise.Get(x0, y0));
        //            //value /= 2;
        //        }
        //    }
        //    noise.GetRange(px, py, values);
        //    for (int x = 0; x < bitmap.Width; x++)
        //    {
        //        for (int y = 0; y < bitmap.Height; y++)
        //        {
        //            var value = values[x + y * bitmap.Width];
        //            int c = (int)((value * 255) + 255) / 2;
        //            if (c < min)
        //                min = c;
        //            if (c > max)
        //                max = c;
        //            if (c > 255 || c < 0)
        //            {

        //            }
        //            bitmap.SetPixel(x, y, Color.FromArgb((255 - c), 0, 0, 0));
        //        }
        //    }
        //    bitmap.Save("d2cuda.png", ImageFormat.Png);
        //    bitmap.Dispose();
        //}

        [Fact]
        public void D3()
        {
            int min = 255, max = 0;
            var noise = new SimplexNoise(0);
            Bitmap bitmap = new Bitmap(1600, 1600, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var x0 = (x / 80f);
                    var y0 = (y / 80f);
                    var value = noise.Get(x0, y0, 0.5f);
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
            bitmap.Save("d3.png", ImageFormat.Png);
            bitmap.Dispose();
        }

        [Fact]
        public void D3SIMD()
        {
            int min = 255, max = 0;
            //DefaultPerlinNoise noise = new DefaultPerlinNoise(perm);
            //DefaultPerlinNoise noise = new DefaultPerlinNoise(1);
            //SIMDPerlinNoise noise = new SIMDPerlinNoise(1);
            var noise = new SimplexNoise(1);
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
            float[] px = new float[bitmap.Width * bitmap.Height];
            float[] py = new float[px.Length];
            float[] pz = new float[px.Length];
            float[] values = new float[px.Length];
            Array.Fill(pz, 0.5f);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var i = x + y * bitmap.Width;
                    px[i] = (x / 80f);
                    py[i] = (y / 80f);
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
            noise.GetRange(px, py, pz, values);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var value = values[x + y * bitmap.Width];
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
            bitmap.Save("d3simd.png", ImageFormat.Png);
            bitmap.Dispose();
        }
    }
}