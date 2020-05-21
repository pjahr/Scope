using System;

namespace Scope.Models.Interfaces
{
  internal interface IOutputDirectory
  {
    string Path { get; set; }
    event Action Changed;
  }
}
