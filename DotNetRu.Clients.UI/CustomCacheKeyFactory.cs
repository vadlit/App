namespace DotNetRu.Clients.UI
{
    using DotNetRu.DataStore.Audit;

    using FFImageLoading.Forms;

    public class CustomCacheKeyFactory : ICacheKeyFactory
    {
        public string GetKey(Xamarin.Forms.ImageSource imageSource, object bindingContext)
        {
            if (!(imageSource is CustomStreamImageSource keySource))
            {
                return null;
            }

            return keySource.Key;
        }
    }
}
