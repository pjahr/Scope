using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Scope.Zip.Checksum;
using Scope.Zip.Core;
using Scope.Zip.Encryption;
using Scope.Zip.Zip.Compression;
using Scope.Zip.Zip.Compression.Streams;
using ZstdNet;

namespace Scope.Zip.Zip
{
  public delegate void ZipTestResultHandler(TestStatus status, string message);

  public class ZipFile : IEnumerable, IDisposable
  {
    /// <summary>
    /// Event handler for handling encryption keys.
    /// </summary>
    public KeysRequiredEventHandler KeysRequired;

    private const int DefaultBufferSize = 4096;

    private bool _isDisposed;

    private string _name;

    private string _comment;

    private string _rawPassword;

    private Stream _baseStream;

    private bool _isStreamOwner;

    private long _offsetOfFirstEntry;

    private ZipEntry[] _entries;

    private byte[] _key;

    private bool _isNewArchive;

    // Default is dynamic which is not backwards compatible and can cause problems
    // with XP's built in compression which cant read Zip64 archives.
    // However it does avoid the situation were a large file is added and cannot be completed correctly.
    // Hint: Set always ZipEntry size before they are added to an archive and this setting isnt needed.
    private UseZip64 _useZip64 = UseZip64.Dynamic;

    private List<ZipUpdate> _updates;

    private long _updateCount;

    // Count is managed manually as updates_ can contain nulls!
    private Dictionary<string, int> _updateIndex;

    private IArchiveStorage _archiveStorage;

    private IDynamicDataSource _updateDataSource;

    private bool _contentsEdited;

    private int _bufferSize = DefaultBufferSize;

    private byte[] _copyBuffer;

    private ZipString _newComment;

    private bool _commentEdited;

    private IEntryFactory _updateEntryFactory = new ZipEntryFactory();

    public ZipFile(string name)
    {
      if (name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      _name = name;

      _baseStream = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read);
      _isStreamOwner = true;

      try
      {
        ReadEntries();
      }
      catch
      {
        DisposeInternal(true);
        throw;
      }
    }

    public ZipFile(FileStream file)
    {
      if (file == null)
      {
        throw new ArgumentNullException(nameof(file));
      }

      if (!file.CanSeek)
      {
        throw new ArgumentException("Stream is not seekable", nameof(file));
      }

      _baseStream = file;
      _name = file.Name;
      _isStreamOwner = true;

      try
      {
        ReadEntries();
      }
      catch
      {
        DisposeInternal(true);
        throw;
      }
    }

    /// <summary>
    /// Opens a Zip file reading the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read archive data from.</param>
    /// <exception cref="IOException">
    /// An i/o error occurs
    /// </exception>
    /// <exception cref="ZipException">
    /// The stream doesn't contain a valid zip archive.<br/>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The <see cref="Stream">stream</see> doesnt support seeking.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The <see cref="Stream">stream</see> argument is null.
    /// </exception>
    public ZipFile(Stream stream)
    {
      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      if (!stream.CanSeek)
      {
        throw new ArgumentException("Stream is not seekable", nameof(stream));
      }

      _baseStream = stream;
      _isStreamOwner = true;

      if (_baseStream.Length > 0)
      {
        try
        {
          ReadEntries();
        }
        catch
        {
          DisposeInternal(true);
          throw;
        }
      }
      else
      {
        _entries = new ZipEntry[0];
        _isNewArchive = true;
      }
    }

    /// <summary>
    /// Initialises a default <see cref="ZipFile"/> instance with no entries and no file storage.
    /// </summary>
    internal ZipFile()
    {
      _entries = new ZipEntry[0];
      _isNewArchive = true;
    }

    /// <summary>
    /// Finalize this instance.
    /// </summary>
    ~ZipFile()
    {
      Dispose(false);
    }

    /// <summary>
    /// Delegate for handling keys/password setting during compresion/decompression.
    /// </summary>
    public delegate void KeysRequiredEventHandler(object sender, KeysRequiredEventArgs e);

    [Flags]
    private enum HeaderTest
    {
      Extract = 0x01, // Check that this header represents an entry whose data can be extracted
      Header = 0x02   // Check that this header contents are valid
    }

    /// <summary>
    /// The kind of update to apply.
    /// </summary>
    private enum UpdateCommand
    {
      Copy,   // Copy original file contents.
      Modify, // Change encryption, compression, attributes, name, time etc, of an existing file.
      Add     // Add a new file to the archive.
    }

    /// <summary>
    /// Get/set the encryption key value.
    /// </summary>
    public byte[] Key { private get { return _key; } set { _key = value; } }

    /// <summary>
    /// Password to be used for encrypting/decrypting files.
    /// </summary>
    /// <remarks>Set to null if no password is required.</remarks>
    public string Password
    {
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          _key = null;
        }
        else
        {
          _rawPassword = value;
          _key = PkzipClassic.GenerateKeys(ZipConstants.ConvertToArray(value));
        }
      }
    }

    /// <summary>
    /// Get/set a flag indicating if the underlying stream is owned by the ZipFile instance.
    /// If the flag is true then the stream will be closed when <see cref="Close">Close</see> is called.
    /// </summary>
    /// <remarks>
    /// The default value is true in all cases.
    /// </remarks>
    public bool IsStreamOwner { get => _isStreamOwner; set => _isStreamOwner = value; }

    /// <summary>
    /// Get a value indicating wether
    /// this archive is embedded in another file or not.
    /// </summary>
    // Not strictly correct in all circumstances currently
    public bool IsEmbeddedArchive => _offsetOfFirstEntry > 0;

    /// <summary>
    /// Get a value indicating that this archive is a new one.
    /// </summary>
    public bool IsNewArchive => _isNewArchive;

    /// <summary>
    /// Gets the comment for the zip file.
    /// </summary>
    public string ZipFileComment => _comment;

    /// <summary>
    /// Gets the name of this zip file.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the number of entries in this zip file.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The Zip file has been closed.
    /// </exception>
    [Obsolete("Use the Count property instead")]
    public int Size => _entries.Length;

    /// <summary>
    /// Get the number of entries contained in this <see cref="ZipFile"/>.
    /// </summary>
    public long Count => _entries.Length;

    /// <summary>
    /// Get / set the <see cref="INameTransform"/> to apply to names when updating.
    /// </summary>
    public INameTransform NameTransform
    {
      get => _updateEntryFactory.NameTransform;
      set => _updateEntryFactory.NameTransform = value;
    }

    /// <summary>
    /// Get/set the <see cref="IEntryFactory"/> used to generate <see cref="ZipEntry"/> values
    /// during updates.
    /// </summary>
    public IEntryFactory EntryFactory
    {
      get => _updateEntryFactory;
      set
      {
        if (value == null)
        {
          _updateEntryFactory = new ZipEntryFactory();
        }
        else
        {
          _updateEntryFactory = value;
        }
      }
    }

    /// <summary>
    /// Get /set the buffer size to be used when updating this zip file.
    /// </summary>
    public int BufferSize
    {
      get => _bufferSize;
      set
      {
        if (value < 1024)
        {
          throw new ArgumentOutOfRangeException(nameof(value), "cannot be below 1024");
        }

        if (_bufferSize != value)
        {
          _bufferSize = value;
          _copyBuffer = null;
        }
      }
    }

    /// <summary>
    /// Get a value indicating an update has <see cref="BeginUpdate()">been started</see>.
    /// </summary>
    public bool IsUpdating => _updates != null;

    /// <summary>
    /// Get / set a value indicating how Zip64 Extension usage is determined when adding entries.
    /// </summary>
    public UseZip64 UseZip64 { get => _useZip64; set => _useZip64 = value; }

    /// <summary>
    /// Get a value indicating wether encryption keys are currently available.
    /// </summary>
    private bool HaveKeys => _key != null;

    /// <summary>
    /// Indexer property for ZipEntries
    /// </summary>
    [IndexerName("EntryByIndex")]
    public ZipEntry this[int index] =>
      (ZipEntry)_entries[index]
       .Clone();

    /// <summary>
    /// Create a new <see cref="ZipFile"/> whose data will be stored in a file.
    /// </summary>
    /// <param name="fileName">The name of the archive to create.</param>
    /// <returns>Returns the newly created <see cref="ZipFile"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="fileName"></paramref> is null</exception>
    public static ZipFile Create(string fileName)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      FileStream fs = File.Create(fileName);

      var result = new ZipFile();
      result._name = fileName;
      result._baseStream = fs;
      result._isStreamOwner = true;
      return result;
    }

    /// <summary>
    /// Create a new <see cref="ZipFile"/> whose data will be stored on a stream.
    /// </summary>
    /// <param name="outStream">The stream providing data storage.</param>
    /// <returns>Returns the newly created <see cref="ZipFile"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="outStream"> is null</paramref></exception>
    /// <exception cref="ArgumentException"><paramref name="outStream"> doesnt support writing.</paramref></exception>
    public static ZipFile Create(Stream outStream)
    {
      if (outStream == null)
      {
        throw new ArgumentNullException(nameof(outStream));
      }

      if (!outStream.CanWrite)
      {
        throw new ArgumentException("Stream is not writeable", nameof(outStream));
      }

      if (!outStream.CanSeek)
      {
        throw new ArgumentException("Stream is not seekable", nameof(outStream));
      }

      var result = new ZipFile();
      result._baseStream = outStream;
      return result;
    }

    public static byte[] ReadAllBytes(Stream stream)
    {
      stream.Position = 0;
      byte[] buffer = new byte[stream.Length];
      for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
      {
        totalBytesCopied += stream.Read(buffer,
                                        totalBytesCopied,
                                        Convert.ToInt32(stream.Length) - totalBytesCopied);
      }

      return buffer;
    }

    /// <summary>
    /// Closes the ZipFile.  If the stream is <see cref="IsStreamOwner">owned</see> then this also closes the underlying input stream.
    /// Once closed, no further instance methods should be called.
    /// </summary>
    /// <exception cref="System.IO.IOException">
    /// An i/o error occurs.
    /// </exception>
    public void Close()
    {
      DisposeInternal(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets an enumerator for the Zip entries in this Zip file.
    /// </summary>
    /// <returns>Returns an <see cref="IEnumerator"/> for this archive.</returns>
    /// <exception cref="ObjectDisposedException">
    /// The Zip file has been closed.
    /// </exception>
    public IEnumerator GetEnumerator()
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      return new ZipEntryEnumerator(_entries);
    }

    /// <summary>
    /// Return the index of the entry with a matching name
    /// </summary>
    /// <param name="name">Entry name to find</param>
    /// <param name="ignoreCase">If true the comparison is case insensitive</param>
    /// <returns>The index position of the matching entry or -1 if not found</returns>
    /// <exception cref="ObjectDisposedException">
    /// The Zip file has been closed.
    /// </exception>
    public int FindEntry(string name, bool ignoreCase)
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      // TODO: This will be slow as the next ice age for huge archives!
      for (int i = 0; i < _entries.Length; i++)
      {
        if (string.Compare(name,
                           _entries[i]
                            .Name,
                           ignoreCase
                             ? StringComparison.OrdinalIgnoreCase
                             : StringComparison.Ordinal)
            == 0)
        {
          return i;
        }
      }

      return -1;
    }

    /// <summary>
    /// Searches for a zip entry in this archive with the given name.
    /// String comparisons are case insensitive
    /// </summary>
    /// <param name="name">
    /// The name to find. May contain directory components separated by slashes ('/').
    /// </param>
    /// <returns>
    /// A clone of the zip entry, or null if no entry with that name exists.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// The Zip file has been closed.
    /// </exception>
    public ZipEntry GetEntry(string name)
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      int index = FindEntry(name, true);
      return index >= 0
               ? (ZipEntry)_entries[index]
                .Clone()
               : null;
    }

    /// <summary>
    /// Gets an input stream for reading the given zip entry data in an uncompressed form.
    /// Normally the <see cref="ZipEntry"/> should be an entry returned by GetEntry().
    /// </summary>
    /// <param name="entry">The <see cref="ZipEntry"/> to obtain a data <see cref="Stream"/> for</param>
    /// <returns>An input <see cref="Stream"/> containing data for this <see cref="ZipEntry"/></returns>
    /// <exception cref="ObjectDisposedException">
    /// The ZipFile has already been closed
    /// </exception>
    /// <exception cref="ZipException">
    /// The compression method for the entry is unknown
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// The entry is not found in the ZipFile
    /// </exception>
    public Stream GetInputStream(ZipEntry entry)
    {
      if (entry == null)
      {
        throw new ArgumentNullException(nameof(entry));
      }

      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      long index = entry.ZipFileIndex;
      if (index < 0
          || index >= _entries.Length
          || _entries[index]
           .Name
          != entry.Name)
      {
        index = FindEntry(entry.Name, true);
        if (index < 0)
        {
          throw new ZipException("Entry cannot be found");
        }
      }

      return GetInputStream(index);
    }

    /// <summary>
    /// Creates an input stream reading a zip entry
    /// </summary>
    /// <param name="entryIndex">The index of the entry to obtain an input stream for.</param>
    /// <returns>
    /// An input <see cref="Stream"/> containing data for this <paramref name="entryIndex"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// The ZipFile has already been closed
    /// </exception>
    /// <exception cref="ZipException">
    /// The compression method for the entry is unknown
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// The entry is not found in the ZipFile
    /// </exception>
    public Stream GetInputStream(long entryIndex)
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      long start = LocateEntry(_entries[entryIndex]);
      CompressionMethod method = _entries[entryIndex]
       .CompressionMethod;
      Stream result = new PartialInputStream(this,
                                             start,
                                             _entries[entryIndex]
                                              .CompressedSize);

      if (_entries[entryIndex]
       .IsAesCrypted)
      {
        result = CreateAndInitAesDecryptionStream(result, _entries[entryIndex]);
        if (result == null)
        {
          throw new ZipException("Unable to decrypt this entry");
        }
      }

      if (_entries[entryIndex]
       .IsCrypted)
      {
        result = CreateAndInitDecryptionStream(result, _entries[entryIndex]);
        if (result == null)
        {
          throw new ZipException("Unable to decrypt this entry");
        }
      }

      switch (method)
      {
        case CompressionMethod.Stored:
          // read as is.
          break;

        case CompressionMethod.Deflated:
          // No need to worry about ownership and closing as underlying stream close does nothing.
          result = new InflaterInputStream(result, new Inflater(true));
          break;

        case CompressionMethod.ZStd:

          var bytes = ReadAllBytes(result);
          byte[] decompressedData;
          using (var decompressor = new Decompressor())
          {
            decompressedData = decompressor.Unwrap(bytes);
          }

          result.Close();
          result.Dispose();

          result = new MemoryStream(decompressedData);
          break;

        default:
          throw new ZipException("Unsupported compression method " + method);
      }

      return result;
    }

    /// <summary>
    /// Begin updating this <see cref="ZipFile"/> archive.
    /// </summary>
    /// <param name="archiveStorage">The <see cref="IArchiveStorage">archive storage</see> for use during the update.</param>
    /// <param name="dataSource">The <see cref="IDynamicDataSource">data source</see> to utilise during updating.</param>
    /// <exception cref="ObjectDisposedException">ZipFile has been closed.</exception>
    /// <exception cref="ArgumentNullException">One of the arguments provided is null</exception>
    /// <exception cref="ObjectDisposedException">ZipFile has been closed.</exception>
    public void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
    {
      if (archiveStorage == null)
      {
        throw new ArgumentNullException(nameof(archiveStorage));
      }

      if (dataSource == null)
      {
        throw new ArgumentNullException(nameof(dataSource));
      }

      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      if (IsEmbeddedArchive)
      {
        throw new ZipException("Cannot update embedded/SFX archives");
      }

      _archiveStorage = archiveStorage;
      _updateDataSource = dataSource;

      // NOTE: the baseStream_ may not currently support writing or seeking.

      _updateIndex = new Dictionary<string, int>();

      _updates = new List<ZipUpdate>(_entries.Length);
      foreach (ZipEntry entry in _entries)
      {
        int index = _updates.Count;
        _updates.Add(new ZipUpdate(entry));
        _updateIndex.Add(entry.Name, index);
      }

      // We must sort by offset before using offset's calculated sizes
      _updates.Sort(new UpdateComparer());

      int idx = 0;
      foreach (ZipUpdate update in _updates)
      {
        //If last entry, there is no next entry offset to use
        if (idx == _updates.Count - 1)
        {
          break;
        }

        update.OffsetBasedSize = _updates[idx + 1]
                                .Entry.Offset
                                 - update.Entry.Offset;
        idx++;
      }

      _updateCount = _updates.Count;

      _contentsEdited = false;
      _commentEdited = false;
      _newComment = null;
    }

    /// <summary>
    /// Begin updating to this <see cref="ZipFile"/> archive.
    /// </summary>
    /// <param name="archiveStorage">The storage to use during the update.</param>
    public void BeginUpdate(IArchiveStorage archiveStorage)
    {
      BeginUpdate(archiveStorage, new DynamicDiskDataSource());
    }

    /// <summary>
    /// Begin updating this <see cref="ZipFile"/> archive.
    /// </summary>
    /// <seealso cref="BeginUpdate(IArchiveStorage)"/>
    /// <seealso cref="CommitUpdate"></seealso>
    /// <seealso cref="AbortUpdate"></seealso>
    public void BeginUpdate()
    {
      if (Name == null)
      {
        BeginUpdate(new MemoryArchiveStorage(), new DynamicDiskDataSource());
      }
      else
      {
        BeginUpdate(new DiskArchiveStorage(this), new DynamicDiskDataSource());
      }
    }

    /// <summary>
    /// Commit current updates, updating this archive.
    /// </summary>
    /// <seealso cref="BeginUpdate()"></seealso>
    /// <seealso cref="AbortUpdate"></seealso>
    /// <exception cref="ObjectDisposedException">ZipFile has been closed.</exception>
    public void CommitUpdate()
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      CheckUpdating();

      try
      {
        _updateIndex.Clear();
        _updateIndex = null;

        if (_contentsEdited)
        {
          RunUpdates();
        }
        else if (_commentEdited)
        {
          UpdateCommentOnly();
        }
        else
        {
          // Create an empty archive if none existed originally.
          if (_entries.Length == 0)
          {
            byte[] theComment = _newComment != null
                                  ? _newComment.RawComment
                                  : ZipConstants.ConvertToArray(_comment);
            using (ZipHelperStream zhs = new ZipHelperStream(_baseStream))
            {
              zhs.WriteEndOfCentralDirectory(0, 0, 0, theComment);
            }
          }
        }
      }
      finally
      {
        PostUpdateCleanup();
      }
    }

    /// <summary>
    /// Abort updating leaving the archive unchanged.
    /// </summary>
    /// <seealso cref="BeginUpdate()"></seealso>
    /// <seealso cref="CommitUpdate"></seealso>
    public void AbortUpdate()
    {
      PostUpdateCleanup();
    }

    /// <summary>
    /// Set the file comment to be recorded when the current update is <see cref="CommitUpdate">commited</see>.
    /// </summary>
    /// <param name="comment">The comment to record.</param>
    /// <exception cref="ObjectDisposedException">ZipFile has been closed.</exception>
    public void SetComment(string comment)
    {
      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      CheckUpdating();

      _newComment = new ZipString(comment);

      if (_newComment.RawLength > 0xffff)
      {
        _newComment = null;
        throw new ZipException("Comment length exceeds maximum - 65535");
      }

      // We dont take account of the original and current comment appearing to be the same
      // as encoding may be different.
      _commentEdited = true;
    }

    /// <summary>
    /// Add a new entry to the archive.
    /// </summary>
    /// <param name="fileName">The name of the file to add.</param>
    /// <param name="compressionMethod">The compression method to use.</param>
    /// <param name="useUnicodeText">Ensure Unicode text is used for name and comment for this entry.</param>
    /// <exception cref="ArgumentNullException">Argument supplied is null.</exception>
    /// <exception cref="ObjectDisposedException">ZipFile has been closed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Compression method is not supported.</exception>
    public void Add(string fileName, CompressionMethod compressionMethod, bool useUnicodeText)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (_isDisposed)
      {
        throw new ObjectDisposedException("ZipFile");
      }

      if (!ZipEntry.IsCompressionMethodSupported(compressionMethod))
      {
        throw new ArgumentOutOfRangeException(nameof(compressionMethod));
      }

      CheckUpdating();
      _contentsEdited = true;

      ZipEntry entry = EntryFactory.MakeFileEntry(fileName);
      entry.IsUnicodeText = useUnicodeText;
      entry.CompressionMethod = compressionMethod;

      AddUpdate(new ZipUpdate(fileName, entry));
    }

    /// <summary>
    /// Add a new entry to the archive.
    /// </summary>
    /// <param name="fileName">The name of the file to add.</param>
    /// <param name="compressionMethod">The compression method to use.</param>
    /// <exception cref="ArgumentNullException">ZipFile has been closed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The compression method is not supported.</exception>
    public void Add(string fileName, CompressionMethod compressionMethod)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!ZipEntry.IsCompressionMethodSupported(compressionMethod))
      {
        throw new ArgumentOutOfRangeException(nameof(compressionMethod));
      }

      CheckUpdating();
      _contentsEdited = true;

      ZipEntry entry = EntryFactory.MakeFileEntry(fileName);
      entry.CompressionMethod = compressionMethod;
      AddUpdate(new ZipUpdate(fileName, entry));
    }

    /// <summary>
    /// Add a file to the archive.
    /// </summary>
    /// <param name="fileName">The name of the file to add.</param>
    /// <exception cref="ArgumentNullException">Argument supplied is null.</exception>
    public void Add(string fileName)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      CheckUpdating();
      AddUpdate(new ZipUpdate(fileName, EntryFactory.MakeFileEntry(fileName)));
    }

    /// <summary>
    /// Add a file to the archive.
    /// </summary>
    /// <param name="fileName">The name of the file to add.</param>
    /// <param name="entryName">The name to use for the <see cref="ZipEntry"/> on the Zip file created.</param>
    /// <exception cref="ArgumentNullException">Argument supplied is null.</exception>
    public void Add(string fileName, string entryName)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (entryName == null)
      {
        throw new ArgumentNullException(nameof(entryName));
      }

      CheckUpdating();
      AddUpdate(new ZipUpdate(fileName, EntryFactory.MakeFileEntry(fileName, entryName, true)));
    }

    /// <summary>
    /// Add a file entry with data.
    /// </summary>
    /// <param name="dataSource">The source of the data for this entry.</param>
    /// <param name="entryName">The name to give to the entry.</param>
    public void Add(IStaticDataSource dataSource, string entryName)
    {
      if (dataSource == null)
      {
        throw new ArgumentNullException(nameof(dataSource));
      }

      if (entryName == null)
      {
        throw new ArgumentNullException(nameof(entryName));
      }

      CheckUpdating();
      AddUpdate(new ZipUpdate(dataSource, EntryFactory.MakeFileEntry(entryName, false)));
    }

    /// <summary>
    /// Add a file entry with data.
    /// </summary>
    /// <param name="dataSource">The source of the data for this entry.</param>
    /// <param name="entryName">The name to give to the entry.</param>
    /// <param name="compressionMethod">The compression method to use.</param>
    public void Add(IStaticDataSource dataSource,
                    string entryName,
                    CompressionMethod compressionMethod)
    {
      if (dataSource == null)
      {
        throw new ArgumentNullException(nameof(dataSource));
      }

      if (entryName == null)
      {
        throw new ArgumentNullException(nameof(entryName));
      }

      CheckUpdating();

      ZipEntry entry = EntryFactory.MakeFileEntry(entryName, false);
      entry.CompressionMethod = compressionMethod;

      AddUpdate(new ZipUpdate(dataSource, entry));
    }

    /// <summary>
    /// Add a file entry with data.
    /// </summary>
    /// <param name="dataSource">The source of the data for this entry.</param>
    /// <param name="entryName">The name to give to the entry.</param>
    /// <param name="compressionMethod">The compression method to use.</param>
    /// <param name="useUnicodeText">Ensure Unicode text is used for name and comments for this entry.</param>
    public void Add(IStaticDataSource dataSource,
                    string entryName,
                    CompressionMethod compressionMethod,
                    bool useUnicodeText)
    {
      if (dataSource == null)
      {
        throw new ArgumentNullException(nameof(dataSource));
      }

      if (entryName == null)
      {
        throw new ArgumentNullException(nameof(entryName));
      }

      CheckUpdating();

      ZipEntry entry = EntryFactory.MakeFileEntry(entryName, false);
      entry.IsUnicodeText = useUnicodeText;
      entry.CompressionMethod = compressionMethod;

      AddUpdate(new ZipUpdate(dataSource, entry));
    }

    /// <summary>
    /// Add a <see cref="ZipEntry"/> that contains no data.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    /// <remarks>This can be used to add directories, volume labels, or empty file entries.</remarks>
    public void Add(ZipEntry entry)
    {
      if (entry == null)
      {
        throw new ArgumentNullException(nameof(entry));
      }

      CheckUpdating();

      if (entry.Size != 0 || entry.CompressedSize != 0)
      {
        throw new ZipException("Entry cannot have any data");
      }

      AddUpdate(new ZipUpdate(UpdateCommand.Add, entry));
    }

    /// <summary>
    /// Add a directory entry to the archive.
    /// </summary>
    /// <param name="directoryName">The directory to add.</param>
    public void AddDirectory(string directoryName)
    {
      if (directoryName == null)
      {
        throw new ArgumentNullException(nameof(directoryName));
      }

      CheckUpdating();

      ZipEntry dirEntry = EntryFactory.MakeDirectoryEntry(directoryName);
      AddUpdate(new ZipUpdate(UpdateCommand.Add, dirEntry));
    }

    /// <summary>
    /// Delete an entry by name
    /// </summary>
    /// <param name="fileName">The filename to delete</param>
    /// <returns>True if the entry was found and deleted; false otherwise.</returns>
    public bool Delete(string fileName)
    {
      if (fileName == null)
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      CheckUpdating();

      bool result = false;
      int index = FindExistingUpdate(fileName);
      if (index >= 0 && _updates[index] != null)
      {
        result = true;
        _contentsEdited = true;
        _updates[index] = null;
        _updateCount -= 1;
      }
      else
      {
        throw new ZipException("Cannot find entry to delete");
      }

      return result;
    }

    /// <summary>
    /// Delete a <see cref="ZipEntry"/> from the archive.
    /// </summary>
    /// <param name="entry">The entry to delete.</param>
    public void Delete(ZipEntry entry)
    {
      if (entry == null)
      {
        throw new ArgumentNullException(nameof(entry));
      }

      CheckUpdating();

      int index = FindExistingUpdate(entry);
      if (index >= 0)
      {
        _contentsEdited = true;
        _updates[index] = null;
        _updateCount -= 1;
      }
      else
      {
        throw new ZipException("Cannot find entry to delete");
      }
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the this instance and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources;
    /// false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      DisposeInternal(disposing);
    }

    private static void CheckClassicPassword(CryptoStream classicCryptoStream, ZipEntry entry)
    {
      byte[] cryptbuffer = new byte[ZipConstants.CryptoHeaderSize];
      StreamUtils.ReadFully(classicCryptoStream, cryptbuffer);
      if (cryptbuffer[ZipConstants.CryptoHeaderSize - 1] != entry.CryptoCheckValue)
      {
        throw new ZipException("Invalid password");
      }
    }

    private static void WriteEncryptionHeader(Stream stream, long crcValue)
    {
      byte[] cryptBuffer = new byte[ZipConstants.CryptoHeaderSize];
      var rnd = new Random();
      rnd.NextBytes(cryptBuffer);
      cryptBuffer[11] = (byte)(crcValue >> 24);
      stream.Write(cryptBuffer, 0, cryptBuffer.Length);
    }

    /// <summary>
    /// Handles getting of encryption keys when required.
    /// </summary>
    /// <param name="fileName">The file for which encryption keys are required.</param>
    private void OnKeysRequired(string fileName)
    {
      if (KeysRequired != null)
      {
        var krea = new KeysRequiredEventArgs(fileName, _key);
        KeysRequired(this, krea);
        _key = krea.Key;
      }
    }

    /// <summary>
    /// Test a local header against that provided from the central directory
    /// </summary>
    /// <param name="entry">
    /// The entry to test against
    /// </param>
    /// <param name="tests">The type of <see cref="HeaderTest">tests</see> to carry out.</param>
    /// <returns>The offset of the entries data in the file</returns>
    private long TestLocalHeader(ZipEntry entry, HeaderTest tests)
    {
      lock (_baseStream)
      {
        bool testHeader = (tests & HeaderTest.Header) != 0;
        bool testData = (tests & HeaderTest.Extract) != 0;

        _baseStream.Seek(_offsetOfFirstEntry + entry.Offset, SeekOrigin.Begin);
        var signature = (int)ReadLEUint();
        if (signature != ZipConstants.LocalHeaderSignature
            && signature != ZipConstants.EncryptedHeaderSignature)
        {
          throw new ZipException(string.Format("Wrong local header signature @{0:X}",
                                               _offsetOfFirstEntry + entry.Offset));
        }

        var extractVersion = (short)(ReadLEUshort() & 0x00ff);
        var localFlags = (short)ReadLEUshort();
        var compressionMethod = (short)ReadLEUshort();
        var fileTime = (short)ReadLEUshort();
        var fileDate = (short)ReadLEUshort();
        uint crcValue = ReadLEUint();
        long compressedSize = ReadLEUint();
        long size = ReadLEUint();
        int storedNameLength = ReadLEUshort();
        int extraDataLength = ReadLEUshort();

        byte[] nameData = new byte[storedNameLength];
        StreamUtils.ReadFully(_baseStream, nameData);

        byte[] extraData = new byte[extraDataLength];
        StreamUtils.ReadFully(_baseStream, extraData);

        var localExtraData = new ZipExtraData(extraData);

        // Extra data / zip64 checks
        if (localExtraData.Find(1))
        {
          // 2010-03-04 Forum 10512: removed checks for version >= ZipConstants.VersionZip64
          // and size or compressedSize = MaxValue, due to rogue creators.

          size = localExtraData.ReadLong();
          compressedSize = localExtraData.ReadLong();

          if ((localFlags & (int)GeneralBitFlags.Descriptor) != 0)
          {
            // These may be valid if patched later
            if (size != -1 && size != entry.Size)
            {
              throw new ZipException("Size invalid for descriptor");
            }

            if (compressedSize != -1 && compressedSize != entry.CompressedSize)
            {
              throw new ZipException("Compressed size invalid for descriptor");
            }
          }
        }
        else
        {
          // No zip64 extra data but entry requires it.
          if (extractVersion >= ZipConstants.VersionZip64
              && ((uint)size == uint.MaxValue || (uint)compressedSize == uint.MaxValue))
          {
            throw new ZipException("Required Zip64 extended information missing");
          }
        }

        if (testData)
        {
          if (entry.IsFile)
          {
            if (!entry.IsCompressionMethodSupported())
            {
              throw new ZipException("Compression method not supported");
            }

            if (extractVersion > ZipConstants.VersionMadeBy
                || extractVersion > 20 && extractVersion < ZipConstants.VersionZip64)
            {
              throw new
                ZipException(string.Format("Version required to extract this entry not supported ({0})",
                                           extractVersion));
            }

            if ((localFlags
                 & (int)(GeneralBitFlags.Patched
                          | GeneralBitFlags.StrongEncryption
                          | GeneralBitFlags.EnhancedCompress
                          | GeneralBitFlags.HeaderMasked))
                != 0)
            {
              throw new
                ZipException("The library does not support the zip version required to extract this entry");
            }
          }
        }

        if (testHeader)
        {
          if (extractVersion <= 63
              && // Ignore later versions as we dont know about them..
              extractVersion != 10
              && extractVersion != 11
              && extractVersion != 20
              && extractVersion != 21
              && extractVersion != 25
              && extractVersion != 27
              && extractVersion != 45
              && extractVersion != 46
              && extractVersion != 50
              && extractVersion != 51
              && extractVersion != 52
              && extractVersion != 61
              && extractVersion != 62
              && extractVersion != 63)
          {
            throw new
              ZipException(string.Format("Version required to extract this entry is invalid ({0})",
                                         extractVersion));
          }

          // Local entry flags dont have reserved bit set on.
          if ((localFlags
               & (int)(GeneralBitFlags.ReservedPKware4
                        | GeneralBitFlags.ReservedPkware14
                        | GeneralBitFlags.ReservedPkware15))
              != 0)
          {
            throw new ZipException("Reserved bit flags cannot be set.");
          }

          // Encryption requires extract version >= 20
          if ((localFlags & (int)GeneralBitFlags.Encrypted) != 0 && extractVersion < 20)
          {
            throw new
              ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})",
                                         extractVersion));
          }

          // Strong encryption requires encryption flag to be set and extract version >= 50.
          if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0)
          {
            if ((localFlags & (int)GeneralBitFlags.Encrypted) == 0)
            {
              throw new ZipException("Strong encryption flag set but encryption flag is not set");
            }

            if (extractVersion < 50)
            {
              throw new
                ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})",
                                           extractVersion));
            }
          }

          // Patched entries require extract version >= 27
          if ((localFlags & (int)GeneralBitFlags.Patched) != 0 && extractVersion < 27)
          {
            throw new ZipException(string.Format("Patched data requires higher version than ({0})",
                                                 extractVersion));
          }

          // Central header flags match local entry flags.
          if (localFlags != entry.Flags)
          {
            throw new ZipException("Central header/local header flags mismatch");
          }

          // Central header compression method matches local entry
          if (entry.CompressionMethod != (CompressionMethod)compressionMethod)
          {
            throw new ZipException("Central header/local header compression method mismatch");
          }

          if (entry.Version != extractVersion)
          {
            throw new ZipException("Extract version mismatch");
          }

          // Strong encryption and extract version match
          if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0)
          {
            if (extractVersion < 62)
            {
              throw new ZipException("Strong encryption flag set but version not high enough");
            }
          }

          if ((localFlags & (int)GeneralBitFlags.HeaderMasked) != 0)
          {
            if (fileTime != 0 || fileDate != 0)
            {
              throw new ZipException("Header masked set but date/time values non-zero");
            }
          }

          if ((localFlags & (int)GeneralBitFlags.Descriptor) == 0)
          {
            if (crcValue != (uint)entry.Crc)
            {
              throw new ZipException("Central header/local header crc mismatch");
            }
          }

          // Crc valid for empty entry.
          // This will also apply to streamed entries where size isnt known and the header cant be patched
          if (size == 0 && compressedSize == 0)
          {
            if (crcValue != 0)
            {
              throw new ZipException("Invalid CRC for empty entry");
            }
          }

          // TODO: make test more correct...  can't compare lengths as was done originally as this can fail for MBCS strings
          // Assuming a code page at this point is not valid?  Best is to store the name length in the ZipEntry probably
          if (entry.Name.Length > storedNameLength)
          {
            throw new ZipException("File name length mismatch");
          }

          // Name data has already been read convert it and compare.
          string localName = ZipConstants.ConvertToStringExt(localFlags, nameData);

          // Central directory and local entry name match
          if (localName != entry.Name)
          {
            throw new ZipException("Central header and local header file name mismatch");
          }

          // Directories have zero actual size but can have compressed size
          if (entry.IsDirectory)
          {
            if (size > 0)
            {
              throw new ZipException("Directory cannot have size");
            }

            // There may be other cases where the compressed size can be greater than this?
            // If so until details are known we will be strict.
            if (entry.IsCrypted)
            {
              if (compressedSize > ZipConstants.CryptoHeaderSize + 2)
              {
                throw new ZipException("Directory compressed size invalid");
              }
            }
            else if (compressedSize > 2)
            {
              // When not compressed the directory size can validly be 2 bytes
              // if the true size wasnt known when data was originally being written.
              // NOTE: Versions of the library 0.85.4 and earlier always added 2 bytes
              throw new ZipException("Directory compressed size invalid");
            }
          }

          if (!ZipNameTransform.IsValidName(localName, true))
          {
            throw new ZipException("Name is invalid");
          }
        }

        // Tests that apply to both data and header.

        // Size can be verified only if it is known in the local header.
        // it will always be known in the central header.
        if ((localFlags & (int)GeneralBitFlags.Descriptor) == 0
            || (size > 0 || compressedSize > 0) && entry.Size > 0)
        {
          if (size != 0 && size != entry.Size)
          {
            throw new
              ZipException(string.Format("Size mismatch between central header({0}) and local header({1})",
                                         entry.Size,
                                         size));
          }

          if (compressedSize != 0
              && compressedSize != entry.CompressedSize
              && compressedSize != 0xFFFFFFFF
              && compressedSize != -1)
          {
            throw new
              ZipException(string.Format("Compressed size mismatch between central header({0}) and local header({1})",
                                         entry.CompressedSize,
                                         compressedSize));
          }
        }

        int extraLength = storedNameLength + extraDataLength;
        return _offsetOfFirstEntry + entry.Offset + ZipConstants.LocalHeaderBaseSize + extraLength;
      }
    }

    private void AddUpdate(ZipUpdate update)
    {
      _contentsEdited = true;

      int index = FindExistingUpdate(update.Entry.Name);

      if (index >= 0)
      {
        if (_updates[index] == null)
        {
          _updateCount += 1;
        }

        // Direct replacement is faster than delete and add.
        _updates[index] = update;
      }
      else
      {
        index = _updates.Count;
        _updates.Add(update);
        _updateCount += 1;
        _updateIndex.Add(update.Entry.Name, index);
      }
    }

    /* Modify not yet ready for public consumption.
		   Direct modification of an entry should not overwrite original data before its read.
		   Safe mode is trivial in this sense.
				public void Modify(ZipEntry original, ZipEntry updated)
				{
					if ( original == null ) {
						throw new ArgumentNullException("original");
					}

					if ( updated == null ) {
						throw new ArgumentNullException("updated");
					}

					CheckUpdating();
					contentsEdited_ = true;
					updates_.Add(new ZipUpdate(original, updated));
				}
		*/
    private void WriteLEShort(int value)
    {
      _baseStream.WriteByte((byte)(value & 0xff));
      _baseStream.WriteByte((byte)((value >> 8) & 0xff));
    }

    /// <summary>
    /// Write an unsigned short in little endian byte order.
    /// </summary>
    private void WriteLEUshort(ushort value)
    {
      _baseStream.WriteByte((byte)(value & 0xff));
      _baseStream.WriteByte((byte)(value >> 8));
    }

    /// <summary>
    /// Write an int in little endian byte order.
    /// </summary>
    private void WriteLEInt(int value)
    {
      WriteLEShort(value & 0xffff);
      WriteLEShort(value >> 16);
    }

    /// <summary>
    /// Write an unsigned int in little endian byte order.
    /// </summary>
    private void WriteLEUint(uint value)
    {
      WriteLEUshort((ushort)(value & 0xffff));
      WriteLEUshort((ushort)(value >> 16));
    }

    /// <summary>
    /// Write a long in little endian byte order.
    /// </summary>
    private void WriteLeLong(long value)
    {
      WriteLEInt((int)(value & 0xffffffff));
      WriteLEInt((int)(value >> 32));
    }

    private void WriteLocalEntryHeader(ZipUpdate update)
    {
      ZipEntry entry = update.OutEntry;

      // TODO: Local offset will require adjusting for multi-disk zip files.
      entry.Offset = _baseStream.Position;

      // TODO: Need to clear any entry flags that dont make sense or throw an exception here.
      if (update.Command != UpdateCommand.Copy)
      {
        if (entry.CompressionMethod == CompressionMethod.Deflated
            || entry.CompressionMethod == CompressionMethod.ZStd)
        {
          if (entry.Size == 0)
          {
            // No need to compress - no data.
            entry.CompressedSize = entry.Size;
            entry.Crc = 0;
            entry.CompressionMethod = CompressionMethod.Stored;
          }
        }
        else if (entry.CompressionMethod == CompressionMethod.Stored)
        {
          entry.Flags &= ~(int)GeneralBitFlags.Descriptor;
        }

        if (HaveKeys)
        {
          entry.IsCrypted = true;
          if (entry.Crc < 0)
          {
            entry.Flags |= (int)GeneralBitFlags.Descriptor;
          }
        }
        else
        {
          entry.IsCrypted = false;
        }

        switch (_useZip64)
        {
          case UseZip64.Dynamic:
            if (entry.Size < 0)
            {
              entry.ForceZip64();
            }

            break;

          case UseZip64.On:
            entry.ForceZip64();
            break;

          case UseZip64.Off:
            // Do nothing.  The entry itself may be using Zip64 independantly.
            break;
        }
      }

      // Write the local file header
      WriteLEInt(ZipConstants.LocalHeaderSignature);

      WriteLEShort(entry.Version);
      WriteLEShort(entry.Flags);

      WriteLEShort((byte)entry.CompressionMethod);
      WriteLEInt((int)entry.DosTime);

      if (!entry.HasCrc)
      {
        // Note patch address for updating CRC later.
        update.CrcPatchOffset = _baseStream.Position;
        WriteLEInt(0);
      }
      else
      {
        WriteLEInt(unchecked((int)entry.Crc));
      }

      if (entry.LocalHeaderRequiresZip64)
      {
        WriteLEInt(-1);
        WriteLEInt(-1);
      }
      else
      {
        if (entry.CompressedSize < 0 || entry.Size < 0)
        {
          update.SizePatchOffset = _baseStream.Position;
        }

        WriteLEInt((int)entry.CompressedSize);
        WriteLEInt((int)entry.Size);
      }

      byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);

      if (name.Length > 0xFFFF)
      {
        throw new ZipException("Entry name too long.");
      }

      var ed = new ZipExtraData(entry.ExtraData);

      if (entry.LocalHeaderRequiresZip64)
      {
        ed.StartNewEntry();

        // Local entry header always includes size and compressed size.
        // NOTE the order of these fields is reversed when compared to the normal headers!
        ed.AddLeLong(entry.Size);
        ed.AddLeLong(entry.CompressedSize);
        ed.AddNewEntry(1);
      }
      else
      {
        ed.Delete(1);
      }

      entry.ExtraData = ed.GetEntryData();

      WriteLEShort(name.Length);
      WriteLEShort(entry.ExtraData.Length);

      if (name.Length > 0)
      {
        _baseStream.Write(name, 0, name.Length);
      }

      if (entry.LocalHeaderRequiresZip64)
      {
        if (!ed.Find(1))
        {
          throw new ZipException("Internal error cannot find extra data");
        }

        update.SizePatchOffset = _baseStream.Position + ed.CurrentReadIndex;
      }

      if (entry.ExtraData.Length > 0)
      {
        _baseStream.Write(entry.ExtraData, 0, entry.ExtraData.Length);
      }
    }

    private int WriteCentralDirectoryHeader(ZipEntry entry)
    {
      if (entry.CompressedSize < 0)
      {
        throw new ZipException("Attempt to write central directory entry with unknown csize");
      }

      if (entry.Size < 0)
      {
        throw new ZipException("Attempt to write central directory entry with unknown size");
      }

      if (entry.Crc < 0)
      {
        throw new ZipException("Attempt to write central directory entry with unknown crc");
      }

      // Write the central file header
      WriteLEInt(ZipConstants.CentralHeaderSignature);

      // Version made by
      WriteLEShort(ZipConstants.VersionMadeBy);

      // Version required to extract
      WriteLEShort(entry.Version);

      WriteLEShort(entry.Flags);

      unchecked
      {
        WriteLEShort((byte)entry.CompressionMethod);
        WriteLEInt((int)entry.DosTime);
        WriteLEInt((int)entry.Crc);
      }

      if (entry.IsZip64Forced() || entry.CompressedSize >= 0xffffffff)
      {
        WriteLEInt(-1);
      }
      else
      {
        WriteLEInt((int)(entry.CompressedSize & 0xffffffff));
      }

      if (entry.IsZip64Forced() || entry.Size >= 0xffffffff)
      {
        WriteLEInt(-1);
      }
      else
      {
        WriteLEInt((int)entry.Size);
      }

      byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);

      if (name.Length > 0xFFFF)
      {
        throw new ZipException("Entry name is too long.");
      }

      WriteLEShort(name.Length);

      // Central header extra data is different to local header version so regenerate.
      var ed = new ZipExtraData(entry.ExtraData);

      if (entry.CentralHeaderRequiresZip64)
      {
        ed.StartNewEntry();

        if (entry.Size >= 0xffffffff || _useZip64 == UseZip64.On)
        {
          ed.AddLeLong(entry.Size);
        }

        if (entry.CompressedSize >= 0xffffffff || _useZip64 == UseZip64.On)
        {
          ed.AddLeLong(entry.CompressedSize);
        }

        if (entry.Offset >= 0xffffffff)
        {
          ed.AddLeLong(entry.Offset);
        }

        // Number of disk on which this file starts isnt supported and is never written here.
        ed.AddNewEntry(1);
      }
      else
      {
        // Should have already be done when local header was added.
        ed.Delete(1);
      }

      byte[] centralExtraData = ed.GetEntryData();

      WriteLEShort(centralExtraData.Length);
      WriteLEShort(entry.Comment != null
                     ? entry.Comment.Length
                     : 0);

      WriteLEShort(0); // disk number
      WriteLEShort(0); // internal file attributes

      // External file attributes...
      if (entry.ExternalFileAttributes != -1)
      {
        WriteLEInt(entry.ExternalFileAttributes);
      }
      else
      {
        if (entry.IsDirectory)
        {
          WriteLEUint(16);
        }
        else
        {
          WriteLEUint(0);
        }
      }

      if (entry.Offset >= 0xffffffff)
      {
        WriteLEUint(0xffffffff);
      }
      else
      {
        WriteLEUint((uint)(int)entry.Offset);
      }

      if (name.Length > 0)
      {
        _baseStream.Write(name, 0, name.Length);
      }

      if (centralExtraData.Length > 0)
      {
        _baseStream.Write(centralExtraData, 0, centralExtraData.Length);
      }

      byte[] rawComment = entry.Comment != null
                            ? Encoding.ASCII.GetBytes(entry.Comment)
                            : new byte[0];

      if (rawComment.Length > 0)
      {
        _baseStream.Write(rawComment, 0, rawComment.Length);
      }

      return ZipConstants.CentralHeaderBaseSize
             + name.Length
             + centralExtraData.Length
             + rawComment.Length;
    }

    private void PostUpdateCleanup()
    {
      _updateDataSource = null;
      _updates = null;
      _updateIndex = null;

      if (_archiveStorage != null)
      {
        _archiveStorage.Dispose();
        _archiveStorage = null;
      }
    }

    private string GetTransformedFileName(string name)
    {
      INameTransform transform = NameTransform;
      return transform != null
               ? transform.TransformFile(name)
               : name;
    }

    /// <summary>
    /// Get a raw memory buffer.
    /// </summary>
    /// <returns>Returns a raw memory buffer.</returns>
    private byte[] GetBuffer()
    {
      if (_copyBuffer == null)
      {
        _copyBuffer = new byte[_bufferSize];
      }

      return _copyBuffer;
    }

    private void CopyDescriptorBytes(ZipUpdate update, Stream dest, Stream source)
    {
      int bytesToCopy = GetDescriptorSize(update);

      if (bytesToCopy > 0)
      {
        byte[] buffer = GetBuffer();

        while (bytesToCopy > 0)
        {
          int readSize = Math.Min(buffer.Length, bytesToCopy);

          int bytesRead = source.Read(buffer, 0, readSize);
          if (bytesRead > 0)
          {
            dest.Write(buffer, 0, bytesRead);
            bytesToCopy -= bytesRead;
          }
          else
          {
            throw new ZipException("Unxpected end of stream");
          }
        }
      }
    }

    private void CopyBytes(ZipUpdate update,
                           Stream destination,
                           Stream source,
                           long bytesToCopy,
                           bool updateCrc)
    {
      if (destination == source)
      {
        throw new InvalidOperationException("Destination and source are the same");
      }

      // NOTE: Compressed size is updated elsewhere.
      var crc = new Crc32();
      byte[] buffer = GetBuffer();

      long targetBytes = bytesToCopy;
      long totalBytesRead = 0;

      int bytesRead;
      do
      {
        int readSize = buffer.Length;

        if (bytesToCopy < readSize)
        {
          readSize = (int)bytesToCopy;
        }

        bytesRead = source.Read(buffer, 0, readSize);
        if (bytesRead > 0)
        {
          if (updateCrc)
          {
            crc.Update(buffer, 0, bytesRead);
          }

          destination.Write(buffer, 0, bytesRead);
          bytesToCopy -= bytesRead;
          totalBytesRead += bytesRead;
        }
      } while (bytesRead > 0 && bytesToCopy > 0);

      if (totalBytesRead != targetBytes)
      {
        throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}",
                                             targetBytes,
                                             totalBytesRead));
      }

      if (updateCrc)
      {
        update.OutEntry.Crc = crc.Value;
      }
    }

    /// <summary>
    /// Get the size of the source descriptor for a <see cref="ZipUpdate"/>.
    /// </summary>
    /// <param name="update">The update to get the size for.</param>
    /// <returns>The descriptor size, zero if there isnt one.</returns>
    private int GetDescriptorSize(ZipUpdate update)
    {
      int result = 0;
      if ((update.Entry.Flags & (int)GeneralBitFlags.Descriptor) != 0)
      {
        result = ZipConstants.DataDescriptorSize - 4;
        if (update.Entry.LocalHeaderRequiresZip64)
        {
          result = ZipConstants.Zip64DataDescriptorSize - 4;
        }
      }

      return result;
    }

    private void CopyDescriptorBytesDirect(ZipUpdate update,
                                           Stream stream,
                                           ref long destinationPosition,
                                           long sourcePosition)
    {
      int bytesToCopy = GetDescriptorSize(update);

      while (bytesToCopy > 0)
      {
        var readSize = bytesToCopy;
        byte[] buffer = GetBuffer();

        stream.Position = sourcePosition;
        int bytesRead = stream.Read(buffer, 0, readSize);
        if (bytesRead > 0)
        {
          stream.Position = destinationPosition;
          stream.Write(buffer, 0, bytesRead);
          bytesToCopy -= bytesRead;
          destinationPosition += bytesRead;
          sourcePosition += bytesRead;
        }
        else
        {
          throw new ZipException("Unxpected end of stream");
        }
      }
    }

    private void CopyEntryDataDirect(ZipUpdate update,
                                     Stream stream,
                                     bool updateCrc,
                                     ref long destinationPosition,
                                     ref long sourcePosition)
    {
      long bytesToCopy = update.Entry.CompressedSize;

      // NOTE: Compressed size is updated elsewhere.
      var crc = new Crc32();
      byte[] buffer = GetBuffer();

      long targetBytes = bytesToCopy;
      long totalBytesRead = 0;

      int bytesRead;
      do
      {
        int readSize = buffer.Length;

        if (bytesToCopy < readSize)
        {
          readSize = (int)bytesToCopy;
        }

        stream.Position = sourcePosition;
        bytesRead = stream.Read(buffer, 0, readSize);
        if (bytesRead > 0)
        {
          if (updateCrc)
          {
            crc.Update(buffer, 0, bytesRead);
          }

          stream.Position = destinationPosition;
          stream.Write(buffer, 0, bytesRead);

          destinationPosition += bytesRead;
          sourcePosition += bytesRead;
          bytesToCopy -= bytesRead;
          totalBytesRead += bytesRead;
        }
      } while (bytesRead > 0 && bytesToCopy > 0);

      if (totalBytesRead != targetBytes)
      {
        throw new ZipException(string.Format("Failed to copy bytes expected {0} read {1}",
                                             targetBytes,
                                             totalBytesRead));
      }

      if (updateCrc)
      {
        update.OutEntry.Crc = crc.Value;
      }
    }

    private int FindExistingUpdate(ZipEntry entry)
    {
      int result = -1;
      string convertedName = GetTransformedFileName(entry.Name);

      if (_updateIndex.ContainsKey(convertedName))
      {
        result = _updateIndex[convertedName];
      }

      /*
						// This is slow like the coming of the next ice age but takes less storage and may be useful
						// for CF?
						for (int index = 0; index < updates_.Count; ++index)
						{
							ZipUpdate zu = ( ZipUpdate )updates_[index];
							if ( (zu.Entry.ZipFileIndex == entry.ZipFileIndex) &&
								(string.Compare(convertedName, zu.Entry.Name, true, CultureInfo.InvariantCulture) == 0) ) {
								result = index;
								break;
							}
						}
			 */
      return result;
    }

    private int FindExistingUpdate(string fileName)
    {
      int result = -1;

      string convertedName = GetTransformedFileName(fileName);

      if (_updateIndex.ContainsKey(convertedName))
      {
        result = _updateIndex[convertedName];
      }

      /*
						// This is slow like the coming of the next ice age but takes less storage and may be useful
						// for CF?
						for ( int index = 0; index < updates_.Count; ++index ) {
							if ( string.Compare(convertedName, (( ZipUpdate )updates_[index]).Entry.Name,
								true, CultureInfo.InvariantCulture) == 0 ) {
								result = index;
								break;
							}
						}
			 */

      return result;
    }

    /// <summary>
    /// Get an output stream for the specified <see cref="ZipEntry"/>
    /// </summary>
    /// <param name="entry">The entry to get an output stream for.</param>
    /// <returns>The output stream obtained for the entry.</returns>
    private Stream GetOutputStream(ZipEntry entry)
    {
      Stream result = _baseStream;

      if (entry.IsCrypted)
      {
        result = CreateAndInitEncryptionStream(result, entry);
      }

      switch (entry.CompressionMethod)
      {
        case CompressionMethod.Stored:
          result = new UncompressedStream(result);
          break;

        case CompressionMethod.Deflated:
          var dos = new DeflaterOutputStream(result, new Deflater(9, true));
          dos.IsStreamOwner = false;
          result = dos;
          break;

        case CompressionMethod.ZStd:
          throw new NotImplementedException("ZStd not implemented");

        default:
          throw new ZipException("Unknown compression method " + entry.CompressionMethod);
      }

      return result;
    }

    private void AddEntry(ZipFile workFile, ZipUpdate update)
    {
      Stream source = null;

      if (update.Entry.IsFile)
      {
        source = update.GetSource();

        if (source == null)
        {
          source = _updateDataSource.GetSource(update.Entry, update.Filename);
        }
      }

      if (source != null)
      {
        using (source)
        {
          long sourceStreamLength = source.Length;
          if (update.OutEntry.Size < 0)
          {
            update.OutEntry.Size = sourceStreamLength;
          }
          else
          {
            // Check for errant entries.
            if (update.OutEntry.Size != sourceStreamLength)
            {
              throw new ZipException("Entry size/stream size mismatch");
            }
          }

          workFile.WriteLocalEntryHeader(update);

          long dataStart = workFile._baseStream.Position;

          using (Stream output = workFile.GetOutputStream(update.OutEntry))
          {
            CopyBytes(update, output, source, sourceStreamLength, true);
          }

          long dataEnd = workFile._baseStream.Position;
          update.OutEntry.CompressedSize = dataEnd - dataStart;

          if ((update.OutEntry.Flags & (int)GeneralBitFlags.Descriptor)
              == (int)GeneralBitFlags.Descriptor)
          {
            var helper = new ZipHelperStream(workFile._baseStream);
            helper.WriteDataDescriptor(update.OutEntry);
          }
        }
      }
      else
      {
        workFile.WriteLocalEntryHeader(update);
        update.OutEntry.CompressedSize = 0;
      }
    }

    private void ModifyEntry(ZipFile workFile, ZipUpdate update)
    {
      workFile.WriteLocalEntryHeader(update);
      long dataStart = workFile._baseStream.Position;

      // TODO: This is slow if the changes don't effect the data!!
      if (update.Entry.IsFile && update.Filename != null)
      {
        using (Stream output = workFile.GetOutputStream(update.OutEntry))
        {
          using (Stream source = GetInputStream(update.Entry))
          {
            CopyBytes(update, output, source, source.Length, true);
          }
        }
      }

      long dataEnd = workFile._baseStream.Position;
      update.Entry.CompressedSize = dataEnd - dataStart;
    }

    private void CopyEntryDirect(ZipFile workFile, ZipUpdate update, ref long destinationPosition)
    {
      bool skipOver = false || update.Entry.Offset == destinationPosition;

      if (!skipOver)
      {
        _baseStream.Position = destinationPosition;
        workFile.WriteLocalEntryHeader(update);
        destinationPosition = _baseStream.Position;
      }

      long sourcePosition = 0;

      const int NameLengthOffset = 26;

      // TODO: Add base for SFX friendly handling
      long entryDataOffset = update.Entry.Offset + NameLengthOffset;

      _baseStream.Seek(entryDataOffset, SeekOrigin.Begin);

      // Clumsy way of handling retrieving the original name and extra data length for now.
      // TODO: Stop re-reading name and data length in CopyEntryDirect.
      uint nameLength = ReadLEUshort();
      uint extraLength = ReadLEUshort();

      sourcePosition = _baseStream.Position + nameLength + extraLength;

      if (skipOver)
      {
        if (update.OffsetBasedSize != -1)
        {
          destinationPosition += update.OffsetBasedSize;
        }
        else
        // TODO: Find out why this calculation comes up 4 bytes short on some entries in ODT (Office Document Text) archives.
        // WinZip produces a warning on these entries:
        // "caution: value of lrec.csize (compressed size) changed from ..."
        {
          destinationPosition += sourcePosition
                                 - entryDataOffset
                                 + NameLengthOffset
                                 + // Header size
                                 update.Entry.CompressedSize
                                 + GetDescriptorSize(update);
        }
      }
      else
      {
        if (update.Entry.CompressedSize > 0)
        {
          CopyEntryDataDirect(update,
                              _baseStream,
                              false,
                              ref destinationPosition,
                              ref sourcePosition);
        }

        CopyDescriptorBytesDirect(update, _baseStream, ref destinationPosition, sourcePosition);
      }
    }

    private void CopyEntry(ZipFile workFile, ZipUpdate update)
    {
      workFile.WriteLocalEntryHeader(update);

      if (update.Entry.CompressedSize > 0)
      {
        const int NameLengthOffset = 26;

        long entryDataOffset = update.Entry.Offset + NameLengthOffset;

        // TODO: This wont work for SFX files!
        _baseStream.Seek(entryDataOffset, SeekOrigin.Begin);

        uint nameLength = ReadLEUshort();
        uint extraLength = ReadLEUshort();

        _baseStream.Seek(nameLength + extraLength, SeekOrigin.Current);

        CopyBytes(update, workFile._baseStream, _baseStream, update.Entry.CompressedSize, false);
      }

      CopyDescriptorBytes(update, workFile._baseStream, _baseStream);
    }

    private void Reopen(Stream source)
    {
      if (source == null)
      {
        throw new ZipException("Failed to reopen archive - no source");
      }

      _isNewArchive = false;
      _baseStream = source;
      ReadEntries();
    }

    private void UpdateCommentOnly()
    {
      long baseLength = _baseStream.Length;

      ZipHelperStream updateFile = null;

      if (_archiveStorage.UpdateMode == FileUpdateMode.Safe)
      {
        Stream copyStream = _archiveStorage.MakeTemporaryCopy(_baseStream);
        updateFile = new ZipHelperStream(copyStream);
        updateFile.IsStreamOwner = true;

        _baseStream.Dispose();
        _baseStream = null;
      }
      else
      {
        if (_archiveStorage.UpdateMode == FileUpdateMode.Direct)
        {
          // TODO: archiveStorage wasnt originally intended for this use.
          // Need to revisit this to tidy up handling as archive storage currently doesnt
          // handle the original stream well.
          // The problem is when using an existing zip archive with an in memory archive storage.
          // The open stream wont support writing but the memory storage should open the same file not an in memory one.

          // Need to tidy up the archive storage interface and contract basically.
          _baseStream = _archiveStorage.OpenForDirectUpdate(_baseStream);
          updateFile = new ZipHelperStream(_baseStream);
        }
        else
        {
          _baseStream.Dispose();
          _baseStream = null;
          updateFile = new ZipHelperStream(Name);
        }
      }

      using (updateFile)
      {
        long locatedCentralDirOffset =
          updateFile.LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature,
                                              baseLength,
                                              ZipConstants.EndOfCentralRecordBaseSize,
                                              0xffff);
        if (locatedCentralDirOffset < 0)
        {
          throw new ZipException("Cannot find central directory");
        }

        const int CentralHeaderCommentSizeOffset = 16;
        updateFile.Position += CentralHeaderCommentSizeOffset;

        byte[] rawComment = _newComment.RawComment;

        updateFile.WriteLEShort(rawComment.Length);
        updateFile.Write(rawComment, 0, rawComment.Length);
        updateFile.SetLength(updateFile.Position);
      }

      if (_archiveStorage.UpdateMode == FileUpdateMode.Safe)
      {
        Reopen(_archiveStorage.ConvertTemporaryToFinal());
      }
      else
      {
        ReadEntries();
      }
    }

    private void RunUpdates()
    {
      long sizeEntries = 0;
      long endOfStream = 0;
      bool directUpdate = false;
      long destinationPosition = 0; // NOT SFX friendly

      ZipFile workFile;

      if (IsNewArchive)
      {
        workFile = this;
        workFile._baseStream.Position = 0;
        directUpdate = true;
      }
      else if (_archiveStorage.UpdateMode == FileUpdateMode.Direct)
      {
        workFile = this;
        workFile._baseStream.Position = 0;
        directUpdate = true;

        // Sort the updates by offset within copies/modifies, then adds.
        // This ensures that data required by copies will not be overwritten.
        _updates.Sort(new UpdateComparer());
      }
      else
      {
        workFile = Create(_archiveStorage.GetTemporaryOutput());
        workFile.UseZip64 = UseZip64;

        if (_key != null)
        {
          workFile._key = (byte[])_key.Clone();
        }
      }

      try
      {
        foreach (ZipUpdate update in _updates)
        {
          if (update != null)
          {
            switch (update.Command)
            {
              case UpdateCommand.Copy:
                if (directUpdate)
                {
                  CopyEntryDirect(workFile, update, ref destinationPosition);
                }
                else
                {
                  CopyEntry(workFile, update);
                }

                break;

              case UpdateCommand.Modify:
                // TODO: Direct modifying of an entry will take some legwork.
                ModifyEntry(workFile, update);
                break;

              case UpdateCommand.Add:
                if (!IsNewArchive && directUpdate)
                {
                  workFile._baseStream.Position = destinationPosition;
                }

                AddEntry(workFile, update);

                if (directUpdate)
                {
                  destinationPosition = workFile._baseStream.Position;
                }

                break;
            }
          }
        }

        if (!IsNewArchive && directUpdate)
        {
          workFile._baseStream.Position = destinationPosition;
        }

        long centralDirOffset = workFile._baseStream.Position;

        foreach (ZipUpdate update in _updates)
        {
          if (update != null)
          {
            sizeEntries += workFile.WriteCentralDirectoryHeader(update.OutEntry);
          }
        }

        byte[] theComment = _newComment != null
                              ? _newComment.RawComment
                              : ZipConstants.ConvertToArray(_comment);
        using (ZipHelperStream zhs = new ZipHelperStream(workFile._baseStream))
        {
          zhs.WriteEndOfCentralDirectory(_updateCount, sizeEntries, centralDirOffset, theComment);
        }

        endOfStream = workFile._baseStream.Position;

        // And now patch entries...
        foreach (ZipUpdate update in _updates)
        {
          if (update != null)
          {
            // If the size of the entry is zero leave the crc as 0 as well.
            // The calculated crc will be all bits on...
            if (update.CrcPatchOffset > 0 && update.OutEntry.CompressedSize > 0)
            {
              workFile._baseStream.Position = update.CrcPatchOffset;
              workFile.WriteLEInt((int)update.OutEntry.Crc);
            }

            if (update.SizePatchOffset > 0)
            {
              workFile._baseStream.Position = update.SizePatchOffset;
              if (update.OutEntry.LocalHeaderRequiresZip64)
              {
                workFile.WriteLeLong(update.OutEntry.Size);
                workFile.WriteLeLong(update.OutEntry.CompressedSize);
              }
              else
              {
                workFile.WriteLEInt((int)update.OutEntry.CompressedSize);
                workFile.WriteLEInt((int)update.OutEntry.Size);
              }
            }
          }
        }
      }
      catch
      {
        workFile.Close();
        if (!directUpdate && workFile.Name != null)
        {
          File.Delete(workFile.Name);
        }

        throw;
      }

      if (directUpdate)
      {
        workFile._baseStream.SetLength(endOfStream);
        workFile._baseStream.Flush();
        _isNewArchive = false;
        ReadEntries();
      }
      else
      {
        _baseStream.Dispose();
        Reopen(_archiveStorage.ConvertTemporaryToFinal());
      }
    }

    private void CheckUpdating()
    {
      if (_updates == null)
      {
        throw new InvalidOperationException("BeginUpdate has not been called");
      }
    }

    private void DisposeInternal(bool disposing)
    {
      if (!_isDisposed)
      {
        _isDisposed = true;
        _entries = new ZipEntry[0];

        if (IsStreamOwner && _baseStream != null)
        {
          lock (_baseStream)
          {
            _baseStream.Dispose();
          }
        }

        PostUpdateCleanup();
      }
    }

    /// <summary>
    /// Read an unsigned short in little endian byte order.
    /// </summary>
    /// <returns>Returns the value read.</returns>
    /// <exception cref="EndOfStreamException">
    /// The stream ends prematurely
    /// </exception>
    private ushort ReadLEUshort()
    {
      int data1 = _baseStream.ReadByte();

      if (data1 < 0)
      {
        throw new EndOfStreamException("End of stream");
      }

      int data2 = _baseStream.ReadByte();

      if (data2 < 0)
      {
        throw new EndOfStreamException("End of stream");
      }

      return unchecked((ushort)((ushort)data1 | (ushort)(data2 << 8)));
    }

    /// <summary>
    /// Read a uint in little endian byte order.
    /// </summary>
    /// <returns>Returns the value read.</returns>
    /// <exception cref="IOException">
    /// An i/o error occurs.
    /// </exception>
    /// <exception cref="System.IO.EndOfStreamException">
    /// The file ends prematurely
    /// </exception>
    private uint ReadLEUint()
    {
      return (uint)(ReadLEUshort() | (ReadLEUshort() << 16));
    }

    private ulong ReadLEUlong()
    {
      return ReadLEUint() | ((ulong)ReadLEUint() << 32);
    }

    // NOTE this returns the offset of the first byte after the signature.
    private long LocateBlockWithSignature(int signature,
                                          long endLocation,
                                          int minimumBlockSize,
                                          int maximumVariableData)
    {
      using (ZipHelperStream les = new ZipHelperStream(_baseStream))
      {
        return les.LocateBlockWithSignature(signature,
                                            endLocation,
                                            minimumBlockSize,
                                            maximumVariableData);
      }
    }

    /// <summary>
    /// Search for and read the central directory of a zip file filling the entries array.
    /// </summary>
    /// <exception cref="System.IO.IOException">
    /// An i/o error occurs.
    /// </exception>
    /// <exception cref="ZipException">
    /// The central directory is malformed or cannot be found
    /// </exception>
    private void ReadEntries()
    {
      // Search for the End Of Central Directory.  When a zip comment is
      // present the directory will start earlier
      //
      // The search is limited to 64K which is the maximum size of a trailing comment field to aid speed.
      // This should be compatible with both SFX and ZIP files but has only been tested for Zip files
      // If a SFX file has the Zip data attached as a resource and there are other resources occuring later then
      // this could be invalid.
      // Could also speed this up by reading memory in larger blocks.

      if (_baseStream.CanSeek == false)
      {
        throw new ZipException("ZipFile stream must be seekable");
      }

      long locatedEndOfCentralDir =
        LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature,
                                 _baseStream.Length,
                                 ZipConstants.EndOfCentralRecordBaseSize,
                                 0xffff);

      if (locatedEndOfCentralDir < 0)
      {
        throw new ZipException("Cannot find central directory");
      }

      // Read end of central directory record
      ushort thisDiskNumber = ReadLEUshort();
      ushort startCentralDirDisk = ReadLEUshort();
      ulong entriesForThisDisk = ReadLEUshort();
      ulong entriesForWholeCentralDir = ReadLEUshort();
      ulong centralDirSize = ReadLEUint();
      long offsetOfCentralDir = ReadLEUint();
      uint commentSize = ReadLEUshort();

      if (commentSize > 0)
      {
        byte[] comment = new byte[commentSize];

        StreamUtils.ReadFully(_baseStream, comment);
        _comment = ZipConstants.ConvertToString(comment);
      }
      else
      {
        _comment = string.Empty;
      }

      bool isZip64 = false;

      // Check if zip64 header information is required.
      if (thisDiskNumber == 0xffff
          || startCentralDirDisk == 0xffff
          || entriesForThisDisk == 0xffff
          || entriesForWholeCentralDir == 0xffff
          || centralDirSize == 0xffffffff
          || offsetOfCentralDir == 0xffffffff)
      {
        isZip64 = true;

        long offset = LocateBlockWithSignature(ZipConstants.Zip64CentralDirLocatorSignature,
                                               locatedEndOfCentralDir,
                                               0,
                                               0x1000);
        if (offset < 0)
        {
          throw new ZipException("Cannot find Zip64 locator");
        }

        // number of the disk with the start of the zip64 end of central directory 4 bytes
        // relative offset of the zip64 end of central directory record 8 bytes
        // total number of disks 4 bytes
        ReadLEUint(); // startDisk64 is not currently used
        ulong offset64 = ReadLEUlong();
        uint totalDisks = ReadLEUint();

        _baseStream.Position = (long)offset64;
        long sig64 = ReadLEUint();

        if (sig64 != ZipConstants.Zip64CentralFileHeaderSignature)
        {
          throw new ZipException(string.Format("Invalid Zip64 Central directory signature at {0:X}",
                                               offset64));
        }

        // NOTE: Record size = SizeOfFixedFields + SizeOfVariableData - 12.
        ulong recordSize = ReadLEUlong();
        int versionMadeBy = ReadLEUshort();
        int versionToExtract = ReadLEUshort();
        uint thisDisk = ReadLEUint();
        uint centralDirDisk = ReadLEUint();
        entriesForThisDisk = ReadLEUlong();
        entriesForWholeCentralDir = ReadLEUlong();
        centralDirSize = ReadLEUlong();
        offsetOfCentralDir = (long)ReadLEUlong();

        // NOTE: zip64 extensible data sector (variable size) is ignored.
      }

      _entries = new ZipEntry[entriesForThisDisk];

      // SFX/embedded support, find the offset of the first entry vis the start of the stream
      // This applies to Zip files that are appended to the end of an SFX stub.
      // Or are appended as a resource to an executable.
      // Zip files created by some archivers have the offsets altered to reflect the true offsets
      // and so dont require any adjustment here...
      // TODO: Difficulty with Zip64 and SFX offset handling needs resolution - maths?
      if (!isZip64 && offsetOfCentralDir < locatedEndOfCentralDir - (4 + (long)centralDirSize))
      {
        _offsetOfFirstEntry =
          locatedEndOfCentralDir - (4 + (long)centralDirSize + offsetOfCentralDir);
        if (_offsetOfFirstEntry <= 0)
        {
          throw new ZipException("Invalid embedded zip archive");
        }
      }

      _baseStream.Seek(_offsetOfFirstEntry + offsetOfCentralDir, SeekOrigin.Begin);

      for (ulong i = 0; i < entriesForThisDisk; i++)
      {
        var headerOffset = _baseStream.Position;
        if (ReadLEUint() != ZipConstants.CentralHeaderSignature)
        {
          throw new ZipException("Wrong Central Directory signature");
        }

        int versionMadeBy = ReadLEUshort();
        int versionToExtract = ReadLEUshort();
        int bitFlags = ReadLEUshort();
        int method = ReadLEUshort();
        uint dostime = ReadLEUint();
        uint crc = ReadLEUint();
        var csize = (long)ReadLEUint();
        var size = (long)ReadLEUint();
        int nameLen = ReadLEUshort();
        int extraLen = ReadLEUshort();
        int commentLen = ReadLEUshort();

        int diskStartNo = ReadLEUshort();        // Not currently used
        int internalAttributes = ReadLEUshort(); // Not currently used

        uint externalAttributes = ReadLEUint();
        long offset = ReadLEUint();

        byte[] buffer = new byte[Math.Max(nameLen, commentLen)];

        StreamUtils.ReadFully(_baseStream, buffer, 0, nameLen);
        string name = ZipConstants.ConvertToStringExt(bitFlags, buffer, nameLen);

        var entry = new ZipEntry(name, versionToExtract, versionMadeBy, (CompressionMethod)method);
        entry.HeaderOffset = headerOffset;
        entry.Crc = crc & 0xffffffffL;
        entry.Size = size & 0xffffffffL;
        entry.CompressedSize = csize & 0xffffffffL;
        entry.Flags = bitFlags;
        entry.DosTime = dostime;
        entry.ZipFileIndex = (long)i;
        entry.Offset = offset;
        entry.ExternalFileAttributes = (int)externalAttributes;

        if ((bitFlags & 8) == 0)
        {
          entry.CryptoCheckValue = (byte)(crc >> 24);
        }
        else
        {
          entry.CryptoCheckValue = (byte)((dostime >> 8) & 0xff);
        }

        if (extraLen > 0)
        {
          byte[] extra = new byte[extraLen];
          StreamUtils.ReadFully(_baseStream, extra);
          entry.ExtraData = extra;
        }

        entry.ProcessExtraData(false);

        if (commentLen > 0)
        {
          StreamUtils.ReadFully(_baseStream, buffer, 0, commentLen);
          entry.Comment = ZipConstants.ConvertToStringExt(bitFlags, buffer, commentLen);
        }

        _entries[i] = entry;
      }
    }

    /// <summary>
    /// Locate the data for a given entry.
    /// </summary>
    /// <returns>
    /// The start offset of the data.
    /// </returns>
    /// <exception cref="System.IO.EndOfStreamException">
    /// The stream ends prematurely
    /// </exception>
    /// <exception cref="ZipException">
    /// The local header signature is invalid, the entry and central header file name lengths are different
    /// or the local and entry compression methods dont match
    /// </exception>
    private long LocateEntry(ZipEntry entry)
    {
      return TestLocalHeader(entry, HeaderTest.Extract);
    }

    private Stream CreateAndInitAesDecryptionStream(Stream baseStream, ZipEntry entry)
    {
      using (Aes aes = new AesManaged())
      {
        aes.Key = _key;
        aes.IV = new byte[16];
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;

        var cipher = aes.CreateDecryptor();

        var crypto = new CryptoStream(baseStream, cipher, CryptoStreamMode.Read);

        var buffer = new MemoryStream();
        crypto.CopyTo(buffer);

        // Trim NULL off end of stream
        buffer.Seek(-1, SeekOrigin.End);
        while (buffer.Position > 1 && buffer.ReadByte() == 0)
        {
          buffer.Seek(-2, SeekOrigin.Current);
        }

        buffer.SetLength(buffer.Position);

        buffer.Seek(0, SeekOrigin.Begin);

        return buffer;
      }
    }

    private Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry)
    {
      CryptoStream result = null;

      if (entry.Version < ZipConstants.VersionStrongEncryption
          || (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0)
      {
        var classicManaged = new PkzipClassicManaged();

        OnKeysRequired(entry.Name);
        if (HaveKeys == false)
        {
          throw new ZipException("No password available for encrypted stream");
        }

        result = new CryptoStream(baseStream,
                                  classicManaged.CreateDecryptor(_key, null),
                                  CryptoStreamMode.Read);
        CheckClassicPassword(result, entry);
      }
      else
      {
        if (entry.Version == ZipConstants.VERSION_AES)
        {
          //
          OnKeysRequired(entry.Name);
          if (HaveKeys == false)
          {
            throw new ZipException("No password available for AES encrypted stream");
          }

          int saltLen = entry.AESSaltLen;
          byte[] saltBytes = new byte[saltLen];
          int saltIn = baseStream.Read(saltBytes, 0, saltLen);
          if (saltIn != saltLen)
          {
            throw new ZipException("AES Salt expected " + saltLen + " got " + saltIn);
          }

          //
          byte[] pwdVerifyRead = new byte[2];
          baseStream.Read(pwdVerifyRead, 0, 2);
          int blockSize = entry.AESKeySize / 8; // bits to bytes

          var decryptor = new ZipAESTransform(_rawPassword, saltBytes, blockSize, false);
          byte[] pwdVerifyCalc = decryptor.PwdVerifier;
          if (pwdVerifyCalc[0] != pwdVerifyRead[0] || pwdVerifyCalc[1] != pwdVerifyRead[1])
          {
            throw new ZipException("Invalid password for AES");
          }

          result = new ZipAESStream(baseStream, decryptor, CryptoStreamMode.Read);
        }
        else
        {
          throw new ZipException("Decryption method not supported");
        }
      }

      return result;
    }

    private Stream CreateAndInitEncryptionStream(Stream baseStream, ZipEntry entry)
    {
      CryptoStream result = null;
      if (entry.Version < ZipConstants.VersionStrongEncryption
          || (entry.Flags & (int)GeneralBitFlags.StrongEncryption) == 0)
      {
        var classicManaged = new PkzipClassicManaged();

        OnKeysRequired(entry.Name);
        if (HaveKeys == false)
        {
          throw new ZipException("No password available for encrypted stream");
        }

        // Closing a CryptoStream will close the base stream as well so wrap it in an UncompressedStream
        // which doesnt do this.
        result = new CryptoStream(new UncompressedStream(baseStream),
                                  classicManaged.CreateEncryptor(_key, null),
                                  CryptoStreamMode.Write);

        if (entry.Crc < 0 || (entry.Flags & 8) != 0)
        {
          WriteEncryptionHeader(result, entry.DosTime << 16);
        }
        else
        {
          WriteEncryptionHeader(result, entry.Crc);
        }
      }

      return result;
    }

    /// <summary>
    /// Class used to sort updates.
    /// </summary>
    private class UpdateComparer : IComparer<ZipUpdate>
    {
      /// <summary>
      /// Compares two objects and returns a value indicating whether one is
      /// less than, equal to or greater than the other.
      /// </summary>
      /// <param name="x">First object to compare</param>
      /// <param name="y">Second object to compare.</param>
      /// <returns>Compare result.</returns>
      public int Compare(ZipUpdate x, ZipUpdate y)
      {
        int result;

        if (x == null)
        {
          if (y == null)
          {
            result = 0;
          }
          else
          {
            result = -1;
          }
        }
        else if (y == null)
        {
          result = 1;
        }
        else
        {
          int xCmdValue = x.Command == UpdateCommand.Copy || x.Command == UpdateCommand.Modify
                            ? 0
                            : 1;
          int yCmdValue = y.Command == UpdateCommand.Copy || y.Command == UpdateCommand.Modify
                            ? 0
                            : 1;

          result = xCmdValue - yCmdValue;
          if (result == 0)
          {
            long offsetDiff = x.Entry.Offset - y.Entry.Offset;
            if (offsetDiff < 0)
            {
              result = -1;
            }
            else if (offsetDiff == 0)
            {
              result = 0;
            }
            else
            {
              result = 1;
            }
          }
        }

        return result;
      }
    }

    /// <summary>
    /// Represents a pending update to a Zip file.
    /// </summary>
    private class ZipUpdate
    {
      private ZipEntry outEntry_;
      private readonly IStaticDataSource dataSource_;

      public ZipUpdate(string fileName, ZipEntry entry)
      {
        Command = UpdateCommand.Add;
        Entry = entry;
        Filename = fileName;
      }

      public ZipUpdate(IStaticDataSource dataSource, ZipEntry entry)
      {
        Command = UpdateCommand.Add;
        Entry = entry;
        dataSource_ = dataSource;
      }

      public ZipUpdate(ZipEntry original, ZipEntry updated)
      {
        throw new ZipException("Modify not currently supported");
        /*
					command_ = UpdateCommand.Modify;
					entry_ = ( ZipEntry )original.Clone();
					outEntry_ = ( ZipEntry )updated.Clone();
				*/
      }

      public ZipUpdate(UpdateCommand command, ZipEntry entry)
      {
        Command = command;
        Entry = (ZipEntry)entry.Clone();
      }

      /// <summary>
      /// Copy an existing entry.
      /// </summary>
      /// <param name="entry">The existing entry to copy.</param>
      public ZipUpdate(ZipEntry entry) : this(UpdateCommand.Copy, entry)
      {
        // Do nothing.
      }

      /// <summary>
      /// Get the <see cref="ZipEntry"/> for this update.
      /// </summary>
      /// <remarks>This is the source or original entry.</remarks>
      public ZipEntry Entry { get; private set; }

      /// <summary>
      /// Get the <see cref="ZipEntry"/> that will be written to the updated/new file.
      /// </summary>
      public ZipEntry OutEntry
      {
        get
        {
          if (outEntry_ == null)
          {
            outEntry_ = (ZipEntry)Entry.Clone();
          }

          return outEntry_;
        }
      }

      /// <summary>
      /// Get the command for this update.
      /// </summary>
      public UpdateCommand Command { get; private set; }

      /// <summary>
      /// Get the filename if any for this update.  Null if none exists.
      /// </summary>
      public string Filename { get; private set; }

      /// <summary>
      /// Get/set the location of the size patch for this update.
      /// </summary>
      public long SizePatchOffset { get; set; } = -1;

      /// <summary>
      /// Get /set the location of the crc patch for this update.
      /// </summary>
      public long CrcPatchOffset { get; set; } = -1;

      /// <summary>
      /// Get/set the size calculated by offset.
      /// Specifically, the difference between this and next entry's starting offset.
      /// </summary>
      public long OffsetBasedSize { get; set; } = -1;

      public Stream GetSource()
      {
        Stream result = null;
        if (dataSource_ != null)
        {
          result = dataSource_.GetSource();
        }

        return result;
      }
    }

    /// <summary>
    /// Represents a string from a <see cref="ZipFile"/> which is stored as an array of bytes.
    /// </summary>
    private class ZipString
    {
      private string comment_;

      private byte[] rawComment_;

      /// <summary>
      /// Initialise a <see cref="ZipString"/> with a string.
      /// </summary>
      /// <param name="comment">The textual string form.</param>
      public ZipString(string comment)
      {
        comment_ = comment;
        IsSourceString = true;
      }

      /// <summary>
      /// Initialise a <see cref="ZipString"/> using a string in its binary 'raw' form.
      /// </summary>
      /// <param name="rawString"></param>
      public ZipString(byte[] rawString)
      {
        rawComment_ = rawString;
      }

      /// <summary>
      /// Get a value indicating the original source of data for this instance.
      /// True if the source was a string; false if the source was binary data.
      /// </summary>
      public bool IsSourceString { get; private set; }

      /// <summary>
      /// Get the length of the comment when represented as raw bytes.
      /// </summary>
      public int RawLength
      {
        get
        {
          MakeBytesAvailable();
          return rawComment_.Length;
        }
      }

      /// <summary>
      /// Get the comment in its 'raw' form as plain bytes.
      /// </summary>
      public byte[] RawComment
      {
        get
        {
          MakeBytesAvailable();
          return (byte[])rawComment_.Clone();
        }
      }

      /// <summary>
      /// Implicit conversion of comment to a string.
      /// </summary>
      /// <param name="zipString">The <see cref="ZipString"/> to convert to a string.</param>
      /// <returns>The textual equivalent for the input value.</returns>
      public static implicit operator string(ZipString zipString)
      {
        zipString.MakeTextAvailable();
        return zipString.comment_;
      }

      /// <summary>
      /// Reset the comment to its initial state.
      /// </summary>
      public void Reset()
      {
        if (IsSourceString)
        {
          rawComment_ = null;
        }
        else
        {
          comment_ = null;
        }
      }

      private void MakeTextAvailable()
      {
        if (comment_ == null)
        {
          comment_ = ZipConstants.ConvertToString(rawComment_);
        }
      }

      private void MakeBytesAvailable()
      {
        if (rawComment_ == null)
        {
          rawComment_ = ZipConstants.ConvertToArray(comment_);
        }
      }
    }

    /// <summary>
    /// An <see cref="IEnumerator">enumerator</see> for <see cref="ZipEntry">Zip entries</see>
    /// </summary>
    private class ZipEntryEnumerator : IEnumerator
    {
      private readonly ZipEntry[] array;

      private int index = -1;

      public ZipEntryEnumerator(ZipEntry[] entries)
      {
        array = entries;
      }

      public object Current => array[index];

      public void Reset()
      {
        index = -1;
      }

      public bool MoveNext()
      {
        return ++index < array.Length;
      }
    }

    /// <summary>
    /// An <see cref="UncompressedStream"/> is a stream that you can write uncompressed data
    /// to and flush, but cannot read, seek or do anything else to.
    /// </summary>
    private class UncompressedStream : Stream
    {
      private readonly Stream baseStream_;

      public UncompressedStream(Stream baseStream)
      {
        baseStream_ = baseStream;
      }

      /// <summary>
      /// Gets a value indicating whether the current stream supports reading.
      /// </summary>
      public override bool CanRead => false;

      /// <summary>
      /// Gets a value indicating whether the current stream supports writing.
      /// </summary>
      public override bool CanWrite => baseStream_.CanWrite;

      /// <summary>
      /// Gets a value indicating whether the current stream supports seeking.
      /// </summary>
      public override bool CanSeek => false;

      /// <summary>
      /// Get the length in bytes of the stream.
      /// </summary>
      public override long Length => 0;

      /// <summary>
      /// Gets or sets the position within the current stream.
      /// </summary>
      public override long Position
      {
        get => baseStream_.Position;
        set => throw new NotImplementedException();
      }

      /// <summary>
      /// Write any buffered data to underlying storage.
      /// </summary>
      public override void Flush()
      {
        baseStream_.Flush();
      }

      /// <summary>
      /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
      /// </summary>
      /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
      /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
      /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
      /// <returns>
      /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
      /// </returns>
      /// <exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
      /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
      public override int Read(byte[] buffer, int offset, int count)
      {
        return 0;
      }

      /// <summary>
      /// Sets the position within the current stream.
      /// </summary>
      /// <param name="offset">A byte offset relative to the origin parameter.</param>
      /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
      /// <returns>
      /// The new position within the current stream.
      /// </returns>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override long Seek(long offset, SeekOrigin origin)
      {
        return 0;
      }

      /// <summary>
      /// Sets the length of the current stream.
      /// </summary>
      /// <param name="value">The desired length of the current stream in bytes.</param>
      /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override void SetLength(long value) { }

      /// <summary>
      /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
      /// </summary>
      /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
      /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
      /// <param name="count">The number of bytes to be written to the current stream.</param>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
      /// <exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
      public override void Write(byte[] buffer, int offset, int count)
      {
        baseStream_.Write(buffer, offset, count);
      }
    }

    /// <summary>
    /// A <see cref="PartialInputStream"/> is an <see cref="InflaterInputStream"/>
    /// whose data is only a part or subsection of a file.
    /// </summary>
    private class PartialInputStream : Stream
    {
      private readonly ZipFile zipFile_;

      private readonly Stream baseStream_;

      private readonly long start_;

      private readonly long length_;

      private long readPos_;

      private readonly long end_;

      /// <summary>
      /// Initialise a new instance of the <see cref="PartialInputStream"/> class.
      /// </summary>
      /// <param name="zipFile">The <see cref="ZipFile"/> containing the underlying stream to use for IO.</param>
      /// <param name="start">The start of the partial data.</param>
      /// <param name="length">The length of the partial data.</param>
      public PartialInputStream(ZipFile zipFile, long start, long length)
      {
        start_ = start;
        length_ = length;

        // Although this is the only time the zipfile is used
        // keeping a reference here prevents premature closure of
        // this zip file and thus the baseStream_.

        // Code like this will cause apparently random failures depending
        // on the size of the files and when garbage is collected.
        //
        // ZipFile z = new ZipFile (stream);
        // Stream reader = z.GetInputStream(0);
        // uses reader here....
        zipFile_ = zipFile;
        baseStream_ = zipFile_._baseStream;
        readPos_ = start;
        end_ = start + length;
      }

      /// <summary>
      /// Gets or sets the position within the current stream.
      /// </summary>
      /// <value></value>
      /// <returns>The current position within the stream.</returns>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override long Position
      {
        get => readPos_ - start_;
        set
        {
          long newPos = start_ + value;

          if (newPos < start_)
          {
            throw new ArgumentException("Negative position is invalid");
          }

          if (newPos > end_)
          {
            throw new InvalidOperationException("Cannot seek past end");
          }

          readPos_ = newPos;
        }
      }

      /// <summary>
      /// Gets the length in bytes of the stream.
      /// </summary>
      /// <value></value>
      /// <returns>A long value representing the length of the stream in bytes.</returns>
      /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override long Length => length_;

      /// <summary>
      /// Gets a value indicating whether the current stream supports writing.
      /// </summary>
      /// <value>false</value>
      /// <returns>true if the stream supports writing; otherwise, false.</returns>
      public override bool CanWrite => false;

      /// <summary>
      /// Gets a value indicating whether the current stream supports seeking.
      /// </summary>
      /// <value>true</value>
      /// <returns>true if the stream supports seeking; otherwise, false.</returns>
      public override bool CanSeek => true;

      /// <summary>
      /// Gets a value indicating whether the current stream supports reading.
      /// </summary>
      /// <value>true.</value>
      /// <returns>true if the stream supports reading; otherwise, false.</returns>
      public override bool CanRead => true;

      /// <summary>
      /// Gets a value that determines whether the current stream can time out.
      /// </summary>
      /// <value></value>
      /// <returns>A value that determines whether the current stream can time out.</returns>
      public override bool CanTimeout => baseStream_.CanTimeout;

      /// <summary>
      /// Read a byte from this stream.
      /// </summary>
      /// <returns>Returns the byte read or -1 on end of stream.</returns>
      public override int ReadByte()
      {
        if (readPos_ >= end_)
        {
          // -1 is the correct value at end of stream.
          return -1;
        }

        lock (baseStream_)
        {
          baseStream_.Seek(readPos_++, SeekOrigin.Begin);
          return baseStream_.ReadByte();
        }
      }

      /// <summary>
      /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
      /// </summary>
      /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
      /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
      /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
      /// <returns>
      /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
      /// </returns>
      /// <exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
      /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
      public override int Read(byte[] buffer, int offset, int count)
      {
        lock (baseStream_)
        {
          if (count > end_ - readPos_)
          {
            count = (int)(end_ - readPos_);
            if (count == 0)
            {
              return 0;
            }
          }

          // Protect against Stream implementations that throw away their buffer on every Seek
          // (for example, Mono FileStream)
          if (baseStream_.Position != readPos_)
          {
            baseStream_.Seek(readPos_, SeekOrigin.Begin);
          }

          int readCount = baseStream_.Read(buffer, offset, count);
          if (readCount > 0)
          {
            readPos_ += readCount;
          }

          return readCount;
        }
      }

      /// <summary>
      /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
      /// </summary>
      /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
      /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
      /// <param name="count">The number of bytes to be written to the current stream.</param>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
      /// <exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
      public override void Write(byte[] buffer, int offset, int count)
      {
        throw new NotSupportedException();
      }

      /// <summary>
      /// When overridden in a derived class, sets the length of the current stream.
      /// </summary>
      /// <param name="value">The desired length of the current stream in bytes.</param>
      /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override void SetLength(long value)
      {
        throw new NotSupportedException();
      }

      /// <summary>
      /// When overridden in a derived class, sets the position within the current stream.
      /// </summary>
      /// <param name="offset">A byte offset relative to the origin parameter.</param>
      /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
      /// <returns>
      /// The new position within the current stream.
      /// </returns>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
      /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
      public override long Seek(long offset, SeekOrigin origin)
      {
        long newPos = readPos_;

        switch (origin)
        {
          case SeekOrigin.Begin:
            newPos = start_ + offset;
            break;

          case SeekOrigin.Current:
            newPos = readPos_ + offset;
            break;

          case SeekOrigin.End:
            newPos = end_ + offset;
            break;
        }

        if (newPos < start_)
        {
          throw new ArgumentException("Negative position is invalid");
        }

        if (newPos >= end_)
        {
          throw new IOException("Cannot seek past end");
        }

        readPos_ = newPos;
        return readPos_;
      }

      /// <summary>
      /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
      /// </summary>
      /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
      public override void Flush()
      {
        // Nothing to do.
      }
    }
  }
}
