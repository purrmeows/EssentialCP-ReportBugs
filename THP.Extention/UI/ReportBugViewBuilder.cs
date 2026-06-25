using EscherGroup.EssentialCP.Client.Core;
using EscherGroup.EssentialCP.Client.UI;
using EscherGroup.EssentialCP.Client.Workflow;
using System.Collections.Generic;
using THP.Extention.Workflows;
    
namespace THP.Extention.UI
{
    public static class ReportBugViewBuilder
    {
        public static FormViewDefinition Build(DataContext dc)
        {
            FormViewDefinition form =
                new FormViewDefinition( 
                    "THP.ReportBug.ReportBugView",
                    ReportBugWorkflow.Submit);

            form.Title = "THP_ReportBug_Title";
            form.IsViewScrollable = true;
            // สั่งงานเบื้องหลังผ่าน Dispatcher เพื่อรอให้หน้าจอปรากฏขึ้นมาก่อน จึงจะเริ่มเจาะระบบควบคุม
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Action(() =>
                {   // หาหน้าต่างหลัก (MainWindow) ของระบบ POS
                    var window = System.Windows.Application.Current.MainWindow;
                    if (window != null)
                    {   // สร้าง HashSet เก็บค่า Grid และ รูปภาพที่เราปรับแต่งแล้วเพื่อป้องกันการลงทะเบียน Event ซ้ำซ้อน
                        var configuredGrids = new System.Collections.Generic.HashSet<System.Windows.Controls.DataGrid>();
                        var configuredImages = new System.Collections.Generic.HashSet<System.Windows.FrameworkElement>();
                        System.EventHandler layoutHandler = null;
                        layoutHandler = (sender, e) => // นิยามฟังก์ชันดักฟังเลย์เอาต์หน้าจอ
                        {
                            var grids = FindVisualChildren<System.Windows.Controls.DataGrid>(window); // ค้นหา DataGrid (ตารางประวัติ) ทั้งหมดที่อยู่ใต้หน้าต่าง POS ขณะนั้น
                                                                                                      // สั่งว่า: ให้หยิบตารางแต่ละตัวในรายการ grids ออกมาทีละหนึ่งตัว 
                                                                                                      // และในรอบลูปนั้น ให้ตั้งชื่อตารางตัวที่ถูกหยิบออกมาว่า "dg"
                            foreach (var dg in grids)
                            {   
                                if (configuredGrids.Add(dg))// ตรวจเช็คว่า Grid นี้พึ่งค้นพบและยังไม่เคยตั้งค่าใช่หรือไม่
                                                            // ณ บรรทัดนี้ ตัวแปร dg จะเป็นตัวแทนของตารางจริงๆ 1 ตัวบนหน้าจอ
                                                            // เราจึงสามารถเข้าถึงความสามารถของตารางตัวนี้ได้ เช่น ผูก Event
                                {   // การใช้ += คือการกดปุ่ม "สมัครสมาชิก + กดกระดิ่งแจ้งเตือน"
                                    dg.PreviewMouseWheel += Grid_PreviewMouseWheel; // เชื่อม Event
                                    System.Windows.RoutedEventHandler unloadedHandler = null; // สร้างคำสั่งเมื่อคอนโทรลถูกปิดใช้งาน (Unloaded)
                                    unloadedHandler = (s, args) => // ให้ถอดถอนตัวดักฟังทั้งหมดออกจากหน่วยความจำทันที ป้องกันแรมรั่วซึม (Memory Leak)
                                    {
                                        dg.Unloaded -= unloadedHandler;
                                        dg.PreviewMouseWheel -= Grid_PreviewMouseWheel;
                                        configuredGrids.Remove(dg);
                                        if (configuredGrids.Count == 0 && configuredImages.Count == 0)
                                        {
                                            window.LayoutUpdated -= layoutHandler;
                                        }
                                    };
                                    dg.Unloaded += unloadedHandler; // ผูก Event ล้างแรม
                                }
                            }
                            // ดับเบิ้ลคลิกซูมรูปใหญ่ผ่านการเจาะ Reflection บน OmniImage
                            var elements = FindVisualChildren<System.Windows.FrameworkElement>(window); // ค้นหา Framework Element ทั้งหมดในหน้าต่าง
                            foreach (var element in elements)
                            {
                                if (element.GetType().Name == "OmniImage") // ตรวจสอบหาคอนโทรลของระบบที่ชื่อว่า OmniImage
                                {
                                    if (configuredImages.Add(element))
                                    {
                                        element.MouseLeftButtonDown += (s, args) =>  // ดักจับเหตุการณ์การกดเมาส์ซ้ายบนภาพ
                                        {
                                            if (args.ClickCount == 2)
                                            {
                                                args.Handled = true; // ยุติการส่งอีเวนต์นี้ต่อ
                                                // ใช้ Reflection เจาะเข้า field Private ชื่อ "ActualImage" ดึงเอาคอนโทรล Image ของ WPF ของจริงออกมาใช้งาน
                                                var field = element.GetType().GetField("ActualImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                                                var actualImage = 
                                                    field?.GetValue(element) 
                                                    as System.Windows.Controls.Image; 

                                                if (actualImage != null && actualImage.Source != null)
                                                {
                                                    var popup = new System.Windows.Window
                                                    {
                                                        Title = "Screenshot Viewer",
                                                        Width = System.Windows.SystemParameters.WorkArea.Width * 0.95,
                                                        Height = System.Windows.SystemParameters.WorkArea.Height * 0.95,
                                                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                                                        Owner = System.Windows.Application.Current.MainWindow,
                                                        Background = System.Windows.Media.Brushes.Black,
                                                        WindowStyle = System.Windows.WindowStyle.SingleBorderWindow
                                                    };

                                                    var img = new System.Windows.Controls.Image // นำรูปภาพที่แคปไว้ มาใส่ในหน้าต่างป๊อปอัพโดยปรับให้กางออกพอดีสัดส่วนเดิม
                                                    {
                                                        Source = actualImage.Source,
                                                        Stretch = System.Windows.Media.Stretch.Uniform
                                                    };

                                                    popup.MouseDown += (s2, args2) => popup.Close(); // หากคลิกเมาส์จุดไหนในจอ หรือ กดปุ่ม ESC บนคีย์บอร์ด ให้ปิดหน้าต่างรูปทันที
                                                    popup.KeyDown += (s2, args2) =>
                                                    {
                                                        if (args2.Key == System.Windows.Input.Key.Escape)
                                                        {
                                                            popup.Close();
                                                        }
                                                    };

                                                    popup.Content = img; // แปะรูปใส่ป๊อปอัพ
                                                    popup.Show(); // สั่งเด้งหน้าต่างแสดงผล
                                                }
                                            }
                                        };
                                        System.Windows.RoutedEventHandler unloadedHandler = null; // เคลียร์แรมเมื่อรูปถูกลบออกจากการจัดวาง
                                        unloadedHandler = (s, args) =>
                                        {
                                            element.Unloaded -= unloadedHandler; 
                                            configuredImages.Remove(element);
                                            if (configuredGrids.Count == 0 && configuredImages.Count == 0)
                                            {
                                                window.LayoutUpdated -= layoutHandler;
                                            }
                                        };
                                        element.Unloaded += unloadedHandler;
                                    }
                                }
                            }
                        };
                        window.LayoutUpdated += layoutHandler; // เปิดตัวจับการเคลื่อนไหวหน้าจอ
                        layoutHandler(null, null); // รันฟังก์ชันพรีวิวทันทีก่อน
                    }
                })
            );
            
            //
            // Columns
            //
            form.AddLayoutColumnDefinition(
                new LayoutColumnDefinition(
                    0,
                    1,
                    LayoutGridUnitType.Star));

            form.AddLayoutColumnDefinition(
                new LayoutColumnDefinition(
                    1,
                    850,
                    LayoutGridUnitType.Pixel));

            //
            // Rows
            //
            form.AddLayoutRowDefinition(
                new LayoutRowDefinition(
                    0,
                    370,
                    LayoutGridUnitType.Pixel));

            form.AddLayoutRowDefinition(
                new LayoutRowDefinition(
                    1,
                    65,
                    LayoutGridUnitType.Pixel));

            form.AddLayoutRowDefinition(
                new LayoutRowDefinition(
                    2,
                    500,
                    LayoutGridUnitType.Pixel));

            form.AddLayoutRowDefinition(
                new LayoutRowDefinition(
                    3,
                    60,
                    LayoutGridUnitType.Pixel));

            //
            // Description
            //
            TextInputDefinition bugText =
                new TextInputDefinition(
                    "THP_ReportBug_Description",
                    "BugDescription");

            bugText.SupportsMultilineText = true;
            bugText.OverwriteOnInitialEdit = false;
            bugText.LayoutRow = 0;
            bugText.LayoutColumn = 0;

            form.AddUIElement(bugText);

            //
            // Screenshot Title
            //
            SimpleTextDisplayDefinition screenshotTitle =
                new SimpleTextDisplayDefinition("THP_ReportBug_Image");

            screenshotTitle.LayoutRow = 0;
            screenshotTitle.LayoutColumn = 1;
            screenshotTitle.FontSize = 18;
            screenshotTitle.VerticalAlignment = VerticalAlignment.Top;

            form.AddUIElement(screenshotTitle);

            //
            // Screenshot
            //
            DisplayImageDefinition image =
                new DisplayImageDefinition(
                    null,
                    "SelectedScreenshotPath");

            image.ElementName = "BugScreenshot";
            image.LayoutRow = 0;
            image.LayoutColumn = 1;
            image.ImageWidth = 850;
            image.ImageHeight = 370;
            image.VerticalAlignment = VerticalAlignment.Top;
            image.Margin = new UIElementMargin { Top = 35 };

            form.AddUIElement(image);


            //
            // ResetScreenshot
            //
            CommandDefinition currentButton =
                new CommandDefinition(
                    "CurrentScreenshot",
                    ReportBugWorkflow.ShowCurrentScreenshot);

            currentButton.Caption =
                "THP_ResetScreenshot";

            currentButton.LayoutRow = 1;
            currentButton.LayoutColumn = 1;

            form.AddUIElement(currentButton);

            //
            // History Title
            //
            SimpleTextDisplayDefinition historyTitle =
                new SimpleTextDisplayDefinition(
                    "THP_ReportBug_HistoryTitle");

            historyTitle.LayoutRow = 1;
            historyTitle.LayoutColumn = 0;
            historyTitle.LayoutColumnSpan = 2;
            historyTitle.FontSize = 18;

            form.AddUIElement(historyTitle);

            //
            // Grid Columns
            //
            var columns = new List<DataGridColumn>();

            columns.Add(
                new DataGridTextColumn("No")
                {
                    Header = "THP_ReportBug_Grid_No",
                    Width =
                        new DataGridColumnWidth
                        {
                            UnitType = LayoutGridUnitType.Pixel,
                            Width = 80
                        },
                    FontSize = 14
                });

            columns.Add(
                new DataGridTextColumn("Date")
                {
                    Header = "THP_ReportBug_Grid_Date",
                    Width =
                        new DataGridColumnWidth
                        {
                            UnitType = LayoutGridUnitType.Pixel,
                            Width = 250
                        },
                    FontSize = 14

                });

            columns.Add(
                new DataGridTextColumn("Status")
                {
                    Header = "THP_ReportBug_Grid_Status",
                    Width =
                        new DataGridColumnWidth
                        {
                            UnitType = LayoutGridUnitType.Pixel,
                            Width = 150
                        },
                    FontSize = 14

                });

            columns.Add(
                new DataGridTextColumn("ShortDescription")
                {
                    Header = "THP_ReportBug_Grid_Description",
                    Width =
                        new DataGridColumnWidth
                        {
                            UnitType = LayoutGridUnitType.Star,
                            Width = 1
                        },
                    FontSize = 14
                });

            //
            // Grid
            //
            DataGridDefinition grid =
                new DataGridDefinition(
                    "BugHistory", // คีย์ระบุตาราง ผูกข้อมูลกับคีย์ลิสต์ BugHistory ใน DataContext
                    columns);
            grid.ItemCommandProperty = "ViewImageCommand"; // เรียกบรรทัดไหนให้ไปเรียก ViewImageCommand ในไอเทมแถวนั้น
            grid.RowStyle = "BugHistoryRowStyle";  // ผูก Style ทำสีแดงชมพูพาสเทลเมื่อติ๊กถูกเลือก

            grid.LayoutRow = 2;
            grid.LayoutColumn = 0;
            grid.LayoutColumnSpan = 2;

            form.AddUIElement(grid);

            //
            // Batch Delete Button
            //
            CommandDefinition batchDeleteButton =
                new CommandDefinition(
                    "BatchDelete",
                    ReportBugWorkflow.ToggleDeleteMode);

            bool isDeleteMode = false;
            if (dc.ContainsKey("IsDeleteMode"))
            {
                isDeleteMode = dc.Get<bool>("IsDeleteMode");
            }

            batchDeleteButton.Caption = isDeleteMode ? "Global_Cancel" : "THP_ReportBug_Delete";
            batchDeleteButton.LayoutRow = 3;
            batchDeleteButton.LayoutColumn = 1;
            batchDeleteButton.HorizontalAlignment = HorizontalAlignment.Right;

            form.AddUIElement(batchDeleteButton);
                
            return form;
        }
        //sender: ออบเจกต์ต้นทางที่ส่งสัญญาณมา (ในที่นี้คือตัว DataGrid)
        //e:      อาร์กิวเมนต์ที่เก็บพารามิเตอร์ของเมาส์ เช่น แรงเลื่อน ความเร็ว ปุ่มที่กด
        private static void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) 
        {
            if (!e.Handled) //ตรวจเช็คว่าเหตุการณ์การหมุนเมาส์นี้ ยังไม่เคยมีโค้ดส่วนอื่นเอาไปประมวลผลใช่หรือไม่

            {
                e.Handled = true; // ตั้งค่าเป็น true เพื่อสั่ง "บล็อก" ห้ามไม่ให้ DataGrid นำการหมุนเมาส์นี้ไปเลื่อนตารางภายในตัวเอง
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = System.Windows.UIElement.MouseWheelEvent,
                    Source = sender
                };
                // ดึง Object ที่เป็นตัวครอบข้างบน (Parent) ของตาราง DataGrid 
                // โดยการ (Cast) sender ให้เป็น object > FrameworkElement เพื่อให้ดึงคำสั่ง .Parent ได้ 
                // จากนั้นแปลงตัว Parent ให้เป็นประเภท UIElement (ScrollViewer ของหน้าจอหลัก)
                var parentUIElement = ((System.Windows.FrameworkElement)sender).Parent as System.Windows.UIElement;
                if (parentUIElement != null)
                {
                    parentUIElement.RaiseEvent(eventArg);
                }
            }
        }
        // ประกาศ Method Static คืนค่ากลับออกไปในฐานะ IEnumerable<T> (ลิสต์ของ T ที่อ่านค่าได้แบบขี้เกียจ/Lazy Evaluation)
        // <T>: ตัวแปรประเภทข้อมูลที่จะส่งเข้ามาตอนเรียกใช้ เช่น DataGrid หรือ Image
        // depObj: ตัวคอนโทรลเริ่มต้นที่จะให้เข้าไปค้นหาลึกลงไปด้านล่าง (Visual Tree Root)
        // where T : DependencyObject: ตัวบล็อกความปลอดภัย บังคับว่า T ต้องเป็นชนิดที่สามารถวาดบนหน้าจอ WPF ได้เท่านั้น
        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject depObj) where T : System.Windows.DependencyObject
        {
            if (depObj != null)
            {   // วนลูปสแกนหาลูกชั้นแรกทั้งหมดของวัตถุนั้น โดยใช้ VisualTreeHelper.GetChildrenCount ดึงจำนวนของวัตถุลูกระดับสายตรงออกมารันลูป
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {   
                    System.Windows.DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T t)
                    {
                        // คำสั่ง yield return จะส่งค่าออกไปทีละรายการทันทีโดยไม่ต้องรอตรวจให้เสร็จทั้งระบบ ช่วยประหยัดแรมอย่างมาก
                        yield return t; // ส่งค่าที่เข้าเงื่อนไขประเภท T ออกไปทีละตัว
                    }

                    // เพื่อค้นหาลูกหลานของลูกลงไปทีละชั้นไม่สิ้นสุด จนกว่าจะถึงจุดสิ้นสุดของ Visual Tree
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {   // ส่งผลลัพธ์ของชั้นลึกๆ ย้อนคืนกลับขึ้นไปยังผู้เรียกด้านบนสุด
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}