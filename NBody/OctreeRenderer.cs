using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NBody
{
  internal class OctreeRenderer
  {
    private readonly float[] Vertices = new[] {
      -1f, -1f, -1f, //0
       1f, -1f, -1f, //1

      -1f, -1f, -1f, //0
      -1f, -1f,  1f, //3

      -1f, -1f, -1f, //0
      -1f,  1f, -1f, //5

       1f, -1f, -1f, //1
       1f, -1f,  1f, //2

       1f, -1f, -1f, //1
       1f,  1f, -1f, //6

       1f, -1f,  1f, //2
      -1f, -1f,  1f, //3

       1f, -1f,  1f, //2
       1f,  1f,  1f, //7
       
      -1f, -1f,  1f, //3
      -1f,  1f,  1f, //4
      
      -1f,  1f,  1f, //4
      -1f,  1f, -1f, //5
      
      -1f,  1f,  1f, //4
       1f,  1f,  1f, //7

      -1f,  1f, -1f, //5
       1f,  1f, -1f, //6

       1f,  1f, -1f, //6
       1f,  1f,  1f, //7
    };
    private readonly Universe Universe;

    private float[] InstanceData;

    private int VertexBufferObject;
    private int VertexArrayObject;
    private int[] InstanceVertexBufferObject = new int[2];
    //private int InstanceVertexArrayObject;


    private Shader Shader;
    private int Count;

    public OctreeRenderer(Universe universe)
    {
      VertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

      VertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(VertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);

      GL.GenBuffers(1, InstanceVertexBufferObject);
      GL.BindBuffer(BufferTarget.ArrayBuffer, InstanceVertexBufferObject[0]);
      GL.EnableVertexAttribArray(1);
      GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
      GL.VertexAttribDivisor(1, 1);
      GL.EnableVertexAttribArray(2);
      GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 5 * sizeof(float), 4 * sizeof(float));
      GL.VertexAttribDivisor(2, 1);

      Shader = new Shader("Shaders/cubeShader.vert", "Shaders/cubeShader.frag");
      Shader.Use();
      Universe = universe;
    }

    public void RenderOcTrees(Camera camera)
    {
      Shader.Use();
      Shader.SetMatrix4(
        "model_view_projection",
        Matrix4.Identity *
        camera.GetViewMatrix() *
        camera.GetProjectionMatrix());
      Shader.SetVector3("camera_pos", camera.Position);

      GL.BindVertexArray(VertexArrayObject);
      GL.DrawArraysInstanced(PrimitiveType.Lines, 0, Vertices.Length / 3, Count);
      GL.BindVertexArray(0);
      GL.EnableVertexAttribArray(0);
    }

    public void UpdateOcTree()
    {
      Count = FillInstanceData();
      GL.EnableVertexAttribArray(1);
      GL.BindBuffer(BufferTarget.ArrayBuffer, InstanceVertexBufferObject[0]);
      GL.BufferData(BufferTarget.ArrayBuffer, InstanceData.Length * sizeof(float), InstanceData, BufferUsageHint.StreamDraw);
    }

    private int FillInstanceData()
    {
      var count = Universe.OcTreeCache.Count;
      if (null == InstanceData || InstanceData.Length < count * 5) {
        InstanceData = new float[count * 5];
      }
      int j = 0;
      foreach (var ocTree in Universe.OcTreeCache.ocTrees) {
        InstanceData[j++] = (float)ocTree.Location.X;
        InstanceData[j++] = (float)ocTree.Location.Y;
        InstanceData[j++] = (float)ocTree.Location.Z;
        InstanceData[j++] = (float)ocTree.HalfWidth;
        InstanceData[j++] = ocTree.BodyCount;
      }
      return count;
    }
  }
}
