using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public static class StrideTileExtensions
    {
        public static Model GenerateFlatMesh(this Tile tile, GraphicsDevice device)
        {
            var length = tile.Settings.TileResolution;
            
            var points = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            int i = 0;
            var terrain = tile.Terrain.Span;
            var cell = 2f / (length - 1);
            for (int y = 0; y < length - 1; y++)
            {
                for (int x = 0; x < length - 1; x++)
                {
                    var i1 = x + y * length;
                    var i2 = i1 + 1;
                    var i4 = i1 + length;
                    var i3 = i4 + 1;
                    var t1 = terrain[i1];
                    var t2 = terrain[i2];
                    var t3 = terrain[i3];
                    var t4 = terrain[i4];
                    var s1 = (t1 + t2) * 0.5f;
                    var s2 = (t4 + t3) * 0.5f;
                    var h = (s1 + s2) * 0.5f;
                    var x1 = x * cell - 1;
                    var x2 = x1 + cell;
                    var x3 = x1 + cell / 2;
                    var y1 = y * cell - 1;
                    var y2 = y1 + cell;
                    var y3 = y1 + cell / 2;
                    var p3 = new Vector3(x3, h, y3);
                    {
                        var p1 = new Vector3(x1, t1, y1);
                        var p2 = new Vector3(x2, t2, y1);
                        points.Add(p3);
                        points.Add(p2);
                        points.Add(p1);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p2 - p1, p3 - p1);
                        normal.Normalize();
                        normals.Add(normal);
                        normals.Add(normal);
                        normals.Add(normal);
                    }
                    {
                        var p1 = new Vector3(x2, t2, y1);
                        var p2 = new Vector3(x2, t3, y2);
                        points.Add(p3);
                        points.Add(p2);
                        points.Add(p1);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p2 - p1, p3 - p1);
                        normal.Normalize();
                        normals.Add(normal);
                        normals.Add(normal);
                        normals.Add(normal);
                    }
                    {
                        var p1 = new Vector3(x2, t3, y2);
                        var p2 = new Vector3(x1, t4, y2);
                        points.Add(p3);
                        points.Add(p2);
                        points.Add(p1);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p2 - p1, p3 - p1);
                        normal.Normalize();
                        normals.Add(normal);
                        normals.Add(normal);
                        normals.Add(normal);
                    }
                    {
                        var p1 = new Vector3(x1, t4, y2);
                        var p2 = new Vector3(x1, t1, y1);
                        points.Add(p3);
                        points.Add(p2);
                        points.Add(p1);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p2 - p1, p3 - p1);
                        normal.Normalize();
                        normals.Add(normal);
                        normals.Add(normal);
                        normals.Add(normal);
                    }
                }
            }
            var pointBuffer = Stride.Graphics.Buffer.Vertex.New(device, points.ToArray(), GraphicsResourceUsage.Dynamic);
            var indexBuffer = Stride.Graphics.Buffer.Index.New(device, triangles.ToArray());
            var mesh = new Stride.Rendering.Mesh
            {
                Draw = new Stride.Rendering.MeshDraw
                {
                    /* Vertex buffer and index buffer setup */
                    PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                    DrawCount = triangles.Count,
                    IndexBuffer = new IndexBufferBinding(indexBuffer, true, triangles.Count),
                    VertexBuffers = new[] { new VertexBufferBinding(pointBuffer,
                                  VertexPositionTexture.Layout, pointBuffer.ElementCount) }                    
                }
            };
            var model = new Model();
            model.Meshes.Add(mesh);
            return model;
        }

        private const float _TileAngleX = 1f / 10f;
        private const float _TileAngleX2 = _TileAngleX * 2f;
        private const float _TileAngleY = 2f / 3f;
        public static Model GenerateSphereModel(this Tile tile, GraphicsDevice device)
        {
            var length = tile.Settings.TileResolution;
            var vertexs = new List<VertexPositionNormalTexture>();
            //var points = new List<Vector3>();
            var triangles = new List<int>();
            //var normals = new List<Vector3>();
            int i = 0;
            var terrain = tile.Terrain.Span;

            int rootTile;
            int baseX = 0, baseY = 0;
            float rootTileX, rootTileY;
            float rootLength;
            if (tile.ZoomLevel == 0)
            {
                rootTile = tile.Index;
                rootLength = length - 1;
            }
            else
            {
                int n = 9, sn = 3;
                for (int ii = 1; ii < tile.ZoomLevel; ii++)
                {
                    n *= 9;
                    sn *= 3;
                }
                rootTile = tile.Index / n;
                var subTitle = tile.Index % n;
                baseX = length * (subTitle % sn);
                baseY = length * (subTitle / sn);
                rootLength = sn * length - 1;
            }
            bool up = rootTile % 2 == 0;
            rootTileX = rootTile * _TileAngleX;
            rootTileY = up ? _TileAngleY / 4 : -_TileAngleY / 4;
            var rootTileXAngle = rootTileX * MathF.PI * 2;
            var rootTileYAngle = rootTileY * MathF.PI;

            float[,] positionX = new float[length, length];
            float[,] positionY = new float[length, length];
            float[,] positionZ = new float[length, length];

            float[,] positionXm = new float[length - 1, length - 1];
            float[,] positionYm = new float[length - 1, length - 1];
            float[,] positionZm = new float[length - 1, length - 1];

            for (int b = 0; b < length; b++)
            {
                for (int a = 0; a < length; a++)
                {
                    var x = (baseX + a) / rootLength;
                    var y = (baseY + b) / rootLength;
                    var p = new Vector2(x, x) + new Vector2(y, -y);
                    float lng;
                    if ((x == 1 && y == 0) || (x == 0 && y == 1))
                        lng = rootTileXAngle + _TileAngleX2 * MathF.PI;
                    else
                    {
                        if (up)
                        {
                            if (p.Y >= 0)
                                lng = rootTileXAngle + (p.X - p.Y) / (2 - p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }
                        else
                        {
                            if (p.Y <= 0)
                                lng = rootTileXAngle + (p.X + p.Y) / (2 + p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }
                    }
                    var lat = rootTileYAngle + p.Y * _TileAngleY * MathF.PI / 2;

                    var px = MathF.Cos(lat) * MathF.Cos(lng);
                    var pz = MathF.Cos(lat) * MathF.Sin(lng);
                    var py = MathF.Sin(lat);
                    positionX[a, b] = px;
                    positionY[a, b] = py;
                    positionZ[a, b] = -pz;

                    if (a != length - 1 && b != length - 1)
                    {
                        x = (baseX + a + 0.5f) / rootLength;
                        y = (baseY + b + 0.5f) / rootLength;
                        p = new Vector2(x, x) + new Vector2(y, -y);
                        if (up)
                        {
                            if (p.Y >= 0)
                                lng = rootTileXAngle + (p.X - p.Y) / (2 - p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }
                        else
                        {
                            if (p.Y <= 0)
                                lng = rootTileXAngle + (p.X + p.Y) / (2 + p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                            else
                                lng = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                        }

                        px = MathF.Cos(lat) * MathF.Cos(lng);
                        pz = MathF.Cos(lat) * MathF.Sin(lng);
                        py = MathF.Sin(lat);
                        positionXm[a, b] = px;
                        positionYm[a, b] = py;
                        positionZm[a, b] = -pz;
                    }
                }
            }

            for (int b = 0; b < length - 1; b++)
            {
                for (int a = 0; a < length - 1; a++)
                {
                    var i1 = a + b * length;
                    var i2 = i1 + 1;
                    var i4 = i1 + length;
                    var i3 = i4 + 1;
                    var t1 = terrain[i1] * 10;
                    var t2 = terrain[i2] * 10;
                    var t3 = terrain[i3] * 10;
                    var t4 = terrain[i4] * 10;
                    var s1 = (t1 + t2) * 0.5f;
                    var s2 = (t4 + t3) * 0.5f;
                    var h = (s1 + s2) * 0.5f;

                    var p1 = new Vector3(positionX[a, b] * (tile.Settings.PlanetRadius + t1), positionY[a, b] * (tile.Settings.PlanetRadius + t1), positionZ[a, b] * (tile.Settings.PlanetRadius + t1));
                    var p2 = new Vector3(positionX[a + 1, b] * (tile.Settings.PlanetRadius + t2), positionY[a + 1, b] * (tile.Settings.PlanetRadius + t2), positionZ[a + 1, b] * (tile.Settings.PlanetRadius + t2));
                    var p3 = new Vector3(positionX[a + 1, b + 1] * (tile.Settings.PlanetRadius + t3), positionY[a + 1, b + 1] * (tile.Settings.PlanetRadius + t3), positionZ[a + 1, b + 1] * (tile.Settings.PlanetRadius + t3));
                    var p4 = new Vector3(positionX[a, b + 1] * (tile.Settings.PlanetRadius + t4), positionY[a, b + 1] * (tile.Settings.PlanetRadius + t4), positionZ[a, b + 1] * (tile.Settings.PlanetRadius + t4));

                    var p5 = new Vector3(positionXm[a, b] * (tile.Settings.PlanetRadius + h), positionYm[a, b] * (tile.Settings.PlanetRadius + h), positionZm[a, b] * (tile.Settings.PlanetRadius + h));
                    {
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p2 - p5, p1 - p5);
                        normal.Normalize();
                        vertexs.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p5, normal, new Vector2(0, 0)));
                    }
                    {
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p3 - p5, p2 - p5);
                        normal.Normalize();
                        vertexs.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p5, normal, new Vector2(0, 0)));
                    }
                    {
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p4 - p5, p3 - p5);
                        normal.Normalize();
                        vertexs.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p4, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p5, normal, new Vector2(0, 0)));
                    }
                    {
                        triangles.Add(i++);
                        triangles.Add(i++);
                        triangles.Add(i++);
                        var normal = Vector3.Cross(p1 - p5, p4 - p5);
                        normal.Normalize();
                        vertexs.Add(new VertexPositionNormalTexture(p4, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(0, 0)));
                        vertexs.Add(new VertexPositionNormalTexture(p5, normal, new Vector2(0, 0)));
                    }
                }
            }
            var pointBuffer = Stride.Graphics.Buffer.Vertex.New(device, vertexs.ToArray(), GraphicsResourceUsage.Dynamic);
            var indexBuffer = Stride.Graphics.Buffer.Index.New(device, triangles.ToArray());
            var mesh = new Stride.Rendering.Mesh
            {
                Draw = new Stride.Rendering.MeshDraw
                {
                    /* Vertex buffer and index buffer setup */
                    PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                    DrawCount = triangles.Count,
                    IndexBuffer = new IndexBufferBinding(indexBuffer, true, triangles.Count),
                    VertexBuffers = new[] { new VertexBufferBinding(pointBuffer,
                                  VertexPositionNormalTexture.Layout, pointBuffer.ElementCount) }
                }
            };
            var material = new MaterialInstance(Material.New(device, new MaterialDescriptor
            {
                Attributes =
                    {
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor { Value = new Color4(0.75f, 0.75f, 0.75f, 1f) })
                    }
            }));
            var model = new Model();
            model.Meshes.Add(mesh);
            model.Materials.Add(material);
            return model;
        }
    }
}
