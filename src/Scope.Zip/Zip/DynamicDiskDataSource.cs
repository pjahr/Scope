using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Default implementation of <see cref="IDynamicDataSource"/> for files stored on disk.
  /// </summary>
  public class DynamicDiskDataSource : IDynamicDataSource
  {
    /// <summary>
    /// Get a <see cref="Stream"/> providing data for an entry.
    /// </summary>
    /// <param name="entry">The entry to provide data for.</param>
    /// <param name="name">The file name for data if known.</param>
    /// <returns>Returns a stream providing data; or null if not available</returns>
    public Stream GetSource(ZipEntry entry, string name)
    {
      Stream result = null;

      if (name != null)
      {
        result = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read);
      }

      return result;
    }
  }
}
