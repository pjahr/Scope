using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// An abstract <see cref="IArchiveStorage"/> suitable for extension by inheritance.
  /// </summary>
  abstract public class BaseArchiveStorage : IArchiveStorage
  {
    #region Constructors

    private FileUpdateMode updateMode_;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseArchiveStorage"/> class.
    /// </summary>
    /// <param name="updateMode">The update mode.</param>
    protected BaseArchiveStorage(FileUpdateMode updateMode)
    {
      updateMode_ = updateMode;
    }

    #endregion Constructors

    /// <summary>
    /// Gets the update mode applicable.
    /// </summary>
    /// <value>The update mode.</value>
    public FileUpdateMode UpdateMode
    {
      get
      {
        return updateMode_;
      }
    }

    /// <summary>
    /// Gets a temporary output <see cref="Stream"/>
    /// </summary>
    /// <returns>Returns the temporary output stream.</returns>
    /// <seealso cref="ConvertTemporaryToFinal"></seealso>
    public abstract Stream GetTemporaryOutput();

    /// <summary>
    /// Converts the temporary <see cref="Stream"/> to its final form.
    /// </summary>
    /// <returns>Returns a <see cref="Stream"/> that can be used to read
    /// the final storage for the archive.</returns>
    /// <seealso cref="GetTemporaryOutput"/>
    public abstract Stream ConvertTemporaryToFinal();

    /// <summary>
    /// Make a temporary copy of a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to make a copy of.</param>
    /// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
    public abstract Stream MakeTemporaryCopy(Stream stream);

    /// <summary>
    /// Return a stream suitable for performing direct updates on the original source.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to open for direct update.</param>
    /// <returns>Returns a stream suitable for direct updating.</returns>
    public abstract Stream OpenForDirectUpdate(Stream stream);

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public abstract void Dispose();
  }

}