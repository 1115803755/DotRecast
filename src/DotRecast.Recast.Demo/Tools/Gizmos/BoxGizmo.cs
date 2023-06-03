using DotRecast.Core;
using DotRecast.Detour.Dynamic.Colliders;
using DotRecast.Recast.Demo.Draw;

namespace DotRecast.Recast.Demo.Tools.Gizmos;

public class BoxGizmo : IColliderGizmo
{
    private static readonly int[] TRIANLGES =
    {
        0, 1, 2, 0, 2, 3, 4, 7, 6, 4, 6, 5, 0, 4, 5, 0, 5, 1, 1, 5, 6, 1, 6, 2,
        2, 6, 7, 2, 7, 3, 4, 0, 3, 4, 3, 7
    };

    private static readonly RcVec3f[] VERTS =
    {
        RcVec3f.Of( -1f, -1f, -1f),
        RcVec3f.Of( 1f, -1f, -1f),
        RcVec3f.Of( 1f, -1f, 1f),
        RcVec3f.Of( -1f, -1f, 1f),
        RcVec3f.Of( -1f, 1f, -1f),
        RcVec3f.Of( 1f, 1f, -1f),
        RcVec3f.Of( 1f, 1f, 1f),
        RcVec3f.Of( -1f, 1f, 1f),
    };

    private readonly float[] vertices = new float[8 * 3];
    private readonly RcVec3f center;
    private readonly RcVec3f[] halfEdges;

    public BoxGizmo(RcVec3f center, RcVec3f extent, RcVec3f forward, RcVec3f up) :
        this(center, BoxCollider.GetHalfEdges(up, forward, extent))
    {
    }

    public BoxGizmo(RcVec3f center, RcVec3f[] halfEdges)
    {
        this.center = center;
        this.halfEdges = halfEdges;
        for (int i = 0; i < 8; ++i)
        {
            float s0 = (i & 1) != 0 ? 1f : -1f;
            float s1 = (i & 2) != 0 ? 1f : -1f;
            float s2 = (i & 4) != 0 ? 1f : -1f;
            vertices[i * 3 + 0] = center.x + s0 * halfEdges[0].x + s1 * halfEdges[1].x + s2 * halfEdges[2].x;
            vertices[i * 3 + 1] = center.y + s0 * halfEdges[0].y + s1 * halfEdges[1].y + s2 * halfEdges[2].y;
            vertices[i * 3 + 2] = center.z + s0 * halfEdges[0].z + s1 * halfEdges[1].z + s2 * halfEdges[2].z;
        }
    }

    public void Render(RecastDebugDraw debugDraw)
    {
        var trX = RcVec3f.Of(halfEdges[0].x, halfEdges[1].x, halfEdges[2].x);
        var trY = RcVec3f.Of(halfEdges[0].y, halfEdges[1].y, halfEdges[2].y);
        var trZ = RcVec3f.Of(halfEdges[0].z, halfEdges[1].z, halfEdges[2].z);
        float[] vertices = new float[8 * 3];
        for (int i = 0; i < 8; i++)
        {
            vertices[i * 3 + 0] = RcVec3f.Dot(VERTS[i], trX) + center.x;
            vertices[i * 3 + 1] = RcVec3f.Dot(VERTS[i], trY) + center.y;
            vertices[i * 3 + 2] = RcVec3f.Dot(VERTS[i], trZ) + center.z;
        }

        debugDraw.Begin(DebugDrawPrimitives.TRIS);
        for (int i = 0; i < 12; i++)
        {
            int col = DebugDraw.DuRGBA(200, 200, 50, 160);
            if (i == 4 || i == 5 || i == 8 || i == 9)
            {
                col = DebugDraw.DuRGBA(160, 160, 40, 160);
            }
            else if (i > 4)
            {
                col = DebugDraw.DuRGBA(120, 120, 30, 160);
            }

            for (int j = 0; j < 3; j++)
            {
                int v = TRIANLGES[i * 3 + j] * 3;
                debugDraw.Vertex(vertices[v], vertices[v + 1], vertices[v + 2], col);
            }
        }

        debugDraw.End();
    }
}
