using System.Windows;
using System.Windows.Input;

namespace RelaxTimerApp
{
    public partial class UserAgreementWindow : Window
    {
        private readonly bool _isFirstRun;

        public UserAgreementWindow(bool isFirstRun = false)
        {
            InitializeComponent();
            _isFirstRun = isFirstRun;
            
            // Show/hide buttons based on mode
            Loaded += UserAgreementWindow_Loaded;
        }

        private void UserAgreementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstRun)
            {
                // First run: show Accept and Decline buttons
                AcceptButton.Visibility = Visibility.Visible;
                DeclineButton.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Subsequent views: show only Close button
                AcceptButton.Visibility = Visibility.Collapsed;
                DeclineButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

