using System;
using Moq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
  public class CurrentItemFacts
  {
    private ICurrentItem _sut;
    private Action _changedRaised;
    private readonly IFile _file = Mock.Of<IFile>();
    private readonly IDirectory _directory = Mock.Of<IDirectory>();

    [Fact]
    public void A_file_can_be_set_as_current_item()
    {
      GivenSut();

      Assert.Null(_sut.CurrentFile);
      Assert.Null(_sut.CurrentDirectory);

      _sut.ChangeTo(_file);

      Assert.Same(_file, _sut.CurrentFile);
      Assert.Null(_sut.CurrentDirectory);

      ThenChangedWasRaisedTimes(Times.Once());
    }

    [Fact]
    public void A_directory_can_be_set_as_current_item()
    {
      GivenSut();

      Assert.Null(_sut.CurrentFile);
      Assert.Null(_sut.CurrentDirectory);

      _sut.ChangeTo(_directory);

      Assert.Same(_directory, _sut.CurrentDirectory);
      Assert.Null(_sut.CurrentFile);

      ThenChangedWasRaisedTimes(Times.Once());
    }

    [Fact]
    public void It_does_not_set_the_same_file_twice()
    {
      GivenSut();

      _sut.ChangeTo(_file);
      _sut.ChangeTo(_file);

      ThenChangedWasRaisedTimes(Times.Once());
    }

    [Fact]
    public void It_does_not_set_the_same_directory_twice()
    {
      GivenSut();

      _sut.ChangeTo(_directory);
      _sut.ChangeTo(_directory);

      ThenChangedWasRaisedTimes(Times.Once());
    }

    [Fact]
    public void Only_one_type_can_be_active()
    {
      GivenSut();

      _sut.ChangeTo(_directory);
      Assert.Same(_directory, _sut.CurrentDirectory);
      Assert.Null(_sut.CurrentFile);

      _sut.ChangeTo(_file);
      Assert.Same(_file, _sut.CurrentFile);
      Assert.Null(_sut.CurrentDirectory);
    }

    private void GivenSut()
    {
      _sut = new CurrentItem();

      // observe change devent
      _changedRaised = Mock.Of<Action>();
      _sut.Changed += _changedRaised;
    }

    private void ThenChangedWasRaisedTimes(Times times)
    {
      _changedRaised.Mock()
                    .Verify(m => m(), times);
    }
  }
}
