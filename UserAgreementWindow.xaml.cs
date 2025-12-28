using System.Windows;

namespace RelaxTimerApp
{
    public partial class UserAgreementWindow : Window
    {
        private bool _isFirstRun;

        public UserAgreementWindow(bool isFirstRun = false)
        {
            InitializeComponent();
            _isFirstRun = isFirstRun;
            
            if (_isFirstRun)
            {
                AcceptButton.Visibility = Visibility.Visible;
                DeclineButton.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AcceptButton.Visibility = Visibility.Collapsed;
                DeclineButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Visible;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.AgreementAccepted = true;
            AppSettings.Instance.Save();
            DialogResult = true;
            Close();
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
