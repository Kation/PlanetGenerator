using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetOptions
    {
        public PlanetSettings Settings { get; set; }

        public string SavePath { get; set; }

        public INoise Noise { get; set; }
    }
}
