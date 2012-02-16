using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Metroball.Lib
{
    public static class ExtensionMethods
    {
        public static bool Intersects(this Rectangle rectangle, Vector2 point)
        {
            return point.X >= rectangle.X && point.X <= rectangle.X + rectangle.Width &&
                   point.Y >= rectangle.Y && point.Y <= rectangle.Y + rectangle.Height;
        }

        public static float Clip(this float num, float min, float max)
        {
            if (num < min)
            {
                return min;
            }

            if (num > max)
            {
                return max;
            }

            return num;
        }

        public static int ToUnixTime(this DateTime dt)
        {
            return (int)(dt - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
