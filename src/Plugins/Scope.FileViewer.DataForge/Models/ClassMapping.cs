using System.Collections.Generic;

namespace Scope.FileViewer.DataForge.Models
{
  public class ClassMapping
  {
    public ushort StructIndex { get; set; }
    public int RecordIndex { get; set; }
    public Property Property { get; set; }
    public List<Property> PropertyContainer { get; set; }
  }
}
