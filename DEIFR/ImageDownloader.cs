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
        public static void Update()
        {
            if (!Directory.Exists("wallpapers"))
                Directory.CreateDirectory("wallpapers");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.reddit.com/r/ImagesOfEarth/.json?limit=" + Program.MaxImages + "&sort=new");
            //string senddata = "?limit=15&sort=new";
            //byte[] sendbytes = Encoding.Unicode.GetBytes(senddata);
            req.Method = "GET";
            //req.ContentLength = sendbytes.Length;
            req.ContentType = "text/plain";
            /*var sendstr = req.GetRequestStream();
            sendstr.Write(sendbytes, 0, sendbytes.Length);
            sendstr.Close();*/
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream respstr = resp.GetResponseStream();
            string response = new StreamReader(respstr).ReadToEnd();
            JObject obj = JObject.Parse(response);
            List<JToken> data = obj["data"]["children"].Children().ToList();
            int i = 0;
            List<string> files = Directory.GetFiles("wallpapers").ToList();
            foreach (JToken result in data)
            {
                string s = result["data"]["url"].ToString();
                string id = result["data"]["id"].ToString();
                if (files.Any(entry => entry.Contains(id)))
                {
                    files.RemoveAll(entry => entry.Contains(id)); //Leave only the files that wasn't processed to remove them later
                    continue; //Don't download twice
                }
                if (!s.Contains(".jpg"))
                {
                    if (s.Contains("imgur.com"))
                        s += ".jpg"; //On imgur.com it is enough to get image
                    else
                        continue; //Otherwise we don't know what is there
                }
                //Image.FromStream(respstr).Save(i + "." + s.Split('/').Last().Split('.')[1]);
                string path = id + "." + s.Split('/').Last().Split('.')[1].Split('?')[0]; //?: FB and similar sites
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(s, "wallpapers" + Path.DirectorySeparatorChar + path);
                    }
                    catch { } //If the image is removed then don't do anything
                }
                files.Remove(path);
                i++;
            }
            if (!Program.KeepImages)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
