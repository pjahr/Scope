namespace Scope.Interfaces
{
  /// <summary>
  /// Creates the view model for a file viewer (<see cref="IFileViewer"/>).
  /// </summary>
  public interface IFileViewerFactory
  {
    /// <summary>
    /// Determines whether this factory can create a <see cref="IFileViewer"/> for the
    /// given file.
    /// </summary>
    bool CanHandle(IFile file);

    /// <summary>
    /// Creates a new instance of a <see cref="IFileViewer"/> that can handle the given
    /// file.
    /// </summary>
    IFileViewer Create(IFile file);
  }
}
