using CSVComparer.support;
using System;
using System.Diagnostics;

namespace CSVComparer
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            /*
             * get all the initial data -> File paths and Primary/Composite Key
             * to change this data refer config.js file
             */
            DataReader data = new DataReader();
            var config = new
            {
                data.ActualFilePath,
                data.ExpectedFilePath,
                data.PrimaryKey
            };

            // Compare the CSV files
            DictionaryComparer.CompareCSVFiles(config.ActualFilePath, config.ExpectedFilePath, config.PrimaryKey);
            watch.Stop();
            LogWriter.WriteLine("=================================");
            LogWriter.WriteLine("Completed CSV Comparison at - " + DateTime.Now.ToString());
            LogWriter.WriteLine($"The Execution time of the program is {watch.Elapsed.TotalSeconds} seconds");
            LogWriter.WriteLine("Please refer log file for results...");
            LogWriter.WriteLine();
            Console.ReadLine();
        }
    }
}


    
