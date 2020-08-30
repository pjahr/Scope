using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  public class Match
  {

    public Match(string term, IFile file, MatchType type, int offset=-1)
    {
      Term = term;
      File = file;
      Type = type;
      Offset =offset;
    }

    public string Term { get; }
    public IFile File { get; }
    public MatchType Type { get; }
    public int Offset { get; }

    public override string ToString()
    {
      return $"{File.Path} : {Offset} ({Type})";
    }
  }  
}
