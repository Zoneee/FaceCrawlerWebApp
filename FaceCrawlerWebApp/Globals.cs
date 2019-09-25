using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class Globals
{
    public static Task TimingCompressTask;
    static CancellationTokenSource compressTaskCts = new CancellationTokenSource();
    public static void StartCompress()
    {
        if (TimingCompressTask != null
            && TimingCompressTask?.Status != TaskStatus.Canceled)
        {
            return;
        }

        TimingCompressTask = Task.Run(async () =>
        {
            while (true)
            {
                var now = DateTime.Now;
                var delay = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 03, 0, 0);
#if DEBUG
                await Task.Delay(delay.Subtract(now), compressTaskCts.Token).ConfigureAwait(false);
#endif
                var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(sourcePath))
                {
                    return;
                }
                var filePath = $"{sourcePath}.7z";
                await FileHelper.Compress(filePath, sourcePath);
                FileHelper.DeleteDirectory(sourcePath, true);
            }
        }, compressTaskCts.Token);
    }

    public static string CloseCompress()
    {
        compressTaskCts.Cancel(true);
        compressTaskCts?.Dispose();
        compressTaskCts = new CancellationTokenSource();
        return "关闭定时压缩";
    }
    public static CancellationTokenSource CrawlTaskCts = new CancellationTokenSource();
    public static string StopTask()
    {
        CrawlTaskCts.Cancel(true);
        CrawlTaskCts?.Dispose();
        CrawlTaskCts = new CancellationTokenSource();
        return "任务已停止";
    }
}
