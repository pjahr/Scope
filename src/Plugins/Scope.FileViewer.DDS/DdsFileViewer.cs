using Scope.Interfaces;
using System.Runtime.InteropServices;
using System;
using Pfim;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Scope.FileViewer.DDS
{
  public class DdsFileViewer : IFileViewer
  {
    GCHandle _gcHandle;

    public DdsFileViewer(IFile file)
    {
      try
      {
        using (var s = file.Read())
        using (var image = Pfim.Pfim.FromStream(s))
        {
          Image = CreateImage(image);
        }
      }
      catch (Exception e)
      {
        ErrorMessage = $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";
      }
    }

    public string Header => "Direct Draw Surface (DDS)";
    public string ErrorMessage { get; }
    public System.Windows.Controls.Image Image { get; }

    public void Dispose()
    {
      if (_gcHandle==null||!_gcHandle.IsAllocated)
      {
        return;
      }
      _gcHandle.Free();
    }

    private System.Windows.Controls.Image CreateImage(IImage image)
    {
      var pinnedArray = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
      var addr = pinnedArray.AddrOfPinnedObject();
      var bsource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
          PixelFormat(image), null, addr, image.DataLen, image.Stride);

      _gcHandle = pinnedArray;

      return new System.Windows.Controls.Image
      {
        Source = bsource,
        Width = image.Width,
        Height = image.Height,
        MaxHeight = image.Height,
        MaxWidth = image.Width
      };
    }

    private static PixelFormat PixelFormat(IImage image)
    {
      switch (image.Format)
      {
        case ImageFormat.Rgb24:
          return PixelFormats.Bgr24;
        case ImageFormat.Rgba32:
          return PixelFormats.Bgr32;
        case ImageFormat.Rgb8:
          return PixelFormats.Gray8;
        case ImageFormat.R5g5b5a1:
        case ImageFormat.R5g5b5:
          return PixelFormats.Bgr555;
        case ImageFormat.R5g6b5:
          return PixelFormats.Bgr565;
        default:
          throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
      }
    }
  }
}