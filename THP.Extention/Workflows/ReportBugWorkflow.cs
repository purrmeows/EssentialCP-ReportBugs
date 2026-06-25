using System;
using System.Collections.Generic;
using System.Windows.Forms;

using EscherGroup.EssentialCP.Client.Core;
using EscherGroup.EssentialCP.Client.Workflow;

using THP.Extention.Models;
using THP.Extention.Repositories;
using THP.Extention.Services;
using THP.Extention.UI;

namespace THP.Extention.Workflows
{
    public static class ReportBugWorkflow
    {
        private static readonly BugHistoryRepository _historyRepository =
            new BugHistoryRepository();

        private static readonly ScreenshotService _screenshotService =
            new ScreenshotService();

        private static readonly ExcelExportService _excelExportService =
            new ExcelExportService();

        public static ActivityDefinition CreateView(DataContext dc)
        {
            string screenshotPath =
                _screenshotService.Capture();

            dc.Set(
                "ScreenshotPath",
                screenshotPath);

            dc.Set(
                "SelectedScreenshotPath",
                screenshotPath);

            dc.Set(
                "IsDeleteMode",
                false);

            LoadHistory(dc);

            return ReportBugViewBuilder.Build(dc);
        }

        public static ActivityDefinition ShowCurrentScreenshot(DataContext dc)
        {
            // Restore original typed description if backup exists
            if (dc.ContainsKey("OriginalBugDescription"))
            {
                dc.Set("BugDescription", dc.Get<string>("OriginalBugDescription"));
                dc.Remove("OriginalBugDescription");
            }
            // Restore original screenshot if backup exists
            if (dc.ContainsKey("OriginalScreenshotPath"))
            {
                string originalPath = dc.Get<string>("OriginalScreenshotPath");
                dc.Set("ScreenshotPath", originalPath);
                dc.Set("SelectedScreenshotPath", originalPath);
                dc.Remove("OriginalScreenshotPath");
            }
            else
            {
                string current = dc.Get<string>("ScreenshotPath");
                dc.Set("SelectedScreenshotPath", current);
            }

            if (dc.ContainsKey("EditingBugId"))
            {
                dc.Remove("EditingBugId");
            }

            dc.Set(
                "IsDeleteMode",
                false);

            LoadHistory(dc);

            return ReportBugViewBuilder.Build(dc);
        }

        private static void LoadHistory(DataContext dc)
        {
            List<BugHistoryItem> history =
                _historyRepository.Load();

            for (int i = 0; i < history.Count; i++)
            {   //รับประกันว่า BugHistoryItem ทุกตัวต้องมี Id เสมอ
                if (string.IsNullOrEmpty(history[i].Id))
                {
                    history[i].Id = Guid.NewGuid().ToString();
                }

                history[i].No = i + 1;
                history[i].IsChecked = false;

                history[i].ViewImageCommand =
                    new ViewImageCommand(
                        history[i],
                        dc);
            }

            _historyRepository.Save(history);

            dc.Set(
                "BugHistory",
                new System.Collections.ObjectModel.ObservableCollection<BugHistoryItem>(history)); // ให้ WPF ตารางรับรู้เมื่อมีการเพิ่ม/ลดแถว และแสดงผลบนหน้าจออัตโนมัติ
                                                                                                   // แจ้งว่าจำนวนสมาชิกใน List เปลี่ยน 
        }

        public static ActivityDefinition Submit(DataContext dc)
        {
            bool isDeleteMode = false;  
            if (dc.ContainsKey("IsDeleteMode"))
            {
                isDeleteMode = dc.Get<bool>("IsDeleteMode");
            }

            if (isDeleteMode)
            {
                List<BugHistoryItem> checkedItems = new List<BugHistoryItem>();
                if (dc.ContainsKey("BugHistory"))
                {
                    var observableHistory = dc.Get<System.Collections.ObjectModel.ObservableCollection<BugHistoryItem>>("BugHistory");
                    if (observableHistory != null)
                    {
                        foreach (var historyItem in observableHistory)
                        {
                            if (historyItem.IsChecked)
                            {
                                checkedItems.Add(historyItem);
                            }
                        }
                    }
                }

                if (checkedItems.Count > 0)
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder();
                    message.AppendLine($"คุณต้องการลบรายการที่เลือกทั้งหมด {checkedItems.Count} รายการใช่หรือไม่?\r\n");
                    message.AppendLine("รายการที่กำลังจะลบ:");
                    foreach (var checkedItem in checkedItems)
                    {
                        message.AppendLine($"- [No. {checkedItem.No}] {checkedItem.Description}");
                    }

                    DialogResult result = MessageBox.Show(
                        message.ToString(), 
                        "ยืนยันการลบหลายรายการ", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        List<BugHistoryItem> dbHistory = _historyRepository.Load();
                        foreach (var checkedItem in checkedItems)
                        {
                            dbHistory.RemoveAll(x => x.Id == checkedItem.Id);
                        }

                        _historyRepository.Save(dbHistory);

                        if (dc.ContainsKey("EditingBugId"))
                        {
                            string editingId = dc.Get<string>("EditingBugId");
                            if (checkedItems.Exists(x => x.Id == editingId))
                            {
                                dc.Remove("EditingBugId");
                                dc.Set("BugDescription", "");
                                if (dc.ContainsKey("ScreenshotPath"))
                                {
                                    dc.Set("SelectedScreenshotPath", dc.Get<string>("ScreenshotPath"));
                                }
                            }
                        }

                        LoadHistory(dc);
                        MessageBox.Show("ลบรายการที่เลือกสำเร็จเรียบร้อยแล้ว");

                        return ShowCurrentScreenshot(dc);
                    }
                    else
                    {
                        return ReportBugViewBuilder.Build(dc);
                    }
                }
                else
                {
                    MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบอย่างน้อย 1 รายการ หรือกด ยกเลิก เพื่อกลับสู่หน้าปกติ");
                    return ReportBugViewBuilder.Build(dc);
                }
            }

            string bug = dc.Get<string>("BugDescription");
            string screenshot = dc.Get<string>("ScreenshotPath");

            List<BugHistoryItem> history = _historyRepository.Load();

            if (dc.ContainsKey("EditingBugId"))
            {
                string editingId = dc.Get<string>("EditingBugId");
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to save changes to this report?", "Confirm Edit", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    BugHistoryItem existingItem = history.Find(x => x.Id == editingId);
                    if (existingItem != null)
                    {
                        existingItem.Description = bug ?? string.Empty;
                        _historyRepository.Save(history);

                        LoadHistory(dc);

                        dc.Remove("EditingBugId");

                        MessageBox.Show("Changes successfully saved");
                    }
                }
                
                return ShowCurrentScreenshot(dc);
            }

            BugHistoryItem item = new BugHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                No = history.Count + 1,
                Date = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Status = "PENDING",
                Description = bug ?? string.Empty,
                ScreenshotPath = screenshot
            };
            history.Insert(0, item);

            _historyRepository.Save(history);

            _excelExportService.Export(item);

            // Clean backups if they exist
            if (dc.ContainsKey("OriginalBugDescription")) dc.Remove("OriginalBugDescription");
            if (dc.ContainsKey("OriginalScreenshotPath")) dc.Remove("OriginalScreenshotPath");

            MessageBox.Show("Data successfully saved");

            return WorkflowDefinitions.Complete();
        }

        public static ActivityDefinition ToggleDeleteMode(DataContext dc)
        {
            bool isDeleteMode = false;
            if (dc.ContainsKey("IsDeleteMode"))
            {
                isDeleteMode = dc.Get<bool>("IsDeleteMode");
            }

            // Toggle deletion mode
            isDeleteMode = !isDeleteMode;
            dc.Set("IsDeleteMode", isDeleteMode);

            // Reset selection marks on all history items
            if (dc.ContainsKey("BugHistory"))
            {
                var observableHistory = dc.Get<System.Collections.ObjectModel.ObservableCollection<BugHistoryItem>>("BugHistory");
                if (observableHistory != null)
                {
                    foreach (var item in observableHistory)
                    {
                        item.IsChecked = false;
                    }
                }
            }

            return ReportBugViewBuilder.Build(dc);
        }
    }
}