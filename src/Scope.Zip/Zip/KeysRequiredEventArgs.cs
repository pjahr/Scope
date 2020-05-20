using System;

namespace Scope.Zip.Zip
{
  /// <summary>
  /// Arguments used with KeysRequiredEvent
  /// </summary>
  public class KeysRequiredEventArgs : EventArgs
  {
    #region Constructors

    private readonly string fileName;

    private byte[] key;

    /// <summary>
    /// Initialise a new instance of <see cref="KeysRequiredEventArgs"></see>
    /// </summary>
    /// <param name="name">The name of the file for which keys are required.</param>
    public KeysRequiredEventArgs(string name)
    {
      fileName = name;
    }

    /// <summary>
    /// Initialise a new instance of <see cref="KeysRequiredEventArgs"></see>
    /// </summary>
    /// <param name="name">The name of the file for which keys are required.</param>
    /// <param name="keyValue">The current key value.</param>
    public KeysRequiredEventArgs(string name, byte[] keyValue)
    {
      fileName = name;
      key = keyValue;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Gets the name of the file for which keys are required.
    /// </summary>
    public string FileName => fileName;

    /// <summary>
    /// Gets or sets the key value
    /// </summary>
    public byte[] Key { get => key; set => key = value; }

    #endregion Properties
  }
}
