using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Provides a static way to obtain a source of data for an entry.
  /// </summary>
  public interface IStaticDataSource
  {
    /// <summary>
    /// Get a source of data by creating a new stream.
    /// </summary>
    /// <returns>Returns a <see cref="Stream"/> to use for compression input.</returns>
    /// <remarks>Ideally a new stream is created and opened to achieve this, to avoid locking problems.</remarks>
    Stream GetSource();
  }
}
