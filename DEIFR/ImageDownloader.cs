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
                client.DownloadStringAsync(new Uri("http://www.reddit.com/r/EarthPorn/.json?limit=" + Program.MaxImages));
                JObject obj = JObject.Parse(response);
                data = obj["data"]["children"].Children().ToList();
                i = 0;
                files = Directory.GetFiles("wallpapers").ToList();
                enumerator = data.GetEnumerator();
            }
            Task.Run(() =>
            {
                while (!Next())
                    ;
            });
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
            while (!Next())
                ;
        }

        private static bool Next()
        {
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
                //Image.FromStream(respstr).Save(i + "." + s.Split('/').Last().Split('.')[1]);
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
    }
}
