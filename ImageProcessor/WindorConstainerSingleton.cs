using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace ImageProcessor
{
    public static class WindsorContainerSingleton
    {
        private static WindsorContainer _windsorContainer;



        internal static void Initialise()
        {
            if(_windsorContainer == null)
            {
                _windsorContainer = new WindsorContainer();
                _windsorContainer.Register(Component.For<ImageProcessorViewModel>(),
                                           Component.For<ImageData>(),
                                           Component.For<ProcessingTaskMonitor>(),
                                           Component.For<SourceImage>()
                                           );
            }
        }

        internal static void Uninitialise()
        {
            if(_windsorContainer != null)
            {
                _windsorContainer.Dispose();
                _windsorContainer = null;
            }
        }

        internal static WindsorContainer Instance
        {
            get
            {
                if (_windsorContainer == null)
                {
                    Initialise();
                }
                return _windsorContainer;
            }
        }


        public static ImageProcessorViewModel ImageProcessorViewModel
        {
            get
            {
                return Instance.Resolve<ImageProcessorViewModel>();
            }
        }
   

    }
}
