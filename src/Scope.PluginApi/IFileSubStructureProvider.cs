using System;
using System.Threading.Tasks;

namespace Scope.Interfaces
{
  /// <summary>
  /// Provides asynconously retireved substrutures (a directory containing files and directories)
  /// that represent the requested files of a certain type.
  /// </summary>
  public interface IFileSubStructureProvider
  {
    /// <summary>
    /// The file extension in a P4k file system that can be requested on this provider
    /// </summary>
    string ApplicableFileExtension { get; }

    /// <summary>
    /// Creates the file substruture in form of a directory that repreents the file as root of a directory tree.
    /// Implementations can provide a progress report.
    /// </summary>
    Task<IDirectory> GetAsDirectoryAsync(IFile file, IProgress<ProgressReport> progress);
  }
}
