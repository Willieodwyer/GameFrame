﻿using System.Drawing;

namespace SpaceInvaders
{
    internal struct Star
    {
        public Point Point;
        public readonly Brush Brush;

        public Star(Point point, Brush brush)
        {
            this.Point = point;
            this.Brush = brush;
        }
    }
}