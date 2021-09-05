using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public interface IEasingFunction
    {
        double Ease(float value);
    }
}
