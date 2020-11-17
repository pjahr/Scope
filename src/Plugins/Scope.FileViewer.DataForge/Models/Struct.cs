namespace Scope.FileViewer.DataForge.Models
{
  internal class Struct
  {
    public string Name { get; set; }
    public Property[] Properties { get; set; } = new Property[0];

    public override string ToString()
    {
      return $"{Name} ({Properties.Length})";
    }
  }
}
