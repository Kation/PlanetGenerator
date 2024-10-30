using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlanetGenerator.Testing
{
    public class PerlinTest
    {
        [Fact]
        public void D2()
        {
            int min = 255, max = 0;
            var noise = new PerlinNoise(1);

            noise.Get(0.25f, 0.25f);

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
    }
}
