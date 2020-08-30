using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Scope.Utils;

namespace Scope.ViewModels
{
  public class TreeNodeViewModel : INotifyPropertyChanged
  {
    private static readonly TreeNodeViewModel DummyChild = new TreeNodeViewModel();

    private readonly ObservableCollection<TreeNodeViewModel> _children;
    private readonly TreeNodeViewModel _parent;

    private bool _isExpanded;
    private bool _isSelected;
    private string _name;

    public TreeNodeViewModel(TreeNodeViewModel parent, string name, string path)
    {
      _parent = parent;
      _name = name;
      Path = path;
      _children = new ObservableCollection<TreeNodeViewModel>();

      ResetChildren();
    }

    private TreeNodeViewModel()
    { /*Only for dummy...*/
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<TreeNodeViewModel> Children => _children;

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

        // Expand all the way up to the root.
        ////                if (_isExpanded && _parent != null)
        ////                    _parent.IsExpanded = true;
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

    public TreeNodeViewModel Parent => _parent;

    public void SetExpand(bool isExpanded)
    {
      IsExpanded = isExpanded;
    }

    public void SetSelect(bool isSelected)
    {
      IsSelected = isSelected;
    }

    public virtual bool HasDummyChild
    {
      get
      {
        if (Children == null)
        {
          return false;
        }

        if (Children.Count != 1)
        {
          return false;
        }

        return Children[0] == DummyChild;
      }
    }

    public string Path { get; } = "";

    public TreeNodeViewModel FindChildByName(string name)
    {
      if (HasDummyChild || name == null)
      {
        return null;
      }

      return Children.SingleOrDefault(item => name == item.Name);
    }

    public void AddChildren(List<TreeNodeViewModel> children)
    {
      if (children == null)
      {
        return;
      }

      foreach (var item in children)
      {
        _children.Add(item);
      }
    }

    protected virtual void ResetChildren()
    {
      _children.Clear();
      _children.Add(DummyChild);
    }

    protected virtual void LoadChildren() { }

    public virtual Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      return Task.FromResult(new List<TreeNodeViewModel>());
    }

    public Task<int> LoadChildrenAsync()
    {
      return Task.FromResult(LoadChildrenListAsync()
                            .Result.Count);
    }
  }
}
