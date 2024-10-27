using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public class StrideTileSettings
    {
        public Dictionary<string, Color4> TextureColors { get; } = new Dictionary<string, Color4>();

        public static StrideTileSettings Default { get; }

        static StrideTileSettings()
        {
            Default = new StrideTileSettings();
            Default.TextureColors.Add("FaultZone", new Color4(1f, 1f, 0f, 1f));
        }
    }
}
