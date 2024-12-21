using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace CSVComparer.support
{
      internal class DataReader
    {
        private static readonly string ProjectPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigPath = Path.Combine(ProjectPath, "config.json");

        internal string ActualFilePath { get; private set; }
        internal string ExpectedFilePath { get; private set; }
        internal string[] PrimaryKey { get; private set; }
        internal string Author { get; private set; }

        public DataReader()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            var json = File.ReadAllText(ConfigPath);
            var serializer = new JavaScriptSerializer();
            dynamic config = serializer.Deserialize(json, typeof(object));

            ActualFilePath = Path.Combine(ProjectPath, "CSV", config["actualFilePath"]);
            ExpectedFilePath = Path.Combine(ProjectPath, "CSV", config["expectedFilePath"]);
            Author = config["author"];
            PrimaryKey = ((object[])config["primaryKey"]).Select(x => x.ToString()).ToArray();
        }

        internal static List<Dictionary<string, string>> ReadCSVFile(string filePath)
        {
            var data = new List<Dictionary<string, string>>();

            try
            {
                var fileInfo = new FileInfo(filePath);
                LogWriter.WriteLine($"{fileInfo.FullName} - Size(bytes) --> {fileInfo.Length}");

                var lines = File.ReadAllLines(filePath);
                var columnNames = lines[0].Split(',');

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    var row = new Dictionary<string, string>();

                    for (int j = 0; j < columnNames.Length; j++)
                    {
                        row[columnNames[j]] = values[j];
                    }

                    data.Add(row);
                }
            }
            catch (FileNotFoundException e)
            {
                LogWriter.WriteLine($"{e.Message} Please ensure CSV files are present in the CSV folder.");
                throw;
            }
            catch (Exception e)
            {
                LogWriter.WriteLine($"{e.Message}");
                throw;
            }

            return data;
        }
    }
}
