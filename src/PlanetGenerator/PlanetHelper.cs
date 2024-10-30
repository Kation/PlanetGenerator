﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PlanetGenerator
{
    public static class PlanetHelper
    {
        private const float _TileAngleX = 1f / 10f;
        private const float _TileAngleX2 = _TileAngleX * 2;
        private const float _TileAngleY = 2f / 3f;

        /// <summary>
        /// 获取经度纬度。
        /// </summary>
        /// <param name="index">分块索引。</param>
        /// <param name="x">X分量。</param>
        /// <param name="y">Y分量。</param>
        /// <param name="longitude">经度。</param>
        /// <param name="latitude">纬度。</param>
        public static void GetLocation(int index, float x, float y, out float longitude, out float latitude)
        {
            float rootTileX, rootTileY;
            bool up = index % 2 == 0;
            rootTileX = index * _TileAngleX;
            rootTileY = up ? _TileAngleY / 4 : -_TileAngleY / 4;
            var rootTileXAngle = rootTileX * MathF.PI * 2;
            var rootTileYAngle = rootTileY * MathF.PI;
            var p = new Vector2(x, x) + new Vector2(y, -y);
            if ((x == 1 && y == 0) || (x == 0 && y == 1))
                longitude = rootTileXAngle + _TileAngleX2 * MathF.PI;
            else
            {
                if (up)
                {
                    if (p.Y >= 0)
                        longitude = rootTileXAngle + (p.X - p.Y) / (2 - p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                    else
                        longitude = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                }
                else
                {
                    if (p.Y <= 0)
                        longitude = rootTileXAngle + (p.X + p.Y) / (2 + p.Y * 2) * _TileAngleX2 * MathF.PI * 2;
                    else
                        longitude = rootTileXAngle + (p.X) / 2 * _TileAngleX2 * MathF.PI * 2;
                }
            }
            latitude = rootTileYAngle + p.Y * _TileAngleY * MathF.PI / 2;
        }

        public static void GetLocationAndPositions(int index, int zoomLevel, int length, float planetRadius, out float[] positionX, out float[] positionY, out float[] positionZ, out float[] longitudes, out float[] latitudes)
        {
            //根块
            int rootTile;
            int baseX = 0, baseY = 0;
            float rootTileX, rootTileY;
            float rootLength;
            if (zoomLevel == 0)
            {
                rootTile = index;
                rootLength = length - 1;
            }
            else
            {
                int n = 9, sn = 3;
                for (int ii = 1; ii < zoomLevel; ii++)
                {
                    n *= 9;
                    sn *= 3;
                }
                rootTile = index / n;
                var subTitle = index % n;
                baseX = length * (subTitle % sn);
                baseY = length * (subTitle / sn);
                rootLength = sn * length - 1;
            }
            bool up = rootTile % 2 == 0;
            rootTileX = rootTile * _TileAngleX;
            rootTileY = up ? _TileAngleY / 4 : -_TileAngleY / 4;
            var rootTileXAngle = rootTileX * MathF.PI * 2;
            var rootTileYAngle = rootTileY * MathF.PI;

            positionX = new float[length * length];
            positionY = new float[length * length];
            positionZ = new float[length * length];
            longitudes = new float[length * length];
            latitudes = new float[length * length];

            int i = 0;
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

                    var px = planetRadius * MathF.Cos(lat) * MathF.Cos(lng);
                    var pz = planetRadius * MathF.Cos(lat) * MathF.Sin(lng);
                    var py = planetRadius * MathF.Sin(lat);
                    longitudes[i] = lng;
                    latitudes[i] = lat;
                    positionX[i] = px;
                    positionY[i] = py;
                    positionZ[i] = -pz;
                    i++;
                }
            }
        }

        /// <summary>
        /// 获取坐标。
        /// </summary>
        /// <param name="planetRadius">星球半径。</param>
        /// <param name="longitude">经度。</param>
        /// <param name="latitude">纬度。</param>
        /// <param name="x">X坐标。</param>
        /// <param name="y">Y坐标。</param>
        /// <param name="z">Z坐标。</param>
        public static void GetPosition(float planetRadius, float longitude, float latitude, out float x, out float y, out float z)
        {
            x = planetRadius * MathF.Cos(latitude) * MathF.Cos(longitude);
            z = planetRadius * MathF.Cos(latitude) * MathF.Sin(longitude);
            y = planetRadius * MathF.Sin(latitude);
        }
    }
}