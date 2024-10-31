using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public sealed class HashSeed : INoiseSeed
    {
        public float GetGrad(int hash, float offsetX)
        {
            hash ^= hash >> 15;
            return ((hash & 0xff) / 127.5f - 1f) * offsetX;
        }

        public float GetGrad(int hash, float offsetX, float offsetY)
        {
            hash ^= hash >> 15;
            hash = (hash & 0xffff);
            var p3 = new Vector3(((float)hash / 0xffff) + 1) * _V3;
            p3 = new Vector3(p3.X - MathF.Floor(p3.X), p3.Y - MathF.Floor(p3.Y), p3.Z - MathF.Floor(p3.Z));
            p3 += new Vector3(Vector3.Dot(p3, new Vector3(p3.Y + 33.33f, p3.Z + 33.33f, p3.X + 33.33f)));
            var p2 = new Vector2(p3.X + p3.Y, p3.X + p3.Z) * new Vector2(p3.Z, p3.Y);
            p2 = Vector2.Normalize(new Vector2(p2.X - MathF.Floor(p2.X), p2.Y - MathF.Floor(p2.Y)));
            //hash ^= hash >> 15;
            //var vector = Vector2.Normalize(new Vector2((hash & 0xff) / 127.5f - 1f, ((hash & 0xff00) >> 8) / 127.5f - 1f));
            return p2.X * offsetX + p2.Y * offsetY;
        }

        public float GetGrad(int hash, float offsetX, float offsetY, float offsetZ)
        {
            throw new NotImplementedException();
        }

        public float GetGrad(int hash, float offsetX, float offsetY, float offsetZ, float offsetW)
        {
            throw new NotImplementedException();
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX)
        {
            throw new NotImplementedException();
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY)
        {
            throw new NotImplementedException();
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ)
        {
            throw new NotImplementedException();
        }

        public Vector<float> GetGrad(Vector<int> hash, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW)
        {
            throw new NotImplementedException();
        }

        private static readonly Vector3 _V3 = new Vector3(0.1031f, 0.1030f, 0.973f);
        public float GetHashGrad(int x, int y, float offsetX, float offsetY)
        {
            var p3 = new Vector3(x, y, x) * _V3;
            p3 = new Vector3(MathF.Abs(p3.X - MathF.Floor(p3.X)), MathF.Abs(p3.Y - MathF.Floor(p3.Y)), MathF.Abs(p3.Z - MathF.Floor(p3.Z)));
            p3 += new Vector3(Vector3.Dot(p3, new Vector3(p3.Y + 33.33f, p3.Z + 33.33f, p3.X + 33.33f)));
            var p2 = new Vector2(p3.X + p3.Y, p3.X + p3.Z) * new Vector2(p3.Z, p3.Y);
            p2 = new Vector2((p2.X - MathF.Floor(p2.X)) * 2 - 1, (p2.Y - MathF.Floor(p2.Y)) * 2 - 1);
            return p2.X * offsetX + p2.Y * offsetY;
        }

        private static readonly Vector<float>
            _Rx = new Vector<float>(0.1031f),
            _Ry = new Vector<float>(0.1030f),
            _Rz = new Vector<float>(0.973f),
            _R3 = new Vector<float>(33.33f),
            _V2 = new Vector<float>(2f);

        public Vector<float> GetHashGrad(Vector<int> x, Vector<int> y, Vector<float> offsetX, Vector<float> offsetY)
        {
            var fx = Vector.ConvertToSingle(x);
            var px = fx * _Rx;
            var py = Vector.ConvertToSingle(y) * _Ry;
            var pz = fx * _Rz;
            px = Vector.Abs(px - Vector.Floor(px));
            py = Vector.Abs(py - Vector.Floor(py));
            pz = Vector.Abs(pz - Vector.Floor(pz));

            var c = px * (py + _R3) + py * (pz + _R3) + pz * (px + _R3);
            px += c;
            py += c;
            pz += c;

            var tx = (px + py) * pz;
            var ty = (px + pz) * py;
            tx -= Vector.Floor(tx);
            ty -= Vector.Floor(ty);
            tx = tx * _V2 - Vector<float>.One;
            ty = ty * _V2 - Vector<float>.One;

            return tx * offsetX + ty * offsetY;
        }

        public int Hash(int value)
        {
            return unchecked(value * 0x27d4eb2d);
        }

        private static readonly Vector<int> _Prime = new Vector<int>(71470727);
        private static readonly Vector<int> _Mask = new Vector<int>(0xff);
        public Vector<int> Hash(Vector<int> value)
        {
            return value ^ _Prime;
        }
    }
}
