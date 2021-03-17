namespace Scope.ViewModels
{
  public class LoadingTreeNodeViewModel : TreeNodeViewModel
  {
    public LoadingTreeNodeViewModel() : base("Loading...", "")
    {
    }

    public bool IsLoadingChildren => false;
  }
}
