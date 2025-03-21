using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using System.Windows.Forms;

namespace MeetingTranscriptionApp
{
    public partial class MainWindow : Window
    {
        private bool isRecording = false;
        private ObservableCollection<TranscriptionEntry> currentTranscription = new ObservableCollection<TranscriptionEntry>();
        
        // Azure Speech config
        private SpeechConfig speechConfig;
        
        // Audio services
        private AudioDeviceManager audioDeviceManager;
        private MicrophoneService microphoneService;
        private SpeakerService speakerService;
        
        // Cancellation tokens for async operations
        private CancellationTokenSource micCts;
        private CancellationTokenSource speakerCts;
        
        // Audio source enabled states
        private bool microphoneEnabled = true;
        private bool speakerEnabled = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            
            // Initialize audio device manager
            audioDeviceManager = new AudioDeviceManager();
            
            // Initialize Azure Speech Services when the window is loaded
            Loaded += async (s, e) => {
                await InitializeSpeechServicesAsync();
                PositionWindowOnStartup();
            };
        }

        private void PositionWindowOnStartup()
        {
            try
            {
                // Get the screen working area (excludes taskbar)
                var screen = Screen.PrimaryScreen;
                var workingArea = screen.WorkingArea;
                
                // Set window width to 1/3 of screen width
                double windowWidth = workingArea.Width / 3;
                
                // Position window on the right side of the screen
                Left = workingArea.Right - windowWidth;
                Top = workingArea.Top;
                Width = windowWidth;
                Height = workingArea.Height;
                
                // Ensure window is visible
                WindowState = WindowState.Normal;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error positioning window: {ex.Message}";
            }
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
                System.Windows.MessageBox.Show($"Failed to initialize WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "WebView2 initialization failed";
            }
        }

        private async Task InitializeSpeechServicesAsync()
        {
            try
            {
                // Initialize Azure Speech Services
                // In a real app, these would be stored securely and not hardcoded
                string speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY") ?? "YOUR_AZURE_SPEECH_KEY";
                string speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION") ?? "eastus";
                
                // Create speech configuration
                speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig.SpeechRecognitionLanguage = "en-US";
                
                // Initialize audio services
                await InitializeAudioServicesAsync();
                
                StatusText.Text = "Speech services initialized";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize speech services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Speech services initialization failed";
            }
        }

        private async Task InitializeAudioServicesAsync()
        {
            try
            {
                // Initialize audio device manager and get devices
                await audioDeviceManager.InitializeAsync();
                
                // Initialize microphone service
                microphoneService = new MicrophoneService(speechConfig);
                microphoneService.TranscriptionReceived += OnTranscriptionReceived;
                
                // Initialize speaker service
                speakerService = new SpeakerService(speechConfig);
                speakerService.TranscriptionReceived += OnTranscriptionReceived;
                
                // Set default devices
                var defaultMic = audioDeviceManager.GetDefaultMicrophone();
                var defaultSpeaker = audioDeviceManager.GetDefaultSpeaker();
                
                if (defaultMic != null)
                {
                    await microphoneService.SetDeviceAsync(defaultMic);
                }
                
                if (defaultSpeaker != null)
                {
                    await speakerService.SetDeviceAsync(defaultSpeaker);
                }
                
                // Send audio devices to the web app
                SendAudioDevicesToWebView();
                
                StatusText.Text = "Audio services initialized";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize audio services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Audio services initialization failed";
            }
        }

        private void OnTranscriptionReceived(object sender, TranscriptionEventArgs e)
        {
            Dispatcher.Invoke(() => {
                currentTranscription.Add(e.Entry);
                
                // Send the transcription to the web app
                SendTranscriptionToWebView(new List<TranscriptionEntry> { e.Entry });
            });
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                // Clear previous transcription
                currentTranscription.Clear();
                
                // Create cancellation tokens
                micCts = new CancellationTokenSource();
                speakerCts = new CancellationTokenSource();
                
                // Start services based on enabled state
                if (microphoneEnabled)
                {
                    await microphoneService.StartRecordingAsync(micCts.Token);
                }
                
                if (speakerEnabled)
                {
                    await speakerService.StartRecordingAsync(speakerCts.Token);
                }
                
                StatusText.Text = "Recording started";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error starting recording: {ex.Message}";
                await StopRecordingAsync();
            }
        }

        private async Task StopRecordingAsync()
        {
            try
            {
                // Cancel recognition tasks
                micCts?.Cancel();
                speakerCts?.Cancel();
                
                // Stop services
                await microphoneService.StopRecordingAsync();
                await speakerService.StopRecordingAsync();
                
                StatusText.Text = "Recording stopped";
                
                // Save the transcription
                await SaveTranscriptionAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error stopping recording: {ex.Message}";
            }
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
                
                string json = JsonConvert.SerializeObject(message);
                webView.CoreWebView2.PostWebMessageAsJson(json);
                
                // Alternative method: call a JavaScript function directly
                string entriesJson = JsonConvert.SerializeObject(entries);
                webView.CoreWebView2.ExecuteScriptAsync($"window.receiveTranscriptionFromHost('{entriesJson.Replace("'", "\\'")}')");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error sending transcription: {ex.Message}";
            }
        }

        private void SendAudioDevicesToWebView()
        {
            try
            {
                var microphones = audioDeviceManager.GetMicrophones().Select(m => new
                {
                    id = m.Id,
                    name = m.FriendlyName,
                    isDefault = m.IsDefault
                }).ToList();
                
                var speakers = audioDeviceManager.GetSpeakers().Select(s => new
                {
                    id = s.Id,
                    name = s.FriendlyName,
                    isDefault = s.IsDefault
                }).ToList();
                
                var message = new
                {
                    type = "AUDIO_DEVICES",
                    data = new
                    {
                        microphones,
                        speakers
                    }
                };
                
                string json = JsonConvert.SerializeObject(message);
                webView.CoreWebView2.PostWebMessageAsJson(json);
                
                // Alternative method: call a JavaScript function directly
                string devicesJson = JsonConvert.SerializeObject(new { microphones, speakers });
                webView.CoreWebView2.ExecuteScriptAsync($"window.receiveAudioDevicesFromHost('{devicesJson.Replace("'", "\\'")}')");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error sending audio devices: {ex.Message}";
            }
        }

        private async Task SaveTranscriptionAsync()
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
                    Participants = 2, // Microphone and Speaker
                    Timestamp = DateTime.Now,
                    Transcript = currentTranscription.ToList()
                };
                
                // Save to a file or database
                string json = JsonConvert.SerializeObject(recording, Formatting.Indented);
                string fileName = $"Transcript_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MeetingTranscriptions",
                    fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllTextAsync(path, json);
                
                StatusText.Text = $"Transcription saved to {path}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error saving transcription: {ex.Message}";
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string messageJson = e.WebMessageAsJson;
                var message = JsonConvert.DeserializeObject<WebMessage>(messageJson);
                
                if (message != null)
                {
                    switch (message.Type)
                    {
                        case "APP_READY":
                            StatusText.Text = "React app is ready";
                            break;
                            
                        case "RECORDING_STATE_CHANGED":
                            if (message.Data != null)
                            {
                                bool newRecordingState = message.Data["isRecording"].Value<bool>();
                                ToggleRecordingAsync(newRecordingState);
                            }
                            break;
                            
                        case "REQUEST_AUDIO_DEVICES":
                            SendAudioDevicesToWebView();
                            break;
                            
                        case "SET_AUDIO_DEVICE":
                            if (message.Data != null)
                            {
                                string deviceType = message.Data["deviceType"].Value<string>();
                                string deviceId = message.Data["deviceId"].Value<string>();
                                SetAudioDeviceAsync(deviceType, deviceId);
                            }
                            break;
                            
                        case "TOGGLE_AUDIO_SOURCE":
                            if (message.Data != null)
                            {
                                string sourceType = message.Data["sourceType"].Value<string>();
                                bool enabled = message.Data["enabled"].Value<bool>();
                                ToggleAudioSource(sourceType, enabled);
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

        private async void SetAudioDeviceAsync(string deviceType, string deviceId)
        {
            try
            {
                if (deviceType == "microphone")
                {
                    var device = audioDeviceManager.GetMicrophoneById(deviceId);
                    if (device != null)
                    {
                        await microphoneService.SetDeviceAsync(device);
                        StatusText.Text = $"Microphone set to: {device.FriendlyName}";
                    }
                }
                else if (deviceType == "speaker")
                {
                    var device = audioDeviceManager.GetSpeakerById(deviceId);
                    if (device != null)
                    {
                        await speakerService.SetDeviceAsync(device);
                        StatusText.Text = $"Speaker set to: {device.FriendlyName}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error setting audio device: {ex.Message}";
            }
        }

        private void ToggleAudioSource(string sourceType, bool enabled)
        {
            try
            {
                if (sourceType == "microphone")
                {
                    microphoneEnabled = enabled;
                    StatusText.Text = $"Microphone transcription {(enabled ? "enabled" : "disabled")}";
                }
                else if (sourceType == "speaker")
                {
                    speakerEnabled = enabled;
                    StatusText.Text = $"Speaker transcription {(enabled ? "enabled" : "disabled")}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error toggling audio source: {ex.Message}";
            }
        }

        private async void ToggleRecordingAsync(bool newState)
        {
            try
            {
                if (newState != isRecording)
                {
                    isRecording = newState;
                    
                    if (isRecording)
                    {
                        await StartRecordingAsync();
                    }
                    else
                    {
                        await StopRecordingAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error toggling recording: {ex.Message}";
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

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure we stop recording before closing
            if (isRecording)
            {
                isRecording = false;
                await StopRecordingAsync();
            }
            
            // Clean up resources
            microphoneService?.Dispose();
            speakerService?.Dispose();
            audioDeviceManager?.Dispose();
            
            Close();
        }
        
        #endregion
    }

    public class WebMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("data")]
        public JObject Data { get; set; }
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

