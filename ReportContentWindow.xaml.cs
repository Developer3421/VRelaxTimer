using System;
using System.IO;
using System.Windows;

namespace RelaxTimerApp
{
    public partial class ReportContentWindow : Window
    {
        public ReportContentWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string reportText = ReportTextBox.Text;
            if (string.IsNullOrWhiteSpace(reportText))
            {
                MessageBox.Show((string)FindResource("ReportEmptyMessage"), (string)FindResource("ReportEmptyTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(reportsDir);
                string filename = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(reportsDir, filename);
                
                File.WriteAllText(filePath, reportText);
                
                MessageBox.Show((string)FindResource("ReportSavedMessage"), (string)FindResource("ReportSavedTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{(string)FindResource("ReportSaveError")} {ex.Message}", (string)FindResource("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
