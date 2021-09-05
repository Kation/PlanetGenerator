using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetSettings
    {
        /// <summary>
        /// Get or set radius of planet.
        /// <br/>
        /// Unit: km
        /// </summary>
        public float PlanetRadius { get; set; }

        /// <summary>
        /// Get or set radius of planet.
        /// <br/>
        /// Unit: Gm(1000*1000km)
        /// </summary>
        public float OrbitRadius { get; set; }

        /// <summary>
        /// Get or set radius of star(sun).
        /// <br/>
        /// Unit: km
        /// </summary>
        public float StarRadius { get; set; }

        /// <summary>
        /// Get or set axial tilt of planet. 
        /// <br/>
        /// 0 to 1. 0 is normal. 1 is upside down.
        /// </summary>
        public float AxialTilt { get; set; }

        /// <summary>
        /// Get or set the seed of planet generate context.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Get or set the count of line that used to locate mineral by line cross.
        /// </summary>
        public int MineralLineCount { get; set; }

        /// <summary>
        /// Get or set the easing function of distance of mineral line to planet centre.
        /// </summary>
        public IEasingFunction MineralLineDistanceEasing { get; set; }
    }
}
