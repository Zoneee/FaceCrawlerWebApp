using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksum;

namespace Helpers
{
    public static class FileHelper
    {
        #region 文件操作

        public static void CreateFile(string path, byte[] buffer)
        {
            CreateFile(path, buffer, false);
        }

        public static void CreateFile(string path, byte[] buffer, bool overwrite)
        {
            if (!overwrite && File.Exists(path))
            {
                throw new IOException("目标文件已存在！");
            }
            CreateDirectories(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, buffer);
        }

        /// <param name="overwrite">是否覆盖</param>
        public static void CreateFile(string path, bool overwrite)
        {
            CreateDirectories(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            else if (overwrite)
            {
                File.Delete(path);
                File.Create(path).Dispose();
            }
        }

        public static void CreateFiles(bool overwrite, params string[] path)
        {
            foreach (var item in path)
            {
                CreateFile(item, overwrite);
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteFiles(params string[] path)
        {
            foreach (var item in path)
            {
                DeleteFile(item);
            }
        }

        /// <param name="destFile">目标文件</param>
        /// <param name="sourceFile">源文件</param>
        public static void CopyFile(string destFile, string sourceFile, bool overwrite)
        {
            CreateDirectories(Path.GetDirectoryName(destFile));
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destFile, overwrite);
            }
        }

        /// <summary>
        /// 复制文件到目标目录，保持文件名不变
        /// </summary>
        /// <param name="destPath">目标目录</param>
        public static void CopyFiles(string destPath, bool overwrite, params string[] sourceFile)
        {
            CreateDirectories(destPath);
            foreach (var item in sourceFile)
            {
                if (File.Exists(item))
                {
                    var path = Path.Combine(destPath, Path.GetFileName(item));
                    File.Copy(item, path, overwrite);
                }
            }
        }

        /// <param name="destFile">目标文件</param>
        /// <param name="sourceFile">源文件</param>
        public static void MoveFile(string destFile, string sourceFile, bool overwrite)
        {
            CreateDirectories(Path.GetDirectoryName(destFile));
            if (File.Exists(sourceFile))
            {
                if (File.Exists(destFile) && overwrite)
                {
                    DeleteFile(destFile);
                }
                File.Move(sourceFile, destFile);
            }
        }

        /// <summary>
        /// 复制文件到目标目录，保持文件名不变
        /// </summary>
        /// <param name="destPath">目标目录</param>
        public static void MoveFiles(string destPath, bool overwrite, params string[] sourcePath)
        {
            CreateDirectories(destPath);
            foreach (var item in sourcePath)
            {
                if (File.Exists(item))
                {
                    var path = Path.Combine(destPath, Path.GetFileName(item));
                    if (File.Exists(path) && overwrite)
                    {
                        DeleteFile(path);
                    }
                    File.Move(item, path);
                }
            }
        }

        public static void AppendAllText(string path, string contetn)
        {
            CreateDirectories(Path.GetDirectoryName(path));
            File.AppendAllText(path, contetn);
        }

        #endregion

        #region 目录操作

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateDirectories(params string[] path)
        {
            foreach (var item in path)
            {
                CreateDirectory(item);
            }
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        public static void DeleteDirectories(bool recursive, params string[] path)
        {
            foreach (var item in path)
            {
                DeleteDirectory(item, recursive);
            }
        }

        public static void CopyDirectory(string destPath, string sourcePath, bool overwrite)
        {
            var allFiles = Directory.GetFiles(sourcePath);
            CopyFiles(destPath, overwrite, allFiles);
            var allDirectory = Directory.GetDirectories(sourcePath);
            foreach (var item in allDirectory)
            {
                CopyDirectory(destPath, item, overwrite);
            }
        }

        public static void CopyDirectories(string destPath, bool overwrite, params string[] sourcePath)
        {
            foreach (var item in sourcePath)
            {
                CopyDirectory(destPath, item, overwrite);
            }
        }

        public static void MoveDirectory(string destPath, string sourcePath, bool overwrite)
        {
            if (overwrite)
            {
                Directory.Delete(destPath, true);
            }

            Directory.Move(sourcePath, destPath);
        }

        public static void MoveDirectories(string destPath, bool overwrite, params string[] sourcePath)
        {
            if (overwrite)
            {
                Directory.Delete(destPath, true);
            }

            foreach (var item in sourcePath)
            {
                Directory.Move(item, destPath);
            }
        }

        #endregion

        public static void DeleteFilesAndDirectories(bool recursive, params string[] path)
        {
            DeleteFiles(path);
            DeleteDirectories(recursive, path);
        }

        #region 压缩/解压

        /// <summary>
        /// 解压文件，重名则覆盖
        /// </summary>
        /// <param name="filePath">压缩文件路径</param>
        /// <param name="targetPath">解压目标目录</param>
        /// <returns></returns>
        public static async Task UnCompress(string filePath, string targetPath)
        {
            CreateDirectories(targetPath);
            using (ZipInputStream stream = new ZipInputStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                ZipEntry entry;
                while ((entry = stream.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory)
                    {
                        string directoryPath = Path.Combine(targetPath, Path.GetDirectoryName(entry.Name));
                        CreateDirectories(directoryPath);
                    }
                    else if (entry.IsFile)
                    {
                        var f = Path.Combine(targetPath, entry.Name);
                        CreateDirectories(Path.GetDirectoryName(f));
                        using (FileStream streamWriter = File.Create(f))
                        {
                            int size = 0;
                            byte[] data = new byte[1024];
                            while ((size = await stream.ReadAsync(data, 0, data.Length)) > 0)
                            {
                                await streamWriter.WriteAsync(data, 0, size);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filePath">压缩文件路径</param>
        /// <param name="sourcePath">待压缩文件路径/待压缩目录</param>
        /// <returns></returns>
        public static async Task Compress(string filePath, params string[] sourcePath)
        {
            //格式化后缀
            filePath = Path.ChangeExtension(filePath, ".zip");
            //创建保存目录
            CreateDirectories(Path.GetDirectoryName(filePath));
            //压缩文件根目录
            string rootDirectory;
            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(filePath)))
            {
                zipStream.SetLevel(9);  // 压缩级别 0-9  //0为最大 9为最小
                foreach (var sourceItem in sourcePath)
                {
                    rootDirectory = Path.GetDirectoryName(sourceItem) + Path.DirectorySeparatorChar;
                    await WriteCompress(sourceItem, zipStream);
                }
                zipStream.Flush();
            }

            async Task WriteCompress(string path, ZipOutputStream zipStream)
            {
                if (File.Exists(path))
                {
                    //压缩
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        await fileStream.ReadAsync(buffer, 0, buffer.Length);

                        Crc32 crc = new Crc32();
                        crc.Reset();
                        crc.Update(buffer);

                        //获得文件位于压缩路径中的相对路径
                        string tempFile = path.Replace(rootDirectory, "");
                        //相对路径与原路径相同时，代表这是一个单独的文件
                        //一般是不会压缩不同路径下的文件，这里特殊处理一下
                        if (string.Equals(tempFile, path))
                        {
                            tempFile = Path.GetFileName(tempFile);
                        }
                        ZipEntry entry = new ZipEntry(tempFile);
                        entry.DateTime = DateTime.Now;
                        entry.Size = fileStream.Length;
                        entry.Crc = crc.Value;

                        zipStream.PutNextEntry(entry);
                        await zipStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                else if (Directory.Exists(path))
                {
                    var all = Directory.GetFileSystemEntries(path);
                    foreach (var item in all)
                    {
                        await WriteCompress(item, zipStream);
                    }
                }
            }
        }

        #endregion
    }
}





