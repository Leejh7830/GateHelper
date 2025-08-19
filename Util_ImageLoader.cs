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
            Image img = null;
            try
            {
                using (var stream = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
                {
                    img = Image.FromStream(stream);
                }
            }
            catch (Exception ex) // 이미지로드 실패
            {
                LogMessage($"Failed to load image from '{imgPath}'. Error: {ex.Message}", Level.Error);
                return;
            }

            // 이미지가 정상적으로 로드되지 않았으면 종료
            if (img == null) return;

            PictureBox pic = new PictureBox();

            // PictureBox에 이미지와 이미지 파일 경로 할당
            pic.Image = img;
            pic.ImageLocation = imgPath;

            // 기본적인 PictureBox 속성 설정
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Width = 10; // 줌 애니메이션을 위해 초기 크기를 작게 설정
            pic.Height = 10;
            pic.Margin = new Padding(5);
            pic.Cursor = Cursors.Hand;
            pic.AllowDrop = true;

            // 드래그 앤 드롭 이벤트 핸들러를 연결
            string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");
            HandleDragAndDrop(pic, panel, folderPath);

            // 마우스 오른쪽 클릭 시 이미지 미리보기를 띄웁니다.
            pic.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowImagePreview(imgPath);
                }
            };

            // PictureBox를 FlowLayoutPanel에 추가합니다.
            panel.Controls.Add(pic);

            // 줌 애니메이션을 적용합니다.
            ApplyZoomAnimation(pic);
        }

        private static void HandleDragAndDrop(PictureBox pic, FlowLayoutPanel panel, string folderPath)
        {
            pic.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    pic.DoDragDrop(pic, DragDropEffects.Move); // Start drag operation
            };

            pic.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(PictureBox))) // Ensure we are dragging a PictureBox
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

                    // Swap the PictureBox positions in the panel
                    panel.Controls.SetChildIndex(source, targetIndex);
                    panel.Controls.SetChildIndex(target, sourceIndex);

                    panel.Invalidate(); // Optional: Force redraw

                    // Save the new order immediately after the drag and drop
                    SaveImageOrder(panel, folderPath);
                }
            };
        }

        private static void ShowImagePreview(string imgPath)
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
        }

        private static void ApplyZoomAnimation(PictureBox pic)
        {
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
