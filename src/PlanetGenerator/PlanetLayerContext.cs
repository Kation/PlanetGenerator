using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public class PlanetLayerContext
    {
        public PlanetLayerContext(INoise noise, PlanetSettings settings, IReadOnlyList<IPlanetLayer> layers)
        {
            Noise = noise;
            Settings = settings;
            Layers = layers;
        }

        /// <summary>
        /// 获取噪声。
        /// </summary>
        public INoise Noise { get; }

        /// <summary>
        /// 获取星球设置。
        /// </summary>
        public PlanetSettings Settings { get; }

        /// <summary>
        /// 获取层。
        /// </summary>
        public IReadOnlyList<IPlanetLayer> Layers { get; }
    }
}
