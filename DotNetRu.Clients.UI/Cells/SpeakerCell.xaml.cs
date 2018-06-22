namespace DotNetRu.Clients.UI.Cells
{
    using DotNetRu.Clients.Portable.Model;
    using DotNetRu.Clients.UI.Pages.Speakers;
    using DotNetRu.DataStore.Audit.Models;

    using Xamarin.Forms;

    public class SpeakerCell : ViewCell
    {
        readonly INavigation navigation;

        public SpeakerCell(INavigation navigation = null)
        {
            this.View = new SpeakerCellView();
            this.navigation = navigation;
        }

        protected override async void OnTapped()
        {
            base.OnTapped();
            if (this.navigation == null)
            {
                return;
            }

            if (!(this.BindingContext is SpeakerModel speaker))
            {
                return;
            }

            App.Logger.TrackPage(AppPage.Speaker.ToString(), speaker.FullName);

            await this.navigation.PushAsync(new SpeakerDetailsPage { SpeakerModel = speaker });
        }
    }

    public partial class SpeakerCellView
    {
        public SpeakerCellView()
        {
            this.InitializeComponent();

            this.Image.CacheKeyFactory = new CustomCacheKeyFactory();
        }
    }
}

