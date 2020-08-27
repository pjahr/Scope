﻿using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  public class SearchViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ISearch _searchIndex;

    private string _searchTerms = string.Empty;
    private Visibility _detailsVisibility = Visibility.Collapsed;
    private Visibility _searchIndicatorVisibility = Visibility.Hidden;

    public SearchViewModel(ISearch searchIndex)
    {
      _searchIndex = searchIndex;

      _searchIndex.ResultsCleared += ClearResults;
      _searchIndex.MatchFound += AddMatch;
      _searchIndex.Began += ShowSearchIndicator;
      _searchIndex.Finished += HideSearchIndicator;

      FindFilesBySearchTermsCommand = new RelayCommand(FindFilesBySearchTerms);
      ToggleDetailsVisibilityCommand = new RelayCommand(ToggleDetailsVisibility);
    }

    public event PropertyChangedEventHandler PropertyChanged;

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

    public Visibility DetailsVisibility
    {
      get => _detailsVisibility;
      set
      {
        if (_detailsVisibility == value)
        {
          return;
        }

        _detailsVisibility = value;
        PropertyChanged.Raise(this, nameof(DetailsVisibility));
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

    public ICommand FindFilesBySearchTermsCommand { get; }
    public ICommand ToggleDetailsVisibilityCommand { get; }
    
    public void Dispose()
    {
      _searchIndex.ResultsCleared -= ClearResults;
      _searchIndex.MatchFound -= AddMatch;
    }

    private void HideSearchIndicator()
    {
      SearchIndicatorVisibility = Visibility.Hidden;
    }

    private void ShowSearchIndicator()
    {
      SearchIndicatorVisibility = Visibility.Visible;
        }

    private void ToggleDetailsVisibility()
    {
      DetailsVisibility = DetailsVisibility == Visibility.Visible
                            ? Visibility.Collapsed
                            : Visibility.Visible;
    }

    private void AddMatch(Match obj) { }

    private void ClearResults() { }

    private void FindFilesBySearchTerms()
    {
      _searchIndex.InitiateSearchFor(SearchTerms);
    }
  }
}
