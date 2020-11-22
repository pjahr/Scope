using System.Collections.Generic;

namespace Scope.Interfaces
{
  /// <summary>
  /// Provides a substruture (files and directories) for files of a certain type.
  /// </summary>
  public interface IFileSubStructureProvider
  {
    string ApplicableFileExtension { get; }
    IReadOnlyCollection<IDirectory> GetDirectories(IFile file);
    IReadOnlyCollection<IFile> GetFiles(IFile file);
  }
}
