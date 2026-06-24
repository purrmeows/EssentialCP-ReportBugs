using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using THP.Extention.Constants;

namespace THP.Extention.Services
{
    public class ScreenshotService
    {
        public string Capture()
        {
            Directory.CreateDirectory(ReportBugPaths.RootFolder);

            string fileName =
                "Bug_" +
                DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                ".png";

            string fullPath =
                Path.Combine(
                    ReportBugPaths.RootFolder,
                    fileName);

            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(
                        bounds.Left,
                        bounds.Top,
                        0,
                        0,
                        bounds.Size);
                }

                bmp.Save(fullPath, ImageFormat.Png);
            }

            return fullPath;
        }
    }
}