using System.IO;
using Scope.Zip.Core;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// An <see cref="IArchiveStorage"/> implementation suitable for in memory streams.
  /// </summary>
  public class MemoryArchiveStorage : BaseArchiveStorage
  {
    #region Constructors

    private MemoryStream temporaryStream_;

    private MemoryStream finalStream_;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryArchiveStorage"/> class.
    /// </summary>
    public MemoryArchiveStorage() : base(FileUpdateMode.Direct) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryArchiveStorage"/> class.
    /// </summary>
    /// <param name="updateMode">The <see cref="FileUpdateMode"/> to use</param>
    /// <remarks>This constructor is for testing as memory streams dont really require safe mode.</remarks>
    public MemoryArchiveStorage(FileUpdateMode updateMode) : base(updateMode) { }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Get the stream returned by <see cref="ConvertTemporaryToFinal"/> if this was in fact called.
    /// </summary>
    public MemoryStream FinalStream => finalStream_;

    #endregion Properties

    /// <summary>
    /// Gets the temporary output <see cref="Stream"/>
    /// </summary>
    /// <returns>Returns the temporary output stream.</returns>
    public override Stream GetTemporaryOutput()
    {
      temporaryStream_ = new MemoryStream();
      return temporaryStream_;
    }

    /// <summary>
    /// Converts the temporary <see cref="Stream"/> to its final form.
    /// </summary>
    /// <returns>Returns a <see cref="Stream"/> that can be used to read
    /// the final storage for the archive.</returns>
    public override Stream ConvertTemporaryToFinal()
    {
      if (temporaryStream_ == null)
      {
        throw new ZipException("No temporary stream has been created");
      }

      finalStream_ = new MemoryStream(temporaryStream_.ToArray());
      return finalStream_;
    }

    /// <summary>
    /// Make a temporary copy of the original stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to copy.</param>
    /// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
    public override Stream MakeTemporaryCopy(Stream stream)
    {
      temporaryStream_ = new MemoryStream();
      stream.Position = 0;
      StreamUtils.Copy(stream, temporaryStream_, new byte[4096]);
      return temporaryStream_;
    }

    /// <summary>
    /// Return a stream suitable for performing direct updates on the original source.
    /// </summary>
    /// <param name="stream">The original source stream</param>
    /// <returns>Returns a stream suitable for direct updating.</returns>
    /// <remarks>If the <paramref name="stream"/> passed is not null this is used;
    /// otherwise a new <see cref="MemoryStream"/> is returned.</remarks>
    public override Stream OpenForDirectUpdate(Stream stream)
    {
      Stream result;
      if (stream == null || !stream.CanWrite)
      {
        result = new MemoryStream();

        if (stream != null)
        {
          stream.Position = 0;
          StreamUtils.Copy(stream, result, new byte[4096]);

          stream.Dispose();
        }
      }
      else
      {
        result = stream;
      }

      return result;
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public override void Dispose()
    {
      if (temporaryStream_ != null)
      {
        temporaryStream_.Dispose();
      }
    }
  }
}
