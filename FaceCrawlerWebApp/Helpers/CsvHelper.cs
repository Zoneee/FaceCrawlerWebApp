using CsvHelper;
using CsvHelper.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class CsvHelper
    {
        public static void SaveCsv<T>(string savePath, List<T> source)
        {
            using (var writer = new StreamWriter(savePath, false, Encoding.UTF8))
            {
                using (var csv = new CsvWriter(writer))
                {
                    csv.Configuration.IgnoreReferences = true;
                    csv.WriteRecords(source);
                }
            }
        }
    }
}
