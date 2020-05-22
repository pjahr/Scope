using System;
using Moq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
  public class PinnedItemsFacts
  {
    private readonly Action _changedWasRaised = Mock.Of<Action>();
    private IPinnedItems _sut;

    [Fact]
    public void Files_can_be_selected()
    {
      WhenItIsCreated();

      var f1 = Mock.Of<IFile>();
      var f2 = Mock.Of<IFile>();

      ThenTheSelectedFilesAre();

      _sut.Add(f1);

      ThenTheSelectedFilesAre(f1);

      _sut.Add(f2);

      ThenTheSelectedFilesAre(f1, f2);
      ThenChangedWasRaised(2);
    }

    [Fact]
    public void Files_can_be_removed()
    {
      WhenItIsCreated();

      var f1 = Mock.Of<IFile>();
      var f2 = Mock.Of<IFile>();

      _sut.Add(f1);
      _sut.Add(f2);

      _sut.Remove(f1);

      ThenTheSelectedFilesAre(f2);

      _sut.Remove(f2);

      ThenTheSelectedFilesAre();
      ThenChangedWasRaised(4);
    }

    [Fact]
    public void Directories_can_be_selected()
    {
      WhenItIsCreated();

      var d1 = Mock.Of<IDirectory>();
      var d2 = Mock.Of<IDirectory>();

      ThenTheSelectedDirectoriesAre();

      _sut.Add(d1);

      ThenTheSelectedDirectoriesAre(d1);

      _sut.Add(d2);

      ThenTheSelectedDirectoriesAre(d1, d2);
      ThenChangedWasRaised(2);
    }

    [Fact]
    public void Directories_can_be_removed()
    {
      WhenItIsCreated();

      var d1 = Mock.Of<IDirectory>();
      var d2 = Mock.Of<IDirectory>();

      _sut.Add(d1);
      _sut.Add(d2);

      _sut.Remove(d1);

      ThenTheSelectedDirectoriesAre(d2);

      _sut.Remove(d2);

      ThenTheSelectedDirectoriesAre();
      ThenChangedWasRaised(4);
    }

    [Fact]
    public void All_items_can_be_cleared()
    {
      WhenItIsCreated();

      var f1 = Mock.Of<IFile>();
      var f2 = Mock.Of<IFile>();
      var d1 = Mock.Of<IDirectory>();
      var d2 = Mock.Of<IDirectory>();

      _sut.Add(f1);
      _sut.Add(f2);
      _sut.Add(d1);
      _sut.Add(d2);

      _sut.Clear();

      ThenTheSelectedFilesAre();
      ThenTheSelectedDirectoriesAre();
      ThenChangedWasRaised(5);
    }

    private void WhenItIsCreated()
    {
      _sut = new PinnedItems();
      _sut.Changed += _changedWasRaised;
    }

    private void ThenTheSelectedFilesAre(params IFile[] files)
    {
      Assert.Equal(files.Length, _sut.Files.Count);

      for (int i = 0; i < files.Length; i++)
      {
        Assert.Same(files[i], _sut.Files[i]);
      }
    }

    private void ThenTheSelectedDirectoriesAre(params IDirectory[] directories)
    {
      Assert.Equal(directories.Length, _sut.Directories.Count);

      for (int i = 0; i < directories.Length; i++)
      {
        Assert.Same(directories[i], _sut.Directories[i]);
      }
    }

    private void ThenChangedWasRaised(int times)
    {
      _changedWasRaised.Mock()
                       .Verify(m => m(), Times.Exactly(times));
    }
  }
}
