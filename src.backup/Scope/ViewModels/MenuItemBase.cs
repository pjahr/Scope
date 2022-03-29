using Scope.Utils;
using System.ComponentModel;
using System.Windows.Input;

namespace Scope.ViewModels
{
  public abstract class MenuItemBase : INotifyPropertyChanged
  {
    private object _tooltip;
    private string _label;

    public string Shortcut { get; protected set; }
    public ICommand Command { get; protected set; }
    
    public string Label
    {
      get => _label;
      protected set
      {
        _label = value;
        PropertyChanged.Raise(this, nameof(Label));
      }
    }
    
    public object Tooltip
    {
      get => _tooltip;
      protected set
      {
        _tooltip = value;
        PropertyChanged.Raise(this, nameof(Label));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
