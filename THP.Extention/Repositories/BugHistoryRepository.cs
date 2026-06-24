using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

using THP.Extention.Constants;
using THP.Extention.Models;

namespace THP.Extention.Repositories
{
    public class BugHistoryRepository
    {
        public List<BugHistoryItem> Load()
        {
            try
            {
                if (!File.Exists(ReportBugPaths.HistoryFile))
                {
                    return new List<BugHistoryItem>();
                }

                using (FileStream fs =
                    new FileStream(
                        ReportBugPaths.HistoryFile,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(
                            typeof(List<BugHistoryItem>));

                    return (List<BugHistoryItem>)serializer.ReadObject(fs);
                }
            }
            catch
            {
                return new List<BugHistoryItem>();
            }
        }

        public void Save(List<BugHistoryItem> items)
        {
            Directory.CreateDirectory(ReportBugPaths.RootFolder);

            using (FileStream fs =
                new FileStream(
                    ReportBugPaths.HistoryFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
            {
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(
                        typeof(List<BugHistoryItem>));

                serializer.WriteObject(fs, items);
            }
        }
    }
}