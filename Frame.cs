
using System;
using System.Drawing;

namespace VideoFrameReader
{
    public class Frame
    {
        public long FrameIndex { get; set; }

        public Bitmap Image { get; set; }

        public TimeSpan Time { get; set; }
        public string Reason { get; set; }
    }
}
