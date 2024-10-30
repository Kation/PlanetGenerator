using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public interface IPlanetLayer
    {
        void HandleBase(PlanetLayerContext context);

        void HandleTile(PlanetLayerTileContext context, int index, int zoomLevel);
    }
}
