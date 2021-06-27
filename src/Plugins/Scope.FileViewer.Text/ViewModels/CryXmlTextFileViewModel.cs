using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Scope.Deserialization;
using Scope.Interfaces;

namespace Scope.FileViewer.Text.ViewModels
{
  internal class CryXmlTextFileViewModel : IFileViewer
  {
    public CryXmlTextFileViewModel(IFile file)
    {
      XmlDocument xmlDoc;

      using (var s = file.Read())
      {
        xmlDoc = new DeserializeCryXml(s).Result;
      }

      StringBuilder text = new StringBuilder();
      using (TextWriter writer = new StringWriter(text))
      {
        xmlDoc.Save(writer);
      }

      var p = new Paragraph {FontFamily = new FontFamily("Consolas"), FontSize = 10};
      p.Inlines.Add(text.ToString());
      var doc = new FlowDocument(p);

      Document = doc;
    }

    public string Header { get; } = "CryXml";

    public FlowDocument Document { get; }

    public void Dispose() { }
  }
}
