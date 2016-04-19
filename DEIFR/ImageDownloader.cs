using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DEIFR
{
    public static class ImageDownloader
    {
        private static List<JToken> data = null;
        private static int i = 0;
        private static List<string> files = null;
        private static IEnumerator<JToken> enumerator;
        public static void Update()
        {
            if (!Directory.Exists("wallpapers"))
                Directory.CreateDirectory("wallpapers");

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += DownloadProgress;
                client.DownloadStringCompleted += DownloadedLinks;
                client.DownloadStringAsync(new Uri("http://www.reddit.com/r/EarthPorn/.json?limit=" + Program.MaxImages));
            }
        }

        private static void DownloadedLinks(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error while getting list from Reddit:\n" + e.Error);
                return;
            }
            JObject obj = JObject.Parse(e.Result);
            data = obj["data"]["children"].Children().ToList();
            i = 0;
            files = Directory.GetFiles("wallpapers").ToList();
            enumerator = data.GetEnumerator();
            Program.Form.Invoke(new Action(delegate
            {
                Program.AllProgress.Maximum = data.Count + 1;
                Program.AllProgress.Value++;
            }));
            while (!Next())
            {
                Program.Form.Invoke(new Action(delegate
                {
                    Program.AllProgress.Value++;
                }));
            }
        }

        private static void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Program.Form.Invoke(new Action(delegate
            {
                Program.Progress.Value = e.ProgressPercentage;
            }));
        }

        private static void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Program.Form.Invoke(new Action(delegate
            {
                Program.Progress.Value = e.ProgressPercentage;
            }));
        }

        private static void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (MessageBox.Show("Error while downloading imgae:\n" + e.Error, "Error", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                    return;
            }
            Program.Form.Invoke(new Action(delegate
            {
                Program.AllProgress.Value++;
            }));
            while (!Next())
            {
                Program.Form.Invoke(new Action(delegate
                {
                    Program.AllProgress.Value++;
                }));
            }
        }

        private static bool Next()
        {
            if (Stop)
            {
                Program.Form.Invoke(new Action(delegate
                {
                    Program.Form.Close();
                }));
                return true;
            }
            if (enumerator.MoveNext())
            {
                JToken result = enumerator.Current;
                string s = result["data"]["url"].ToString();
                string id = result["data"]["id"].ToString();
                if (files.Any(entry => entry.Contains(id)))
                {
                    files.RemoveAll(entry => entry.Contains(id)); //Leave only the files that wasn't processed to remove them later
                    return false; //Don't download twice
                }
                if (!s.Contains(".jpg"))
                {
                    if (s.Contains("imgur.com"))
                        s += ".jpg"; //On imgur.com it is enough to get image
                    else
                        return false; //Otherwise we don't know what is there
                }
                string path = id + "." + s.Split('/').Last().Split('.')[1].Split('?')[0]; //?: FB and similar sites
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        client.DownloadFileCompleted += Client_DownloadFileCompleted;
                        client.DownloadFileAsync(new Uri(s), "wallpapers" + Path.DirectorySeparatorChar + path);
                    }
                    catch { } //If the image is removed then don't do anything
                }
                files.Remove(path);
                i++;
            }
            else //Finish
            {
                if (!Program.KeepImages)
                {
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            return true;
        }

        public static bool Stop { get; set; }
    }
}
