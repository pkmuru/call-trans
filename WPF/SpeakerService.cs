using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class SpeakerService : IDisposable
    {
        private readonly string subscriptionKey;
        private readonly string region;
        private WasapiLoopbackCapture capture;
        private PushAudioInputStream pushStream;
        private ConversationTranscriber recognizer;
        public MMDevice AudioDevice { get; private set; }

        // Speaker color for visual distinction
        private readonly string SpeakerColor = "#D83B01"; // Orange for system audio
        
        // Event for transcription results
        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        public event Action<string> ErrorOccurred;

 
        public SpeakerService(string subscriptionKey, string region)
        {
            this.subscriptionKey = subscriptionKey;
            this.region = region;
        }

     

        public async Task StartRecordingAsync(System.Threading.CancellationToken cancellationToken, String deviceID)
        {
            try
            {
                // Configure speech settings

                var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
                speechConfig.SpeechRecognitionLanguage = "en-US";
                speechConfig.OutputFormat = OutputFormat.Detailed;
                speechConfig.SetProperty("SpeechServiceResponse_SpeakerDiarizationEnabled", "true");
                speechConfig.SetProperty(PropertyId.SpeechServiceResponse_DiarizeIntermediateResults, "true");
                speechConfig.EnableDictation();

                // Create audio stream
                var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
                pushStream = AudioInputStream.CreatePushStream(audioFormat);
                var audioConfig = AudioConfig.FromStreamInput(pushStream);
                
                // Create conversation transcriber
                recognizer = new ConversationTranscriber(speechConfig, audioConfig);

                // Set up event handlers
                recognizer.Transcribed += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        var json = e.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                        ProcessTranscriptionResult(e.Result, json);
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    ErrorOccurred?.Invoke($"Canceled: {e.Reason} - {e.ErrorDetails}");
                };

                // Start transcribing
                await recognizer.StartTranscribingAsync();

                if(deviceID == null)
                {
                    var enumerator = new MMDeviceEnumerator();
                    AudioDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                }
                else
                {
                    var enumerator = new MMDeviceEnumerator();
                    try
                    {
                        AudioDevice = enumerator.GetDevice(deviceID);
                    }
                    catch (Exception ex)
                    {
                        // Fallback to default or notify the user
                        AudioDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                    }
                }


                // Initialize audio capture
                capture = new WasapiLoopbackCapture(AudioDevice);

                // Set up data available handler
                capture.DataAvailable += (s, a) =>
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            byte[] buffer = new byte[a.BytesRecorded];
                            Array.Copy(a.Buffer, 0, buffer, 0, a.BytesRecorded);
                            pushStream.Write(buffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke($"System Audio Write Error: {ex.Message}");
                    }
                };

                // Start recording
                capture.StartRecording();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"System Audio Capture Error: {ex.Message}");
                throw;
            }
        }

        public async Task StopRecordingAsync()
        {
            try
            {
                // Stop and dispose capture
                if (capture != null)
                {
                    capture.StopRecording();
                    capture.Dispose();
                    capture = null;
                }

                // Close push stream
                if (pushStream != null)
                {
                    pushStream.Close();
                    pushStream = null;
                }

                // Stop and dispose recognizer
                if (recognizer != null)
                {
                    await recognizer.StopTranscribingAsync();
                    recognizer.Dispose();
                    recognizer = null;
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Error stopping speaker recording: {ex.Message}");
                throw;
            }
        }

        private void ProcessTranscriptionResult(ConversationTranscriptionResult result, string jsonResult)
        {
            try
            {
                // Parse the JSON to extract speaker information if available
                string speakerName = "System Audio";
                string speakerInitials = "SPK";
                
                if (!string.IsNullOrEmpty(jsonResult))
                {
                    var json = JObject.Parse(jsonResult);
                    if (json["NBest"] is JArray nBest && nBest.Count > 0)
                    {
                        var firstResult = nBest[0];
                        if (firstResult["Speaker"] != null)
                        {
                            var speakerId = firstResult["Speaker"].ToString();
                            speakerName = $"Speaker {speakerId}";
                            speakerInitials = $"S{speakerId}";
                        }
                    }
                }

                // Create transcription entry
                var entry = new TranscriptionEntry
                {
                    SpeakerName = speakerName,
                    SpeakerInitials = speakerInitials,
                    SpeakerColor = SpeakerColor,
                    Text = result.Text,
                    Timestamp = DateTime.Now.ToString("h:mm:ss tt")
                };

                // Raise event
                TranscriptionReceived?.Invoke(this, new TranscriptionEventArgs(entry));
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Error processing transcription result: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopRecordingAsync().Wait();
            capture?.Dispose();
            recognizer?.Dispose();
            pushStream?.Dispose();
        }
    }
}

