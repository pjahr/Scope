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

    string Term { get; }
    IFile File { get; }
    int Offset { get; }
  }
}
