using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LitWiki.Views.Controls
{
    class MaskImage : Image
    {
        public static DependencyProperty MaskControlProperty = DependencyProperty.Register(
            "MaskColor",
            typeof(Color),
            typeof(MaskImage),
            new PropertyMetadata(Colors.Black, new PropertyChangedCallback(MaskControlPropertyChanged)));
        public static void MaskControlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MaskImage maskImage)
            {
                maskImage.MaskColor = (Color)e.NewValue;
            }
        }
        public Color MaskColor
        {
            get { return (Color)GetValue(MaskControlProperty); }
            set
            {
                SetValue(MaskControlProperty, value);
                UpdateImage(MaskColor);
            }
        }


        public MaskImage() : base()
        {

        }

        public void UpdateImage(Color color)
        {
            if (Source is BitmapSource bmpSource)
            {
                WriteableBitmap bmp = new(bmpSource);

                Array pixels = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
                byte[] newData = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
                bmp.CopyPixels(pixels, bmp.PixelWidth * 4, 0);
                byte[] data = (byte[])pixels;


                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte a = data[i + 3];

                    if (a != 0)
                    {
                        newData[i] = color.B;
                        newData[i + 1] = color.G;
                        newData[i + 2] = color.R;
                        newData[i + 3] = a;
                    }
                }

                bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), newData, bmp.PixelWidth * 4, 0);
                Source = bmp;
            }
        }
    }
}
