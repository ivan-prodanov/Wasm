using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Wasm.Sdk.Tasks
{
    public static class ProcessExtensions
    {
        public static (int exitCode, string output, string error) RunProcess(this Task task, string executable, string parameters, string? workingDirectory = null)
        {
            if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                parameters = $"{executable} {parameters}";
                executable = "dotnet";
            }

            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = executable,
                    Arguments = parameters
                }
            };

            if (workingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }

            task.Log.LogMessage($"Running [{process.StartInfo.WorkingDirectory}]: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

            var output = new StringBuilder();
            var error = new StringBuilder();
            var elapsed = Stopwatch.StartNew();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) { task.Log.LogMessage($"[{elapsed.Elapsed}] {e.Data}"); output.Append(e.Data); } };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) { task.Log.LogError($"[{elapsed.Elapsed}] {e.Data}"); error.Append(e.Data); } };

            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                var exitCore = process.ExitCode;
                process.Close();

                return (exitCore, output.ToString(), error.ToString());
            }
            else
            {
                throw new Exception($"Failed to start [{executable}]");
            }
        }
    }
}
