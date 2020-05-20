using System.Linq;
using Scope.Models.Interfaces;
using Scope.Zip.Zip;

namespace Scope.Models
{
  internal class GenerateFileSystem
  {
    private P4kDirectory _root;

    public IFileSystem Generate(ZipFile zipFile)
    {
      _root = P4kDirectory.Root();
      foreach (ZipEntry item in zipFile.Cast<ZipEntry>()
                                       .Where(f => f.IsFile))
      {
        var directory = EnsureDirectoryExists(item.Name);
        directory.Add(new P4kFile(item, zipFile));
      }

      return new P4kFileSystem(_root);
    }

    private P4kDirectory EnsureDirectoryExists(string zipItemName)
    {
      var pathSegments = zipItemName.Split('/');
      P4kDirectory current = _root;
      for (int i = 0; i < pathSegments.Length - 1; i++)
      {
        var directory = current.Directories.SingleOrDefault(d => d.Name == pathSegments[i]);
        if (directory == null)
        {
          directory = new P4kDirectory(pathSegments[i]);
          current.Add(directory);
        }

        current = (P4kDirectory) directory;
      }

      return current;
    }
  }
}
