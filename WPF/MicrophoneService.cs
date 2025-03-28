using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class MicrophoneService : IDisposable
    {
        private readonly string subscriptionKey;
        private readonly string region;
        private ConversationTranscriber recognizer;
        private PushAudioInputStream pushStream;
        private WaveInEvent waveIn;
 
        private readonly string MicColor = "#4F6BED";

        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        public event Action<string> ErrorOccurred;

 
        public MicrophoneService(string subscriptionKey, string region)
        {
            this.subscriptionKey = subscriptionKey;
            this.region = region;
        }



        public async Task StartRecordingAsync(int deviceIndex, CancellationToken cancellationToken)
        {
            try
            {
                var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
                speechConfig.SpeechRecognitionLanguage = "en-US";
                speechConfig.OutputFormat = OutputFormat.Detailed;
                speechConfig.SetProperty("SpeechServiceResponse_SpeakerDiarizationEnabled", "true");
                speechConfig.SetProperty(PropertyId.SpeechServiceResponse_DiarizeIntermediateResults, "true");

                var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
                pushStream = AudioInputStream.CreatePushStream(audioFormat);
                var audioConfig = AudioConfig.FromStreamInput(pushStream);

                recognizer = new ConversationTranscriber(speechConfig, audioConfig);

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

                await recognizer.StartTranscribingAsync();

                waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(16000, 16, 1)
                };

                // 🟢 For display only (safe)
                var naudioDeviceName = WaveIn.GetCapabilities(deviceIndex).ProductName;
                

                waveIn.DataAvailable += (s, e) =>
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            byte[] buffer = new byte[e.BytesRecorded];
                            Array.Copy(e.Buffer, 0, buffer, 0, e.BytesRecorded);
                            pushStream.Write(buffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke($"Audio Write Error: {ex.Message}");
                    }
                };

                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Microphone Capture Error: {ex.Message}");
                throw;
            }
        }


        public async Task StopRecordingAsync()
        {
            try
            {
                waveIn?.StopRecording();
                waveIn?.Dispose();
                waveIn = null;

                pushStream?.Close();
                pushStream = null;

                if (recognizer != null)
                {
                    await recognizer.StopTranscribingAsync();
                    recognizer.Dispose();
                    recognizer = null;
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Error stopping microphone recording: {ex.Message}");
                throw;
            }
        }

        private void ProcessTranscriptionResult(ConversationTranscriptionResult result, string jsonResult)
        {
            try
            {
                string speakerName = "Microphone";
                string speakerInitials = "MIC";

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

                var entry = new TranscriptionEntry
                {
                    SpeakerName = speakerName,
                    SpeakerInitials = speakerInitials,
                    SpeakerColor = MicColor,
                    Text = result.Text,
                    Timestamp = DateTime.Now.ToString("h:mm:ss tt")
                };

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
            waveIn?.Dispose();
            recognizer?.Dispose();
            pushStream?.Dispose();
        }
              

    }
}
