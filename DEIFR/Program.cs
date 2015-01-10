using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DEIFR
{
    static class Program
    {
        public static bool KeepImages;
        public static int MaxImages;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if(!File.Exists("Newtonsoft.Json.dll"))
            {
                MessageBox.Show("Could not find library Newtonsoft.Json.dll\nPlease download the file to use this program.");
                return;
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (File.Exists("DEIFRSettings.ini"))
            {
                string[] ss = File.ReadAllLines("DEIFRSettings.ini");
                foreach (var s in ss)
                {
                    string[] x = s.Split('=');
                    switch (x[0])
                    {
                        case "keepimages":
                            KeepImages = bool.Parse(x[1]);
                            break;
                        case "maximages":
                            MaxImages = int.Parse(x[1]);
                            break;
                    }
                }
            }
            ImageDownloader.Update();
            if (args.Length == 0)
                Application.Run(new Form1());
            else if (args[0].ToLower() != "silent")
                Console.WriteLine("Error: Unknown parameter(s). Use \"silent\" to open in background, otherwise don't give any parameters to show settings.");
            List<string> sw = new List<string>();
            sw.Add("maximages=" + MaxImages);
            sw.Add("keepimages=" + KeepImages);
            File.WriteAllLines("DEIFRSettings.ini", sw);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error!\n" + ((Exception)e.ExceptionObject).Message);
        }
    }
}
