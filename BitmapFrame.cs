
namespace VideoFrameReader
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    internal class BitmapFrame
    {
        public BitmapFrame(Bitmap frame)
        {
            this.Frame = frame;

            this.BytesSize = frame.Width * frame.Height * (Image.GetPixelFormatSize(frame.PixelFormat) / 8);

            this.BitmapData = frame.LockBits(
                new Rectangle(0, 0, frame.Width - 1, frame.Height - 1),
                ImageLockMode.ReadOnly,
                frame.PixelFormat);
            this.Bytes = new byte[this.BytesSize];
            Marshal.Copy(this.BitmapData.Scan0, this.Bytes, 0, this.BytesSize);
        }

        public Bitmap Frame { get; set; }

        public BitmapData BitmapData { get; set; }

        public int BytesSize { get; private set; }

        public byte[] Bytes { get; private set; }

        public void Dispose()
        {
            this.Frame.UnlockBits(this.BitmapData);
        }
    }
}
