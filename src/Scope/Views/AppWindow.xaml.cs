using System.ComponentModel.Composition;
using System.Windows;
using Scope.Models.Interfaces;
using Scope.ViewModels;

namespace Scope.Views
{
  [Export]
  internal partial class AppWindow : Window
  {
    private readonly IDialogs _dialogs;

    public AppWindow(MainWindowViewModel viewModel, IDialogs dialogs)
    {
      _dialogs = dialogs;
      InitializeComponent();
      DataContext = viewModel;

      _dialogs.DisplayDialogRequested += ShowRequestedDialog;
    }

    private void ShowRequestedDialog(IDialog dialog)
    {
      var dialogWindow = new Window
      {
        WindowStyle = WindowStyle.ToolWindow,
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        Content = dialog,
        SizeToContent = SizeToContent.WidthAndHeight
      };

      dialogWindow.ShowDialog();
    }
  }
}
