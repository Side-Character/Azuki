using Chris.BaseClasses;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Chris.Converters {
    public class FFmpegConverter  {
        private Process process;
        private string inputpath;

        public event EventHandler<FileSystemEventArgs> TempfileClosed;

        public Stream Convert(string inputpath, CancellationTokenSource cancel) {
            this.inputpath = inputpath;
            process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = @"ffmpeg";

            process.StartInfo.Arguments = $"-loglevel quiet -i {inputpath} -vn -f s16le -ar 48000 -ac 2 pipe:1";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            try {
                Task.Run(() => StopCheckLoop(process, cancel));
                process.Start();
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                //process.WaitForExit();
                return process.StandardOutput.BaseStream;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Console.WriteLine(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Console.WriteLine(e.Data);
        }

        private void StopCheckLoop(Process process, CancellationTokenSource cancel) {
            while (!cancel.IsCancellationRequested) {
                Thread.Sleep(10);
            }
            if (!process.HasExited) {
                process.Kill();
            }
            FileInfo fi = new FileInfo(inputpath);
            TempfileClosed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, fi.Directory.FullName, fi.Name));
            return;
        }
    }
}
