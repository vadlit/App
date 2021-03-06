namespace DotNetRu.Clients.Portable.Helpers
{
    using System;

    using DotNetRu.Clients.Portable.ViewModel;
    using Xamarin.Essentials;

    public static class Settings
    {
        public static Language? CurrentLanguage
        {
            get
            {
                string languageCode = Preferences.Get(nameof(CurrentLanguage), null);
                return languageCode == null ? (Language?)null : GetLanguage(languageCode);
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                string languageCode = value.Value.GetLanguageCode();
                Preferences.Set(nameof(CurrentLanguage), languageCode);
            }
        }

        private static Language GetLanguage(string languageCode)
        {
            if (languageCode == "ru")
            {
                return Language.Russian;
            }

            return Language.English;
        }
    }
}
