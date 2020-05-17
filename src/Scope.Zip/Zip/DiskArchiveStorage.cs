using System;
using System.IO;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// An <see cref="IArchiveStorage"/> implementation suitable for hard disks.
  /// </summary>
  public class DiskArchiveStorage : BaseArchiveStorage
  {
    #region Constructors

    private Stream temporaryStream_;

    private string fileName_;

    private string temporaryName_;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskArchiveStorage"/> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="updateMode">The update mode.</param>
    public DiskArchiveStorage(ZipFile file, FileUpdateMode updateMode)
      : base(updateMode)
    {
      if (file.Name == null)
      {
        throw new ZipException("Cant handle non file archives");
      }

      fileName_ = file.Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskArchiveStorage"/> class.
    /// </summary>
    /// <param name="file">The file.</param>
    public DiskArchiveStorage(ZipFile file)
      : this(file, FileUpdateMode.Safe)
    {
    }

    #endregion Constructors

    /// <summary>
    /// Gets a temporary output <see cref="Stream"/> for performing updates on.
    /// </summary>
    /// <returns>Returns the temporary output stream.</returns>
    public override Stream GetTemporaryOutput()
    {
      if (temporaryName_ != null)
      {
        temporaryName_ = GetTempFileName(temporaryName_, true);
        temporaryStream_ = File.Open(temporaryName_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
      }
      else
      {
        // Determine where to place files based on internal strategy.
        // Currently this is always done in system temp directory.
        temporaryName_ = Path.GetTempFileName();
        temporaryStream_ = File.Open(temporaryName_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
      }

      return temporaryStream_;
    }

    /// <summary>
    /// Converts a temporary <see cref="Stream"/> to its final form.
    /// </summary>
    /// <returns>Returns a <see cref="Stream"/> that can be used to read
    /// the final storage for the archive.</returns>
    public override Stream ConvertTemporaryToFinal()
    {
      if (temporaryStream_ == null)
      {
        throw new ZipException("No temporary stream has been created");
      }

      Stream result = null;

      string moveTempName = GetTempFileName(fileName_, false);
      bool newFileCreated = false;

      try
      {
        temporaryStream_.Dispose();
        File.Move(fileName_, moveTempName);
        File.Move(temporaryName_, fileName_);
        newFileCreated = true;
        File.Delete(moveTempName);

        result = File.Open(fileName_, FileMode.Open, FileAccess.Read, FileShare.Read);
      }
      catch (Exception)
      {
        result = null;

        // Try to roll back changes...
        if (!newFileCreated)
        {
          File.Move(moveTempName, fileName_);
          File.Delete(temporaryName_);
        }

        throw;
      }

      return result;
    }

    /// <summary>
    /// Make a temporary copy of a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to copy.</param>
    /// <returns>Returns a temporary output <see cref="Stream"/> that is a copy of the input.</returns>
    public override Stream MakeTemporaryCopy(Stream stream)
    {
      stream.Dispose();

      temporaryName_ = GetTempFileName(fileName_, true);
      File.Copy(fileName_, temporaryName_, true);

      temporaryStream_ = new FileStream(temporaryName_,
        FileMode.Open,
        FileAccess.ReadWrite);
      return temporaryStream_;
    }

    /// <summary>
    /// Return a stream suitable for performing direct updates on the original source.
    /// </summary>
    /// <param name="stream">The current stream.</param>
    /// <returns>Returns a stream suitable for direct updating.</returns>
    /// <remarks>If the <paramref name="stream"/> is not null this is used as is.</remarks>
    public override Stream OpenForDirectUpdate(Stream stream)
    {
      Stream result;
      if ((stream == null) || !stream.CanWrite)
      {
        if (stream != null)
        {
          stream.Dispose();
        }

        result = new FileStream(fileName_,
            FileMode.Open,
            FileAccess.ReadWrite);
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

    private static string GetTempFileName(string original, bool makeTempFile)
    {
      string result = null;

      if (original == null)
      {
        result = Path.GetTempFileName();
      }
      else
      {
        int counter = 0;
        int suffixSeed = DateTime.Now.Second;

        while (result == null)
        {
          counter += 1;
          string newName = string.Format("{0}.{1}{2}.tmp", original, suffixSeed, counter);
          if (!File.Exists(newName))
          {
            if (makeTempFile)
            {
              try
              {
                // Try and create the file.
                using (FileStream stream = File.Create(newName))
                {
                }
                result = newName;
              }
              catch
              {
                suffixSeed = DateTime.Now.Second;
              }
            }
            else
            {
              result = newName;
            }
          }
        }
      }
      return result;
    }
  }

}