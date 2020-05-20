using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Defines facilities for data storage when updating Zip Archives.
  /// </summary>
  public interface IArchiveStorage
  {
    /// <summary>
    /// Get the <see cref="FileUpdateMode"/> to apply during updates.
    /// </summary>
    FileUpdateMode UpdateMode { get; }

    /// <summary>
    /// Get an empty <see cref="Stream"/> that can be used for temporary output.
    /// </summary>
    /// <returns>Returns a temporary output <see cref="Stream"/></returns>
    /// <seealso cref="ConvertTemporaryToFinal"></seealso>
    Stream GetTemporaryOutput();

    /// <summary>
    /// Convert a temporary output stream to a final stream.
    /// </summary>
    /// <returns>The resulting final <see cref="Stream"/></returns>
    /// <seealso cref="GetTemporaryOutput"/>
    Stream ConvertTemporaryToFinal();

    /// <summary>
    /// Make a temporary copy of the original stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to copy.</param>
    /// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
    Stream MakeTemporaryCopy(Stream stream);

    /// <summary>
    /// Return a stream suitable for performing direct updates on the original source.
    /// </summary>
    /// <param name="stream">The current stream.</param>
    /// <returns>Returns a stream suitable for direct updating.</returns>
    /// <remarks>This may be the current stream passed.</remarks>
    Stream OpenForDirectUpdate(Stream stream);

    /// <summary>
    /// Dispose of this instance.
    /// </summary>
    void Dispose();
  }
}
