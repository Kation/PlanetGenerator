using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlanetGenerator.Unity
{
    public class UnityPerlinNoise : DefaultPerlinNoise
    {
        public UnityPerlinNoise(int seed) : base(seed)
        {

        }
        public UnityPerlinNoise(byte[] perm) : base(perm)
        {

        }

        protected override float Floor(float value)
        {
            return Mathf.Floor(value);
        }

        protected override int FloorToInt(float value)
        {
            return Mathf.FloorToInt(value);
        }
    }
}
