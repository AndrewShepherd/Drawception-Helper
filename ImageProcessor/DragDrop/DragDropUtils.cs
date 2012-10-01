using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ImageProcessor.DragDrop
{

    internal class ExtractedImageData
    {
        public bool Success;
        public byte[] ImageData;
    }

    static class DragDropUtils
    {
        private static async Task<ExtractedImageData> TryExtractImageDataFromUri(Uri uri)
        {
            ExtractedImageData rv = new ExtractedImageData { Success = false };
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    int threadIdBefore = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    rv.ImageData = await webClient.DownloadDataTaskAsync(uri.ToString());
                    int threadIdAfter = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    rv.Success = true;
                }
            }
            catch
            {
                rv.Success = false;
            }
            return rv;
        }

        private static bool TryExtractImageUri(string fragment, out Uri imageUri)
        {
            try
            {
                XmlReader rdr = new XmlTextReader(fragment, XmlNodeType.Element, null);
                rdr.Read();
                if (rdr.Name != "img")
                {
                    if (!rdr.ReadToDescendant("img"))
                    {
                        imageUri = null;
                        return false;
                    }
                }
                string attribute = rdr.GetAttribute("src");
                if (!string.IsNullOrEmpty(attribute))
                {
                    imageUri = new Uri(attribute);
                    return true;
                }
                else
                {
                    imageUri = default(Uri);
                    return false;
                }
            }
            catch (Exception ex)
            {
                imageUri = default(Uri);
                return false;
            }
        }



        internal static async Task<ExtractedImageData> TryExtractImageDataFromHtml(IDataObject data, ProcessingTaskMonitor processingTaskMonitor, long taskMonitorPanel)
        {
            string[] formats = data.GetFormats();
            if (!formats.Contains("text/html"))
            {
                return new ExtractedImageData { Success = false };
            }
            var obj = data.GetData("text/html");
            string html = string.Empty;
            if (obj is string)
            {
                html = (string)obj;
            }
            else if (obj is MemoryStream)
            {
                MemoryStream ms = (MemoryStream)obj;
                byte[] buffer = new byte[ms.Length];
                ms.Read(buffer, 0, (int)ms.Length);
                if (buffer[1] == (byte)0)  // Detecting unicode
                {
                    html = System.Text.Encoding.Unicode.GetString(buffer);
                }
                else
                {
                    html = System.Text.Encoding.ASCII.GetString(buffer);
                }
            }
            Uri imageUri;
            if (TryExtractImageUri(html, out imageUri))
            {
                processingTaskMonitor.SetPanelText(taskMonitorPanel, "Uploading image from web...");
                var imageData = await TryExtractImageDataFromUri(imageUri);
                processingTaskMonitor.SetPanelText(taskMonitorPanel, "Uploaded");
                return imageData;
            }
            else
            {
                return new ExtractedImageData { Success = false };
            }
        }

    }
}
