using System;
using System.Linq;
using System.Windows;

namespace RelaxTimerApp
{
    public class LocalizationHelper
    {
        private static LocalizationHelper _instance;
        public static LocalizationHelper Instance => _instance ??= new LocalizationHelper();

        public string CurrentLanguage => AppSettings.Instance.Language;

        public void SetLanguage(string languageCode)
        {
            AppSettings.Instance.Language = languageCode;
            AppSettings.Instance.Save();
            ApplyLanguage(languageCode);
        }

        public void ApplyLanguage(string languageCode)
        {
            string dictionaryPath = languageCode == "en" 
                ? "Resources/Languages/StringResources.xaml" 
                : $"Resources/Languages/StringResources.{languageCode}.xaml";

            var dict = new ResourceDictionary();
            try
            {
                dict.Source = new Uri(dictionaryPath, UriKind.Relative);
            }
            catch (Exception)
            {
                // Fallback to English if file not found
                dict.Source = new Uri("Resources/Languages/StringResources.xaml", UriKind.Relative);
            }

            var appResources = Application.Current.Resources;
            var oldDict = appResources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Resources/Languages/"));
            
            if (oldDict != null)
            {
                appResources.MergedDictionaries.Remove(oldDict);
            }
            
            appResources.MergedDictionaries.Add(dict);
        }
    }
}
