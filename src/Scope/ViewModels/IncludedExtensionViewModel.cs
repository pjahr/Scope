using Scope.Utils;
using System;
using System.ComponentModel;

namespace Scope.ViewModels
{
  public class IncludedExtensionViewModel : INotifyPropertyChanged
  {
    private bool _isIncluded;

    public IncludedExtensionViewModel(string name)
    {
      Name = name.ToLower();
    }
    public event PropertyChangedEventHandler PropertyChanged;
    public event Action<bool> IsIncludedChanged;

    public string Name { get; private set; }

    public bool IsIncluded
    {
      get => _isIncluded;
      set
      {
        if (_isIncluded == value)
        {
          return;
        }
        _isIncluded = value;
        PropertyChanged.Raise(this, nameof(IsIncluded));
        IsIncludedChanged.Raise(_isIncluded);
      }
    }

    public override string ToString()
    {
      return $"{Name}";
    }
  }
}