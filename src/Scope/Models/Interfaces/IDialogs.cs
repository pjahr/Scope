using System;

namespace Scope.Models.Interfaces
{
  public interface IDialogs
  {
    void Show(IDialog dialog);
    event Action<IDialog> DisplayDialogRequested;
  }
}
