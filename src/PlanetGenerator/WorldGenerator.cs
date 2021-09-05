using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class WorldGenerator
    {
        public WorldGenerator(PlanetSettings planetSettings)
        {
            Settings = planetSettings ?? throw new ArgumentNullException(nameof(planetSettings));
        }

        public PlanetSettings Settings { get; }
    }
}
