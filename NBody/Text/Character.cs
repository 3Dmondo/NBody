using OpenTK.Mathematics;

namespace NBody.Text
{
  internal struct Character
  {
    public int TextureID { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Bearing { get; set; }
    public int Advance { get; set; }
  }
}
