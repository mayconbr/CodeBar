using ZXing;
using ZXing.Common;
using SkiaSharp;

namespace Estoque.Tools
{
    public unsafe class CustomLuminanceSource : BaseLuminanceSource
    {
        private byte[] luminances;

        public CustomLuminanceSource(SKBitmap bitmap) : base(bitmap.Width, bitmap.Height)
        {
            luminances = new byte[bitmap.Width * bitmap.Height];
            var pixels = bitmap.Pixels;

            int pixelSize = 4; // Cada pixel é representado por 4 bytes em SKBitmap (RGBA)
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int offset = (y * Width + x) * pixelSize;
                    int r = pixels[y * Width + x].Red;
                    int g = pixels[y * Width + x].Green;
                    int b = pixels[y * Width + x].Blue;
                    luminances[y * Width + x] = (byte)((r * 0.3) + (g * 0.59) + (b * 0.11));
                }
            }
        }

        public CustomLuminanceSource(byte[] luminances, int width, int height) : base(width, height)
        {
            this.luminances = luminances;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new CustomLuminanceSource(newLuminances, width, height);
        }

        public override byte[] Matrix => luminances;
        public override byte[] getRow(int y, byte[] row)
        {
            if (row == null || row.Length < Width)
            {
                row = new byte[Width];
            }

            for (int x = 0; x < Width; x++)
            {
                row[x] = luminances[y * Width + x];
            }

            return row;
        }
    }
}
