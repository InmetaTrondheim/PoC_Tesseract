using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PoC_Tesseract
{
    public class VideoDeviceHandler
    {
        private static FilterInfoCollection CaptureDevice;
        private static VideoCaptureDevice FinalFrame;

        private static Stopwatch stopwatch = new Stopwatch();
        private static int selectedDevice;

        public VideoDeviceHandler()
        {

        }

        public void FindCapturingDevice()
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);//constructor
            if (CaptureDevice.Count > 0)
            {
                FinalFrame = new VideoCaptureDevice();
                SelectCapturingDevice();
            }
            else
            {
                Console.WriteLine("You do not have a device for video capture");
            }
        }

        public void SelectCapturingDevice()
        {
            if (CaptureDevice.Count == 1)
            {
                selectedDevice = 1;
                Console.WriteLine("\nUsing " + CaptureDevice[0].Name + "for capturing image");
                SaveImage();
            }
            else
            {
                Console.WriteLine("Please select your desired camera");
                int count = 1;
                foreach (FilterInfo cam in CaptureDevice)
                {
                    Console.WriteLine($"{count}. {cam.Name}");
                    count++;
                }


                char userInput = Console.ReadKey().KeyChar;

                if (char.IsDigit(userInput))
                {
                    selectedDevice = (int)char.GetNumericValue(userInput);
                    SaveImage();
                }
            }
        }
        private void SaveImage()
        {
            FinalFrame = new VideoCaptureDevice(CaptureDevice[selectedDevice - 1].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
        }

        private async void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var image = (Bitmap)eventArgs.Frame.Clone();
            Bitmap varBmp = new Bitmap(image);

            varBmp.Save(GetNextFileName(), ImageFormat.Png);
            Console.WriteLine($"\nSaving photo");
            varBmp.Dispose();
            varBmp = null;
            stopwatch.Restart();
            var dir = GetDirectory();
            DirectoryInfo directory = new DirectoryInfo(dir);
            var filePairs = directory
                .GetFiles("*.png")
                .OrderByDescending(f => f.CreationTime);
            ImageHandler.ReadImage(filePairs.First().FullName);
            FinalFrame.SignalToStop();
        }

        private static string GetNextFileName()
        {
            string path = GetDirectory();
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
            {
                var directory = Directory.CreateDirectory(path);
            }
            int fCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;

            var imgPath = path + @"\capture" + fCount + ".png";
            return imgPath;
        }
        private static string GetDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\images";
        }
    }
}
