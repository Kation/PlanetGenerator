using System;
using System.Collections.Generic;
using System.Text;

namespace PlanetGenerator
{
    public class PlanetSettings
    {
        /// <summary>
        /// 获取或设置行星半径。
        /// <br/>
        /// 单位：公里。
        /// </summary>
        public float PlanetRadius { get; set; } = 6000f;

        /// <summary>
        /// 获取或设置行星公转半径。
        /// <br/>
        /// 单位：百万公里（1000*1000公里）
        /// </summary>
        public float OrbitRadius { get; set; } = 150f;

        /// <summary>
        /// 获取或设置太阳半径。
        /// <br/>
        /// 单位：公里。
        /// </summary>
        public float StarRadius { get; set; } = 700000f;

        /// <summary>
        /// 获取或设置行星倾角。 
        /// <br/>
        /// 0至1。0为不倾斜，0.5为水平，1为上下颠倒。
        /// </summary>
        public float AxialTilt { get; set; } = 0.125f;

        /// <summary>
        /// 获取或设置随机种子。
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// 获取或设置地形分辨率。
        /// </summary>
        public int TileResolution { get; set; } = 4096;

        ///// <summary>
        ///// Get or set the count of line that used to locate mineral by line cross.
        ///// </summary>
        //public int MineralLineCount { get; set; }

        ///// <summary>
        ///// Get or set the easing function of distance of mineral line to planet centre.
        ///// </summary>
        //public IEasingFunction MineralLineDistanceEasing { get; set; }

        /// <summary>
        /// 获取或设置板块数量。
        /// </summary>
        public int PlateCount { get; set; } = 15;

        /// <summary>
        /// 获取或设置板块最大半径。
        /// <br/>
        /// 单位：公里。
        /// </summary>
        public float PlateMaxRadius { get; set; } = 4000;

        /// <summary>
        /// 获取或设置板块最小半径。
        /// <br/>
        /// 单位：公里。
        /// </summary>
        public float PlateMinRadius { get; set; } = 500;

        /// <summary>
        /// 获取或设置板块半径最大偏差比例。
        /// <br/>
        /// 0至1。数值越大越不规则，0为圆形。
        /// </summary>
        public float PlateMaxRadiusOffset { get; set; } = 0.2f;

        /// <summary>
        /// 获取或设置纹理倍数。
        /// <br/>
        /// 最小为1，或者为2的倍数。
        /// </summary>
        public int TextureMultiple { get; set; } = 4;
    }
}
