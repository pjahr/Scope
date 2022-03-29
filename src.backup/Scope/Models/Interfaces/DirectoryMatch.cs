using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  public class DirectoryMatch
  {
    public DirectoryMatch(string term, IDirectory directory)
    {
      Term = term;
      Directory = directory;
    }

    public string Term { get; }
    public IDirectory Directory { get; }

    public override string ToString()
    {
      return $"{Directory.Path} ({Term})";
    }
  }
}
