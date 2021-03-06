﻿using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Scope.FileViewer.Text.Models;
using Scope.Interfaces;

namespace Scope.FileViewer.Text.ViewModels
{
  internal class CryXmlTextFileViewModel : IFileViewer
  {
    public CryXmlTextFileViewModel(IFile file)
    {
      XmlDocument x;



      using (var s = file.Read())
      {
        var header = new byte[4];
        s.Read(header, 0, 4);
        var headerText=Encoding.UTF8.GetString(header);
        x = CryXmlSerializer.ReadStream(s);
      }

      StringBuilder text = new StringBuilder();
      using (TextWriter writer = new StringWriter(text))
      {
        x.Save(writer);
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
