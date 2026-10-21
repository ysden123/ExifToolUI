using System.Diagnostics;
using System.Windows;

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

        private void MenuItem_Test1_Click(object sender, RoutedEventArgs e)
        {
            // Configure the process start information
            var startInfo = new ProcessStartInfo
            {
                FileName = "exiftool",             // The executable to run (e.g., cmd.exe, powershell.exe, or any custom app)
                Arguments = "\"c:\\Users\\yuriy\\Downloads\\Classicals.de - Tchaikovsky - Swan Lake, Op. 20 - Act 1 - Introduction.mp3\"",        // Arguments to pass to the command
                RedirectStandardOutput = true,    // Capture standard output
                RedirectStandardError = true,     // Capture standard error (recommended)
                UseShellExecute = false,          // Required to redirect I/O
                CreateNoWindow = true             // Hides the console window
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

                // Display results
                MessageBox.Show($"Exit Code: {process.ExitCode}\n\nOutput:\n{output}\n\nError:\n{error}", "Process Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while executing the process:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
            }
        }

        private void MenuItem_AllMetadata_Click(object sender, RoutedEventArgs e)
        {
            CC.Content = new AllMetadataControl();
        }
    }
}