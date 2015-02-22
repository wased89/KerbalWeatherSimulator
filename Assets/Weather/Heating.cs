using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GeodesicGrid;

namespace Weather
{
    class Heating
    {
        private const float SBC = 0.000000056704f;
        private const double KSun = 3.17488630646816122100221E24;
        private const double LSun = 6.8930893418241730829829167104E24;






        internal static double calculateBodyKSun(double radius)
        {
            double numb;
            numb = KSun / (4 * Math.PI * (radius * radius));
            return numb;
        }

        internal static float ToTheFourth(float numb)
        {
            return numb * numb * numb * numb;
        }
    }
}
