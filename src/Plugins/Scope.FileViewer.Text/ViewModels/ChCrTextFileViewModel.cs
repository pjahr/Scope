using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using Scope.Interfaces;

namespace Scope.FileViewer.Text.ViewModels
{
  internal class ChCrTextFileViewModel : IFileViewer
  {
    public ChCrTextFileViewModel(IFile file)
    {
      string text;
      using (var s = file.Read())
      {
        var lines = Deserialization.DeserializeChCr.Deserialize(s);
        text = lines.Aggregate((c,n)=>$"{c}\r\n{n}");
      }

      var p = new Paragraph { FontFamily = new FontFamily("Consolas"), FontSize = 10 };
      p.Inlines.Add(text);
      var doc = new FlowDocument(p);

      Document = doc;
    }

    public string Header { get; } = "Json";

    public FlowDocument Document { get; }

    public void Dispose() { }
  }
}
