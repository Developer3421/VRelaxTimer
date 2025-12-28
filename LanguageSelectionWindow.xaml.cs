using System.Windows;
using System.Windows.Input;

namespace RelaxTimerApp
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void English_Click(object sender, RoutedEventArgs e)
        {
            LocalizationHelper.Instance.CurrentLanguage = "en";
            Close();
        }

        private void Ukrainian_Click(object sender, RoutedEventArgs e)
        {
            LocalizationHelper.Instance.CurrentLanguage = "uk";
            Close();
        }

        private void German_Click(object sender, RoutedEventArgs e)
        {
            LocalizationHelper.Instance.CurrentLanguage = "de";
            Close();
        }

        private void Turkish_Click(object sender, RoutedEventArgs e)
        {
            LocalizationHelper.Instance.CurrentLanguage = "tr";
            Close();
        }

        private void Russian_Click(object sender, RoutedEventArgs e)
        {
            LocalizationHelper.Instance.CurrentLanguage = "ru";
            Close();
        }
    }
}

