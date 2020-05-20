namespace Scope.Zip.Zip
{
  /// <summary>
  /// The possible ways of <see cref="ZipFile.CommitUpdate()">applying updates</see> to an archive.
  /// </summary>
  public enum FileUpdateMode
  {
    /// <summary>
    /// Perform all updates on temporary files ensuring that the original file is saved.
    /// </summary>
    Safe,

    /// <summary>
    /// Update the archive directly, which is faster but less safe.
    /// </summary>
    Direct
  }
}
