using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FidelityFXOnVideo
{
    class FidelityFXOnVideo
    {
        public string WorkingDirectory { get; init; }
        private string FidelityFXExe { get; init; }
        public const string FRAMES_DIR = "frames";
        public const string EASU_DIR = "frames_easu";
        public const string RESULT_DIR = "frames_result";

        private const int step = 200;

        public FidelityFXOnVideo(string workingDirectory, string fidelityFXExe)
        {
            this.WorkingDirectory = workingDirectory;
            this.FidelityFXExe = fidelityFXExe;
        }

        public void FidelityFXProcessor(Mode mode, string args, string inputDir, string outputDir)
        {

            if (Directory.Exists(Path.Combine(WorkingDirectory, outputDir)))
            {
                Directory.Delete(Path.Combine(WorkingDirectory, outputDir), true);
            }

            Directory.CreateDirectory(Path.Combine(WorkingDirectory, outputDir));

            Process proc = new();
            proc.StartInfo.FileName = FidelityFXExe;
            proc.StartInfo.WorkingDirectory = WorkingDirectory;

            string[] files = Directory.GetFiles(Path.Combine(WorkingDirectory, FRAMES_DIR));

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

        public enum Mode
        {
            EASU,
            RCAS,
            CAS
        }

        public void FfmpegExtract(string videoFullpathFile)
        {
            if (Directory.Exists(Path.Combine(WorkingDirectory, FRAMES_DIR)))
            {
                Directory.Delete(Path.Combine(WorkingDirectory, FRAMES_DIR), true);
            }

            Directory.CreateDirectory(Path.Combine(WorkingDirectory, FRAMES_DIR));

            Process proc = new();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-i {videoFullpathFile} -qscale:v 3 {Path.Combine(WorkingDirectory, FRAMES_DIR)}/frame%06d.jpg -hide_banner";
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();

            proc = new();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-i {videoFullpathFile} -ab 160k -ac 2 -ar 44100 -vn {WorkingDirectory}/audio.wav";
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();

            proc = new();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-i {videoFullpathFile}";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();

            using (StreamWriter fs = new($"{WorkingDirectory}/params.txt"))
            {
                fs.Write(proc.StandardError.ReadToEnd());
                fs.Flush();
            }
            proc.WaitForExit();
        }

        public void FfmpegRender(string outputFile)
        {
            Process proc = new();
            proc.StartInfo.FileName = "ffmpeg";
            proc.StartInfo.Arguments = $"-framerate {GetFps()} -i {Path.Combine(WorkingDirectory, RESULT_DIR, "frame%06d.jpg")} -i {Path.Combine(WorkingDirectory, "audio.wav")} -strict -2 {outputFile}";
            proc.Start();
            proc.WaitForExit();
        }

        private string GetFps()
        {
            using StreamReader sr = File.OpenText($"{WorkingDirectory}/params.txt");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("fps"))
                {
                    Match m = Regex.Match(line, ".*,(.*) fps.*", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        return m.Groups[1].Value;
                    }
                }
            }

            return "0";
        }
    }
}
