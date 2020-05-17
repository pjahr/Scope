using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Moq;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
  public class CurrentP4kFacts
  {
    private ICurrentP4k _sut;
    private IFileInfo _path;
    private Action _changedWasRaised = Mock.Of<Action>();
    private OpenP4kFileResult _openResult;

    [Fact]
    public async void It_raises_Changed_when_it_loaded_a_p4k_file()
    {
      GivenSomeValidPath();
      WhenSutIsCreated();

      await WhenItOpensTheP4kLocatedAtTheGivenPath();

      ThenChangedWasRaised();
    }

    private void GivenSomeValidPath()
    {
      _path = Mock.Of<IFileInfo>();
      _path.Mock()
           .Setup(m => m.FullName)
           .Returns("Some/valid/path/to/a.p4k");
    }

    private void WhenSutIsCreated()
    {
      _sut = new CurrentP4k();
      _sut.Changed += _changedWasRaised;
    }

    private async Task WhenItOpensTheP4kLocatedAtTheGivenPath()
    {
      _openResult = await _sut.ChangeAsync(_path);
    }

    private void ThenChangedWasRaised()
    {
      _changedWasRaised.Mock().Verify();
    }
  }
}
