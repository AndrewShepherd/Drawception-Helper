using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageProcessor
{
    public class ImageData : NotifyPropertyChangedImpl<ImageData, ImmediateNotificationDispatcher>
    {
        private readonly SourceImage _sourceImage;
        private readonly Dispatcher _mainThreadDispatcher;
        private readonly ProcessingTaskMonitor _processingTaskMonitor;
        public ImageData(SourceImage sourceImage, ProcessingTaskMonitor processingTaskMonitor)
        {
            _mainThreadDispatcher = Dispatcher.CurrentDispatcher;
            _sourceImage = sourceImage;
            _sourceImage.PropertyChanged += SourceImage_PropertyChanged;
            _processingTaskMonitor = processingTaskMonitor;
            RunTheWorkerThread();

            ResetDisplayImage();
        }

        void SourceImage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResetDisplayImage();
        }


        ImageProcessingWorkDispatcher _imageProcessingWorkDispatcher = new ImageProcessingWorkDispatcher();


        long? _panelId = null;
        async Task<ImageProcessingResults> GetJobAndProcessIt()
        {
            if (!_panelId.HasValue)
            {
                _panelId = this._processingTaskMonitor.OpenPanel();
            }
            var processingResults = await Task.Run(() =>
            {
                this._processingTaskMonitor.SetPanelText(_panelId.Value, "Waiting for processing job");
                var processingJob = this._imageProcessingWorkDispatcher.GetJob();
                this._processingTaskMonitor.SetPanelText(_panelId.Value, "Performing processing job");
                return ImageProcessingFunctions.Process(processingJob);
            });
            return processingResults;
        }

        async void RunTheWorkerThread()
        {
            Task<ImageProcessingResults> imageProcessingTask = GetJobAndProcessIt(); 
            while (true)
            {
                ImageProcessingResults processingResults = await (imageProcessingTask);
                imageProcessingTask = GetJobAndProcessIt();
                BitmapSource bitmapSource = ImageProcessingFunctions.CreateBitmap(processingResults);
                this.DisplayImage = bitmapSource;
            }
        }




        public BitmapSource SourceImage
        {
            get
            {

                return _sourceImage.Image;
            }
        }

        private BitmapSource _displayImage;
        public BitmapSource DisplayImage
        {
            get
            {
                return _displayImage;
            }
            private set
            {
                _displayImage = value;
                Notify(p => p.DisplayImage);
            }
        }

        

        List<Color> _allowableColors = new List<Color>();

        public List<Color> AllowableColors
        {
            get
            {
                return _allowableColors;
            }
        }
        

 

        public void ResetDisplayImage()
        {
            if (_sourceImage.Image == null)
            {
                return;
            }

            var bitmap = new FormatConvertedBitmap(_sourceImage.Image, PixelFormats.Bgra32, null, 0);
            ImageProcessingJob imageProcessingJob = new ImageProcessingJob
            {
                PixelColors = BitmapUtils.GetPixels(bitmap),
                Fidelity = this.ScaleProportion,
                AllowableColors = _allowableColors.ToList(),
                PixelWidth = bitmap.PixelWidth,
               PixelHeight = bitmap.PixelHeight,
              DpiX = bitmap.DpiX,
            DpiY = bitmap.DpiY,
            ColorMatchingAlgorithm = this.PatternMatchingAlgorithm
            };
            this._imageProcessingWorkDispatcher.PushJob(imageProcessingJob);
        }

        

        double _scaleProportion = 1.0;
        public double ScaleProportion 
        {
            get
            {
                return _scaleProportion;
            }
            set
            {
                if (_scaleProportion != value)
                {
                    _scaleProportion = value;
                    ResetDisplayImage();
                }
            }
        }

        private ColorMatchingAlgorithm _patternMatchingAlgorithm;
        public ColorMatchingAlgorithm PatternMatchingAlgorithm 
        {
            get
            {
                return _patternMatchingAlgorithm;
            }
            set
            {
                if (_patternMatchingAlgorithm != value)
                {
                    _patternMatchingAlgorithm = value;
                    Notify(_ => _.PatternMatchingAlgorithm);
                    ResetDisplayImage();
                }
            }
        }
    }
}
