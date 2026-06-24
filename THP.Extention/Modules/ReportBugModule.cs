using EscherGroup.EssentialCP.Client.Core;
using EscherGroup.EssentialCP.Client.UI;
using EscherGroup.EssentialCP.Client.Workflow;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using THP.Extention.UI;
using THP.Extention.Workflows;

public class ReportBugModule : IModule
{
    private readonly IUIController _uiController;

    public string Name => "THP.Extention.ReportBugs";

    public ReportBugModule(IUIController uiController)
    {
        _uiController = uiController;
        RegisterCustomIcons();
    }

    public void RegisterEntryPoints(
        IModuleRegistrationManager manager)
    {
        manager.RegisterEntryPoint(
            this,
            "ReportBugs",
            ReportBugWorkflow.CreateView);
    }

    private void RegisterCustomIcons()
    {
        // 1. Merge compiled WPF resource dictionary from DLL
        try
        {
            if (System.Windows.Application.Current != null)
            {
                var dict = new System.Windows.ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/THP.Extention;component/Icons.xaml", UriKind.Absolute)
                };
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
            }
        }
        catch { }

        // 2. Scan folders for dynamic image registration
        try
        {
            var paths = new List<string>
            {
                @"C:\Program Files (x86)\EssentialCP\images",
                @"c:\THP.Counters\images",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images")
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path, "*.png"))
                    {
                        try
                        {
                            string key = Path.GetFileNameWithoutExtension(file);
                            if (System.Windows.Application.Current != null && 
                                !System.Windows.Application.Current.Resources.Contains(key))
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.UriSource = new Uri(file);
                                bitmap.EndInit();
                                bitmap.Freeze();

                                System.Windows.Application.Current.Resources[key] = bitmap;
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        catch { }
    }
}