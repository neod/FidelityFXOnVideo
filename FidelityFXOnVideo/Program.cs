using System;
using System.IO;

namespace FidelityFXOnVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = args[1];

            FidelityFXOnVideo fov = new FidelityFXOnVideo(Path.GetFullPath(Path.GetDirectoryName(inputFile)), args[0]);

            fov.FfmpegExtract(inputFile);

            fov.FidelityFXProcessor(FidelityFXOnVideo.Mode.EASU, "-Scale 1440 1440", FidelityFXOnVideo.FRAMES_DIR, FidelityFXOnVideo.EASU_DIR);

            Directory.Delete(Path.Combine(fov.WorkingDirectory, FidelityFXOnVideo.FRAMES_DIR), true);

            fov.FidelityFXProcessor(FidelityFXOnVideo.Mode.RCAS, "-Sharpness 0.2", FidelityFXOnVideo.EASU_DIR, FidelityFXOnVideo.RESULT_DIR);

            Directory.Delete(Path.Combine(fov.WorkingDirectory, FidelityFXOnVideo.EASU_DIR), true);

            fov.FfmpegRender(Path.Combine(fov.WorkingDirectory, Path.GetFileNameWithoutExtension(inputFile) + "_ALTERED" + Path.GetExtension(inputFile)));

            Directory.Delete(Path.Combine(fov.WorkingDirectory, FidelityFXOnVideo.RESULT_DIR), true);
            File.Delete(Path.Combine(fov.WorkingDirectory, "audio.wav"));
            File.Delete(Path.Combine(fov.WorkingDirectory, "params.txt"));
            
        }
    }
}
