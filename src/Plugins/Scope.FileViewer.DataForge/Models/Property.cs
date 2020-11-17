namespace Scope.FileViewer.DataForge.Models
{
  internal class Property
  {
    public string Name { get; set; }
    public DataType Type { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
      return $"{Name}: {Value}";
    }
  }
}
