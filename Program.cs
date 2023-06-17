
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace VideoFrameReader
{
    class Program
    {
        private const string Folder = @".\Frames\";

        private static readonly string[] Instructions =
        {
            "########################################################################################",
            "# VIDEO FRAME READER USAGE",
            "#",
            "# VideoFrameReader.exe <video path> [-target <result path>]",
            "#  => Search for all frames contained on the \"./Frames/\" folder",
            "#     and write the results in the result file (by default \"./result.txt\")",
            "#",
            "# VideoFrameReader.exe <video path> -add <frame index> [-message <frame description>]",
            "#  => Add the frame n° index to be detected to the folder.",
            "#     If provided, the description message will be written onto the result.txt ",
            "#          upon detection (should not contain spaces)",
            "########################################################################################"
        };

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            string videoPath = args[0];
            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"File {videoPath} does not exists.");
                return;
            }

            string resultPath;
            int frameNumber;
            string message;
            if (TryParseSearchParameters(args, out resultPath))
            {
                SearchParameters(videoPath, resultPath);
            }
            else if (TryParseAddFrameToFolder(args, out frameNumber, out message))
            {
                AddFrameToFolder(videoPath, frameNumber, message);
            }
            else
            {
                DisplayHelp();
            }
        }

        private static void DisplayHelp()
        {
            foreach (string instruction in Instructions)
            {
                Console.WriteLine(instruction);
            }

            Console.WriteLine();
        }

        private static bool TryParseSearchParameters(string[] args, out string resultPath)
        {
            resultPath = ".";
            if (args.Length == 1)
            {
                return true;
            }

            if (args.Length == 3 && string.Equals(args[1], "-target"))
            {
                resultPath = args[2];
                return true;
            }

            return false;
        }

        private static void SearchParameters(string videoPath, string resultPath)
        {
            var comparer = new VideoFrameComparer();

            foreach (string frame in GetFrames())
            {
                comparer.AddFrame(frame, (Bitmap)Image.FromFile(frame));
            }

            var frames = comparer.FindFrames(videoPath);

            using (StreamWriter file = new StreamWriter(resultPath))
            {
                foreach (Frame frame in frames)
                {
                    file.WriteLine($"{frame.Reason} at {frame.Time} (frame #{frame.FrameIndex})");
                }
            }

            Console.WriteLine(frames.Count + " results");
            Console.ReadLine();
        }

        private static bool TryParseAddFrameToFolder(string[] args, out int frameNumber, out string message)
        {
            frameNumber = -1;
            message = null;

            if (args.Length > 2 && string.Equals(args[1], "-add")
                && int.TryParse(args[2], out frameNumber))
            {
                if (args.Length == 3)
                {
                    return true;
                }

                if (args.Length == 5 && string.Equals(args[3], "-message"))
                {
                    message = args[4];
                }
            }

            return false;
        }

        private static void AddFrameToFolder(string videoPath, int frameNumber, string message)
        {
            var comparer = new VideoFrameComparer();

            foreach (string frame in GetFrames())
            {
                comparer.AddFrame(frame, (Bitmap)Image.FromFile(frame));
            }

            string framePath = Folder;
            if (message != null)
            {
                framePath += message + ".png";
            }
            else
            {
                string[] video = videoPath.Split(Path.PathSeparator);
                framePath += video[video.Length - 1] + ".png";
            }

            string error;
            if (comparer.ExportFrame(videoPath, frameNumber, framePath, out error))
            {
                Console.WriteLine("Frame has been successfuly added.");
            }
            else
            {
                Console.WriteLine($"ERROR: {error}");
            }

        }

        private static IEnumerable<string> GetFrames()
        {
            if (Directory.Exists(Folder))
            {
                return Directory.EnumerateFiles(Folder);
            }

            return Enumerable.Empty<string>();
        }

        private static string GetLastSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string[] segments = path.Split(Path.PathSeparator);
            if (segments.Length < 2)
            {
                return path;
            }

            return segments[segments.Length - 1];
        }

        private string GetNewPath(string path, string extension)
        {
            string newPath = path + extension;
            int count = 0;
            while (File.Exists(newPath))
            {
                newPath = path + "(" + (++count) + ")" + extension;
            }

            return newPath;
        }
    }
}
