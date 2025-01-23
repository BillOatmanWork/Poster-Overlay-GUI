using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace PosterOverlay
{
    public partial class PosterOverlay : Form
    {
        private PictureBox pbResultImage;
        private ComboBox cboOverlayImage;
        private Image baseImage;
        private object overlayImagePath;
        private string currentFileandPath;

        public static List<string> overlayFilenames = new List<string>
        {
            "UHDBD with DV and IMAX",
            "UHDBD with DV",
            "UHDBD with HDR",
            "UHDBD with HDR and IMAX",
            "UHDBD with HDR10+",
            "UHDBD with HDR10+ and IMAX",
            "UHDBD with IMAX",
            "UHDBD",
            "BD with DV and IMAX",
            "BD with DV",
            "BD with HDR",
            "BD with HDR and IMAX",
            "BD with HDR10+",
            "BD with HDR10+ and IMAX",
            "BD with IMAX",
            "BD 3D",
            "BD 3D with IMAX",
            "BD",
            "DTheater"
        };

        public PosterOverlay()
        {
            InitializeComponent();
            ConfigureForm();
        }

        private void ConfigureForm()
        {
            SetFormAppearance();
            CreateOverlayImageComboBox();
            CreateBaseImageButton();
            CreateResultImagePictureBox();
            CreateSaveAsImageButton();
            CreateSaveImageButton();
            CreateResetButton();
            CreateAutoDetectButton();
        }

        private void SetFormAppearance()
        {
            this.BackColor = Color.DarkGray;
        }

        private void CreateOverlayImageComboBox()
        {
            cboOverlayImage = new ComboBox();
            cboOverlayImage.Items.Clear();
            foreach (var filename in overlayFilenames)
            {
                cboOverlayImage.Items.Add(filename);
            }
            cboOverlayImage.Size = new Size(160, 23);
            cboOverlayImage.Location = new Point(193, 17);
            cboOverlayImage.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOverlayImage.BackColor = Color.LightGray;
            cboOverlayImage.ForeColor = Color.Black;
            cboOverlayImage.Font = new Font("Arial", 10, FontStyle.Regular);
            cboOverlayImage.FlatStyle = FlatStyle.Flat;
            cboOverlayImage.Text = "Choose Overlay";
            cboOverlayImage.SelectedIndex = 0;
            Controls.Add(cboOverlayImage);
            cboOverlayImage.SelectedIndexChanged += new EventHandler(cboOverlayImage_SelectedIndexChanged);
        }

        private void CreateBaseImageButton()
        {
            Button btnBaseImage = new Button
            {
                Text = "Select Poster",
                Size = cboOverlayImage.Size,
                Location = new Point(23, 17),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnBaseImage.Click += new EventHandler(btnBaseImage_Click);
            Controls.Add(btnBaseImage);
        }

        private void CreateResultImagePictureBox()
        {
            pbResultImage = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(502, 755),
                Location = new Point(23, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(pbResultImage);
        }

        private void CreateSaveAsImageButton()
        {
            Button btnSaveAsImage = new Button
            {
                Text = "Save as...",
                Size = cboOverlayImage.Size,
                Location = new Point(365, 842),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnSaveAsImage.Click += new EventHandler(btnSaveAsImage_Click);
            Controls.Add(btnSaveAsImage);
        }

        private void CreateSaveImageButton()
        {
            Button btnSaveImage = new Button
            {
                Text = "Save",
                Size = cboOverlayImage.Size,
                Location = new Point(365, 17),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnSaveImage.Click += new EventHandler(btnSaveImage_Click);
            Controls.Add(btnSaveImage);
        }

        private void CreateResetButton()
        {
            Button btnReset = new Button
            {
                Text = "Reset",
                Size = cboOverlayImage.Size,
                Location = new Point(23, 842),
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnReset.Click += new EventHandler(btnReset_Click);
            Controls.Add(btnReset);
        }

        private void CreateAutoDetectButton()
        {
            Button btnAutoDetect = new Button
            {
                Text = "Auto Detect",
                Size = cboOverlayImage.Size,
                Location = new Point(195, 842),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnAutoDetect.Click += new EventHandler(btnAutoDetect_Click);
            Controls.Add(btnAutoDetect);
        }

        private void pbResultImage_Paint(object sender, PaintEventArgs e)
        {
            Pen borderPen = new Pen(Color.White, 10);
            e.Graphics.DrawRectangle(borderPen, new Rectangle(pbResultImage.Location, pbResultImage.Size));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            cboOverlayImage.SelectedIndex = -1;
            pbResultImage.Image = null;

            //Clear the base image so overwriting it is possible
            baseImage.Dispose();
        }

        private void btnAutoDetect_Click(object sender, EventArgs e)
        {
            try
            {
                DetectionResult detectionResult = AutoDetect.AutoDetectProperties(currentFileandPath);

                string suggestedOverlay = SelectBestFilename(detectionResult);
                ApplyOverlay(suggestedOverlay);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnBaseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Load the base image using method
                LoadBaseImage(openFileDialog.FileName);

                //Ensure we save the current filename and path so we can do quick overwrites later
                currentFileandPath = openFileDialog.FileName;
            }
        }

        public static string SelectBestFilename(DetectionResult result)
        {
            var priorityOrder = new List<Func<DetectionResult, bool>>
            {
                r => r.Is3D && r.IsIMAX,
                r => r.Is3D,
                r => r.IsDV && r.IsIMAX,
                r => r.IsDV,
                r => r.IsHDR && r.IsIMAX,
                r => r.IsHDR,
                r => r.IsHDR10Plus && r.IsIMAX,
                r => r.IsHDR10Plus,
                r => r.IsIMAX,
                r => r.IsDolbyTheatre,
                r => r.Resolution?.Contains("UHD") == true,
                r => true
            };

            foreach (var condition in priorityOrder)
            {
                if (condition(result))
                {
                    return overlayFilenames.FirstOrDefault(f => MatchesCondition(f, result)) + ".png" ?? "BD.png";
                }
            }

            return "BD.png";
        }

        private static bool MatchesCondition(string filename, DetectionResult result)
        {
            if (result.Is3D && filename.Contains("3D"))
            {
                if (result.IsIMAX && filename.Contains("IMAX")) return true;
                if (!result.IsIMAX && !filename.Contains("IMAX")) return true;
            }
            if (result.IsDV && filename.Contains("DV"))
            {
                if (result.IsIMAX && filename.Contains("IMAX")) return true;
                if (!result.IsIMAX && !filename.Contains("IMAX")) return true;
            }
            if (result.IsHDR && filename.Contains("HDR"))
            {
                if (result.IsIMAX && filename.Contains("IMAX")) return true;
                if (!result.IsIMAX && !filename.Contains("IMAX")) return true;
            }
            if (result.IsHDR10Plus && filename.Contains("HDR10+"))
            {
                if (result.IsIMAX && filename.Contains("IMAX")) return true;
                if (!result.IsIMAX && !filename.Contains("IMAX")) return true;
            }
            if (result.IsIMAX && filename.Contains("IMAX")) return true;
            if (result.IsDolbyTheatre && filename.Contains("DTheater")) return true;
            if (result.Resolution?.Contains("UHD") == true && filename.Contains("UHDBD")) return true;

            return filename == "BD.png";
        }

        private void LoadBaseImage(string filenameAndPath)
        {
            baseImage = Image.FromFile(filenameAndPath);
            int maxWidth = 1000;
            int maxHeight = 1500;
            double ratioX = (double)maxWidth / baseImage.Width;
            double ratioY = (double)maxHeight / baseImage.Height;
            double ratio = Math.Min(ratioX, ratioY);
            int newWidth = (int)(baseImage.Width * ratio);
            int newHeight = (int)(baseImage.Height * ratio);
            Bitmap bitmap = new Bitmap(baseImage, newWidth, newHeight);
            pbResultImage.Image = bitmap;

            //Select whatever Overlay is currently selected in the dropdown list as the one to apply when base image is loaded
            SelectOverlay();
        }

        private void SelectOverlay()
        {
            string overlayFilename = "";

            switch (cboOverlayImage.SelectedIndex)
            {
                case 0:
                    overlayFilename = @"OverlayImages\UHDBD with DV and Imax.png";
                    break;
                case 1:
                    overlayFilename = @"OverlayImages\UHDBD with DV.png";
                    break;
                case 2:
                    overlayFilename = @"OverlayImages\UHDBD with HDR.png";
                    break;
                case 3:
                    overlayFilename = @"OverlayImages\UHDBD with HDR and Imax.png";
                    break;
                case 4:
                    overlayFilename = @"OverlayImages\UHDBD with HDR10+.png";
                    break;
                case 5:
                    overlayFilename = @"OverlayImages\UHDBD with HDR10+ and Imax.png";
                    break;
                case 6:
                    overlayFilename = @"OverlayImages\UHDBD with Imax.png";
                    break;
                case 7:
                    overlayFilename = @"OverlayImages\UHDBD.png";
                    break;
                case 8:
                    overlayFilename = @"OverlayImages\BD with DV and Imax.png";
                    break;
                case 9:
                    overlayFilename = @"OverlayImages\BD with DV.png";
                    break;
                case 10:
                    overlayFilename = @"OverlayImages\BD with HDR.png";
                    break;
                case 11:
                    overlayFilename = @"OverlayImages\BD with HDR and Imax.png";
                    break;
                case 12:
                    overlayFilename = @"OverlayImages\BD with HDR10+.png";
                    break;
                case 13:
                    overlayFilename = @"OverlayImages\BD with HDR10+ and Imax.png";
                    break;
                case 14:
                    overlayFilename = @"OverlayImages\BD with Imax.png";
                    break;
                case 15:
                    overlayFilename = @"OverlayImages\BD 3D.png";
                    break;
                case 16:
                    overlayFilename = @"OverlayImages\BD 3D with IMAX.png";
                    break;
                case 17:
                    overlayFilename = @"OverlayImages\BD.png";
                    break;
                case 18:
                    overlayFilename = @"OverlayImages\DTheater.png";
                    break;
            }
            ApplyOverlay(overlayFilename);
        }

        private void cboOverlayImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectOverlay();
        }

        private void ApplyOverlay(string overlayFilename)
        {
            if (overlayFilename != "")
            {
                overlayImagePath = overlayFilename;
                Bitmap baseBitmap = pbResultImage.Image != null ? new Bitmap(pbResultImage.Image) : new Bitmap(1000, 1500);
                if (baseImage != null)
                {
                    int maxWidth = 1000;
                    int maxHeight = 1500;
                    double ratioX = (double)maxWidth / baseImage.Width;
                    double ratioY = (double)maxHeight / baseImage.Height;
                    double ratio = Math.Min(ratioX, ratioY);
                    int resizedWidth = (int)(baseImage.Width * ratio);
                    int resizedHeight = (int)(baseImage.Height * ratio);
                    Bitmap resizedBitmap = new Bitmap(resizedWidth, resizedHeight);
                    using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                    {
                        graphics.DrawImage(baseImage, 0, 0, resizedWidth, resizedHeight);
                    }
                    baseBitmap = resizedBitmap;
                }
                Bitmap overlayBitmap = new Bitmap(overlayFilename);
                float scaleX = (float)baseBitmap.Width / overlayBitmap.Width;
                float scaleY = (float)baseBitmap.Height / overlayBitmap.Height;
                float scale = Math.Min(scaleX, scaleY);
                int newWidth = (int)(overlayBitmap.Width * scale);
                int newHeight = (int)(overlayBitmap.Height * scale);
                Bitmap resizedOverlayBitmap = new Bitmap(newWidth, newHeight);
                using (Graphics graphics = Graphics.FromImage(resizedOverlayBitmap))
                {
                    graphics.DrawImage(overlayBitmap, 0, 0, newWidth, newHeight);
                }
                Bitmap resultBitmap = new Bitmap(baseBitmap.Width, baseBitmap.Height);
                using (Graphics graphics = Graphics.FromImage(resultBitmap))
                {
                    graphics.DrawImage(baseBitmap, 0, 0);
                    graphics.DrawImage(resizedOverlayBitmap, 0, 0);
                }
                pbResultImage.Image = resultBitmap;
            }
        }
        private void btnSaveAsImage_Click(object sender, EventArgs e)
        {
            if (baseImage == null || overlayImagePath == null)
            {
                MessageBox.Show("Please select a base image and overlay image.", "Error");
                return;
            }

            Image overlayImage = Image.FromFile((string)overlayImagePath);
            Bitmap resultBitmap = new Bitmap(pbResultImage.Width, pbResultImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                g.DrawImage(baseImage, 0, 0, pbResultImage.Width, pbResultImage.Height);
                g.DrawImage(overlayImage, 0, 0, pbResultImage.Width, pbResultImage.Height);
            }

            pbResultImage.Image = resultBitmap;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*",
                DefaultExt = "jpg"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Clear the base image so overwriting it is possible
                baseImage.Dispose();

                //Save the new image
                resultBitmap.Save(saveFileDialog.FileName, ImageFormat.Jpeg);

                //Reload the now updated base image so we can continue to work with it if needed
                LoadBaseImage(saveFileDialog.FileName);
            }
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (baseImage == null || overlayImagePath == null)
            {
                MessageBox.Show("Please select a base image and overlay image.", "Error");
                return;
            }

            Image overlayImage = Image.FromFile((string)overlayImagePath);
            Bitmap resultBitmap = new Bitmap(pbResultImage.Width, pbResultImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                g.DrawImage(baseImage, 0, 0, pbResultImage.Width, pbResultImage.Height);
                g.DrawImage(overlayImage, 0, 0, pbResultImage.Width, pbResultImage.Height);
            }

            pbResultImage.Image = resultBitmap;

            //Clear the base image so overwriting it is possible
            baseImage.Dispose();

            //Save the new image by overwriting the old image directly
            resultBitmap.Save(currentFileandPath);

            //Reload the now updated base image so we can continue to work with it if needed
            LoadBaseImage(currentFileandPath);
        }
        private void PosterOverlay_Load(object sender, EventArgs e)
        {

        }
    }
}
