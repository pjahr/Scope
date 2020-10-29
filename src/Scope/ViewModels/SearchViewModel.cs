using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  public class SearchViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ISearch _searchIndex;
    private readonly IUiDispatch _uiDispatch;
    private string _searchTerms = string.Empty;
    private Visibility _optionsVisibility = Visibility.Collapsed;
    private Visibility _searchIndicatorVisibility = Visibility.Hidden;

    public SearchViewModel(ISearch searchIndex,
                           IUiDispatch uiDispatch,
                           SearchOptionsViewModel searchOptionsViewModel)
    {
      _searchIndex = searchIndex;
      _uiDispatch = uiDispatch;

      _searchIndex.Began += ShowSearchIndicator;
      _searchIndex.Finished += HideSearchIndicator;

      FindFilesBySearchTermsCommand = new RelayCommand(FindFilesBySearchTerms);
      ToggleDetailsVisibilityCommand = new RelayCommand(ToggleDetailsVisibility);
      SearchOptions = searchOptionsViewModel;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public SearchOptionsViewModel SearchOptions { get; }

    public string SearchTerms
    {
      get => _searchTerms;
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

    public Visibility OptionsVisibility
    {
      get => _optionsVisibility;
      set
      {
        if (_optionsVisibility == value)
        {
          return;
        }

        _optionsVisibility = value;
        PropertyChanged.Raise(this, nameof(OptionsVisibility));
      }
    }

    public Visibility SearchIndicatorVisibility
    {
      get => _searchIndicatorVisibility;
      set
      {
        if (_searchIndicatorVisibility == value)
        {
          return;
        }

        _searchIndicatorVisibility = value;
        PropertyChanged.Raise(this, nameof(SearchIndicatorVisibility));
      }
    }

    public string ProgressText { get; private set; }
    public int ProgressValue { get; private set; }
    public int ProgressMaximum { get; private set; }

    public ICommand FindFilesBySearchTermsCommand { get; }
    public ICommand ToggleDetailsVisibilityCommand { get; }

    public void Dispose()
    {
      
    }

    private void HideSearchIndicator()
    {
      _uiDispatch.Do(() => SearchIndicatorVisibility = Visibility.Hidden);
    }

    private void ShowSearchIndicator()
    {
      SearchIndicatorVisibility = Visibility.Visible;
    }

    private void ToggleDetailsVisibility()
    {
      OptionsVisibility = OptionsVisibility == Visibility.Visible
                            ? Visibility.Collapsed
                            : Visibility.Visible;
    }

    int _progress = 0;
    private void FindFilesBySearchTerms()
    {
      _progress = 0;
      var progress = new Progress<SearchProgress>(PublishProgress);
      
      _searchIndex.FindMatches(progress, SearchTerms);
    }

    private void PublishProgress(SearchProgress progress)
    {
      _progress += progress.Progress;
      ProgressText = $"{_progress}/{progress.Total}";
      ProgressValue = _progress;
      ProgressMaximum = progress.Total;

      PropertyChanged.Raise(this, nameof(ProgressText));
      PropertyChanged.Raise(this, nameof(ProgressValue));
      PropertyChanged.Raise(this, nameof(ProgressMaximum));
    }
  }
}
