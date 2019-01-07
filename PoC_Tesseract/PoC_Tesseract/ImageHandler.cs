using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace PoC_Tesseract
{
    public class ImageHandler
    {
        private static bool again = true;
        private VideoDeviceHandler videoDeviceHandler = new VideoDeviceHandler();
        public ImageHandler()
        {
            Start();
        }
        private void Start()
        {
            while (again)
            {
                Console.Clear();
                DecideImageProcessing();
            }
        }

        public async void DecideImageProcessing()
        {
            Console.WriteLine("Press 0 for image processing using camera or 1-9 for stored images");
            char userInput = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");
            if (userInput == '0')
            {
                videoDeviceHandler.FindCapturingDevice();
            }
            else if (char.IsDigit(userInput))
            {
                try
                {
                    string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\images\" + userInput + ".jpg";

                    if (File.Exists(path))
                    {
                        ReadImage(path);
                    }
                    else
                    {
                        Console.WriteLine("\nInvalid file path");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            else
            {
                again = false;
            }
        }

       

        public static void ReadImage(string path)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(path))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            Console.WriteLine("Text (GetText): \r\n{0}", text);
                            Console.WriteLine("Text (iterator):");
                            using (var iter = page.GetIterator())
                            {
                                iter.Begin();

                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                            do
                                            {
                                                if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                                {
                                                    Console.WriteLine("<BLOCK>");
                                                }

                                                Console.Write(iter.GetText(PageIteratorLevel.Word));
                                                Console.Write(" ");

                                                if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                                {
                                                    Console.WriteLine();
                                                }
                                            } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                            if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                            {
                                                Console.WriteLine();
                                            }
                                        } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                    } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                                } while (iter.Next(PageIteratorLevel.Block));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }


}
