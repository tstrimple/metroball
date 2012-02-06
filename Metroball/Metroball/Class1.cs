using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Metroball
{
    public interface IImageUriProvider
    {
        Uri GetNextImageUri();
    }

    public delegate void NewImageHandler(BitmapImage image);

    public class ImageLoader
    {
        private BitmapImage _loadingImage;
        private Queue<BitmapImage> _pendingImages;
        private IImageUriProvider _imageUriProvider;

        public ImageLoader(IImageUriProvider imageUriProvider)
        {
            _pendingImages = new Queue<BitmapImage>();
            _imageUriProvider = imageUriProvider;


        }

        private void StartLoadImage()
        {
            _loadingImage = new BitmapImage();
            _loadingImage.UriSource = _imageUriProvider.GetNextImageUri();
            _loadingImage.ImageOpened += (sender, args) => _pendingImages.Enqueue((BitmapImage) sender);
        }

        public void GetNextImage(NewImageHandler handler)
        {
            if(_pendingImages.Count > 0)
            {
                handler.Invoke(_pendingImages.Dequeue());
            }
        }
    }
}
