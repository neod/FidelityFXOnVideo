using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFMpegCore;

namespace FidelityFXOnVideo
{
    class FidelityFXOnVideo
    {
        public string WorkingDirectory { get; init; }
        private string FidelityFXExe { get; init; }
        private const string FRAMES_DIR = "frames";
        private const string EASU_DIR = "frames_easu";
        private const string RESULT_DIR = "frames_result";
        private string audioFile;
        private IMediaAnalysis MediaInfo { get; set; }
        private const int step = 200;

        public FidelityFXOnVideo(string workingDirectory, string fidelityFXExe)
        {
            this.WorkingDirectory = workingDirectory;
            this.FidelityFXExe = fidelityFXExe;
        }

        /// <summary>
        /// Applie FidelityFX EASU, RCAS or CAS on frames
        /// </summary>
        /// <param name="mode">EASU, RCAS or CAS mode</param>
        /// <param name="args">specific arguments for the selected mode</param>
        public void FidelityFXProcessor(Mode mode, string args)
        {
            string inputDir, outputDir;

            switch (mode)
            {
                case Mode.CAS:
                    inputDir = FRAMES_DIR;
                    outputDir = RESULT_DIR;
                    break;
                case Mode.EASU:
                    inputDir = FRAMES_DIR;
                    outputDir = EASU_DIR;
                    break;
                case Mode.RCAS:
                default:
                    Directory.Delete(Path.Combine(WorkingDirectory, FRAMES_DIR), true);
                    inputDir = EASU_DIR;
                    outputDir = RESULT_DIR;
                    break;
            }


            if (Directory.Exists(Path.Combine(WorkingDirectory, outputDir)))
            {
                Directory.Delete(Path.Combine(WorkingDirectory, outputDir), true);
            }

            Directory.CreateDirectory(Path.Combine(WorkingDirectory, outputDir));

            Process proc = new();
            proc.StartInfo.FileName = FidelityFXExe;
            proc.StartInfo.WorkingDirectory = WorkingDirectory;

            string[] files = Directory.GetFiles(Path.Combine(WorkingDirectory, inputDir));

            string[] subFiles = new string[step];

            for (int i = 0; i < files.Length; i += step)
            {
                Array.Copy(files, i, subFiles, 0, Math.Min(step, files.Length - i));

                string filesList = String.Join(' ', subFiles.Select(f => $"{Path.Combine(inputDir, Path.GetFileName(f))} {Path.Combine(outputDir, Path.GetFileName(f))}"));

                proc.StartInfo.Arguments = $"-Mode {mode} {args} {filesList}";


                proc.Start();
                proc.WaitForExit();
            }
        }

        /// <summary>
        /// EASU upscale the frame, RCAS makes the image sharp and CAS is a old mode
        /// </summary>
        public enum Mode
        {
            EASU,
            RCAS,
            CAS
        }

        /// <summary>
        /// Extract each video frames and audio separately
        /// </summary>
        /// <param name="videoFullpathFile">The video file fullpath to extract</param>
        public void FfmpegExtract(string videoFullpathFile)
        {
            if (Directory.Exists(Path.Combine(WorkingDirectory, FRAMES_DIR)))
            {
                Directory.Delete(Path.Combine(WorkingDirectory, FRAMES_DIR), true);
            }

            Directory.CreateDirectory(Path.Combine(WorkingDirectory, FRAMES_DIR));

            this.MediaInfo = FFProbe.Analyse(videoFullpathFile);

            this.audioFile = $"audio.{MediaInfo.PrimaryAudioStream.CodecName}";

            FFMpegArguments.FromFileInput(videoFullpathFile)
                .OutputToFile(Path.Combine(WorkingDirectory, FRAMES_DIR, "frame%06d.jpg"), true, args => args
                .WithCustomArgument("-qscale:v 2"))
                .ProcessSynchronously();

            FFMpegArguments.FromFileInput(videoFullpathFile)
                .OutputToFile(Path.Combine(WorkingDirectory, audioFile), true, args => args
                    .CopyChannel(FFMpegCore.Enums.Channel.Audio))
                .ProcessSynchronously();

        }

        /// <summary>
        /// Render the final video file
        /// </summary>
        /// <param name="outputFile">The output video file path</param>
        public void FfmpegRender(string outputFile)
        {
            Directory.Delete(Path.Combine(WorkingDirectory, EASU_DIR), true);
            Process proc = new();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-framerate {this.MediaInfo.PrimaryVideoStream.FrameRate.ToString(CultureInfo.InvariantCulture.NumberFormat)} " +
                $"-i {Path.Combine(WorkingDirectory, RESULT_DIR, "frame%06d.jpg")} " +
                $"-i {Path.Combine(WorkingDirectory, audioFile)} " +
                $"-strict -2 {outputFile}";
            proc.Start();
            proc.WaitForExit();

            /*FFMpegArguments.FromFileInput(Path.Combine(WorkingDirectory, RESULT_DIR, "frame%06d.jpg"))
                .AddFileInput(Path.Combine(WorkingDirectory, audioFile))
                .OutputToFile(outputFile, true, arguments => arguments
                    .WithFramerate(this.MediaInfo.PrimaryVideoStream.FrameRate)
                    .WithFastStart())
                .ProcessSynchronously();*/

            Directory.Delete(Path.Combine(WorkingDirectory, RESULT_DIR), true);
            File.Delete(Path.Combine(WorkingDirectory, audioFile));
        }
    }
}
