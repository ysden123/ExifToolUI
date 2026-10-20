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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fvi != null && fvi.FileVersion != null)
            {
                string version = fvi.FileVersion;
                Title = $"{Title} {version}";
            }
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void MenuItem_Test1_Click(object sender, RoutedEventArgs e)
        {
            using (var exiftool = new SharpExifTool.ExifTool())
            {
                try
                {
                    var metadataList = await exiftool.ExtractAllMetadataAsync(filename: "d:\\DaVinci Video Projects\\Музыкальная фантазия\\Звук\\Танец маленьких лебедей.mp3");
                    var count = metadataList.Count;
                    MessageBox.Show($"count={count}", "Info",  MessageBoxButton.OK, MessageBoxImage.Information);


                    for(int i =0; i < count && i < 3; i++)
                    {
                        var pair = metadataList.ElementAt(i);
                        MessageBox.Show($"pair {i}: {pair.Key} = {pair.Value}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItem_AllMetadata_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = new AllMetadataControl();
        }
    }
}