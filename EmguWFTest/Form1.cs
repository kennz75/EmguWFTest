using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;                  //
using Emgu.CV.CvEnum;           // usual Emgu Cv imports
using Emgu.CV.Structure;        //
using Emgu.CV.Cvb;
using Emgu.CV.UI; //

//https://github.com/MicrocontrollersAndMore/OpenCV_3_Windows_10_Installation_Tutorial
//https://www.youtube.com/watch?v=7iyfJ-YaKvw&t=0s&index=3&list=PLoLaqVexEviOkxJIMh3e8p6Ylu8bXfGZ9


namespace EmguWFTest
{
    public partial class Form1 : Form, INotifyPropertyChanged
    {
        Emgu.CV.VideoCapture capture;
        string defInputFile = @"C:\Users\Keny\Videos\2 bar tic tac.mp4";
        Color trackedColor = Color.Red;
//        Color trackedColorMin, trackedColorMax;
        //public int ColSens = 20;

        Hsv trackedColorMin, trackedColorMax;
        Hsv Sensitivity = new Hsv(10, 50, 50);
        Mat Frame = new Mat();


        VideoState VideoState = VideoState.Init;

        public event PropertyChangedEventHandler PropertyChanged;
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }


        private int colSens;
        public int ColSens
        {
            get { return colSens; }
            set
            {
                colSens = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("ColSens"));
                SetTrackedColor(trackedColor);
            }
        }

        private int maxFrame;
        public int MaxFrames
        {
            get
            {
                return maxFrame;
            }
            set
            {
                
                maxFrame = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("MaxFrames"));
            }
        }

        private int actFrame;
        public int ActFrame
        {
            get
            {
                return actFrame;
            }
            set
            {
                if (actFrame != value && value >= 0 && value < MaxFrames)
                {
                    actFrame = value;
                    //trackBarFrames.Value = actFrame;
                    if (capture != null)
                    {
                        //capture.SetCaptureProperty(CapProp.PosFrames, actFrame);
                    }                    
                }
                InvokePropertyChanged(new PropertyChangedEventArgs("ActFrame"));
            }
        }

        //Capture 
        public Form1()
        {
            InitializeComponent();
            //lblTrackedColor.BackColor = trackedColor;

            numericColSens.DataBindings.Add("Value", this, "ColSens", true, DataSourceUpdateMode.OnPropertyChanged);
            trackBarFrames.DataBindings.Add("Value", this, "ActFrame", true, DataSourceUpdateMode.OnPropertyChanged);
            trackBarFrames.DataBindings.Add("Maximum", this, "MaxFrames", true, DataSourceUpdateMode.OnPropertyChanged);

            textBoxFrame.DataBindings.Add("Text", this, "ActFrame", true, DataSourceUpdateMode.OnPropertyChanged);
            ColSens = 10;
            SetTrackedColor(Color.Red);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (capture == null || VideoState == VideoState.Init)
            {
                toolStripButtonStop.Enabled = false;
                toolStripButtonStart.Enabled = false;
                toolStripButtonRew.Enabled = false;
            }
            else
            {
                toolStripButtonStop.Enabled = true;
                toolStripButtonStart.Enabled = true;
                toolStripButtonRew.Enabled = true;

                if (VideoState == VideoState.Running)
                    toolStripButtonStart.Text = "Pause";
                else
                    toolStripButtonStart.Text = "Start";
            }
            //colSen
        }
        //private void button1_Click(object sender, EventArgs e)
        //{

        //}

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void OpenFile(string fileName)
        {
            Mat imgOriginal = null;

            try
            {
                imgOriginal = new Mat(fileName, ImreadModes.Color);
            }
            catch (Exception ex)
            {
                capture = new VideoCapture(fileName);
                if (capture == null)
                {
                    //lblTrackedColor.Text = "unable to open image, error: " + ex.Message;
                    return;
                }

                MaxFrames = Convert.ToInt32(capture.GetCaptureProperty(CapProp.FrameCount)) - 1;
                ActFrame = 0;

                capture.ImageGrabbed += Capture_ImageGrabbed;                
                capture.Start();
                VideoState = VideoState.Running; 
            }

            if (imgOriginal == null)
            {
                if (capture == null)
                {
                    //lblTrackedColor.Text = "unable to open image";
                    return;
                }
                //imgOriginal = capture.QueryFrame();
            }
            else
            {
                ProcessFrame(imgOriginal);
            }

            UpdateUI();
        }

        private void ProcessFrame(Mat imgOriginal)
        {
            Mat imgGrayscale = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgBlurred = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgCanny = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            CvInvoke.CvtColor(imgOriginal, imgGrayscale, ColorConversion.Bgr2Gray);

            //CvInvoke.GaussianBlur(imgGrayscale, imgBlurred, new Size(5, 5), 1.5);
            //CvInvoke.Canny(imgBlurred, imgCanny, 100, 200);

            CvInvoke.Canny(imgGrayscale, imgCanny, 100, 200);


            ibOriginal.Image = imgOriginal;
            ibCanny.Image = imgCanny;
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            ActFrame = Convert.ToInt32(capture.GetCaptureProperty(CapProp.PosFrames));
            capture.Retrieve(Frame);
            //ProcessFrame(frame);
            ProcessFrameBallSearch(Frame);
        }



        //private void ProcessFrameBallSearch_orig(Mat imgOriginal)
        //{
        //    Mat imgHSV = new Mat(imgOriginal.Size, DepthType.Cv8U, 3);

        //    Mat imgThreshLow = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
        //    Mat imgThreshHigh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

        //    Mat imgThresh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

        //    CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

        //    CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(0, 155, 155)), new ScalarArray(new MCvScalar(18, 255, 255)), imgThreshLow);
        //    CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(165, 155, 155)), new ScalarArray(new MCvScalar(179, 255, 255)), imgThreshHigh);

        //    CvInvoke.Add(imgThreshLow, imgThreshHigh, imgThresh);

        //    CvInvoke.GaussianBlur(imgThresh, imgThresh, new Size(3, 3), 0);

        //    Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

        //    CvInvoke.Dilate(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
        //    CvInvoke.Erode(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

        //    CircleF[] circles = CvInvoke.HoughCircles(imgThresh, HoughType.Gradient, 2.0, imgThresh.Rows / 4, 100, 50, 10, 400);

        //    foreach (CircleF circle in circles)
        //    {
        //        //if (txtXYRadius.Text != "")
        //        //{                         // if we are not on the first line in the text box
        //        //    txtXYRadius.AppendText(Environment.NewLine);         // then insert a new line char
        //        //}

        //        //txtXYRadius.AppendText("ball position x = " + circle.Center.X.ToString().PadLeft(4) + ", y = " + circle.Center.Y.ToString().PadLeft(4) + ", radius = " + circle.Radius.ToString("###.000").PadLeft(7));
        //        //txtXYRadius.ScrollToCaret();             // scroll down in text box so most recent line added (at the bottom) will be shown

        //        CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius, new MCvScalar(0, 0, 255), 2);
        //        CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), 3, new MCvScalar(0, 255, 0), -1);
        //    }
        //    ibOriginal.Image = imgOriginal;
        //    ibCanny.Image = imgThresh;
        //}

        //private void ProcessFrameBallSearch(Mat imgOriginal)
        //{
        //    Mat imgHSV = new Mat(imgOriginal.Size, DepthType.Cv8U, 3);

        //    Mat imgThreshLow = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
        //    Mat imgThreshHigh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

        //    Mat imgThresh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

        //    //CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

        //    //CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(0, 155, 155)), new ScalarArray(new MCvScalar(18, 255, 255)), imgThreshLow);
        //    //CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(165, 155, 155)), new ScalarArray(new MCvScalar(179, 255, 255)), imgThreshHigh);
        //    //CvInvoke.Add(imgThreshLow, imgThreshHigh, imgThresh);

        //    //CvInvoke.InRange(imgOriginal, new ScalarArray(new MCvScalar(50, 10, 100)), new ScalarArray(new MCvScalar(150, 100, 255)), imgThresh);

        //    Bgr bgrMin = new Bgr(trackedColorMin);
        //    Bgr bgrMax = new Bgr(trackedColorMax);

        //    //CvInvoke.CvtColor(color, 0, ColorConversion.Bgr2Hsv);
        //    //color.
        //    CvInvoke.InRange(imgOriginal, new ScalarArray(bgrMin.MCvScalar), new ScalarArray(bgrMax.MCvScalar), imgThresh);
        //    //CvInvoke.InRange(imgOriginal, new ScalarArray(new MCvScalar(165, 155, 155)), new ScalarArray(new MCvScalar(179, 255, 255)), imgThreshHigh);



        //    //CvInvoke.GaussianBlur(imgThresh, imgThresh, new Size(3, 3), 0);

        //    Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

        //    //CvInvoke.Dilate(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
        //    //CvInvoke.Erode(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

        //    CvBlobs blobs = new CvBlobs();
        //    CvBlobDetector detector = new CvBlobDetector();

        //    detector.Detect(imgThresh.ToImage<Gray, Byte>(), blobs);
        //    blobs.FilterByArea(500, 50000);
        //    foreach (var b in blobs)
        //    {
        //        CvInvoke.Rectangle(imgOriginal, b.Value.BoundingBox, new MCvScalar(255, 255, 0), 2);
        //    }
        //    //CircleF[] circles = CvInvoke.HoughCircles(imgThresh, HoughType.Gradient, 2.0, imgThresh.Rows / 4, 100, 50, 10, 400);

        //    //foreach (CircleF circle in circles)
        //    //{
        //    //    //if (txtXYRadius.Text != "")
        //    //    //{                         // if we are not on the first line in the text box
        //    //    //    txtXYRadius.AppendText(Environment.NewLine);         // then insert a new line char
        //    //    //}

        //    //    //txtXYRadius.AppendText("ball position x = " + circle.Center.X.ToString().PadLeft(4) + ", y = " + circle.Center.Y.ToString().PadLeft(4) + ", radius = " + circle.Radius.ToString("###.000").PadLeft(7));
        //    //    //txtXYRadius.ScrollToCaret();             // scroll down in text box so most recent line added (at the bottom) will be shown

        //    //    CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius, new MCvScalar(0, 0, 255), 2);
        //    //    CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), 3, new MCvScalar(0, 255, 0), -1);
        //    //    break;
        //    //}
        //    ibOriginal.Image = imgOriginal;
        //    ibCanny.Image = imgThresh;
        //}

        private void ProcessFrameBallSearch(Mat imgInput)
        {
            if (imgInput.IsEmpty)
                return;

            Mat imgOriginal = new Mat();
            imgInput.CopyTo(imgOriginal);
            Mat imgHSV = new Mat(imgOriginal.Size, DepthType.Cv8U, 3);

            Mat imgThreshLow = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);
            Mat imgThreshHigh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            Mat imgThresh = new Mat(imgOriginal.Size, DepthType.Cv8U, 1);

            CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

            //CvInvoke.InRange(imgOriginal, new ScalarArray(new MCvScalar(50, 10, 100)), new ScalarArray(new MCvScalar(150, 100, 255)), imgThresh);

            //CvInvoke.CvtColor(color, 0, ColorConversion.Bgr2Hsv);
            //color.
            if (trackedColorMin.Hue < trackedColorMax.Hue)
            {
                CvInvoke.InRange(imgHSV, new ScalarArray(trackedColorMin.MCvScalar), new ScalarArray(trackedColorMax.MCvScalar), imgThresh);
            }
            else
            {
                CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(0, trackedColorMin.Satuation, trackedColorMin.Value)),
                    new ScalarArray(trackedColorMax.MCvScalar), imgThreshLow);
                CvInvoke.InRange(imgHSV, new ScalarArray(trackedColorMin.MCvScalar), new ScalarArray(new MCvScalar(179, trackedColorMax.Satuation, trackedColorMax.Value)), imgThreshHigh);
                CvInvoke.Add(imgThreshLow, imgThreshHigh, imgThresh);
            }
            //CvInvoke.InRange(imgHSV, new ScalarArray(new MCvScalar(165, 155, 155)), new ScalarArray(new MCvScalar(179, 255, 255)), imgThresh);

            CvInvoke.GaussianBlur(imgThresh, imgThresh, new Size(3, 3), 0);

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));

            CvInvoke.Dilate(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            CvInvoke.Erode(imgThresh, imgThresh, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

            //CvBlobs blobs = new CvBlobs();
            //CvBlobDetector detector = new CvBlobDetector();

            //detector.Detect(imgThresh.ToImage<Gray, Byte>(), blobs);
            //blobs.FilterByArea(1000, 50000);

            //foreach (var b in blobs)
            //{
            //    CvInvoke.Rectangle(imgOriginal, b.Value.BoundingBox, new MCvScalar(255, 255, 0), 2);
            //}
            CircleF[] circles = CvInvoke.HoughCircles(imgThresh, HoughType.Gradient, 2.0, imgThresh.Rows / 4, 100, 50, 10, 400);

            foreach (CircleF circle in circles)
            {
                //if (txtXYRadius.Text != "")
                //{                         // if we are not on the first line in the text box
                //    txtXYRadius.AppendText(Environment.NewLine);         // then insert a new line char
                //}

                //txtXYRadius.AppendText("ball position x = " + circle.Center.X.ToString().PadLeft(4) + ", y = " + circle.Center.Y.ToString().PadLeft(4) + ", radius = " + circle.Radius.ToString("###.000").PadLeft(7));
                //txtXYRadius.ScrollToCaret();             // scroll down in text box so most recent line added (at the bottom) will be shown

                CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), (int)circle.Radius, new MCvScalar(0, 0, 255), 2);
                CvInvoke.Circle(imgOriginal, new Point((int)circle.Center.X, (int)circle.Center.Y), 3, new MCvScalar(0, 255, 0), -1);
                break;
            }

            ibOriginal.Image = imgOriginal;
            ibCanny.Image = imgThresh;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(defInputFile))
                OpenFile(defInputFile);
        }

        private void ibOriginal_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            if (me == null || (ModifierKeys & Keys.Alt)==0)                 
                return;
            int x, y;
            ConvertCoordinates(ibOriginal, out x, out y, me.X, me.Y);

            SetTrackedColor(ibOriginal.DisplayedImage.Bitmap.GetPixel(x, y));            
        }

        //private void SetTrackedColor(Color col)
        //{
        //    trackedColor = col;
        //    trackedColorMin = Color.FromArgb(255, Math.Max(0, trackedColor.R - ColSens), Math.Max(0, trackedColor.G - ColSens), Math.Max(0, trackedColor.B - ColSens));
        //    trackedColorMax = Color.FromArgb(255, Math.Min(255, trackedColor.R + ColSens), Math.Min(255, trackedColor.G + ColSens), Math.Min(255, trackedColor.B + ColSens));

        //    lblTrackedColor.BackColor = trackedColor;
        //}

        private void SetTrackedColor(Color col)
        {

            trackedColor = col;
            Sensitivity.Hue = ColSens;

            trackedColorMin = new Hsv(trackedColor.GetHue() / 2, trackedColor.GetSaturation() * 255, trackedColor.GetBrightness() * 255);
            trackedColorMax = trackedColorMin;

            trackedColorMin.Hue -= Sensitivity.Hue;
            if (trackedColorMin.Hue < 0)
                trackedColorMin.Hue += 180;

            trackedColorMin.Satuation -= Sensitivity.Satuation;
            if (trackedColorMin.Satuation < 0)
                trackedColorMin.Satuation = 0;
            trackedColorMin.Value -= Sensitivity.Value;
            if (trackedColorMin.Value < 0)
                trackedColorMin.Value = 0;


            trackedColorMax.Hue += Sensitivity.Hue;
            if (trackedColorMax.Hue > 180)
                trackedColorMax.Hue -= 180;

            trackedColorMax.Satuation += Sensitivity.Satuation;
            if (trackedColorMax.Satuation > 255)
                trackedColorMax.Satuation = 255;
            trackedColorMax.Value += Sensitivity.Value;
            if (trackedColorMax.Value > 255)
                trackedColorMax.Value = 255;

            //trackedColorMin.Satuation = 120;
            //trackedColorMin.Value = 120;
            //trackedColorMax.Satuation = 255;
            //trackedColorMax.Value = 255;


            lblTrackedColor.BackColor = trackedColor;

            ProcessFrameBallSearch(Frame);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (VideoState == VideoState.Running)
            {
                capture.Pause();
                VideoState = VideoState.Paused;
            }
            else
            {
                capture.Start();
                VideoState = VideoState.Running;
            }
            UpdateUI();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            if (VideoState != VideoState.Stopped)
            {
                capture.Stop();
                VideoState = VideoState.Stopped;
            }
            UpdateUI();
        }

        private void toolStripButtonRew_Click(object sender, EventArgs e)
        {
            //capture.SetCaptureProperty(CapProp.PosFrames, 0);
            ActFrame = 0;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DialogResult drChosenFile;
            if (capture != null)
                capture.Stop();

            drChosenFile = ofdOpenFile.ShowDialog();

            if (drChosenFile != DialogResult.OK || ofdOpenFile.FileName == "")
            {
                //lblTrackedColor.Text = "file not chosen";
                return;
            }

            OpenFile(ofdOpenFile.FileName);
            UpdateUI();
        }

        // Convert the coordinates for the image's SizeMode.
        private void ConvertCoordinates(PictureBox pic,
            out int X0, out int Y0, int x, int y)
        {
            int pic_hgt = pic.ClientSize.Height;
            int pic_wid = pic.ClientSize.Width;
            int img_hgt = pic.Image.Height;
            int img_wid = pic.Image.Width;

            X0 = x;
            Y0 = y;
            switch (pic.SizeMode)
            {
                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                    // These are okay. Leave them alone.
                    break;
                case PictureBoxSizeMode.CenterImage:
                    X0 = x - (pic_wid - img_wid) / 2;
                    Y0 = y - (pic_hgt - img_hgt) / 2;
                    break;
                case PictureBoxSizeMode.StretchImage:
                    X0 = (int)(img_wid * x / (float)pic_wid);
                    Y0 = (int)(img_hgt * y / (float)pic_hgt);
                    break;
                case PictureBoxSizeMode.Zoom:
                    float pic_aspect = pic_wid / (float)pic_hgt;
                    float img_aspect = img_wid / (float)img_hgt;
                    if (pic_aspect > img_aspect)
                    {
                        // The PictureBox is wider/shorter than the image.
                        Y0 = (int)(img_hgt * y / (float)pic_hgt);

                        // The image fills the height of the PictureBox.
                        // Get its width.
                        float scaled_width = img_wid * pic_hgt / img_hgt;
                        float dx = (pic_wid - scaled_width) / 2;
                        X0 = (int)((x - dx) * img_hgt / (float)pic_hgt);
                    }
                    else
                    {
                        // The PictureBox is taller/thinner than the image.
                        X0 = (int)(img_wid * x / (float)pic_wid);

                        // The image fills the height of the PictureBox.
                        // Get its height.
                        float scaled_height = img_hgt * pic_wid / img_wid;
                        float dy = (pic_hgt - scaled_height) / 2;
                        Y0 = (int)((y - dy) * img_wid / pic_wid);
                    }
                    break;
            }
        }
    }
}
