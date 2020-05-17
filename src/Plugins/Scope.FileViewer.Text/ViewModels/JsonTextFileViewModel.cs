using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text.ViewModels
{
  internal class JsonTextFileViewModel : IFileViewer
  {
    public JsonTextFileViewModel(IFile file)
    {
      string text;
      using (var s = file.Read())
      {
        text = Encoding.UTF8.GetString(s.ReadAllBytes());
      }

      var p = new Paragraph() { FontFamily = new FontFamily("Consolas"), FontSize = 10 };
      p.Inlines.Add(text.ToString());
      var doc = new FlowDocument(p);

      Document = doc;
    }

    public string Header { get; } = "Json";

    public FlowDocument Document { get; }

    public void Dispose()
    {
    }
  }
}
