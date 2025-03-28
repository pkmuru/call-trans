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
using System.Diagnostics;

namespace MeetingTranscriptionApp
{
    public partial class MainWindow : Window
    {
        string subscriptionKey = "iMjcNbfY9mmvNEqVdIKyf6HeIJovQGn0G9S9yWzf6EMylyciNP1oJQQJ99BAACYeBjFXJ3w3AAAYACOGnIbK";
        string subscriptionRegion = "eastus";

        private bool isRecording = false;
        private ObservableCollection<TranscriptionEntry> currentTranscription = new ObservableCollection<TranscriptionEntry>();
        
        // Azure Speech config
        private SpeechConfig speechConfig;
        
        // Audio services
        private AudioDeviceManager audioDeviceManager;
        private MicrophoneService microphoneService;
        private SpeakerService speakerService;
        
        // Cancellation tokens for async operations
        private CancellationTokenSource recordingCts;
        
        // Audio source enabled states
        private bool microphoneEnabled = true;
        private bool speakerEnabled = true;
        
        // Recording start time for duration calculation
        private Stopwatch recordingStopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize audio device manager
            audioDeviceManager = new AudioDeviceManager();
            
            // Initialize WebView and services when the window is loaded
            Loaded += async (s, e) => {
                await InitializeWebViewAsync();
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

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // Set up WebView2 environment
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MeetingTranscriptionApp");
                
                //var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await webView.EnsureCoreWebView2Async();

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
             
                
                // Create speech configuration
                speechConfig = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);
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
                microphoneService = new MicrophoneService(subscriptionKey, subscriptionRegion);
                microphoneService.TranscriptionReceived += OnTranscriptionReceived;
                microphoneService.ErrorOccurred += (message) => {
                    Dispatcher.InvokeAsync(() => {
                        StatusText.Text = message;
                    });
                };
                
                // Initialize speaker service
                speakerService = new SpeakerService(subscriptionKey, subscriptionRegion);
                speakerService.TranscriptionReceived += OnTranscriptionReceived;
                speakerService.ErrorOccurred += (message) => {
                    Dispatcher.InvokeAsync(() => {
                        StatusText.Text = message;
                    });
                };
                
                // Set default devices
                var defaultMic = audioDeviceManager.GetDefaultMicrophone();
                var defaultSpeaker = audioDeviceManager.GetDefaultSpeaker();
                
              
                
                
                
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
            // Use Dispatcher to update UI from a background thread
            Dispatcher.InvokeAsync(() => {
                currentTranscription.Add(e.Entry);
                
                // Send the transcription to the web app
                SendTranscriptionToWebView(new List<TranscriptionEntry> { e.Entry });
            });
        }
        int micIndex = 0;

        private async Task StartRecordingAsync()
        {
            try
            {
                // Clear previous transcription
                currentTranscription.Clear();
                
                // Create a single cancellation token source for all recording operations
                recordingCts = new CancellationTokenSource();
                
                // Start services based on enabled state
                var startTasks = new List<Task>();
                
                if (microphoneEnabled)
                {
                    int? deviceIndex;

                    if (!string.IsNullOrEmpty(selectedMic))
                    {                    
                        deviceIndex = audioDeviceManager.GetWaveInDeviceIndexById(selectedMic);
                    }
                    else
                    {
                    
                        var defaultMic = audioDeviceManager.GetDefaultMicrophone();
                        deviceIndex = audioDeviceManager.GetWaveInDeviceIndexById(defaultMic?.ID);
                    }

                    if (deviceIndex.HasValue)
                    {
                        startTasks.Add(microphoneService.StartRecordingAsync(deviceIndex.Value, recordingCts.Token));
                    }
                    
                }
                
                if (speakerEnabled)
                {
                    if (selectedSpeaker != null)
                    {
                        startTasks.Add(speakerService.StartRecordingAsync(recordingCts.Token, selectedSpeaker));
                    }                 
                }
                
                // Wait for all services to start
                await Task.WhenAll(startTasks);
                
                // Start the stopwatch for duration tracking
                recordingStopwatch.Restart();
                
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
                // Stop the stopwatch
                recordingStopwatch.Stop();
                
                // Cancel all recording operations
                recordingCts?.Cancel();
                
                // Stop services
                var stopTasks = new List<Task>();
                
                stopTasks.Add(microphoneService.StopRecordingAsync());
                stopTasks.Add(speakerService.StopRecordingAsync());
                
                // Wait for all services to stop
                await Task.WhenAll(stopTasks);
                
                StatusText.Text = "Recording stopped";
                
                // Save the transcription
                await SaveTranscriptionAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error stopping recording: {ex.Message}";
            }
            finally
            {
                // Dispose the cancellation token source
                recordingCts?.Dispose();
                recordingCts = null;
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
                // Format the duration
                TimeSpan duration = recordingStopwatch.Elapsed;
                string formattedDuration = $"{(int)duration.TotalMinutes} minutes {duration.Seconds} seconds";
                
                // Create a new recording entry
                var recording = new Recording
                {
                    Id = DateTime.Now.Ticks,
                    Title = $"Meeting {DateTime.Now:MMM d, yyyy}",
                    Date = DateTime.Now.ToString("MMMM d, yyyy"),
                    Time = DateTime.Now.ToString("h:mm tt"),
                    Duration = formattedDuration,
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

        string selectedSpeaker ;
        string selectedMic;

        private async void SetAudioDeviceAsync(string deviceType, string deviceId)
        {
            try
            {
                if (deviceType == "microphone")
                {
                    var device = audioDeviceManager.GetMicrophoneById(deviceId);
                    if (device != null)
                    {
                        StatusText.Text = $"Microphone set to: {device.FriendlyName}";
                        selectedMic = deviceId;
                    }
                }
                else if (deviceType == "speaker")
                {
                    var device = audioDeviceManager.GetSpeakerById(deviceId);
                    if (device != null)
                    {                      
                        StatusText.Text = $"Speaker set to: {device.FriendlyName}";
                        selectedSpeaker = deviceId;
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

