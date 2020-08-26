using Scope.Models.Interfaces;
using Scope.Utils;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;

namespace Scope.Models
{
  [Export]
  internal class SearchIndex : ISearchIndex
  {
    private readonly ICurrentP4k _currentP4K;

    public event System.Action ResultsCleared;
    public event System.Action<Match> MatchFound;

    public IEnumerable<Match> Results { get; }

    public SearchIndex(ICurrentP4k currentP4K)
    {
      _currentP4K = currentP4K;
    }
    
    public void Either(params string[] searchTerms)
    {
      if (_currentP4K.FileSystem == null)
      {
        return;
      }

      var c = _currentP4K.FileSystem.TotalNumberOfFiles;

      ResultsCleared.Raise();

      for (int i = 0; i < c; i++)
      {
        var f = _currentP4K.FileSystem[i];

        if (!f.Name.EndsWith(".json"))
        {
          continue;
        }

        string text;
        using (var s = f.Read())
        {
          text = Encoding.UTF8.GetString(s.ReadAllBytes());
          foreach (var term in searchTerms)
          {
            if (text.ToLowerInvariant().Contains(term.ToLowerInvariant()))
            {
              var match = new Match("", f, 0);
              MatchFound.Raise(match);
            }
          }
        }

      }
    }

    public void BuildUp()
    {
      
    }
  }
}
