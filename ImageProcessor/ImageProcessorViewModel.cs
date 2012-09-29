using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace ImageProcessor
{
    public class ImageProcessorViewModel : NotifyPropertyChangedImpl<ImageProcessorViewModel, MainThreadNotificationDispatcher>
    {
        private readonly ImageData _imageData;
        private readonly ProcessingTaskMonitor _processingTaskMonitor;
        private readonly SourceImage _sourceImage;



        private Color[] AllowableColors = {
                                              Color.FromRgb(68, 68, 68),
                                              Color.FromRgb(0, 0, 0),
                                              Color.FromRgb(153, 153, 153),
                                              Color.FromRgb(255, 255, 255),
                                              Color.FromRgb(96, 57, 19),
                                              Color.FromRgb(198, 156, 109),
                                              Color.FromRgb(255, 218, 185), // Peach
                                              Color.FromRgb(255, 0, 0), // Red
                                              Color.FromRgb(255, 215, 0), // Yellow
                                              Color.FromRgb(255, 102, 0), // Orange
                                              Color.FromRgb(22, 255, 0), // Light Green
                                              Color.FromRgb(15, 173, 0), // Green
                                              Color.FromRgb(0, 255, 255), // Light blue
                                              Color.FromRgb(2, 71, 254), // Blue
                                              Color.FromRgb(236, 0, 140), // Pink
                                              Color.FromRgb(134, 1, 175) // Purple
                                          };

        public ImageProcessorViewModel(ImageData imageData, ProcessingTaskMonitor processingTaskMonitor, SourceImage sourceImage)
        {
            this._imageData = imageData;
            this._sourceImage = sourceImage;
            this._processingTaskMonitor = processingTaskMonitor;
            this._processingTaskMonitor.PropertyChanged += ProcessingTaskMonitor_PropertyChanged;


            this._imageData.PropertyChanged += ImageData_PropertyChanged;
            this._sourceImage.PropertyChanged += _sourceImage_PropertyChanged;


        }




        void _sourceImage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Notify(p => p.GranularityDescription);
            Notify(p => p.SliderMaximum);
        }


        void ImageData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayImage")
            {
                this.Notify(p => p.DisplayImage);
            }
        }

        void ProcessingTaskMonitor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Notify(p => p.ProcessingTasks);   
        }

        public IEnumerable<ProcessingTask> ProcessingTasks
        {
            get
            {
                return this._processingTaskMonitor.Tasks;
            }
        }


        private List<ColorToggleViewModel> _colorToggles = null;
        public IEnumerable<ColorToggleViewModel> ColorToggles
        {
            get
            {
                if (_colorToggles == null)
                {
                    _colorToggles = new List<ColorToggleViewModel>();
                    foreach (var color in AllowableColors)
                    {
                        var vm = new ColorToggleViewModel(color, _imageData);
                        vm.PropertyChanged += ColorToggle_PropertyChanged;
                        _colorToggles.Add(vm);
                    }
                }
                return _colorToggles;
            }
        }

        void ColorToggle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this._imageData.ResetDisplayImage();
            Notify(p => p.DisplayImage);
        }






        public BitmapSource DisplayImage
        {
            get
            {
                return _imageData.DisplayImage;
            }
        }








        public string GranularityDescription
        {
            get
            {
                if (this._imageData.SourceImage == null)
                {
                    return "No image set";
                }
                else
                {
                    double proportion = ScaleProportion;
                    return string.Format("{0:N2} by {1:N2}", this._imageData.SourceImage.PixelWidth * proportion, this._imageData.SourceImage.PixelHeight * proportion);
                }
            }
        }



        private double ScaleProportion
        {
            get
            {
                var width = this._imageData.SourceImage.PixelWidth;
                var height = this._imageData.SourceImage.PixelHeight;
                WidthOrHeight sliderValueLinkedTo = width > height ? WidthOrHeight.Width : WidthOrHeight.Height;

                return this.SliderValueLinkedTo == WidthOrHeight.Width ? SliderValue / width : SliderValue / height;
            }
        }

        private WidthOrHeight SliderValueLinkedTo
        {
            get
            {
                if (this._imageData.SourceImage == null)
                {
                    return default(WidthOrHeight);
                }
                return this._imageData.SourceImage.PixelWidth > this._imageData.SourceImage.PixelHeight ? WidthOrHeight.Width : WidthOrHeight.Height;
            }
        }


        public double SliderMaximum
        {
            get
            {
                if (this._imageData.SourceImage == null)
                {
                    return 0.0;
                }
                else
                {
                    return (double)Math.Max(this._imageData.SourceImage.PixelWidth, this._imageData.SourceImage.PixelHeight);
                }
            }
        }

        private enum WidthOrHeight { Width, Height };



        private double _sliderValue = 0.0;

        public double SliderValue
        {
            get
            {
                return _sliderValue;
            }
            set
            {
                if (_sliderValue != value)
                {
                    _sliderValue = value;
                    this._imageData.ScaleProportion = this.ScaleProportion;
                    Notify(p => p.SliderValue);
                    Notify(p => p.GranularityDescription);
                    Notify(p => p.DisplayImage);
                }
            }
        }

        private class ExtractedImageData
        {
            public bool Success;
            public byte[] ImageData;
        }

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
//                        webClient.DownloadFileAsync(uri, fileName);
                    }
                //    using (FileStream fs = File.OpenRead(fileName))
                //    {
                //        rv.ImageData = new byte[fs.Length];
                //        fs.Read(rv.ImageData, 0, (int)fs.Length);
                //    }
                //    File.Delete(fileName);
                //    rv.Success = true;
                //
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

        private long? _panelId;

        private async Task<ExtractedImageData> TryExtractImageDataFromHtml(IDataObject data)
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
                if (!this._panelId.HasValue)
                {
                    this._panelId = this._processingTaskMonitor.OpenPanel();
                }
                this._processingTaskMonitor.SetPanelText(this._panelId.Value, "Uploading image from web...");
                var imageData = await TryExtractImageDataFromUri(imageUri);
                this._processingTaskMonitor.SetPanelText(this._panelId.Value, "Uploaded");
                return imageData;
            }
            else
            {
                return new ExtractedImageData { Success = false };
            }

        }



        internal async Task LoadImageData(IDataObject dataObject)
        {

            var imageFromHtml = await TryExtractImageDataFromHtml(dataObject);

            if (imageFromHtml.Success)
            {
                this._sourceImage.ImageBinary = imageFromHtml.ImageData;
                return;
            }

            if (dataObject.GetFormats().Contains("FileDrop"))
            {
                var filePaths = (string[])dataObject.GetData("FileDrop");
                using (var fileStream = File.OpenRead(filePaths[0]))
                {
                    var buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, (int)fileStream.Length);
                    this._sourceImage.ImageBinary = buffer;
                }
            }   
        }
    }
}
