using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace RelaxTimerApp
{
    public partial class ReportContentWindow : Window
    {
        public ReportContentWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SaveReport_Click(object sender, RoutedEventArgs e)
        {
            var reportText = ReportTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(reportText))
            {
                MessageBox.Show(
                    LocalizationHelper.Instance["ReportEmptyMessage"],
                    LocalizationHelper.Instance["ReportEmptyTitle"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"AI_Content_Report_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var content = $"AI Content Report\n" +
                                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                $"Application: VRelaxTimer\n" +
                                $"Language: {LocalizationHelper.Instance.CurrentLanguage}\n" +
                                $"\n{new string('-', 50)}\n\n" +
                                $"{reportText}\n\n" +
                                $"{new string('-', 50)}\n" +
                                $"End of Report";

                    File.WriteAllText(saveDialog.FileName, content);

                    MessageBox.Show(
                        LocalizationHelper.Instance["ReportSavedMessage"],
                        LocalizationHelper.Instance["ReportSavedTitle"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ReportTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"{LocalizationHelper.Instance["ReportSaveError"]}\n\n{ex.Message}",
                        LocalizationHelper.Instance["Error"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

