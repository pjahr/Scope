using Scope.Interfaces;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System;
using Pfim;
using System.Windows.Media;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Scope.FileViewer.DDS
{
  public class DdsFileViewer : IFileViewer
  {
    private System.Windows.Controls.Image _image;
    GCHandle _gcHandle;

    public DdsFileViewer(IFile file)
    {
      using (var s = file.Read())
      using (var image = Pfim.Pfim.FromStream(s))
      {
        _image = CreateImage(image);
      }

    }

    public string Header => "Direct Draw Surface (DDS)";
    public System.Windows.Controls.Image Image => _image;

    public void Dispose()
    {
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
        MaxWidth = image.Width,
        Margin = new Thickness(4)
      };
    }

    private static System.Windows.Media.PixelFormat PixelFormat(IImage image)
    {
      switch (image.Format)
      {
        case Pfim.ImageFormat.Rgb24:
          return PixelFormats.Bgr24;
        case Pfim.ImageFormat.Rgba32:
          return PixelFormats.Bgr32;
        case Pfim.ImageFormat.Rgb8:
          return PixelFormats.Gray8;
        case Pfim.ImageFormat.R5g5b5a1:
        case Pfim.ImageFormat.R5g5b5:
          return PixelFormats.Bgr555;
        case Pfim.ImageFormat.R5g6b5:
          return PixelFormats.Bgr565;
        default:
          throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
      }
    }
  }
}