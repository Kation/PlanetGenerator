using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public interface IPlanetLayer
    {
        void Handle(PlanetLayerContext context, int index, int zoomLevel);
    }
}
