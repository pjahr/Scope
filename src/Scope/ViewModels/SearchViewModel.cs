using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Scope.ViewModels
{
  [Export]
  public class SearchViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ISearchIndex _searchIndex;
    private string _searchTerms;
    private RelayCommand _findFilesBySearchTermsCommand;

    public SearchViewModel(ISearchIndex searchIndex)
    {
      _searchIndex = searchIndex;

      _searchIndex.ResultsCleared += ClearResults;
      _searchIndex.MatchFound += AddMatch;

      _findFilesBySearchTermsCommand = new RelayCommand(FindFilesBySearchTerms);
    }

    public string SearchTerms
    {
      get { return _searchTerms; }
      set
      {
        if (_searchTerms == value)
        {
          return;
        }
        _searchTerms = value;
        PropertyChanged.Raise(this, nameof(SearchTerms));
      }
    }

    public ObservableCollection<string> Matches { get; } = new ObservableCollection<string>();

    public ICommand FindFilesBySearchTermsCommand { get { return _findFilesBySearchTermsCommand; } }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Dispose()
    {
      _searchIndex.ResultsCleared -= ClearResults;
      _searchIndex.MatchFound -= AddMatch;
    }

    private void AddMatch(Match obj)
    {
      Matches.Add(obj.File.Name);
    }

    private void ClearResults()
    {
      Matches.Clear();
    }

    private void FindFilesBySearchTerms()
    {
      _searchIndex.Either(SearchTerms);
    }
  }
}