using System;
using System.ComponentModel.Composition;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class Dialogs : IDialogs
  {
    public event Action<IDialog> DisplayDialogRequested;

    public void Show(IDialog dialog)
    {
      DisplayDialogRequested.Raise(dialog);
    }
  }
}