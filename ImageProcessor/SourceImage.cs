using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageProcessor
{
    public class SourceImage : NotifyPropertyChangedImpl<SourceImage, ImmediateNotificationDispatcher>
    {
        private readonly Dispatcher _mainThreadDispatcher;
        private readonly ProcessingTaskMonitor _processingTaskMonitor;
        public SourceImage(ProcessingTaskMonitor processingTaskMonitor)
        {
            _mainThreadDispatcher = Dispatcher.CurrentDispatcher;
            string imageUriString = "pack://application:,,,/angry face.jpg";
            try
            {
                var resourceStream = App.GetResourceStream(new Uri(imageUriString));
                var length = resourceStream.Stream.Length;
                BinaryReader binaryReader = new BinaryReader(resourceStream.Stream);
                this.ImageBinary = binaryReader.ReadBytes((int)length);
            }
            catch
            {
            }
        }

        private BitmapSource _sourceImage;
        public BitmapSource Image
        {
            get
            {
                return _sourceImage;
            }
            private set
            {
                _sourceImage = value;
                Notify(p => p.Image);
            }
        }

        internal byte[] ImageBinary 
        {
            get
            {
                return _sourceImageBinary;
            }
            set
            {
                _sourceImageBinary = value;
                Action act = () =>
                {
                    BitmapImage sourceImage = new BitmapImage();
                    using (MemoryStream ms = new MemoryStream(_sourceImageBinary))
                    {
                        sourceImage.BeginInit();
                        sourceImage.CacheOption = BitmapCacheOption.OnLoad;
                        sourceImage.StreamSource = ms;
                        sourceImage.EndInit();
                    }
                    this.Image = sourceImage;
                };
                this._mainThreadDispatcher.BeginInvoke(act, null);
                
            }
        }
        private byte[] _sourceImageBinary = null;
    }
}
