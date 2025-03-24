using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static GateHelper.LogManager;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;


namespace GateHelper
{
    internal class Util_ImageLoader
    {
        // 25.03.23 Added - Create ReferenceImages folder if not exists
        public static void EnsureReferenceImagesFolderExists()
        {
            string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                LogMessage($"'{folderPath}' 폴더가 생성되었습니다.", Level.Info);
            }
        }


        // 25.03.23 Added - Load reference images from ReferenceImages folder
        public static void LoadReferenceImages(FlowLayoutPanel panel)
        {
            string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");

            // Load saved order or fallback to file system order
            List<string> imageFiles = LoadImageOrder(folderPath);

            if (imageFiles == null)
            {
                imageFiles = Directory.GetFiles(folderPath, "*.*")
                                      .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                                      .OrderBy(Path.GetFileName) // optional: filename sort fallback
                                      .ToList();
            }

            panel.Controls.Clear();

            foreach (string imgPath in imageFiles)
            {
                AddPictureBoxWithZoomEffect(panel, imgPath);
            }
        }

        // 25.03.23 Added - Open ReferenceImages folder
        public static void OpenReferenceImagesFolder()
        {
            string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            Process.Start("explorer.exe", folderPath);
        }

        public static void AddPictureBoxWithZoomEffect(FlowLayoutPanel panel, string imgPath)
        {
            PictureBox pic = new PictureBox();

            try
            {
                pic.Image = Image.FromFile(imgPath);
                pic.ImageLocation = imgPath;
            }
            catch
            {
                return; // Image load failed
            }

            // Basic PictureBox setup
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Width = 10; // Start small for zoom animation
            pic.Height = 10;
            pic.Margin = new Padding(5);
            pic.Cursor = Cursors.Hand;
            pic.AllowDrop = true; // Enable drop for drag-and-drop sorting

            // NEW: Drag-and-drop support
            pic.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    pic.DoDragDrop(pic, DragDropEffects.Move);
            };

            pic.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(PictureBox)))
                    e.Effect = DragDropEffects.Move;
            };

            pic.DragDrop += (s, e) =>
            {
                PictureBox source = e.Data.GetData(typeof(PictureBox)) as PictureBox;
                PictureBox target = (PictureBox)s;

                if (source != null && target != null && source != target)
                {
                    int sourceIndex = panel.Controls.GetChildIndex(source);
                    int targetIndex = panel.Controls.GetChildIndex(target);

                    panel.Controls.SetChildIndex(source, targetIndex);
                    panel.Controls.SetChildIndex(target, sourceIndex);

                    panel.Invalidate(); // Optional: redraw

                    // ✅ Save new order immediately after drop
                    string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");
                    SaveImageOrder(panel, folderPath);
                }
            };

            // Image preview on click
            pic.Click += (s, e) =>
            {
                Form preview = new Form
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(600, 600)
                };

                PictureBox previewPic = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                try
                {
                    previewPic.Image = Image.FromFile(imgPath);
                }
                catch
                {
                    preview.Close();
                    return;
                }

                preview.Controls.Add(previewPic);
                preview.ShowDialog();
            };

            panel.Controls.Add(pic);

            // Zoom-in animation
            Timer growTimer = new Timer();
            growTimer.Interval = 15;
            growTimer.Tick += (s, e) =>
            {
                if (pic.Width < 100)
                {
                    pic.Width += 10;
                    pic.Height += 10;
                }
                else
                {
                    pic.Width = 100;
                    pic.Height = 100;
                    growTimer.Stop();
                    growTimer.Dispose();
                }
            };
            growTimer.Start();
        }


        public static List<string> LoadImageOrder(string imageFolder)
        {
            string metaFolder = Util.CreateMetaFolderAndGetPath();
            string orderPath = Path.Combine(metaFolder, "image_order.dat");

            if (!File.Exists(orderPath))
                return null;

            var orderedFiles = File.ReadAllLines(orderPath)
                                   .Where(line => !string.IsNullOrWhiteSpace(line))
                                   .Select(fileName => Path.Combine(imageFolder, fileName))
                                   .Where(File.Exists)
                                   .ToList();

            return orderedFiles;
        }

        public static void SaveImageOrder(FlowLayoutPanel panel, string imageFolder)
        {
            string metaFolder = Util.CreateMetaFolderAndGetPath();
            if (!Directory.Exists(metaFolder))
                Directory.CreateDirectory(metaFolder);

            string orderPath = Path.Combine(metaFolder, "image_order.dat");

            var fileNames = panel.Controls.OfType<PictureBox>()
                                 .Select(pb => Path.GetFileName(pb.ImageLocation))
                                 .Where(name => !string.IsNullOrWhiteSpace(name))
                                 .ToList();

            File.WriteAllLines(orderPath, fileNames);
        }



    }
}
