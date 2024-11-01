using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public sealed class HashSeed : INoiseSeed
    {
        private static readonly Vector3 _V3 = new Vector3(0.1031f, 0.1030f, 0.973f);
        private static readonly Vector4 _V4 = new Vector4(0.1031f, 0.1030f, 0.973f, 0.1099f);
        private static readonly Vector<float>
            _Rx = new Vector<float>(0.1031f),
            _Ry = new Vector<float>(0.1030f),
            _Rz = new Vector<float>(0.973f),
            _Rw = new Vector<float>(0.1099f),
            _R3 = new Vector<float>(33.33f),
            _V2 = new Vector<float>(2f);

        public float GetGrad(int x, float offsetX)
        {
            return GetHashValue(x) * offsetX;
        }

        public float GetGrad(int x, int y, float offsetX, float offsetY)
        {
            var p2 = GetHashValue(x, y);
            return p2.X * offsetX + p2.Y * offsetY;
        }

        public float GetGrad(int x, int y, int z, float offsetX, float offsetY, float offsetZ)
        {
            var p3 = GetHashValue(x, y, z);
            return Vector3.Dot(p3, new Vector3(offsetX, offsetY, offsetZ));
        }

        public float GetGrad(int x, int y, int z, int w, float offsetX, float offsetY, float offsetZ, float offsetW)
        {
            var p4 = GetHashValue(x, y, z, w);
            return Vector4.Dot(p4, new Vector4(offsetX, offsetY, offsetZ, offsetW));
        }

        public Vector<float> GetGrad(Vector<int> x, Vector<float> offsetX)
        {
            var p = Vector.ConvertToSingle(x) * _Rx;
            p -= Vector.Floor(p);
            p *= p + _R3;
            p *= p + p;
            p -= Vector.Floor(p);
            return (p * _V2 - Vector<float>.One) * offsetX;
        }

        public Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<float> offsetX, Vector<float> offsetY)
        {
            var fx = Vector.ConvertToSingle(x);
            var px = fx * _Rx;
            var py = Vector.ConvertToSingle(y) * _Ry;
            var pz = fx * _Rz;
            px -= Vector.Floor(px);
            py -= Vector.Floor(py);
            pz -= Vector.Floor(pz);

            var s = px * (py + _R3) + py * (pz + _R3) + pz * (px + _R3);
            px += s;
            py += s;
            pz += s;

            var tx = (px + py) * pz;
            var ty = (px + pz) * py;
            tx -= Vector.Floor(tx);
            ty -= Vector.Floor(ty);
            tx = tx * _V2 - Vector<float>.One;
            ty = ty * _V2 - Vector<float>.One;

            return tx * offsetX + ty * offsetY;
        }

        public Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<int> z, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ)
        {
            var px = Vector.ConvertToSingle(x) * _Rx;
            var py = Vector.ConvertToSingle(y) * _Ry;
            var pz = Vector.ConvertToSingle(z) * _Rz;
            px -= Vector.Floor(px);
            py -= Vector.Floor(py);
            pz -= Vector.Floor(pz);

            var s = px * (py + _R3) + py * (px + _R3) + pz * (pz + _R3);
            px += s;
            py += s;
            pz += s;

            var tx = (px + py) * pz;
            var ty = (px + px) * py;
            var tz = (py + px) * px;
            tx -= Vector.Floor(tx);
            ty -= Vector.Floor(ty);
            tz -= Vector.Floor(tz);
            tx = tx * _V2 - Vector<float>.One;
            ty = ty * _V2 - Vector<float>.One;
            tz = tz * _V2 - Vector<float>.One;

            return tx * offsetX + ty * offsetY + tz * offsetZ;
        }

        public Vector<float> GetGrad(Vector<int> x, Vector<int> y, Vector<int> z, Vector<int> w, Vector<float> offsetX, Vector<float> offsetY, Vector<float> offsetZ, Vector<float> offsetW)
        {
            var px = Vector.ConvertToSingle(x) * _Rx;
            var py = Vector.ConvertToSingle(y) * _Ry;
            var pz = Vector.ConvertToSingle(z) * _Rz;
            var pw = Vector.ConvertToSingle(w) * _Rw;
            px -= Vector.Floor(px);
            py -= Vector.Floor(py);
            pz -= Vector.Floor(pz);
            pw -= Vector.Floor(pw);

            var s = px * (pw + _R3) + py * (pz + _R3) + pz * (px + _R3) + pw * (py * _R3);
            px += s;
            py += s;
            pz += s;
            pw += s;

            var tx = (px + py) * pz;
            var ty = (px + pz) * py;
            var tz = (py + pz) * pw;
            var tw = (pz + pw) * px;
            tx -= Vector.Floor(tx);
            ty -= Vector.Floor(ty);
            tz -= Vector.Floor(tz);
            tw -= Vector.Floor(tw);
            tx = tx * _V2 - Vector<float>.One;
            ty = ty * _V2 - Vector<float>.One;
            tz = tz * _V2 - Vector<float>.One;
            tw = tw * _V2 - Vector<float>.One;

            return tx * offsetX + ty * offsetY + tz * offsetZ + tw * offsetW;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetHashValue(int x)
        {
            var p = x * 0.1031f;
            p -= MathF.Floor(p);
            p *= p + 33.33f;
            p *= p + p;
            p = p - MathF.Floor(p);
            return p * 2f - 1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetHashValue(int x, int y)
        {
            var p3 = new Vector3(x, y, x) * _V3;
            p3 = new Vector3(p3.X - MathF.Floor(p3.X), p3.Y - MathF.Floor(p3.Y), p3.Z - MathF.Floor(p3.Z));
            p3 += new Vector3(Vector3.Dot(p3, new Vector3(p3.Y + 33.33f, p3.Z + 33.33f, p3.X + 33.33f)));
            var p2 = new Vector2(p3.X + p3.Y, p3.X + p3.Z) * new Vector2(p3.Z, p3.Y);
            p2 -= new Vector2(MathF.Floor(p2.X), MathF.Floor(p2.Y));
            p2 = p2 * new Vector2(2) - Vector2.One;
            return p2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetHashValue(int x, int y, int z)
        {
            var p3 = new Vector3(x, y, x) * _V3;
            p3 = new Vector3(p3.X - MathF.Floor(p3.X), p3.Y - MathF.Floor(p3.Y), p3.Z - MathF.Floor(p3.Z));
            p3 += new Vector3(Vector3.Dot(p3, new Vector3(p3.Y + 33.33f, p3.X + 33.33f, p3.Z + 33.33f)));
            p3 = (new Vector3(p3.X, p3.X, p3.Y) + new Vector3(p3.Y, p3.X, p3.X)) * new Vector3(p3.Z, p3.Y, p3.X);
            p3 -= new Vector3(MathF.Floor(p3.X), MathF.Floor(p3.Y), MathF.Floor(p3.Z));
            p3 = p3 * new Vector3(2) - Vector3.One;
            return p3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 GetHashValue(int x, int y, int z, int w)
        {
            var p4 = new Vector4(x, y, z, w) * _V4;
            p4 = new Vector4(p4.X - MathF.Floor(p4.X), p4.Y - MathF.Floor(p4.Y), p4.Z - MathF.Floor(p4.Z), p4.W - MathF.Floor(p4.W));
            p4 += new Vector4(Vector4.Dot(p4, new Vector4(p4.W + 33.33f, p4.Z + 33.33f, p4.X + 33.33f, p4.Y + 33.33f)));
            p4 = (new Vector4(p4.X, p4.X, p4.Y, p4.Z) + new Vector4(p4.Y, p4.Z, p4.Z, p4.W)) * new Vector4(p4.Z, p4.Y, p4.W, p4.X);
            p4 -= new Vector4(MathF.Floor(p4.X), MathF.Floor(p4.Y), MathF.Floor(p4.Z), MathF.Floor(p4.W));
            p4 = p4 * new Vector4(2) - Vector4.One;
            return p4;
        }
    }
}
