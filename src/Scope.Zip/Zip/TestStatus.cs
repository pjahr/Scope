namespace Scope.Zip.Zip
{
  /// <summary>
  /// Status returned returned by <see cref="ZipTestResultHandler"/> during testing.
  /// </summary>
  /// <seealso cref="ZipFile.TestArchive(bool)">TestArchive</seealso>
  public class TestStatus
  {
    #region Constructors

    private readonly ZipFile file_;

    private ZipEntry entry_;

    private bool entryValid_;

    private int errorCount_;

    private long bytesTested_;

    private TestOperation operation_;

    /// <summary>
    /// Initialise a new instance of <see cref="TestStatus"/>
    /// </summary>
    /// <param name="file">The <see cref="ZipFile"/> this status applies to.</param>
    public TestStatus(ZipFile file)
    {
      file_ = file;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Get the current <see cref="TestOperation"/> in progress.
    /// </summary>
    public TestOperation Operation => operation_;

    /// <summary>
    /// Get the <see cref="ZipFile"/> this status is applicable to.
    /// </summary>
    public ZipFile File => file_;

    /// <summary>
    /// Get the current/last entry tested.
    /// </summary>
    public ZipEntry Entry => entry_;

    /// <summary>
    /// Get the number of errors detected so far.
    /// </summary>
    public int ErrorCount => errorCount_;

    /// <summary>
    /// Get the number of bytes tested so far for the current entry.
    /// </summary>
    public long BytesTested => bytesTested_;

    /// <summary>
    /// Get a value indicating wether the last entry test was valid.
    /// </summary>
    public bool EntryValid => entryValid_;

    #endregion Properties

    internal void AddError()
    {
      errorCount_++;
      entryValid_ = false;
    }

    internal void SetOperation(TestOperation operation)
    {
      operation_ = operation;
    }

    internal void SetEntry(ZipEntry entry)
    {
      entry_ = entry;
      entryValid_ = true;
      bytesTested_ = 0;
    }

    internal void SetBytesTested(long value)
    {
      bytesTested_ = value;
    }
  }
}
