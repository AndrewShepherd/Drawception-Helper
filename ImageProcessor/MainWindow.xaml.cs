using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ImageProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();




        }






        private ImageProcessorViewModel ImageProcessorViewModel
        {
            get
            {
                return (ImageProcessorViewModel)this.DataContext;
            }
        }



        private async void Window_Drop(object sender, DragEventArgs e)
        {
            ImageProcessorViewModel viewModel = this.ImageProcessorViewModel;
            await viewModel.LoadImageData(e.Data);
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {

        }



    }
}
