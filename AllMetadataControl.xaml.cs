using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExifToolUI
{
    /// <summary>
    /// Interaction logic for AllMetadataControl.xaml
    /// </summary>
    public partial class AllMetadataControl : UserControl
    {
        public AllMetadataControl()
        {
            InitializeComponent();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select a file to extract metadata from"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathText.Text = openFileDialog.FileName;
                ExtractButton.IsEnabled = true;
            }
        }

        private async void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            var currentCursor = MetadataDataGrid.Cursor;
            MetadataDataGrid.Cursor = Cursors.Wait;
            ExtractButton.IsEnabled = false;
            MetadataDataGrid.ItemsSource = null;

            try
            {
                var exifToolResult = await ExtractExifDataAsync2(FilePathText.Text);
                MetadataDataGrid.ItemsSource = exifToolResult.Tags;
                if (!string.IsNullOrEmpty(exifToolResult.Error))
                {
                    MessageBox.Show($"Error: {exifToolResult.Error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            finally
            {
                MetadataDataGrid.Cursor = currentCursor;
                ExtractButton.IsEnabled = FilePathText.Text.Length > 0;
            }
        }

        private static async Task<ExifToolResult> ExtractExifDataAsync2(string? filePath)
        {
            return await Task.Run(async () =>
            {
                // Configure the process start information
                var startInfo = new ProcessStartInfo
                {
                    FileName = "exiftool",          // The executable to run (e.g., cmd.exe, powershell.exe, or any custom app)
                    Arguments = $"\"{filePath}\"",  // Arguments to pass to the command
                    RedirectStandardOutput = true,  // Capture standard output
                    RedirectStandardError = true,   // Capture standard error (recommended)
                    UseShellExecute = false,        // Required to redirect I/O
                    CreateNoWindow = true           // Hides the console window
                };

                try
                {
                    // Start the process
                    using var process = Process.Start(startInfo);
                    // Read the standard output and error streams
                    string output = process!.StandardOutput.ReadToEnd();
                    string error = process!.StandardError.ReadToEnd();

                    // Wait for the process to exit
                    process!.WaitForExit();

                    using var reader = new StringReader(output);
                    string? line;
                    List<KeyValuePair<string, string>> tags = [];
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(":");
                        if (parts.Length > 1)
                        {
                            var tag = parts[0].Trim();
                            var value = parts[1].Trim();
                            tags.Add(new KeyValuePair<string, string>(tag, value));
                        }
                    }
                    var lineCount = tags.Count;
                    return new ExifToolResult(tags, error);
                }
                catch (Exception ex)
                {
                    return new ExifToolResult([], ex.Message);
                }
            });
        }

        private void FilePathTextChanged(object sender, TextChangedEventArgs e)
        {
            ExtractButton.IsEnabled = FilePathText.Text.Length > 0;
        }

        private void MetadataDataGridRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is KeyValuePair<string, string> tag)
            {
                Clipboard.SetText($"{tag.Key}: {tag.Value}");
                MessageBox.Show($"Copied to clipboard:\n{tag.Key}: {tag.Value}", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
