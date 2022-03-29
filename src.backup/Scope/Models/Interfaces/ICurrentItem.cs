using System;
using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  /// <summary>
  /// The currently active/selected item of the application.
  /// Can be either a file or a directory or neither. 
  /// </summary>
  public interface ICurrentItem
  {
    /// <summary>
    /// The currently active file, if any was selected.
    /// </summary>
    IFile CurrentFile { get; }

    /// <summary>
    /// The currently active directory, if any was selected.
    /// </summary>
    IDirectory CurrentDirectory { get; }

    /// <summary>
    /// Selects the given file.
    /// Changes <see cref="CurrentDirectory"/> to <c>null</c>.
    /// </summary>
    void ChangeTo(IFile file);

    /// <summary>
    /// Selects the given directory.
    /// Changes <see cref="CurrentFile"/> to <c>null</c>.
    /// </summary>
    void ChangeTo(IDirectory directory);

    /// <summary>
    /// Clears either of the current items.
    /// </summary>
    void Clear();

    /// <summary>
    /// Raised when the selected item has changed.
    /// </summary>
    event Action Changed;
  }
}
