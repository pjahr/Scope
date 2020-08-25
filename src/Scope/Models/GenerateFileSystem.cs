using System;
using System.Collections.Generic;
using System.Linq;
using Scope.Interfaces;
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
      List<IFile> files = new List<IFile>();
      foreach (ZipEntry item in zipFile.Cast<ZipEntry>()
                                       .Where(f => f.IsFile))
      {
        var directory = EnsureDirectoryExists(item.Name);
        var file = new P4kFile(item, zipFile);
        files.Add(file);
        directory.Add(file);
      }

      return new P4kFileSystem(_root, Convert.ToInt32(zipFile.Count), files);
    }

    private P4kDirectory EnsureDirectoryExists(string zipItemName)
    {
      var pathSegments = zipItemName.Split('/');
      P4kDirectory current = _root;
      for (int i = 0; i < pathSegments.Length - 1; i++)
      {
        var name = pathSegments[i];
        var directory = current.Directories.SingleOrDefault(d => d.Name == name);
        if (directory == null)
        {
          string path = $"{current.Path}\\{name}";
          directory = new P4kDirectory(pathSegments[i], path);
          current.Add(directory);
        }

        current = (P4kDirectory) directory;
      }

      return current;
    }
  }
}
