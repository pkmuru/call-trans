using System;
using System.Windows;

namespace MeetingTranscriptionApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"An unhandled exception occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };
            
            // Set up UI exception handling
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"An unhandled UI exception occurred: {args.Exception.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}

