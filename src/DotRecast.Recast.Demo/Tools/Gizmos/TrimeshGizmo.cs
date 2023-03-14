﻿using DotRecast.Recast.Demo.Draw;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class TrimeshGizmo : ColliderGizmo {
    private readonly float[] vertices;
    private readonly int[] triangles;

    public TrimeshGizmo(float[] vertices, int[] triangles) {
        this.vertices = vertices;
        this.triangles = triangles;
    }

    public void render(RecastDebugDraw debugDraw) {
        debugDraw.begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < triangles.Length; i += 3) {
            int v0 = 3 * triangles[i];
            int v1 = 3 * triangles[i + 1];
            int v2 = 3 * triangles[i + 2];
            int col = GizmoHelper.getColorByNormal(vertices, v0, v1, v2);
            debugDraw.vertex(vertices[v0], vertices[v0 + 1], vertices[v0 + 2], col);
            debugDraw.vertex(vertices[v1], vertices[v1 + 1], vertices[v1 + 2], col);
            debugDraw.vertex(vertices[v2], vertices[v2 + 1], vertices[v2 + 2], col);
        }
        debugDraw.end();
    }

}