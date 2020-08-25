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

    public SearchIndex(ICurrentP4k currentP4K)
    {
      _currentP4K = currentP4K;
    }

    public Task<int> AllOff(IReadOnlyCollection<string> searchTerms)
    {
      throw new System.NotImplementedException();
    }

    public Task<int> Either(IReadOnlyCollection<string> searchTerms)
    {
      throw new System.NotImplementedException();
    }

    public void BuildUp()
    {
      if (_currentP4K.FileSystem==null)
      {
        return;
      }

      var c = _currentP4K.FileSystem.TotalNumberOfFiles;


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
        }

      }
    }
  }
}
