using System.Windows;
using System.Windows.Controls;

namespace RelaxTimerApp
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string languageCode)
            {
                LocalizationHelper.Instance.SetLanguage(languageCode);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
