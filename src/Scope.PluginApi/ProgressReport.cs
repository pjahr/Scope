namespace Scope.Interfaces
{
  public class ProgressReport
  {
    public ProgressReport(double percentageCompleted, string message = "")
    {
      PercentageCompleted = percentageCompleted;
      Message = message;
    }
    double PercentageCompleted { get; }
    string Message { get;  }
  }
}
