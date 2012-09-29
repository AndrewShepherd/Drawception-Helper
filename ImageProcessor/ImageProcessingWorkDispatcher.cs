using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{
    class ImageProcessingWorkDispatcher
    {
        AutoResetEvent _imageProcessingJobAvailable = new AutoResetEvent(false);
        AutoResetEvent _bitmapSourceAvailable = new AutoResetEvent(false);

        ImageProcessingJob _imageProcessingJob = default(ImageProcessingJob);


        public void PushJob(ImageProcessingJob imageProcessingJob)
        {
            _imageProcessingJob = imageProcessingJob;
            _imageProcessingJobAvailable.Set();
        }

        public ImageProcessingJob GetJob()
        {
            _imageProcessingJobAvailable.WaitOne();
            return _imageProcessingJob;
        }
    }
}
