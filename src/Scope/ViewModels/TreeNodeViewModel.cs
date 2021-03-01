using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Scope.Utils;

namespace Scope.ViewModels
{
  public interface ITreeNodeViewModel : INotifyPropertyChanged, IDisposable
  {
    string Path { get; }

  }

  public class TreeNodeViewModel : ITreeNodeViewModel
  {
    protected static readonly LoadingTreeNodeViewModel Loading = new LoadingTreeNodeViewModel();

    private readonly ObservableCollection<ITreeNodeViewModel> _children;
    private bool _isExpanded;
    private bool _isSelected;
    private string _name;
    private readonly string _path;
    private readonly bool _hasChildren;

    public TreeNodeViewModel(string name,
                             string path,
                             bool hasChildren = false)
    {
      _name = name;
      _path = path;
      _hasChildren = hasChildren;
      _children = new ObservableCollection<ITreeNodeViewModel>();

      ExpandCommand = new RelayCommand(async () => await LoadChildrenAsync());

      if (_hasChildren)
      {
        Children.Add(Loading);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand ExpandCommand { get; }
    public string Path => _path;
    public ObservableCollection<ITreeNodeViewModel> Children => _children;
    public virtual bool HasChildren => _hasChildren;

    protected void ResetChildren()
    {
      ExpandCommand.Execute(null);
    }

    public bool IsExpanded
    {
      get => _isExpanded;
      set
      {
        if (value == _isExpanded)
        {
          return;
        }

        _isExpanded = value;
        PropertyChanged.Raise(this, nameof(IsExpanded));
      }
    }

    public bool IsSelected
    {
      get => _isSelected;
      set
      {
        if (value == _isSelected)
        {
          return;
        }

        _isSelected = value;
        PropertyChanged.Raise(this, nameof(IsSelected));
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (value == _name)
        {
          return;
        }

        _name = value;
        PropertyChanged.Raise(this, nameof(Name));
      }
    }

    public void SetExpand(bool isExpanded)
    {
      IsExpanded = isExpanded;
    }

    public void SetSelect(bool isSelected)
    {
      IsSelected = isSelected;
    }

    protected virtual ITreeNodeViewModel[] LoadChildren() { return new TreeNodeViewModel[0]; }

    protected virtual void OnDisposing() { }

    private async Task LoadChildrenAsync() 
    {
      if (!HasChildren)
      {
        return;
      }
      Children.Clear();
      Children.Add(Loading);

      ITreeNodeViewModel[] children = new ITreeNodeViewModel[0];
      try
      {
        children = await Task.Run(() => LoadChildren());
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed loading children of {Name}.\r\n{e.Message}");
      }
      finally
      {
        Children.Remove(Loading);
        foreach (var child in children)
        {
          Children.Add(child);
        }
      }
    }

    public void Dispose()
    {
      OnDisposing();
    }
  }
}
