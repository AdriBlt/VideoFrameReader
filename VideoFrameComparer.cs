
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace VideoFrameReader
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Accord.Video.FFMPEG;

    public class VideoFrameComparer
    {
        public VideoFrameComparer()
        {
            this.Frames = new Dictionary<BitmapFrame, string>();
        }

        private Dictionary<BitmapFrame, string> Frames { get; }

        public void AddFrame(string name, Bitmap frame)
        {
            this.Frames.Add(new BitmapFrame(frame), name);
        }

        public void ReadAndExport(string filePath, string exportFolder)
        {
            // create instance of video reader
            VideoFileReader reader = new VideoFileReader();

            if (!File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            // open video file
            reader.Open(filePath);

            // read 100 video frames out of it
            for (long i = 0; i < reader.FrameCount; i++)
            {
                Bitmap videoFrame = reader.ReadVideoFrame();
                if (videoFrame == null)
                {
                    TimeSpan time = TimeSpan.FromMilliseconds(1000.0 * i / reader.FrameRate);
                    Console.WriteLine($"Frame Missing at {time} (frame {i})");
                }
                else
                {
                    string path = $"{exportFolder}/{i}.png";

                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                        {
                            videoFrame.Save(memory, ImageFormat.Png);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    videoFrame.Dispose();
                }
            }

            reader.Close();
        }

        public List<Frame> FindFrames(string filePath)
        {
            // create instance of video reader
            VideoFileReader reader = new VideoFileReader();

            // open video file
            reader.Open(filePath);

            int rate = reader.FrameRate;
            List<Frame> foundFrames = new List<Frame>();
            // read 100 video frames out of it
            for (long i = 0; i < reader.FrameCount; i++)
            {
                try
                {
                    Bitmap videoFrame = reader.ReadVideoFrame();

                    string search = this.GetSearchedFrame(videoFrame);
                    if (search != null)
                    {
                        foundFrames.Add(new Frame
                        {
                            FrameIndex = i,
                            Time = TimeSpan.FromMilliseconds(1000.0 * i / rate),
                            Reason = search
                        });
                    }
                    else
                    {
                        // dispose the frame when it is no longer required
                        videoFrame.Dispose();
                    }
                }
                catch (Exception e)
                {
                    foundFrames.Add(new Frame
                    {
                        FrameIndex = i,
                        Time = TimeSpan.FromMilliseconds(1000.0 * i / rate),
                        Reason = e.Message
                    });
                }
            }

            reader.Close();

            return foundFrames;
        }

        private string GetSearchedFrame(Bitmap videoFrame)
        {
            if (videoFrame == null)
            {
                return "Missing Frame";
            }

            var bitmapVideoFrame = new BitmapFrame(videoFrame);

            string result = null;
            foreach (KeyValuePair<BitmapFrame, string> frame in Frames.AsEnumerable())
            {
                if (AreFramesEqual(bitmapVideoFrame, frame.Key))
                {
                    result = frame.Value;
                    break;
                }
            }

            bitmapVideoFrame.Dispose();

            return result;
        }

        private bool AreFramesEqual(BitmapFrame videoFrame, BitmapFrame frame)
        {
            if (!videoFrame.Frame.Size.Equals(frame.Frame.Size)
                || !videoFrame.Frame.PixelFormat.Equals(frame.Frame.PixelFormat))
            {
                return false;
            }

            bool result = AreBytesEqual(videoFrame.Bytes, frame.Bytes, videoFrame.BytesSize);

            return result;
        }

        private bool AreBytesEqual(byte[] bytes1, byte[] bytes2, int bytes)
        {
            for (int n = 0; n <= bytes - 1; n++)
            {
                if (bytes1[n] != bytes2[n])
                {
                    return false;
                }
            }

            return true;
        }

        public bool ExportFrame(string videoPath, int frameNumber, string framePath, out string error)
        {
            throw new NotImplementedException();
        }
    }
}
