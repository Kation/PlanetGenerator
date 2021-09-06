using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlanetGenerator.Unity
{
    public static class WorldGeneratorExtensions
    {
        public static void GeneratePlanet(this WorldGenerator generator, GameObject gameObject, float scaleToRadius, int subdivide = 1)
        {
            var m_Vertices = new List<Vector3>();
            var m_Polygons = new List<PolygonIndex>();

            float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
            m_Vertices.Add(new Vector3(-1, t, 0).normalized);
            m_Vertices.Add(new Vector3(1, t, 0).normalized);
            m_Vertices.Add(new Vector3(-1, -t, 0).normalized);
            m_Vertices.Add(new Vector3(1, -t, 0).normalized);
            m_Vertices.Add(new Vector3(0, -1, t).normalized);
            m_Vertices.Add(new Vector3(0, 1, t).normalized);
            m_Vertices.Add(new Vector3(0, -1, -t).normalized);
            m_Vertices.Add(new Vector3(0, 1, -t).normalized);
            m_Vertices.Add(new Vector3(t, 0, -1).normalized);
            m_Vertices.Add(new Vector3(t, 0, 1).normalized);
            m_Vertices.Add(new Vector3(-t, 0, -1).normalized);
            m_Vertices.Add(new Vector3(-t, 0, 1).normalized);

            m_Polygons.Add(new PolygonIndex(0, 11, 5));
            m_Polygons.Add(new PolygonIndex(0, 5, 1));
            m_Polygons.Add(new PolygonIndex(0, 1, 7));
            m_Polygons.Add(new PolygonIndex(0, 7, 10));
            m_Polygons.Add(new PolygonIndex(0, 10, 11));
            m_Polygons.Add(new PolygonIndex(1, 5, 9));
            m_Polygons.Add(new PolygonIndex(5, 11, 4));
            m_Polygons.Add(new PolygonIndex(11, 10, 2));
            m_Polygons.Add(new PolygonIndex(10, 7, 6));
            m_Polygons.Add(new PolygonIndex(7, 1, 8));
            m_Polygons.Add(new PolygonIndex(3, 9, 4));
            m_Polygons.Add(new PolygonIndex(3, 4, 2));
            m_Polygons.Add(new PolygonIndex(3, 2, 6));
            m_Polygons.Add(new PolygonIndex(3, 6, 8));
            m_Polygons.Add(new PolygonIndex(3, 8, 9));
            m_Polygons.Add(new PolygonIndex(4, 9, 5));
            m_Polygons.Add(new PolygonIndex(2, 4, 11));
            m_Polygons.Add(new PolygonIndex(6, 2, 10));
            m_Polygons.Add(new PolygonIndex(8, 6, 7));
            m_Polygons.Add(new PolygonIndex(9, 8, 1));


            var midPointCache = new Dictionary<int, int>();

            int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
            {
                // We create a key out of the two original indices
                // by storing the smaller index in the upper two bytes
                // of an integer, and the larger index in the lower two
                // bytes. By sorting them according to whichever is smaller
                // we ensure that this function returns the same result
                // whether you call
                // GetMidPointIndex(cache, 5, 9)
                // or...
                // GetMidPointIndex(cache, 9, 5)

                int smallerIndex = Mathf.Min(indexA, indexB);
                int greaterIndex = Mathf.Max(indexA, indexB);
                int key = (smallerIndex << 16) + greaterIndex;

                // If a midpoint is already defined, just return it.

                int ret;
                if (cache.TryGetValue(key, out ret))
                    return ret;

                // If we're here, it's because a midpoint for these two
                // vertices hasn't been created yet. Let's do that now!

                Vector3 p1 = m_Vertices[indexA];
                Vector3 p2 = m_Vertices[indexB];
                Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

                ret = m_Vertices.Count;
                m_Vertices.Add(middle);

                // Add our new midpoint to the cache so we don't have
                // to do this again. =)

                cache.Add(key, ret);
                return ret;
            }

            for (int i = 0; i < subdivide; i++)
            {
                var newPolys = new List<PolygonIndex>();
                foreach (var poly in m_Polygons)
                {
                    int a = poly.m_Vertices[0];
                    int b = poly.m_Vertices[1];
                    int c = poly.m_Vertices[2];

                    // Use GetMidPointIndex to either create a
                    // new vertex between two old vertices, or
                    // find the one that was already created.

                    int ab = GetMidPointIndex(midPointCache, a, b);
                    int bc = GetMidPointIndex(midPointCache, b, c);
                    int ca = GetMidPointIndex(midPointCache, c, a);

                    // Create the four new polygons using our original
                    // three vertices, and the three new midpoints.
                    newPolys.Add(new PolygonIndex(a, ab, ca));
                    newPolys.Add(new PolygonIndex(b, bc, ab));
                    newPolys.Add(new PolygonIndex(c, ca, bc));
                    newPolys.Add(new PolygonIndex(ab, bc, ca));
                }
                // Replace all our old polygons with the new set of
                // subdivided ones.
                m_Polygons = newPolys;
            }

            var m_Material = new Material(Shader.Find("Standard"));
            m_Material.color = Color.white;

            MeshRenderer surfaceRenderer = gameObject.AddComponent<MeshRenderer>();
            surfaceRenderer.material = m_Material;

            Mesh terrainMesh = new Mesh();

            int vertexCount = m_Polygons.Count * 3;

            int[] indices = new int[vertexCount];

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Color32[] colors = new Color32[vertexCount];

            Color32 green = new Color32(20, 255, 30, 255);
            Color32 brown = new Color32(220, 150, 70, 255);

            for (int i = 0; i < m_Polygons.Count; i++)
            {
                var poly = m_Polygons[i];

                indices[i * 3 + 0] = i * 3 + 0;
                indices[i * 3 + 1] = i * 3 + 1;
                indices[i * 3 + 2] = i * 3 + 2;

                vertices[i * 3 + 0] = m_Vertices[poly.m_Vertices[0]];
                vertices[i * 3 + 1] = m_Vertices[poly.m_Vertices[1]];
                vertices[i * 3 + 2] = m_Vertices[poly.m_Vertices[2]];

                Color32 polyColor = Color32.Lerp(green, brown, UnityEngine.Random.Range(0.0f, 1.0f));

                colors[i * 3 + 0] = polyColor;
                colors[i * 3 + 1] = polyColor;
                colors[i * 3 + 2] = polyColor;

                normals[i * 3 + 0] = m_Vertices[poly.m_Vertices[0]];
                normals[i * 3 + 1] = m_Vertices[poly.m_Vertices[1]];
                normals[i * 3 + 2] = m_Vertices[poly.m_Vertices[2]];
            }

            terrainMesh.vertices = vertices;
            terrainMesh.normals = normals;
            terrainMesh.colors32 = colors;

            terrainMesh.SetTriangles(indices, 0);

            MeshFilter terrainFilter = gameObject.AddComponent<MeshFilter>();
            terrainFilter.mesh = terrainMesh;
        }

        private class PolygonIndex
        {
            public List<int> m_Vertices;

            public PolygonIndex(int a, int b, int c)
            {
                m_Vertices = new List<int>() { a, b, c };
            }
        }
    }
}
