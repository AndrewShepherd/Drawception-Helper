using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageProcessor
{
    public class ColorToggleViewModel : NotifyPropertyChangedImpl<ColorToggleViewModel, MainThreadNotificationDispatcher>
    {
        private readonly Color _color;
        private readonly ImageData _imageData;
        internal ColorToggleViewModel(Color color, ImageData imageData)
        {
            _color = color;
            _imageData = imageData;
        }

        public Color Color
        {
            get
            {
                return _color;
            }
        }

        public bool IsActivated
        {
            get
            {
                return _imageData.AllowableColors.Contains(_color);
            }
            set
            {
                if (value)
                {
                    EnsureColorIsIncluded();
                }
                else
                {
                    EnsureColorIsExcluded();
                }
            }
        }

        private void EnsureColorIsExcluded()
        {
            if (_imageData.AllowableColors.Contains(_color))
            {
                _imageData.AllowableColors.Remove(_color);
                Notify(p => p.IsActivated);
            }
        }

        private void EnsureColorIsIncluded()
        {
            if (!_imageData.AllowableColors.Contains(_color))
            {
                _imageData.AllowableColors.Add(_color);
                Notify(p => p.IsActivated);
            }
        }

    }
}
