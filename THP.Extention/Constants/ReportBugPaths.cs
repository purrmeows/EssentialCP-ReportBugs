using System.IO;

namespace THP.Extention.Constants
{
    public static class ReportBugPaths
    {
        public static string RootFolder => @"C:\Temp";

        public static string HistoryFile =>
            Path.Combine(RootFolder, "BugHistory.json");

        public static string ExcelFile =>
            Path.Combine(RootFolder, "BugHistory.xlsx");
    }
}