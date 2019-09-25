using System;
using System.Diagnostics;
using System.IO;

namespace Helpers
{
    class CmdHelper
    {
        public static string ReadSuccessMessage(string workPath, string command)
        {

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.WorkingDirectory = workPath;
            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
            }
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;//接受来自调用程序的输入信息
            info.RedirectStandardOutput = true;//由调用程序获取输出信息
            info.RedirectStandardError = true;//重定向标准错误输出
            info.CreateNoWindow = false;//显示程序窗口
            info.WindowStyle = ProcessWindowStyle.Maximized;
            info.Arguments = "/C" + command;
            using (Process process = Process.Start(info))
            {
                //获取cmd窗口的输出信息
                return process.StandardOutput.ReadToEnd();
            }
        }

        public static string ReadErrorMessage(string workPath, string command)
        {

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.WorkingDirectory = workPath;
            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
            }
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;//接受来自调用程序的输入信息
            info.RedirectStandardOutput = true;//由调用程序获取输出信息
            info.RedirectStandardError = true;//重定向标准错误输出
            info.CreateNoWindow = false;//显示程序窗口
            info.WindowStyle = ProcessWindowStyle.Maximized;
            info.Arguments = "/C" + command;
            using (Process process = Process.Start(info))
            {
                //获取cmd窗口的输出信息
                return process.StandardError.ReadToEnd();
            }
        }

        /// <returns>Item1成功结果，Item2失败结果</returns>
        public static Tuple<string, string> ReadAllMessage(string workPath, string command)
        {

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.WorkingDirectory = workPath;
            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
            }
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;//接受来自调用程序的输入信息
            info.RedirectStandardOutput = true;//由调用程序获取输出信息
            info.RedirectStandardError = true;//重定向标准错误输出
            info.CreateNoWindow = false;//显示程序窗口
            info.WindowStyle = ProcessWindowStyle.Maximized;
            info.Arguments = "/C" + command;
            using (Process process = Process.Start(info))
            {
                //获取cmd窗口的输出信息
                var success = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                Tuple<string, string> tuple = Tuple.Create(success, error);
                return tuple;
            }
        }
    }
}