using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  public class Match
  {

    public Match(string term, IFile file, int Offset)
    {
      Term = term;
      File = file;
      this.Offset = Offset;
    }

    public string Term { get; }
    public IFile File { get; }
    public int Offset { get; }
  }
}
