using DotRecast.Core;

namespace DotRecast.Recast.Demo.Draw;

public class OpenGLVertex
{
    private readonly float x;
    private readonly float y;
    private readonly float z;
    private readonly int color;
    private readonly float u;
    private readonly float v;

    public OpenGLVertex(float[] pos, float[] uv, int color) :
        this(pos[0], pos[1], pos[2], uv[0], uv[1], color)
    {
    }

    public OpenGLVertex(float[] pos, int color) :
        this(pos[0], pos[1], pos[2], 0f, 0f, color)
    {
    }

    public OpenGLVertex(float x, float y, float z, int color) :
        this(x, y, z, 0f, 0f, color)
    {
    }

    public OpenGLVertex(float x, float y, float z, float u, float v, int color)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.u = u;
        this.v = v;
        this.color = color;
    }

    public void store(ByteBuffer buffer)
    {
        buffer.putFloat(x);
        buffer.putFloat(y);
        buffer.putFloat(z);
        buffer.putFloat(u);
        buffer.putFloat(v);
        buffer.putInt(color);
    }
}