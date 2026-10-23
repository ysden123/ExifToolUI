using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExifToolUI
{
    /// <summary>
    /// Interaction logic for ShowOnGoogleMapsControl.xaml
    /// </summary>
    public partial class ShowOnGoogleMapsControl : UserControl
    {
        public ShowOnGoogleMapsControl()
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
                ShowButton.IsEnabled = true;
            }
        }

        private async void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            ShowButton.IsEnabled = false;

            try
            {
                var exifToolResult = await ExtractGpsfDataAsync(FilePathText.Text);
                if (!string.IsNullOrEmpty(exifToolResult.Error))
                {
                    MessageBox.Show($"Error: {exifToolResult.Error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (exifToolResult.Tags.Count != 2)
                {
                    MessageBox.Show("Error: no GPS data found", "Error", MessageBoxButton.OK);
                    return;
                }

                string latitude = exifToolResult.Tags.Find(tag => tag.Key == "GPS Latitude").Value;
                string longitude = exifToolResult.Tags.Find(tag => tag.Key == "GPS Longitude").Value;

                if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
                {
                    MessageBox.Show("Error: GPS data is incomplete", "Error", MessageBoxButton.OK);
                    return;
                }
                
                string url = $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            finally
            {
                ShowButton.IsEnabled = FilePathText.Text.Length > 0;
            }

        }

        private void FilePathTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowButton.IsEnabled = FilePathText.Text.Length > 0;
        }

        private static async Task<ExifToolResult> ExtractGpsfDataAsync(string? filePath)
        {
            return await Task.Run(async () =>
            {
                // Configure the process start information
                var startInfo = new ProcessStartInfo
                {
                    FileName = "exiftool",          // The executable to run (e.g., cmd.exe, powershell.exe, or any custom app)
                    Arguments = $"-n -GPSLatitude -GPSLongitude \"{filePath}\"",  // Arguments to pass to the command
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
    }
}
