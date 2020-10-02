namespace Scope.Interfaces
{
  /// <summary>
  /// Implementations of this class provide a single searchable file type.
  /// </summary>
  public interface ISearchableFileType
  {
    string Name { get; }
    string Extension { get; }
  }
}
