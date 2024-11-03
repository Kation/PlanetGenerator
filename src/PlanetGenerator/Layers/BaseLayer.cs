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
        private Vector3[]? _mantlePlumes;
        private const int _FaultZoneLength = 1024 * 1024;

        public void HandleBase(PlanetLayerContext context)
        {
            //计算地幔柱
            List<Vector2> mantlePlumes = new List<Vector2>();
            //半径
            var r = context.Settings.PlanetRadius / 500f;
            var rs = r * r;
            var p = (int)MathF.Ceiling(r);
            List<CubeVertex> vertices = [
                new CubeVertex(0, p, 0),
                new CubeVertex(1, p, 0),
                new CubeVertex(0, p, 1),
                new CubeVertex(1, p, 1),
                new CubeVertex(0, p - 1, 0),
                new CubeVertex(1, p - 1, 0),
                new CubeVertex(0, p - 1, 1),
                new CubeVertex(1, p - 1, 1)
            ];
            List<Cube> cubes =
            [
                new Cube(vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5], vertices[6], vertices[7]),
            ];
            //找到八分之一的立方体
            FindNextCubes(rs, cubes[0], vertices, cubes);
            var templateCubes = new List<Cube>(cubes);
            //反转至其它方向立方体
            FilpCubes(true, false, false, templateCubes, vertices, cubes);
            FilpCubes(false, false, true, templateCubes, vertices, cubes);
            FilpCubes(true, false, true, templateCubes, vertices, cubes);
            FilpCubes(false, true, false, templateCubes, vertices, cubes);
            FilpCubes(true, true, false, templateCubes, vertices, cubes);
            FilpCubes(false, true, true, templateCubes, vertices, cubes);
            FilpCubes(true, true, true, templateCubes, vertices, cubes);
            //获取顶点向量
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].Vector = context.Noise.Seed.GetHashValue(vertices[i].X, vertices[i].Y, vertices[i].Z);
            }
            var matchRR = 1f;
            List<Vector4> matches = new List<Vector4>();
            //遍历立方体查找符合条件的位置
            foreach (var cube in cubes)
            {
                var vector = cube.UpLeftTop.Vector + cube.UpRightTop.Vector + cube.UpLeftBottom.Vector + cube.UpRightBottom.Vector +
                    cube.DownLeftTop.Vector + cube.DownRightTop.Vector + cube.DownLeftBottom.Vector + cube.DownRightBottom.Vector;
                int x, y, z;
                x = cube.X < 0 ? cube.X + 1 : cube.X;
                y = cube.Y < 0 ? cube.Y + 1 : cube.Y;
                z = cube.Z < 0 ? cube.Z + 1 : cube.Z;
                vector.X += x + 0.5f;
                vector.Y += y + 0.5f;
                vector.Z += z + 0.5f;

                var value = MathF.Abs(vector.LengthSquared() - rs);
                if (value < matchRR)
                {
                    matches.Add(new Vector4(vector, MathF.Sqrt(matchRR - value)));
                }
            }
            //将坐标转换为经纬度
            _mantlePlumes = new Vector3[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                PlanetHelper.GetLocation(match.X, match.Y, match.Z, r, out var lng, out var lat);
                _mantlePlumes[i] = new Vector3(lng, lat, match.W);
            }

            _faultZones = new float[10 * _FaultZoneLength];
            for (int index = 0; index < 10; index++)
            {
                var faultZone = _faultZones.AsSpan().Slice(index * _FaultZoneLength, _FaultZoneLength);
                PlanetHelper.GetLocations(index, 0, 1024, out var longitudes, out var latitudes);

                for (int i = 0; i < _FaultZoneLength; i++)
                {
                    float value = 0;
                    for (int j = 0; j < _mantlePlumes.Length; j++)
                    {
                        var d = PlanetHelper.GetDistance(longitudes[i], latitudes[i], _mantlePlumes[j].X, _mantlePlumes[j].Y, context.Settings.PlanetRadius);
                        if (d < 100f)
                        {
                            value += 1 - d / 100f;
                            if (value > 1)
                                value = 1;
                        }
                    }
                    faultZone[i] = value;
                }
            }
            _baseHandled = true;
        }

        private static void FindNextCubes(float rs, Cube cube, List<CubeVertex> vertices, List<Cube> cubes)
        {
            var x1y1z1 = new Vector3(cube.UpRightBottom.X, cube.UpRightBottom.Y, cube.UpRightBottom.Z).LengthSquared();
            var x1y0z1 = new Vector3(cube.DownRightBottom.X, cube.DownRightBottom.Y, cube.DownRightBottom.Z).LengthSquared();
            var x0y0z1 = new Vector3(cube.DownLeftBottom.X, cube.DownLeftBottom.Y, cube.DownLeftBottom.Z).LengthSquared();
            var x1y0z0 = new Vector3(cube.DownRightTop.X, cube.DownRightTop.Y, cube.DownRightTop.Z).LengthSquared();
            //判断(x1,?,z1)或(?,y0,z1)与球面相交，相交则z面相交于球面
            if ((x1y1z1 >= rs && x1y0z1 <= rs) || (x0y0z1 <= rs && x1y0z1 >= rs))
            {
                //判断z面不存在则添加z面
                if (!cubes.Any(t => t.X == cube.X && t.Y == cube.Y && t.Z == cube.Z + 1))
                {
                    var upLeftBottom = new CubeVertex(cube.UpLeftBottom.X, cube.UpLeftBottom.Y, cube.UpLeftBottom.Z + 1);
                    var upRightBottom = new CubeVertex(cube.UpRightBottom.X, cube.UpRightBottom.Y, cube.UpRightBottom.Z + 1);
                    var downLeftBottom = new CubeVertex(cube.DownLeftBottom.X, cube.DownLeftBottom.Y, cube.DownLeftBottom.Z + 1);
                    var downRightBottom = new CubeVertex(cube.DownRightBottom.X, cube.DownRightBottom.Y, cube.DownRightBottom.Z + 1);
                    vertices.Add(upLeftBottom);
                    vertices.Add(upRightBottom);
                    vertices.Add(downLeftBottom);
                    vertices.Add(downRightBottom);
                    var zCube = new Cube(cube.UpLeftBottom, cube.UpRightBottom, upLeftBottom, upRightBottom, cube.DownLeftBottom, cube.DownRightBottom, downLeftBottom, downRightBottom);
                    cubes.Add(zCube);
                    FindNextCubes(rs, zCube, vertices, cubes);
                }
            }
            //判断(x1,?,z1)或(x1,y0,?)与球面相交，相交则x面相交于球面
            if ((x1y1z1 >= rs && x1y0z1 <= rs) || (x1y0z0 <= rs && x1y0z1 >= rs))
            {
                //判断x面不存在则添加x面
                if (!cubes.Any(t => t.X == cube.X + 1 && t.Y == cube.Y && t.Z == cube.Z))
                {
                    var upRightTop = new CubeVertex(cube.UpRightTop.X + 1, cube.UpRightTop.Y, cube.UpRightTop.Z);
                    var upRightBottom = new CubeVertex(cube.UpRightBottom.X + 1, cube.UpRightBottom.Y, cube.UpRightBottom.Z);
                    var downRightTop = new CubeVertex(cube.DownRightTop.X + 1, cube.DownRightTop.Y, cube.DownRightTop.Z);
                    var downRightBottom = new CubeVertex(cube.DownRightBottom.X + 1, cube.DownRightBottom.Y, cube.DownRightBottom.Z);
                    vertices.Add(upRightTop);
                    vertices.Add(upRightBottom);
                    vertices.Add(downRightTop);
                    vertices.Add(downRightBottom);
                    var xCube = new Cube(cube.UpRightTop, upRightTop, cube.UpRightBottom, upRightBottom, cube.DownRightTop, downRightTop, cube.DownRightBottom, downRightBottom);
                    cubes.Add(xCube);
                    FindNextCubes(rs, xCube, vertices, cubes);
                }
            }
            //判断(x1,y0,z1)位于球面外，相交则y面相交于球面
            //排除y=0
            if (cube.Y != 0 && x1y0z1 >= rs)
            {
                //判断y面不存在则添加y面
                if (!cubes.Any(t => t.X == cube.X && t.Y == cube.Y - 1 && t.Z == cube.Z))
                {
                    var downLeftTop = new CubeVertex(cube.DownLeftTop.X, cube.DownLeftTop.Y - 1, cube.DownLeftTop.Z);
                    var downLeftBottom = new CubeVertex(cube.DownLeftBottom.X, cube.DownLeftBottom.Y - 1, cube.DownLeftBottom.Z);
                    var downRightTop = new CubeVertex(cube.DownRightTop.X, cube.DownRightTop.Y - 1, cube.DownRightTop.Z);
                    var downRightBottom = new CubeVertex(cube.DownRightBottom.X, cube.DownRightBottom.Y - 1, cube.DownRightBottom.Z);
                    vertices.Add(downLeftTop);
                    vertices.Add(downLeftBottom);
                    vertices.Add(downRightTop);
                    vertices.Add(downRightBottom);
                    var xCube = new Cube(cube.DownLeftTop, cube.DownRightTop, cube.DownLeftBottom, cube.DownRightBottom, downLeftTop, downRightTop, downLeftBottom, downRightBottom);
                    cubes.Add(xCube);
                    FindNextCubes(rs, xCube, vertices, cubes);
                }
            }
        }

        private static void FilpCubes(bool flipX, bool flipY, bool flipZ, List<Cube> templateCubes, List<CubeVertex> vertices, List<Cube> cubes)
        {
            foreach (var cube in templateCubes)
            {
                var upLeftTop = cube.UpLeftTop;
                var upRightTop = cube.UpRightTop;
                var upLeftBottom = cube.UpLeftBottom;
                var upRightBottom = cube.UpRightBottom;
                var downLeftTop = cube.DownLeftTop;
                var downRightTop = cube.DownRightTop;
                var downLeftBottom = cube.DownLeftBottom;
                var downRightBottom = cube.DownRightBottom;
                if (flipX)
                {
                    var upLeftTop2 = new CubeVertex(-upRightTop.X, upRightTop.Y, upRightTop.Z);
                    var upRightTop2 = new CubeVertex(-upLeftTop.X, upLeftTop.Y, upLeftTop.Z);
                    var upLeftBottom2 = new CubeVertex(-upRightBottom.X, upRightBottom.Y, upRightBottom.Z);
                    var upRightBottom2 = new CubeVertex(-upLeftBottom.X, upRightBottom.Y, upRightBottom.Z);
                    var downLeftTop2 = new CubeVertex(-downRightTop.X, downRightTop.Y, downRightTop.Z);
                    var downRightTop2 = new CubeVertex(-downLeftTop.X, downLeftTop.Y, downLeftTop.Z);
                    var downLeftBottom2 = new CubeVertex(-downRightBottom.X, downRightBottom.Y, downRightBottom.Z);
                    var downRightBottom2 = new CubeVertex(-downLeftBottom.X, downRightBottom.Y, downRightBottom.Z);

                    upLeftTop = upLeftTop2;
                    upRightTop = upRightTop2;
                    upLeftBottom = upLeftBottom2;
                    upRightBottom = upRightBottom2;
                    downLeftTop = downLeftTop2;
                    downRightTop = downRightTop2;
                    downLeftBottom = downLeftBottom2;
                    downRightBottom = downRightBottom2;
                }
                if (flipY)
                {
                    var upLeftTop2 = new CubeVertex(downLeftTop.X, -downLeftTop.Y, downLeftTop.Z);
                    var upRightTop2 = new CubeVertex(downRightTop.X, -downRightTop.Y, downRightTop.Z);
                    var upLeftBottom2 = new CubeVertex(downLeftBottom.X, -downLeftBottom.Y, downLeftBottom.Z);
                    var upRightBottom2 = new CubeVertex(downRightBottom.X, -downRightBottom.Y, downRightBottom.Z);
                    var downLeftTop2 = new CubeVertex(upLeftTop.X, -upLeftTop.Y, upLeftTop.Z);
                    var downRightTop2 = new CubeVertex(upRightTop.X, -upRightTop.Y, upRightTop.Z);
                    var downLeftBottom2 = new CubeVertex(upLeftBottom.X, -upLeftBottom.Y, upLeftBottom.Z);
                    var downRightBottom2 = new CubeVertex(upRightBottom.X, -upRightBottom.Y, upRightBottom.Z);

                    upLeftTop = upLeftTop2;
                    upRightTop = upRightTop2;
                    upLeftBottom = upLeftBottom2;
                    upRightBottom = upRightBottom2;
                    downLeftTop = downLeftTop2;
                    downRightTop = downRightTop2;
                    downLeftBottom = downLeftBottom2;
                    downRightBottom = downRightBottom2;
                }
                if (flipZ)
                {
                    var upLeftTop2 = new CubeVertex(upLeftBottom.X, upLeftBottom.Y, -upLeftBottom.Z);
                    var upRightTop2 = new CubeVertex(upRightBottom.X, upRightBottom.Y, -upRightBottom.Z);
                    var upLeftBottom2 = new CubeVertex(upLeftTop.X, upLeftTop.Y, -upLeftTop.Z);
                    var upRightBottom2 = new CubeVertex(upRightTop.X, upRightTop.Y, -upRightTop.Z);
                    var downLeftTop2 = new CubeVertex(downLeftBottom.X, downLeftBottom.Y, -downLeftBottom.Z);
                    var downRightTop2 = new CubeVertex(downRightBottom.X, downRightBottom.Y, -downRightBottom.Z);
                    var downLeftBottom2 = new CubeVertex(downLeftTop.X, downLeftTop.Y, -downLeftTop.Z);
                    var downRightBottom2 = new CubeVertex(downRightTop.X, downRightTop.Y, -downRightTop.Z);

                    upLeftTop = upLeftTop2;
                    upRightTop = upRightTop2;
                    upLeftBottom = upLeftBottom2;
                    upRightBottom = upRightBottom2;
                    downLeftTop = downLeftTop2;
                    downRightTop = downRightTop2;
                    downLeftBottom = downLeftBottom2;
                    downRightBottom = downRightBottom2;
                }

                var existUpLeftTop = vertices.FirstOrDefault(t => t == upLeftTop);
                if (existUpLeftTop == null)
                    vertices.Add(upLeftTop);
                else
                    upLeftTop = existUpLeftTop;
                var existUpRightTop = vertices.FirstOrDefault(t => t == upRightTop);
                if (existUpRightTop == null)
                    vertices.Add(upRightTop);
                else
                    upRightTop = existUpRightTop;
                var existUpLeftBottom = vertices.FirstOrDefault(t => t == upLeftBottom);
                if (existUpLeftBottom == null)
                    vertices.Add(upLeftBottom);
                else
                    upLeftBottom = existUpLeftBottom;
                var existUpRightBottom = vertices.FirstOrDefault(t => t == upRightBottom);
                if (existUpRightBottom == null)
                    vertices.Add(upRightBottom);
                else
                    upRightBottom = existUpRightBottom;
                var existDownLeftTop = vertices.FirstOrDefault(t => t == downLeftTop);
                if (existDownLeftTop == null)
                    vertices.Add(downLeftTop);
                else
                    downLeftTop = existDownLeftTop;
                var existDownRightTop = vertices.FirstOrDefault(t => t == downRightTop);
                if (existDownRightTop == null)
                    vertices.Add(downRightTop);
                else
                    downRightTop = existDownRightTop;
                var existDownLeftBottom = vertices.FirstOrDefault(t => t == downLeftBottom);
                if (existDownLeftBottom == null)
                    vertices.Add(downLeftBottom);
                else
                    downLeftBottom = existDownLeftBottom;
                var existDownRightBottom = vertices.FirstOrDefault(t => t == downRightBottom);
                if (existDownRightBottom == null)
                    vertices.Add(downRightBottom);
                else
                    downRightBottom = existDownRightBottom;

                cubes.Add(new Cube(upLeftTop, upRightTop, upLeftBottom, upRightBottom, downLeftTop, downRightTop, downLeftBottom, downRightBottom));
            }
        }
                private class CubeVertex
        {
            public CubeVertex(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

            public Vector3 Vector { get; set; }

            public static bool operator ==(CubeVertex? left, CubeVertex? right)
            {
                if (left is null && right is null)
                    return true;
                if (left is null || right is null)
                    return false;
                return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
            }

            public static bool operator !=(CubeVertex? left, CubeVertex? right)
            {
                if (left is null && right is null)
                    return false;
                if (left is null || right is null)
                    return true;
                return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                    return false;
                if (obj is CubeVertex vertex)
                    return this == vertex;
                return false;
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
            }
        }

        private class Cube
        {
            public Cube(CubeVertex upLeftTop, CubeVertex upRightTop, CubeVertex upLeftBottom, CubeVertex upRightBottom, CubeVertex downLeftTop, CubeVertex downRightTop, CubeVertex downLeftBottom, CubeVertex downRightBottom)
            {
                UpLeftTop = upLeftTop;
                UpRightTop = upRightTop;
                UpLeftBottom = upLeftBottom;
                UpRightBottom = upRightBottom;
                DownLeftTop = downLeftTop;
                DownRightTop = downRightTop;
                DownLeftBottom = downLeftBottom;
                DownRightBottom = downRightBottom;
            }

            public CubeVertex UpLeftTop { get; set; }
            public CubeVertex UpRightTop { get; set; }
            public CubeVertex UpLeftBottom { get; set; }
            public CubeVertex UpRightBottom { get; set; }
            public CubeVertex DownLeftTop { get; set; }
            public CubeVertex DownRightTop { get; set; }
            public CubeVertex DownLeftBottom { get; set; }
            public CubeVertex DownRightBottom { get; set; }

            public int X => DownLeftTop.X;
            public int Y => DownLeftTop.Y;
            public int Z => DownLeftTop.Z;

            public static bool operator ==(Cube? left, Cube? right)
            {
                if (left is null && right is null)
                    return true;
                if (left is null || right is null)
                    return false;
                return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
            }

            public static bool operator !=(Cube? left, Cube? right)
            {
                if (left == null && right == null)
                    return false;
                if (left == null || right == null)
                    return true;
                return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                    return false;
                if (obj is Cube cube)
                    return this == cube;
                return false;
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
            }
        }

        public void HandleTile(PlanetLayerTileContext context, int index, int zoomLevel)
        {
            if (!_baseHandled)
                throw new InvalidOperationException("先处理基础部分才能处理分片。");
            var length = context.PositionX.Length;
            var data = new float[length];
            var mantlePlumeScale = context.Settings.PlanetRadius / 500f;
            var mantlePlumeScale2 = context.Settings.PlanetRadius / 250f;
            for (int i = 0; i < length; i++)
            {
                float mpv = 0;
                for (int j = 0; j < _mantlePlumes!.Length; j++)
                {
                    var d = PlanetHelper.GetDistance(context.Longitudes[i], context.Latitudes[i], _mantlePlumes[j].X, _mantlePlumes[j].Y, context.Settings.PlanetRadius);
                    const float maxD = 300f;
                    if (d < maxD)
                    {
                        var v = d / maxD;
                        if (v < 0.1f)
                            v = 0.1f;
                        mpv += MathF.Log10(v) * 10f;
                    }
                }
                if (mpv != 0f)
                {
                    var n = (context.Noise.Get(context.PositionX[i] * mantlePlumeScale, context.PositionY[i] * mantlePlumeScale, context.PositionZ[i] * mantlePlumeScale) + 1f);
                    n += (context.Noise.Get(context.PositionX[i] * mantlePlumeScale2, context.PositionY[i] * mantlePlumeScale2, context.PositionZ[i] * mantlePlumeScale2) + 1f) * 0.5f;
                    mpv *= n;
                }
                data[i] = mpv;
            }

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
                    PlanetHelper.GetLocations(index, zoomLevel, 1024, out var longitudes, out var latitudes);
                    for (int i = 0; i < _FaultZoneLength; i++)
                    {
                        float value = 0;
                        for (int j = 0; j < _mantlePlumes!.Length; j++)
                        {
                            var d = PlanetHelper.GetDistance(longitudes[i], latitudes[i], _mantlePlumes[j].X, _mantlePlumes[j].Y, context.Settings.PlanetRadius);
                            if (d < 100f)
                            {
                                value += 1 - d / 100f;
                                if (value > 1)
                                    value = 1;
                            }
                        }
                        faultZone[i] = value;
                    }
                }
            }
            context.CreateLayer(PlanetLayers.Plate, data);
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
