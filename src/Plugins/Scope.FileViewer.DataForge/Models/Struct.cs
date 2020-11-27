using System.Collections.Generic;

namespace Scope.FileViewer.DataForge.Models
{
  public class Struct
  {
    public string Name { get; set; }
    public List<Property> Properties { get; set; } = new List<Property>();

    public override string ToString()
    {
      return $"{Name} ({Properties.Count})";
    }
  }
}
