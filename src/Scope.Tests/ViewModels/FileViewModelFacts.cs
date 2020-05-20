//using Scope.Model.Interfaces;
//using Scope.ViewModels;
//using Moq;
//using Xunit;
//using Scope.Interfaces;

//namespace Scope.Tests.ViewModels
//{
//  public class FileViewModelFacts
//  {
//    private FileViewModel _sut;
//    private readonly IFile _file = Mock.Of<IFile>();
//    private readonly ICurrentItem _currentFile = Mock.Of<ICurrentItem>();
//    private readonly ISelectedItems _selectedItems = Mock.Of<ISelectedItems>();

//    [Fact]
//    public void It_is_not_selected_when_the_current_file_is_its_file()
//    {
//      GivenTheCurrentFileIsUnknown();

//      GivenSut();

//      Assert.False(_sut.IsSelected);
//    }

//    [Fact]
//    public void It_is_selected_when_the_current_file_is_its_file()
//    {
//      GivenTheCurrentFileIsItsFile();

//      GivenSut();

//      Assert.True(_sut.IsActive);
//      Assert.Same(_file, _currentFile.CurrentFile);
//    }

//    [Fact]
//    public void It_changes_its_selected_state_when_the_current_file_changes()
//    {
//      GivenTheCurrentFileIsUnknown();
//      GivenSut();

//      Assert.False(_sut.IsActive);

//      _currentFile.ReturnsOn(m => m.CurrentFile, _file);
//      _currentFile.Mock().Raise(m => m.Changed += null);

//      Assert.True(_sut.IsActive);

//      _currentFile.ReturnsOn(m => m.CurrentFile, Mock.Of<IFile>());
//      _currentFile.Mock().Raise(m => m.Changed += null);

//      Assert.False(_sut.IsActive);

//      _sut.Dispose();

//      _currentFile.ReturnsOn(m => m.CurrentFile, _file);
//      _currentFile.Mock().Raise(m => m.Changed += null);

//      Assert.False(_sut.IsActive);
//    }

//    [Fact]
//    public void It_sets_its_file_as_the_currently_selected_file_when_the_related_command_is_executed()
//    {
//      GivenSut();

//      _sut.SetCurrentItemCommand.Execute(null);

//      _currentFile.Mock().Verify(m => m.ChangeTo(_file), Times.Once);
//    }

//    private void GivenSut()
//    {
//      _sut = new FileViewModel(_file, _currentFile, _selectedItems);
//    }

//    private void GivenTheCurrentFileIsUnknown()
//    {
//      _currentFile.ReturnsOn(m => m.CurrentFile, Mock.Of<IFile>());
//    }

//    private void GivenTheCurrentFileIsItsFile()
//    {
//      _currentFile.ReturnsOn(m => m.CurrentFile, _file);
//    }
//  }
//}


