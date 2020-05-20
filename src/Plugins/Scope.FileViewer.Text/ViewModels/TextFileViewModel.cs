using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text.ViewModels
{
  internal class TextFileViewModel : IFileViewer
  {
    public TextFileViewModel(IFile file)
    {
      string text;
      using (var s = file.Read())
      {
        text = Encoding.UTF8.GetString(s.ReadAllBytes());
      }

      var p = new Paragraph {FontFamily = new FontFamily("Consolas"), FontSize = 10};

      p.Inlines.Add(text);
      var doc = new FlowDocument(p);

      Document = doc;
    }

    public string Header { get; } = "Text";

    public FlowDocument Document { get; }

    public void Dispose() { }
  }
}
