using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
namespace PlanetRenderer
{
  internal class Renderer
  {
    private Shader Shader;
    private readonly float[] Vertices;
    private int VertexBufferObject;
    private int VertexArrayObject;
    private Texture EarthDay;
    private Texture EarthNight;
    private Texture EarthNormal;
    private float frame = 0;
    private Texture EarthSpec;
    private Texture EarthCloud;

    public Renderer()
    {
      Shader = new Shader("PlanetRenderer.Shaders.Shader.vert", "PlanetRenderer.Shaders.Shader.frag");
      Vertices = new float[] {0f, 0f, 0f};
      VertexBufferObject = GL.GenBuffer();
      UpdateFrame(false);
      VertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(VertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);

      EarthDay = Texture.LoadFromResource("PlanetRenderer.Textures.EarthDay.jpg");
      EarthDay.Use(TextureUnit.Texture0);
      EarthNight = Texture.LoadFromResource("PlanetRenderer.Textures.EarthNight.jpg");
      EarthNight.Use(TextureUnit.Texture1);
      EarthNormal = Texture.LoadFromResource("PlanetRenderer.Textures.EarthNormal.png");
      EarthNormal.Use(TextureUnit.Texture2);
      EarthSpec = Texture.LoadFromResource("PlanetRenderer.Textures.EarthSpec.png");
      EarthSpec.Use(TextureUnit.Texture3);
      EarthCloud = Texture.LoadFromResource("PlanetRenderer.Textures.EarthClouds.jpg");
      EarthCloud.Use(TextureUnit.Texture4);
      Shader.SetInt("texture0", 0);
      Shader.SetInt("texture1", 1);
      Shader.SetInt("texture2", 2);
      Shader.SetInt("texture3", 3);
      Shader.SetInt("texture4", 4);
    }

    public void RenderFrame(Camera camera)
    {
      GL.Enable(EnableCap.PointSprite);
      GL.Enable(EnableCap.VertexProgramPointSize);

      EarthDay.Use(TextureUnit.Texture0);
      EarthNight.Use(TextureUnit.Texture1);
      EarthNormal.Use(TextureUnit.Texture2);
      EarthSpec.Use(TextureUnit.Texture3);
      EarthCloud.Use(TextureUnit.Texture4);

      Shader.Use();
      Shader.SetMatrix4(
        "model_view_projection",
        Matrix4.Identity *
        camera.GetViewMatrix() *
        camera.GetProjectionMatrix());
      Shader.SetVector3("camera_pos", camera.Position);
      Shader.SetVector3("lightPos", new Vector3(1,0,0) );
      Shader.SetVector3("rotation", new Vector3(frame, 0, 0));


      GL.BindVertexArray(VertexArrayObject);
      GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Length / 3);

      GL.Disable(EnableCap.PointSprite);
      GL.Disable(EnableCap.VertexProgramPointSize);
    }

    public void UpdateFrame(bool pause)
    {
      if (!pause)
        frame += 1f / 3600f;
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StreamDraw);
    }
  }
}
