using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetOptions
    {
        public PlanetOptions(PlanetSettings settings, string savePath, INoise noise)
        {
            Settings = settings;
            SavePath = savePath;
            Noise = noise;
        }

        public PlanetSettings Settings { get; }

        public string SavePath { get; }

        public INoise Noise { get;  }
    }
}
