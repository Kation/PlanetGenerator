using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlanetGenerator.Testing
{
    public class PlanetGeneratorTest
    {
        [Fact]
        public void TestGeneratorRoot()
        {
            PlanetSettings settings = new PlanetSettings();
            settings.TileResolution = 8;
            PlanetBuilder generator = new PlanetBuilder(settings, new SimplexNoise(settings.Seed));
            var t0 = generator.GenerateTile(0, 0);
            var t1 = generator.GenerateTile(1, 0);
            for (int i = 0; i < settings.TileResolution; i++)
            {
                Assert.Equal(t0.Terrain.Span[settings.TileResolution * (settings.TileResolution - 1) + i], t1.Terrain.Span[i]);
            }
        }

        [Fact]
        public void TestGeneratorTile()
        {
            PlanetSettings settings = new PlanetSettings();
            settings.TileResolution = 128;
            PlanetBuilder generator = new PlanetBuilder(settings, new SimplexNoise(settings.Seed));
            var t0 = generator.GenerateTile(0, 1);
            var t1 = generator.GenerateTile(1, 1);
            Assert.Equal(t0.Terrain.Span[settings.TileResolution - 1], t1.Terrain.Span[0]);
        }
    }
}
