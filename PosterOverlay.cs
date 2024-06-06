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
            CreateSaveImageButton();
            CreateResetButton();
        }

        private void SetFormAppearance()
        {
            this.BackColor = Color.DarkGray;
        }

        private void CreateOverlayImageComboBox()
        {
            cboOverlayImage = new ComboBox();
            cboOverlayImage.Items.AddRange(new object[] {
                "4K with DV and IMAX",
                "4K with DV",
                "4K with HDR",
                "4K with HDR and IMAX",
                "4K with IMAX",
                "4K",
                "1080p with DV and IMAX",
                "1080p with DV",
                "1080p with HDR",
                "1080p with HDR and IMAX",
                "1080p with IMAX",
                "1080p"
            });
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

        private void CreateSaveImageButton()
        {
            Button btnSaveImage = new Button
            {
                Text = "Save Poster",
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
                Location = new Point(190, 842),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            btnReset.Click += new EventHandler(btnReset_Click);
            Controls.Add(btnReset);
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
        }

        private void btnBaseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                baseImage = Image.FromFile(openFileDialog.FileName);
                int maxWidth = 1000;
                int maxHeight = 1500;
                double ratioX = (double)maxWidth / baseImage.Width;
                double ratioY = (double)maxHeight / baseImage.Height;
                double ratio = Math.Min(ratioX, ratioY);
                int newWidth = (int)(baseImage.Width * ratio);
                int newHeight = (int)(baseImage.Height * ratio);
                Bitmap bitmap = new Bitmap(baseImage, newWidth, newHeight);
                pbResultImage.Image = bitmap;
            }
        }


        private void cboOverlayImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            string overlayFilename = "";

            switch (cbo.SelectedIndex)
            {
                case 0:
                    overlayFilename = @"OverlayImages\4K with DV and Imax.png";
                    break;
                case 1:
                    overlayFilename = @"OverlayImages\4K with DV.png";
                    break;
                case 2:
                    overlayFilename = @"OverlayImages\4K with HDR.png";
                    break;
                case 3:
                    overlayFilename = @"OverlayImages\4K-HDR with Imax.png";
                    break;
                case 4:
                    overlayFilename = @"OverlayImages\4K with Imax.png";
                    break;
                case 5:
                    overlayFilename = @"OverlayImages\4K.png";
                    break;
                case 6:
                    overlayFilename = @"OverlayImages\1080p with DV and Imax.png";
                    break;
                case 7:
                    overlayFilename = @"OverlayImages\1080p with DV.png";
                    break;
                case 8:
                    overlayFilename = @"OverlayImages\1080p with HDR.png";
                    break;
                case 9:
                    overlayFilename = @"OverlayImages\1080p with HDR and Imax.png";
                    break;
                case 10:
                    overlayFilename = @"OverlayImages\1080p with Imax.png";
                    break;
                case 11:
                    overlayFilename = @"OverlayImages\1080p.png";
                    break;
            }
            ApplyOverlay(overlayFilename);
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

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*",
                DefaultExt = "png"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                resultBitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }
        private void PosterOverlay_Load(object sender, EventArgs e)
        {

        }
    }
}
