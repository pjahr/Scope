using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Default implementation of a <see cref="IStaticDataSource"/> for use with files stored on disk.
  /// </summary>
  public class StaticDiskDataSource : IStaticDataSource
  {
    private readonly string fileName_;

    /// <summary>
    /// Initialise a new instnace of <see cref="StaticDiskDataSource"/>
    /// </summary>
    /// <param name="fileName">The name of the file to obtain data from.</param>
    public StaticDiskDataSource(string fileName)
    {
      fileName_ = fileName;
    }

    /// <summary>
    /// Get a <see cref="Stream"/> providing data.
    /// </summary>
    /// <returns>Returns a <see cref="Stream"/> provising data.</returns>
    public Stream GetSource()
    {
      return File.Open(fileName_, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
  }
}
