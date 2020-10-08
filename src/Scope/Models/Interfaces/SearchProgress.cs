namespace Scope.Models.Interfaces
{
  public struct SearchProgress
  {
    public SearchProgress(int progress, int total)
    {
      Progress = progress;
      Total = total;
    }

    public int Progress { get; }
    public int Total { get; }
  }
}
