using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlanetGenerator
{
    public class DefaultPerlinNoise : PerlinNoise
    {
        public DefaultPerlinNoise(int seed) : base(seed)
        {
            int i, j, k;
            System.Random rnd = new System.Random(seed);

            for (i = 0; i < B; i++)
            {
                p[i] = i;

                for (j = 0; j < 2; j++)
                    g2[i, j] = (float)(rnd.Next(B + B) - B) / B;
                normalize2(ref g2[i, 0], ref g2[i, 1]);
            }

            while (--i != 0)
            {
                k = p[i];
                p[i] = p[j = rnd.Next(B)];
                p[j] = k;
            }

            for (i = 0; i < B + 2; i++)
            {
                p[B + i] = p[i];
                for (j = 0; j < 2; j++)
                    g2[B + i, j] = g2[i, j];
            }
        }

        //public DefaultPerlinNoise(byte[] seeds)
        //{
        //    if (seeds == null)
        //        throw new ArgumentNullException(nameof(seeds));
        //    if (seeds.Length == 0)
        //        throw new ArgumentException("Seeds must have value.");
        //    _perm = seeds;
        //    _gradsX = new float[0xff];
        //    _gradsY = new float[0xff];
        //}

        int[] p = new int[B + B + 2];
        float[,] g2 = new float[B + B + 2, 2];
        const int B = 0x100;
        const int BM = 0xff;
        const int N = 0x1000;

        void normalize2(ref float x, ref float y)
        {
            float s;

            s = (float)Math.Sqrt(x * x + y * y);
            x = x / s;
            y = y / s;
        }

        float s_curve(float t)
        {
            return t * t * (3.0F - 2.0F * t);
        }

        float lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        void setup(float value, out int b0, out int b1, out float r0, out float r1)
        {
            float t = value + N;
            b0 = ((int)t) & BM;
            b1 = (b0 + 1) & BM;
            r0 = t - (int)t;
            r1 = r0 - 1.0F;
        }

        float at2(float rx, float ry, float x, float y) { return rx * x + ry * y; }
        float at3(float rx, float ry, float rz, float x, float y, float z) { return rx * x + ry * y + rz * z; }

        public float Get(float x, float y)
        {
            int bx0, bx1, by0, by1, b00, b10, b01, b11;
            float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
            int i, j;

            setup(x, out bx0, out bx1, out rx0, out rx1);
            setup(y, out by0, out by1, out ry0, out ry1);

            i = p[bx0];
            j = p[bx1];

            b00 = p[i + by0];
            b10 = p[j + by0];
            b01 = p[i + by1];
            b11 = p[j + by1];

            sx = s_curve(rx0);
            sy = s_curve(ry0);

            u = at2(rx0, ry0, g2[b00, 0], g2[b00, 1]);
            v = at2(rx1, ry0, g2[b10, 0], g2[b10, 1]);
            a = lerp(sx, u, v);

            u = at2(rx0, ry1, g2[b01, 0], g2[b01, 1]);
            v = at2(rx1, ry1, g2[b11, 0], g2[b11, 1]);
            b = lerp(sx, u, v);

            return lerp(sy, a, b);

            //int x1 = (int)MathF.Floor(x);
            //int x2 = x1 + 1;
            //int y1 = (int)MathF.Floor(y);
            //int y2 = y1 + 1;
            //float ox1 = x - x1;
            //float ox2 = x - x2;
            //float oy1 = y - y1;
            //float oy2 = y - y2;

            //x1 += N;
            //y1 += N;
            //x2 += N;
            //y2 += N;
            //x1 &= 0xff;
            //x2 &= 0xff;
            //y1 &= 0xff;
            //y2 &= 0xff;
            //var i = p[x1];
            //var j = p[x2];

            //var b00 = p[(i + y1)];
            //var b10 = p[(j + y1)];
            //var b01 = p[(i + y2)];
            //var b11 = p[(j + y2)];

            //var grade1 = ox1 * g2[b00, 0] + oy1 * g2[b00, 1];
            //var grade2 = ox2 * g2[b10, 0] + oy1 * g2[b10, 1];
            //var grade3 = ox1 * g2[b01, 0] + oy2 * g2[b01, 1];
            //var grade4 = ox2 * g2[b11, 0] + oy2 * g2[b11, 1];

            //var sx = ox1 * ox1 * (3f - 2f * ox1);
            //var sy = oy1 * oy1 * (3f - 2f * oy1);

            //var l1 = Lerp(grade1, grade2, ox1);
            //var l2 = Lerp(grade3, grade4, ox1);
            //var l = Lerp(l1, l2, oy1);
            //return l;
        }
    }
}
