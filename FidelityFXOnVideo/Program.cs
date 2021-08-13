using System;
using System.IO;

namespace FidelityFXOnVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = args[1];

            FidelityFXOnVideo fov = new(Path.GetFullPath(Path.GetDirectoryName(inputFile)), args[0]);

            Console.WriteLine("Extracting video frames and audio");
            fov.FfmpegExtract(inputFile);

            Console.WriteLine("Apply EASU on frames");
            fov.FidelityFXProcessor(FidelityFXOnVideo.Mode.EASU, $"-Scale {args[2]} {args[3]}");


            Console.WriteLine("Apply RCAS on frames");
            fov.FidelityFXProcessor(FidelityFXOnVideo.Mode.RCAS, "-Sharpness 0.2");


            Console.WriteLine("Video result rendering");
            fov.FfmpegRender(Path.Combine(fov.WorkingDirectory, Path.GetFileNameWithoutExtension(inputFile) + "_ALTERED" + Path.GetExtension(inputFile)));

            Console.WriteLine("Finished");
        }
    }
}
