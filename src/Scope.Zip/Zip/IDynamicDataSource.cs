using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Represents a source of data that can dynamically provide
  /// multiple <see cref="Stream">data sources</see> based on the parameters passed.
  /// </summary>
  public interface IDynamicDataSource
  {
    /// <summary>
    /// Get a data source.
    /// </summary>
    /// <param name="entry">The <see cref="ZipEntry"/> to get a source for.</param>
    /// <param name="name">The name for data if known.</param>
    /// <returns>Returns a <see cref="Stream"/> to use for compression input.</returns>
    /// <remarks>Ideally a new stream is created and opened to achieve this, to avoid locking problems.</remarks>
    Stream GetSource(ZipEntry entry, string name);
  }
}
