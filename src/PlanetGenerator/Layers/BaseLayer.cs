using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator.Layers
{
    public class BaseLayer : IPlanetLayer
    {
        private const float _Move1 = 10000f;
        private const float _Move2 = -10000f;
        private const float _Move3 = 20000f;
        private bool _baseHandled = false;
        private float[]? _faultZones;
        private const int _FaultZoneLength = 1024 * 1024;

        public void HandleBase(PlanetLayerContext context)
        {
            


            _faultZones = new float[10 * _FaultZoneLength];
            for (int index = 0; index < 10; index++)
            {
                var faultZone = _faultZones.AsSpan().Slice(index * _FaultZoneLength, _FaultZoneLength);
                PlanetHelper.GetLocationAndPositions(index, 0, 1024, context.Settings.PlanetRadius, out var positionX, out var positionY, out var positionZ, out var longitudes, out var latitudes);

                for (int i = 0; i < _FaultZoneLength; i++)
                {
                    var value = context.Noise.Get(positionX[i] / 500f, positionY[i] / 500f, positionZ[i] / 500f) * 0.5f;
                    if (value < 0)
                        value = 0;
                    //else
                    //{
                    //    var addValue = context.Noise.Get(positionX[i] / 250f, positionY[i] / 250f, positionZ[i] / 250f) * 0.75f;
                    //    addValue += context.Noise.Get(positionX[i] / 125f, positionY[i] / 125f, positionZ[i] / 125f) * 0.25f;
                    //    value *= (addValue + 1f) / 2f;
                    //}
                    //if (value < 0.25f)
                    //    value = 0f;
                    //else
                    //{
                    //    value = (value * 4f - 1f) / 3f * 2f;
                    //    if (value > 1f)
                    //        value = 1f;
                    //}
                    //var value = context.Noise.Get(positionX[i] / 250f, positionY[i] / 250f, positionZ[i] / 250f) * 0.5f;
                    //value += context.Noise.Get(positionX[i] / 500f, positionY[i] / 500f, positionZ[i] / 500f) * 0.25f;
                    //value += context.Noise.Get(positionX[i] / 1000f, positionY[i] / 1000f, positionZ[i] / 1000f) * 0.125f;
                    //value += context.Noise.Get(positionX[i] / 2000f, positionY[i] / 2000f, positionZ[i] / 2000f) * 0.0625f;
                    //value = (value + 1f) / 2f;
                    faultZone[i] = value;
                }
            }
            _baseHandled = true;
        }

        public void HandleTile(PlanetLayerTileContext context, int index, int zoomLevel)
        {
            if (!_baseHandled)
                throw new InvalidOperationException("先处理基础部分才能处理分片。");
            //List<Plate> plates = new List<Plate>();
            var lngCell = 1f / context.Settings.PlateCount;
            var rnd = new Random(context.Settings.Seed);
            var c = 0;
            //for (int i = 0; i < context.Settings.PlateCount; i++)
            //{
            //    while (true)
            //    {
            //        var offset = (context.Noise.GetHashFloat(c * 2) + context.Noise.GetHashFloat(c * 2 + 1)) / 2;
            //        var lat = context.Noise.GetHashFloat(10000 + c * 100) * MathF.PI / 2;
            //        var size = (context.Noise.GetHashFloat(-10000 + c * 100) + 1) / 2;
            //        //var offset = (float)((rnd.NextDouble() - 0.5d) * 2);
            //        //var lat = (float)((rnd.NextDouble() - 0.5d) * 2) * MathF.PI / 2;
            //        //var size = (float)rnd.NextDouble();
            //        var lng = lngCell * (c + offset) * MathF.PI * 2;
            //        size = (context.Settings.PlateMaxRadius - context.Settings.PlateMinRadius) * size + context.Settings.PlateMinRadius;
            //        var px = context.Settings.PlanetRadius * MathF.Cos(lat) * MathF.Cos(lng);
            //        var pz = -context.Settings.PlanetRadius * MathF.Cos(lat) * MathF.Sin(lng);
            //        var py = context.Settings.PlanetRadius * MathF.Sin(lat);
            //        c++;
            //        if (plates.Any(t => GetDistance(t.Longitude, t.Latitude, lng, lat, context.Settings.PlanetRadius) < t.Radius || GetDistance(t.Longitude, t.Latitude, lng, lat, context.Settings.PlanetRadius) < size))
            //            continue;
            //        plates.Add(new Plate
            //        {
            //            Longitude = lng,
            //            Latitude = lat,
            //            X = px,
            //            Y = py,
            //            Z = pz,
            //            Radius = size
            //        });
            //        break;
            //    }
            //}
            var length = context.PositionX.Length;
            var data = new float[length];
            //for (int i = 0; i < length; i++)
            //{
            //    List<EffectivePlate> effectivePlates = new List<EffectivePlate>();
            //    for (int ii = 0; ii < plates.Count; ii++)
            //    {
            //        var plate = plates[ii];

            //        //经过随机处理，计算实际半径
            //        var angle = GetAngle(plate.X, plate.Y, plate.Z, context.PositionX[i], context.PositionY[i], context.PositionZ[i]);
            //        if (plate.Longitude >= 0)
            //        {
            //            if (context.Longitudes[i] < plate.Longitude || ((context.Longitudes[i] - plate.Longitude > MathF.PI) && context.Longitudes[i] - MathF.PI * 2 < plate.Longitude))
            //                angle = MathF.PI * 2 - angle;
            //        }
            //        else if (plate.Longitude < 0)
            //        {
            //            if (context.Longitudes[i] - plate.Longitude > MathF.PI && context.Longitudes[i] < plate.Longitude + MathF.PI * 2)
            //                angle = MathF.PI * 2 - MathF.PI;
            //        }
            //        float x, y;
            //        const float right = MathF.PI / 2;
            //        const float down = right * 2;
            //        const float left = right * 3;
            //        if (angle == 0)
            //        {
            //            x = 0; y = 1;
            //        }
            //        else if (angle == right)
            //        {
            //            x = 1; y = 0;
            //        }
            //        else if (angle == down)
            //        {
            //            x = 0; y = -1;
            //        }
            //        else if (angle == left)
            //        {
            //            x = -1; y = 0;
            //        }
            //        else
            //        {
            //            y = MathF.Cos(angle);
            //            x = MathF.Sin(angle);
            //        }
            //        //var o = context.Noise.Get(x + ii * _Move1, y + ii * _Move1) * 1f;
            //        var o = context.Noise.Get(x * 2 + ii * _Move1 + 100000, y * 2 + ii * _Move1 + 100000) * 0.9f;
            //        o += context.Noise.Get(x * 4 + ii * _Move1 + 100000, y * 4 + ii * _Move1 + 100000) * 0.05f;
            //        o += context.Noise.Get(x * 8 + ii * _Move1 + 200000, y * 8 + ii * _Move1 + 200000) * 0.025f;
            //        o += context.Noise.Get(x * 16 + ii * _Move1 + 300000, y * 16 + ii * _Move1 + 300000) * 0.0125f;
            //        //var radiusActually = plate.Z; //plate.Z * (1 + context.Settings.PlateMaxRadiusOffset * context.Noise.Get(p.X, p.Y, p.Z));
            //        var radiusActually = plate.Radius * (1 + context.Settings.PlateMaxRadiusOffset * o);
            //        bool inner = false;
            //        foreach (var item in effectivePlates)
            //        {
            //            var d = GetDistance(plate.Longitude, plate.Latitude, item.Longitude, item.Latitude, context.Settings.PlanetRadius);
            //            if (d < item.Radius || d < radiusActually)
            //            {
            //                if (item.Radius < radiusActually)
            //                {
            //                    effectivePlates.Remove(item);
            //                    break;
            //                }
            //                else
            //                {
            //                    inner = true;
            //                    break;
            //                }
            //            }
            //        }
            //        if (inner)
            //            continue;
            //        var distance = GetDistance(context.Longitudes[i], context.Latitudes[i], plate.Longitude, plate.Latitude, context.Settings.PlanetRadius);
            //        if (distance < radiusActually + 500)
            //        {
            //            effectivePlates.Add(new EffectivePlate { Distance = distance, Longitude = plate.Longitude, Latitude = plate.Latitude, Radius = radiusActually, Index = ii });
            //        }
            //    }
            //    if (effectivePlates.Count == 0)
            //    {
            //        data[i] = -8f;
            //    }
            //    else
            //    {
            //        if (effectivePlates.Any(t => t.Distance < t.Radius))
            //        {
            //            foreach (var item in effectivePlates.Where(t => t.Distance > t.Radius).ToArray())
            //                effectivePlates.Remove(item);
            //            if (effectivePlates.Count > 1)
            //            {
            //                var mid = effectivePlates.Average(t => t.Distance - t.Radius);
            //                var max = effectivePlates.Sum(t => t.Distance / 500f);
            //                float o1, o2 = 0f;
            //                foreach (var plate in effectivePlates)
            //                {
            //                    var n = (plate.Distance / plate.Radius) * (plate.Index * _Move3);
            //                    o1 = context.Noise.Get(n + context.PositionX[i] / 100f, n + context.PositionY[i] / 100f, n + context.PositionZ[i] / 100f) * 0.5f;
            //                    o1 += context.Noise.Get(n + context.PositionX[i] / 50f, n + context.PositionY[i] / 50f, n + context.PositionZ[i] / 50f) * 0.25f;
            //                    o1 += context.Noise.Get(n + context.PositionX[i] / 25f, n + context.PositionY[i] / 25f, n + context.PositionZ[i] / 25f) * 0.125f;
            //                    //o += context.Noise.Get(n + context.PositionX[i] / 2f, n + context.PositionY[i] / 2f, n + context.PositionZ[i] / 2f) * 0.0625f;
            //                    o1 = (o1 + 1) / 2;
            //                    data[i] += max * (1 - MathF.Abs((plate.Distance - plate.Radius) / mid - 1)) * o1 / effectivePlates.Count;
            //                    o2 += context.Noise.Get(context.PositionX[i] / 1000f + plate.Index * _Move1, context.PositionY[i] / 1000f + plate.Index * _Move1, context.PositionZ[i] / 1000f + plate.Index * _Move1) / effectivePlates.Count;
            //                }
            //                data[i] += (5f + o2 * 5) * (1f - effectivePlates.Min(t => t.Distance / t.Radius));
            //            }
            //            else
            //            {
            //                var o = context.Noise.Get(context.PositionX[i] / 1000f, context.PositionY[i] / 1000f, context.PositionZ[i] / 1000f);
            //                data[i] += (5f + o * 5) * (1f - effectivePlates[0].Distance / effectivePlates[0].Radius);
            //            }
            //        }
            //        if (effectivePlates.All(t => t.Distance > t.Radius))
            //        {
            //            data[i] = effectivePlates.Min(t => (t.Distance - t.Radius) / 500f) * -4f;
            //        }
            //        foreach (var plate in effectivePlates)
            //        {
            //            if (plate.Distance < 50)
            //            {
            //                data[i] += 50f;
            //            }
            //            if (plate.Distance < 100)
            //            {
            //                data[i] += 25f;
            //            }
            //            if (plate.Distance < 200)
            //            {
            //                data[i] += 10f;
            //            }
            //        }
            //    }
            //    if (MathF.Abs(context.PositionX[i]) < 50 && MathF.Abs(context.PositionZ[i]) < 50)
            //    {
            //        data[i] += 30f;
            //    }
            //}

            //断裂带计算
            if (zoomLevel == 0)
            {
                context.Textures.Add(new LayerTexture("FaultZone", _faultZones!.AsMemory().Slice(index * _FaultZoneLength, _FaultZoneLength), 1024));
            }
            else
            {
                var faultZone = new float[context.Settings.TileResolution * context.Settings.TextureMultiple * context.Settings.TileResolution * context.Settings.TextureMultiple];
                context.Textures.Add(new LayerTexture("FaultZone", faultZone, context.Settings.TileResolution * context.Settings.TextureMultiple));
                if (context.Settings.TextureMultiple == 1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        var value = context.Noise.Get(context.PositionX[i] / 500f, context.PositionY[i] / 500f, context.PositionZ[i] / 500f) * 0.5f;
                        value += context.Noise.Get(context.PositionX[i] / 250f, context.PositionY[i] / 250f, context.PositionZ[i] / 250f) * 0.25f;
                        value += context.Noise.Get(context.PositionX[i] / 125f, context.PositionY[i] / 125f, context.PositionZ[i] / 125f) * 0.25f;
                        if (value < 0.25f)
                            value = 0f;
                        else
                        {
                            value = (value * 4f - 1f) / 3f * 2f;
                            if (value > 1f)
                                value = 1f;
                        }
                        //else
                        //    value = (value - 0.5f) * 2f;
                        //value += 1;
                        //value /= 2;
                        faultZone[i] = value;
                    }
                }
            }
            context.CreateLayer(PlanetLayers.Plate, data);
        }

        private float GetDistance(float lng1, float lat1, float lng2, float lat2, float radius)
        {
            float a = lat1 - lat2;
            float b = lng1 - lng2;
            float s = 2 * MathF.Asin(MathF.Sqrt(
                    MathF.Pow(MathF.Sin(a / 2), 2) + MathF.Cos(lat1) * MathF.Cos(lat2) * MathF.Pow(MathF.Sin(b / 2), 2)));
            s = s * radius;
            return s;
        }
        public float GetAngle(float lng1, float lat1, float lng2, float lat2)
        {
            var px1 = MathF.Cos(lat1) * MathF.Cos(lng1);
            var pz1 = -MathF.Cos(lat1) * MathF.Sin(lng1);
            var py1 = MathF.Sin(lat1);
            var px2 = MathF.Cos(lat2) * MathF.Cos(lng2);
            var pz2 = -MathF.Cos(lat2) * MathF.Sin(lng2);
            var py2 = MathF.Sin(lat2);
            return GetAngle(px1, py1, pz1, px2, py2, pz2);
        }

        public float GetAngle(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            var v1 = new Vector3(x1, y1, z1);
            var v2 = new Vector3(x2, y2, z2);
            var n1 = Vector3.Cross(v1, v2);
            var n2 = Vector3.Cross(v1, new Vector3(0, 1, 0));
            var dot = Vector3.Dot(n1, n2);
            return MathF.Acos(dot / (n1.Length() * n2.Length()));
        }

        private class Plate
        {
            public float Longitude { get; set; }

            public float Latitude { get; set; }

            public float X { get; set; }

            public float Y { get; set; }

            public float Z { get; set; }

            public float Radius { get; set; }
        }

        private class EffectivePlate
        {
            public float Longitude { get; set; }

            public float Latitude { get; set; }

            public float Radius { get; set; }

            public float Distance { get; set; }

            public int Index { get; set; }
        }
    }
}
