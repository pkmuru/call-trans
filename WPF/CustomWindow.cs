using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MeetingTranscriptionApp
{
    public class CustomWindow
    {
        // Win32 constants
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_SIZE = 0xF000;
        private const int SC_MOVE = 0xF010;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_RESTORE = 0xF120;
        
        // Win32 functions
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        // Helper methods for window operations
        public static void Minimize(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MINIMIZE, IntPtr.Zero);
        }
        
        public static void Maximize(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);
        }
        
        public static void Restore(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
        }
        
        public static void DragMove(Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            ReleaseCapture();
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)SC_MOVE, IntPtr.Zero);
        }
        
        // Apply custom window chrome
        public static void ApplyCustomChrome(Window window)
        {
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.CanResizeWithGrip;
            window.AllowsTransparency = true;
            window.Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}

