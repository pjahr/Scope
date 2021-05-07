using ICSharpCode.SharpZipLib.Zip;
using Scope.Interfaces;
using Scope.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Scope.File.SOCPAK
{
  [Export]
  public class FileSubStructureProvider : IFileSubStructureProvider
  {
    public string ApplicableFileExtension => ".socpak";

    public Task<IDirectory> GetAsDirectoryAsync(IFile file, IProgress<ProgressReport> progress)
    {
      List<File> files = new List<File>();
      using (var zipStream = file.Read())
      {
        var zipFile = new ZipFile(zipStream);
        foreach (var entry in zipFile.Cast<ZipEntry>().Where(e=>e.IsFile))
        {
          byte[] bytes;
          int bytesCompressed = Convert.ToInt32(entry.CompressedSize);
          using (var contentStream = zipFile.GetInputStream(entry))
          {
            bytes = contentStream.ReadAllBytes();
          }
          var path = entry.Name.Split('/');
          var contentFile = new File(path.Last(), entry.Name, bytes, bytesCompressed);
          files.Add(contentFile);
        }
      }

      IDirectory root = new Directory("", "",new Directory[0], files);
      return Task.FromResult(root);
    }
  }
}
