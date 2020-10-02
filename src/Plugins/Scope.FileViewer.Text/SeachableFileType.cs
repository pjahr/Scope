using Scope.Interfaces;

namespace Scope.FileViewer.Text
{
  internal class SeachableFileType : ISearchableFileType
  {
    public SeachableFileType(string extension, string name = "")
    {
      Name = string.IsNullOrEmpty(name) ? extension : name;
      Extension = extension;
    }

    public string Name { get; }
    public string Extension { get; }
  }
}
