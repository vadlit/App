namespace DotNetRu.Clients.UI.Pages.Speakers
{
    using System.IO;

    using DotNetRu.Clients.Portable.Model;

    using Xamarin.Forms;

    public partial class SpeakerFacePage
    {
        public SpeakerFacePage(ImageSource image)
        {
            this.InitializeComponent();
            this.faceImage.Source = image;
        }

        public override AppPage PageType => AppPage.SpeakerFace;
    }
}
