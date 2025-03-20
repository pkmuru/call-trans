using Microsoft.Web.WebView2.Core;
using System;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Threading.Tasks;
using System.Speech.Recognition;
using NAudio.Wave;
using System.Collections.Generic;

namespace MeetingTranscriptionApp
{
    public partial class MainWindow : Window
    {
        private bool isRecording = false;
        private SpeechRecognitionEngine recognizer;
        private WaveInEvent waveIn;
        private List<TranscriptionEntry> currentTranscription = new List<TranscriptionEntry>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            InitializeSpeechRecognition();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Set up WebView2 environment
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MeetingTranscriptionApp");
                
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await webView.EnsureCoreWebView2Async(env);

                // Set up event handlers
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                
                // Enable dev tools in debug mode
                #if DEBUG
                webView.CoreWebView2.OpenDevToolsWindow();
                #endif

                // Navigate to the React app
                // In production, this would be a local HTML file bundled with the app
                #if DEBUG
                webView.Source = new Uri("http://localhost:3000");
                #else
                webView.Source = new Uri($"file:///{AppDomain.CurrentDomain.BaseDirectory}wwwroot/index.html");
                #endif

                StatusText.Text = "WebView2 initialized";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "WebView2 initialization failed";
            }
        }

        private void InitializeSpeechRecognition()
        {
            try
            {
                // Create a speech recognition engine
                recognizer = new SpeechRecognitionEngine();
                
                // Configure the recognizer
                var grammar = new DictationGrammar();
                recognizer.LoadGrammar(grammar);
                
                // Set up event handlers
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.SpeechHypothesized += Recognizer_SpeechHypothesized;
                
                // Configure audio input
                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0, // Default microphone
                    WaveFormat = new WaveFormat(16000, 1) // 16kHz mono
                };
                
                waveIn.DataAvailable += WaveIn_DataAvailable;
                
                StatusText.Text = "Speech recognition initialized";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize speech recognition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Speech recognition initialization failed";
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recognizer != null && isRecording)
            {
                // Feed audio data to the recognizer
                recognizer.SetInputToWaveStream(new MemoryStream(e.Buffer, 0, e.BytesRecorded));
                recognizer.Recognize();
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (isRecording && e.Result.Confidence > 0.3)
            {
                var entry = new TranscriptionEntry
                {
                    SpeakerName = "Current Speaker", // This would be determined by speaker diarization
                    SpeakerInitials = "CS",
                    SpeakerColor = "#4F6BED",
                    Text = e.Result.Text,
                    Timestamp = DateTime.Now.ToString("h:mm:ss tt")
                };
                
                currentTranscription.Add(entry);
                
                // Send the transcription to the web app
                SendTranscriptionToWebView(new List<TranscriptionEntry> { entry });
            }
        }

        private void Recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Could be used to show real-time partial results
        }

        private void SendTranscriptionToWebView(List<TranscriptionEntry> entries)
        {
            try
            {
                var message = new
                {
                    type = "TRANSCRIPTION_DATA",
                    data = new
                    {
                        entries = entries
                    }
                };
                
                string json = JsonSerializer.Serialize(message);
                webView.CoreWebView2.PostWebMessageAsJson(json);
                
                // Alternative method: call a JavaScript function directly
                string entriesJson = JsonSerializer.Serialize(entries);
                webView.CoreWebView2.ExecuteScriptAsync($"window.receiveTranscriptionFromHost('{entriesJson.Replace("'", "\\'")}')");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error sending transcription: {ex.Message}";
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string messageJson = e.WebMessageAsJson;
                var message = JsonSerializer.Deserialize<WebMessage>(messageJson);
                
                if (message != null)
                {
                    switch (message.Type)
                    {
                        case "APP_READY":
                            StatusText.Text = "React app is ready";
                            break;
                            
                        case "RECORDING_STATE_CHANGED":
                            if (message.Data != null && message.Data.TryGetProperty("isRecording", out var isRecordingValue))
                            {
                                bool newRecordingState = isRecordingValue.GetBoolean();
                                ToggleRecording(newRecordingState);
                            }
                            break;
                            
                        default:
                            StatusText.Text = $"Received unknown message type: {message.Type}";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error processing web message: {ex.Message}";
            }
        }

        private void ToggleRecording(bool newState)
        {
            try
            {
                isRecording = newState;
                
                if (isRecording)
                {
                    // Start recording
                    currentTranscription.Clear();
                    waveIn.StartRecording();
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    StatusText.Text = "Recording started";
                }
                else
                {
                    // Stop recording
                    waveIn.StopRecording();
                    recognizer.RecognizeAsyncStop();
                    StatusText.Text = "Recording stopped";
                    
                    // Save the transcription
                    SaveTranscription();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error toggling recording: {ex.Message}";
            }
        }

        private void SaveTranscription()
        {
            try
            {
                // Create a new recording entry
                var recording = new Recording
                {
                    Id = DateTime.Now.Ticks,
                    Title = $"Meeting {DateTime.Now:MMM d, yyyy}",
                    Date = DateTime.Now.ToString("MMMM d, yyyy"),
                    Time = DateTime.Now.ToString("h:mm tt"),
                    Duration = "00:00", // Calculate actual duration
                    Participants = 5, // Get actual participant count
                    Timestamp = DateTime.Now,
                    Transcript = currentTranscription
                };
                
                // Save to a file or database
                string json = JsonSerializer.Serialize(recording);
                string fileName = $"Transcript_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MeetingTranscriptions",
                    fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
                
                StatusText.Text = $"Transcription saved to {path}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error saving transcription: {ex.Message}";
            }
        }

        #region Window Controls
        
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "\uE922"; // Maximize icon
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "\uE923"; // Restore icon
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        #endregion
    }

    public class WebMessage
    {
        public string Type { get; set; }
        public JsonElement? Data { get; set; }
    }

    public class TranscriptionEntry
    {
        public string SpeakerName { get; set; }
        public string SpeakerInitials { get; set; }
        public string SpeakerColor { get; set; }
        public string Text { get; set; }
        public string Timestamp { get; set; }
    }

    public class Recording
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Duration { get; set; }
        public int Participants { get; set; }
        public DateTime Timestamp { get; set; }
        public List<TranscriptionEntry> Transcript { get; set; }
    }
}

