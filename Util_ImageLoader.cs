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

        // 25.03.23 Added - Load reference images from ReferenceImages folder
        // 25.08.25 Relaod 수정
        public static void LoadReferenceImages(FlowLayoutPanel panel)
        {
            string folderPath = Path.Combine(Application.StartupPath, "ReferenceImages");

            // 1. 기존 PictureBox와 이미지 리소스 해제
            foreach (Control control in panel.Controls)
            {
                if (control is PictureBox pb)
                {
                    pb.Image?.Dispose();
                }
                control.Dispose();
            }
            panel.Controls.Clear();

            // 2. 폴더확인, 없으면 생성
            EnsureReferenceImagesFolderExists();

            // 3. 이미지 파일 가져옴(jpg,png,jpeg,bmp)
            List<string> currentImageFiles = Directory.GetFiles(folderPath, "*.*")
                                                       .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                      file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                                      file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                      file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                                                       .ToList();

            // 4. image_order.dat 파일의 정렬 순서를 불러옵니다.
            List<string> savedOrderList = LoadImageOrder(folderPath);
            List<string> finalImageFiles;

            if (savedOrderList != null && savedOrderList.Count > 0)
            {
                // 5. 저장된 순서(savedOrderList)와 현재 폴더의 파일(currentImageFiles)을 병합합니다.

                // **새로 추가된 파일 여부를 판단하기 위해 저장된 파일 이름 목록을 만듭니다.**
                var savedFileNames = savedOrderList.Select(Path.GetFileName).ToList();

                // **저장된 순서대로 유효한 파일들만 가져옵니다.**
                finalImageFiles = savedOrderList.Where(path => File.Exists(path)).ToList();

                // **현재 폴더의 파일 중 저장된 목록에 없는 파일을 찾아 추가합니다.**
                var newFiles = currentImageFiles.Where(path => !savedFileNames.Contains(Path.GetFileName(path))).ToList();
                finalImageFiles.AddRange(newFiles);
            }
            else
            {
                // 저장된 순서가 없으면 파일 이름으로 정렬합니다.
                finalImageFiles = currentImageFiles.OrderBy(Path.GetFileName).ToList();
            }

            // 6. 최종 리스트를 FlowLayoutPanel에 추가 (이 루프는 한 번만 실행되어야함.)
            foreach (string imgPath in finalImageFiles)
            {
                AddPictureBoxWithZoomEffect(panel, imgPath);
            }
        }

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
            HandleDragAndDrop(pic, panel);

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

        private static void HandleDragAndDrop(PictureBox pic, FlowLayoutPanel panel)
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
                    // 1. 드롭 타겟의 인덱스를 기억해둡니다.
                    int targetIndex = panel.Controls.GetChildIndex(target);

                    // 2. 패널에서 드래그된 컨트롤을 제거합니다.
                    panel.Controls.Remove(source);

                    // 3. 드래그된 컨트롤을 원하는 위치에 다시 삽입합니다.
                    // 이 코드가 가장 안정적입니다.
                    panel.Controls.Add(source);
                    panel.Controls.SetChildIndex(source, targetIndex);

                    // 4. 변경된 순서를 저장합니다.
                    SaveImageOrder(panel);
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

        public static void SaveImageOrder(FlowLayoutPanel panel)
        {
            string metaFolder = Util.CreateMetaFolderAndGetPath();
            if (!Directory.Exists(metaFolder))
                Directory.CreateDirectory(metaFolder);

            string orderPath = Path.Combine(metaFolder, "image_order.dat");

            var fileNames = panel.Controls.OfType<PictureBox>()
                                     .Select(pb => Path.GetFileName(pb.ImageLocation))
                                     .Where(name => !string.IsNullOrWhiteSpace(name))
                                     .Distinct() // 중복제거
                                     .ToList();

            File.WriteAllLines(orderPath, fileNames);
        }

    }
}
