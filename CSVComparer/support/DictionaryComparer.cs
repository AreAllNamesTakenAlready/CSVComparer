using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSVComparer.support
{
    public class DictionaryComparer
    {
        internal static void CompareCSVFiles(string actualFilePath, string expectedFilePath, string[] primaryKey)
        {
            DataReader data = new DataReader();
            var config = new
            {
                data.Author,
            };
            LogWriter.WriteLine("=================================");
            LogWriter.WriteLine("CSV Comparison Tool");
            LogWriter.WriteLine($"Author - {config.Author}");
            LogWriter.WriteLine("=================================");
            LogWriter.WriteLine($"Begin CSV Comparison at - {DateTime.Now}");
            LogWriter.WriteLine();
            LogWriter.WriteLine("Now Comparing files...");

            // Read the CSV files into lists of dictionaries
            List<Dictionary<string, string>> actualData = DataReader.ReadCSVFile(actualFilePath);
            List<Dictionary<string, string>> expectedData = DataReader.ReadCSVFile(expectedFilePath);
            LogWriter.WriteLine("=================================");

            //Check if name, sequence and count of fields are same. If true break out
            if (CompareCSVFields(actualData[0], expectedData[0]))
                return;

            // Find the records that are in the expected file but not in the actual file
            List<Dictionary<string, string>> missingRecords = FindMissingRecords(actualData, expectedData, primaryKey);

            // Find the records that are in the actual file but not in the expected file
            List<Dictionary<string, string>> extraRecords = FindExtraRecords(actualData, expectedData, primaryKey);

            // Find the records that are in both files but have differences
            List<(Dictionary<string, string>, Dictionary<string, string>)> differentRecords = FindDifferentRecords(actualData, expectedData, primaryKey);
                
            LogWriter.WriteLine($"1. Missing Records (Count = {missingRecords.Count}): Records that are in the expected file but not in the actual file");
            foreach (var record in missingRecords)
            {
                    LogWriter.WriteLine(string.Join(", ", record.Values));   
            }
                
            LogWriter.WriteLine();

            LogWriter.WriteLine($"2. Extra Records (Count = {extraRecords.Count}): Records that are in the actual file but not in the expected file");
            foreach (var record in extraRecords)
            {
                  LogWriter.WriteLine(string.Join(", ", record.Values));
            }

            LogWriter.WriteLine();

            LogWriter.WriteLine($"3. Different Records (Count = {differentRecords.Count}): Records that are present in both but few field values are different");
            foreach (var (record, differences) in differentRecords)
            {

                 string[] key = GetKey(record, primaryKey);
                    
                 LogWriter.Write("Key:--> ");
                 LogWriter.WriteLine(string.Join(", ", key));
                   
                 LogWriter.WriteLine("Differences:");
                 foreach (var kvp in differences)
                 {   
                        LogWriter.WriteLine($"{kvp.Key}: {kvp.Value}");
                 }
                    LogWriter.WriteLine();
             }
        }

        static List<Dictionary<string, string>> FindExtraRecords1(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var expectedKeys = expectedData.Select(record => string.Join(",", GetKey(record, primaryKey))).ToHashSet();
            return actualData.Where(record => !expectedKeys.Contains(string.Join(",", GetKey(record, primaryKey)))).ToList();
        }

        static List<Dictionary<string, string>> FindExtraRecords(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var expectedKeys = expectedData.Where(record => record.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
                                                 .Select(record => string.Join(",", GetKey(record, primaryKey)))
                                                 .ToHashSet();

            return actualData.Where(record => record.Values.Any(value => !string.IsNullOrWhiteSpace(value))
                                                    && !expectedKeys.Contains(string.Join(",", GetKey(record, primaryKey))))
                                 .ToList();
        }



        static List<Dictionary<string, string>> FindMissingRecords(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var actualKeys = actualData.Select(record => string.Join(",", GetKey(record, primaryKey))).ToHashSet();
            return expectedData.Where(record => !actualKeys.Contains(string.Join(",", GetKey(record, primaryKey)))).ToList();
        }

        static List<(Dictionary<string, string>, Dictionary<string, string>)> FindDifferentRecords(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var expectedRecords = expectedData.GroupBy(record => string.Join(",", GetKey(record, primaryKey)))
                                                  .ToDictionary(group => group.Key, group => group.First());

            var differentRecords = actualData.Where(actualRecord =>
            {
                var actualKey = string.Join(",", GetKey(actualRecord, primaryKey));
                return expectedRecords.TryGetValue(actualKey, out var expectedRecord) && !AreRecordsEqual(actualRecord, expectedRecord);
            }).Select(actualRecord =>
            {
                var actualKey = string.Join(",", GetKey(actualRecord, primaryKey));
                var expectedRecord = expectedRecords[actualKey];
                var differentRecord = new Dictionary<string, string>();

                foreach (var kvp in actualRecord)
                {
                    if (!expectedRecord.ContainsKey(kvp.Key) || expectedRecord[kvp.Key] != kvp.Value)
                    {
                        differentRecord[kvp.Key] = $"Expected: {expectedRecord[kvp.Key]}, Actual: {kvp.Value}";
                    }
                }

                return (actualRecord, differentRecord);
            }).ToList();

            return differentRecords;
        }

        static List<(Dictionary<string, string>, Dictionary<string, string>)> FindDifferentRecords2(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var expectedRecords = expectedData.ToDictionary(record => string.Join(",", GetKey(record, primaryKey)), record => record);

            var differentRecords = actualData.Where(actualRecord =>
            {
                var actualKey = string.Join(",", GetKey(actualRecord, primaryKey));
                return expectedRecords.TryGetValue(actualKey, out var expectedRecord) && !AreRecordsEqual(actualRecord, expectedRecord);
            }).Select(actualRecord =>
            {
                var actualKey = string.Join(",", GetKey(actualRecord, primaryKey));
                var expectedRecord = expectedRecords[actualKey];
                var differentRecord = new Dictionary<string, string>();

                foreach (var kvp in actualRecord)
                {
                    if (!expectedRecord.ContainsKey(kvp.Key) || expectedRecord[kvp.Key] != kvp.Value)
                    {
                        differentRecord[kvp.Key] = $"Expected: {expectedRecord[kvp.Key]}, Actual: {kvp.Value}";
                    }
                }

                return (actualRecord, differentRecord);
            }).ToList();

            return differentRecords;
        }

        static List<(Dictionary<string, string>, Dictionary<string, string>)> FindDifferentRecords1(List<Dictionary<string, string>> actualData, List<Dictionary<string, string>> expectedData, string[] primaryKey)
        {
            var expectedRecords = expectedData.ToDictionary(record => string.Join(",", GetKey(record, primaryKey)), record => record);

            var differentRecords = new List<(Dictionary<string, string>, Dictionary<string, string>)>();

            foreach (var actualRecord in actualData)
            {
                var actualKey = string.Join(",", GetKey(actualRecord, primaryKey));

                if (expectedRecords.TryGetValue(actualKey, out var expectedRecord))
                {
                    var differentRecord = new Dictionary<string, string>();

                    foreach (var kvp in actualRecord)
                    {
                        if (!expectedRecord.ContainsKey(kvp.Key) || expectedRecord[kvp.Key] != kvp.Value)
                        {
                            differentRecord[kvp.Key] = $"Expected: {expectedRecord[kvp.Key]}, Actual: {kvp.Value}";
                        }
                    }

                    if (differentRecord.Count > 0)
                    {
                        differentRecords.Add((actualRecord, differentRecord));
                    }
                }
            }

            return differentRecords;
        }



        static string[] GetKey(Dictionary<string, string> record, string[] primaryKey)
        {
            string[] key = null;
            try
            {
                key = new string[primaryKey.Length];

                for (int i = 0; i < primaryKey.Length; i++)
                {
                    key[i] = record[primaryKey[i]];
                }

            }
            catch (Exception e)
            {
                LogWriter.WriteLine(e.Message + " Please set the correct primary/composite key in the json config file");
                throw e;
            }
            
            return key;
        }

        static bool AreKeysEqual(string[] key1, string[] key2)
        {
            if (key1.Length != key2.Length)
            {
                return false;
            }

            for (int i = 0; i < key1.Length; i++)
            {
                if (key1[i] != key2[i])
                {
                    return false;
                }
            }

            return true;
        }

        static bool AreRecordsEqual(Dictionary<string, string> record1, Dictionary<string, string> record2)
        {
            if (record1.Count != record2.Count)
            {
                return false;
            }

            foreach (var kvp in record1)
            {
                if (!record2.ContainsKey(kvp.Key) || record2[kvp.Key] != kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }
        
        //This method is used to compare the fields --> counts, name and sequence
        static bool CompareCSVFields(Dictionary<string, string> actual, Dictionary<string, string> expected)
        {
            bool check = false;
            // Check if the field counts are equal
            if (actual.Count != expected.Count)
            {
                LogWriter.WriteLine("Field count is different:");
                LogWriter.WriteLine("Actual Fields: " + actual.Count);
                LogWriter.WriteLine("Expected Fields: " + expected.Count);
                check = true;
            }

            // Check if the key sequences are equal
            if (!actual.Keys.SequenceEqual(expected.Keys))
            {
                LogWriter.WriteLine("Field sequences are different:");
                LogWriter.WriteLine("Actual: " + string.Join(", ", actual.Keys));
                LogWriter.WriteLine("Expected: " + string.Join(", ", expected.Keys));
                check = true;
            }

            // Check if the key names are equal
            var dict1Keys = actual.Keys.ToList();
            var dict2Keys = expected.Keys.ToList();

            var missingKeys = dict1Keys.Except(dict2Keys).ToList();
            var extraKeys = dict2Keys.Except(dict1Keys).ToList();

            if (missingKeys.Count > 0)
            {
                LogWriter.WriteLine("Missing Field in Expected file : ");
                LogWriter.WriteLine(string.Join(", ", missingKeys));
                check = true;
            }

            if (extraKeys.Count > 0)
            {
                LogWriter.WriteLine("Extra Field in Expected file :");
                LogWriter.WriteLine(string.Join(", ", extraKeys));
                check = true;
            }

            return check;
        }

    }
}
