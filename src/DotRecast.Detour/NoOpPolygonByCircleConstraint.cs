﻿using DotRecast.Core;

namespace DotRecast.Detour
{
    public class NoOpPolygonByCircleConstraint : IPolygonByCircleConstraint
    {
        public float[] Aply(float[] polyVerts, Vector3f circleCenter, float radius)
        {
            return polyVerts;
        }
    }
}