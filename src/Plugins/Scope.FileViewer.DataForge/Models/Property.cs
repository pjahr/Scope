namespace Scope.FileViewer.DataForge.Models
{
  public class Property
  {
    public string Name { get; set; }
    public DataType Type { get; set; }
    public object Value { get; set; }
    public bool IsList { get; set; } = false;
    public override string ToString()
    {
      var s = IsList ? "s" : "";
      return $"{Name}: {Value} ({Type}{s})";
    }
  }
}
