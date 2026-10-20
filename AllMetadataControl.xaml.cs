using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            MetadataTextBox.Text = string.Empty;

            var currentCursor = MetadataTextBox.Cursor;
            MetadataTextBox.Cursor = Cursors.Wait;
            ExtractButton.IsEnabled = false;

            try
            {
                var exifData = await ExtractExifDataAsync(FilePathText.Text);
                MetadataTextBox.Text = exifData.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            finally
            {
                MetadataTextBox.Cursor = currentCursor;
                ExtractButton.IsEnabled = FilePathText.Text.Length > 0 ? true : false;
            }
        }

        private static async Task<StringBuilder> ExtractExifDataAsync(string? filePath)
        {
            return await Task.Run(async () =>
            {
                var exiftool = new SharpExifTool.ExifTool();
                StringBuilder exifData = new();
                try
                {
                    var metadataList = await exiftool.ExtractAllMetadataAsync(filename: filePath);
                    var count = metadataList.Count;
                    if (count == 0)
                    {
                        MessageBox.Show($"No metadata found for file: {filePath}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return exifData;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        var pair = metadataList.ElementAt(i);
                        exifData.AppendLine($"{pair.Key}: {pair.Value}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return exifData;
            });
        }

        private void FilePathTextChanged(object sender, TextChangedEventArgs e)
        {
            ExtractButton.IsEnabled = FilePathText.Text.Length > 0 ? true : false;
        }
    }
}
