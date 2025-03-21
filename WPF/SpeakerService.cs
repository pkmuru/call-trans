using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class SpeakerService : IDisposable
    {
        private readonly SpeechConfig speechConfig;
        private SpeechRecognizer recognizer;
        private WasapiLoopbackCapture speakerCapture;
        private BufferedWaveProvider speakerBuffer;
        private MMDevice currentDevice;
        private bool isRecording;
        
        // Speaker color for visual distinction
        private readonly string SpeakerColor = "#D83B01"; // Orange for system audio
        
        // Event for transcription results
        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        
        public SpeakerService(SpeechConfig speechConfig)
        {
            this.speechConfig = speechConfig;
        }
        
        public async Task SetDeviceAsync(MMDevice device)
        {
            // Stop recording if active
            if (isRecording)
            {
                await StopRecordingAsync();
            }
            
            // Dispose of existing capture
            speakerCapture?.Dispose();
            
            // Set new device
            currentDevice = device;
            
            // Initialize capture for new device
            speakerCapture = new WasapiLoopbackCapture(device);
            speakerBuffer = new BufferedWaveProvider(speakerCapture.WaveFormat);
            speakerCapture.DataAvailable += SpeakerCapture_DataAvailable;
        }
        
        private void SpeakerCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (isRecording)
            {
                speakerBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }
        
        public async Task StartRecordingAsync(CancellationToken cancellationToken)
        {
            if (speakerCapture == null || currentDevice == null)
            {
                throw new InvalidOperationException("Speaker device not set");
            }
            
            try
            {
                // Clear buffer
                speakerBuffer.ClearBuffer();
                
                // Start capture
                speakerCapture.StartRecording();
                
                // Start recognition
                _ = Task.Run(() => StartRecognitionAsync(cancellationToken));
                
                isRecording = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start speaker recording: {ex.Message}", ex);
            }
        }
        
        public async Task StopRecordingAsync()
        {
            try
            {
                isRecording = false;
                
                // Stop capture
                if (speakerCapture != null && speakerCapture.CaptureState == CaptureState.Capturing)
                {
                    speakerCapture.StopRecording();
                }
                
                // Stop recognizer
                if (recognizer != null)
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to stop speaker recording: {ex.Message}", ex);
            }
        }
        
        private async Task StartRecognitionAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Create an audio stream from the speaker buffer
                var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(
                    (uint)speakerCapture.WaveFormat.SampleRate,
                    (byte)speakerCapture.WaveFormat.BitsPerSample,
                    (byte)speakerCapture.WaveFormat.Channels));
                
                // Create audio config from the stream
                var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                
                // Create speech recognizer
                recognizer = new SpeechRecognizer(speechConfig, audioConfig);
                
                // Set up event handlers
                recognizer.Recognized += (s, e) => {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        ProcessRecognizedSpeech(e.Result.Text);
                    }
                };
                
                // Start continuous recognition
                await recognizer.StartContinuousRecognitionAsync();
                
                // Process audio data in a loop
                byte[] buffer = new byte[16000];
                while (!cancellationToken.IsCancellationRequested && isRecording)
                {
                    int bytesRead = speakerBuffer.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        audioInputStream.Write(buffer, bytesRead);
                    }
                    await Task.Delay(100, cancellationToken);
                }
                
                // Stop recognition
                await recognizer.StopContinuousRecognitionAsync();
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no need to handle
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in speaker recognition: {ex.Message}");
            }
        }
        
        private void ProcessRecognizedSpeech(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            var entry = new TranscriptionEntry
            {
                SpeakerName = "System Audio",
                SpeakerInitials = "SPK",
                SpeakerColor = SpeakerColor,
                Text = text,
                Timestamp = DateTime.Now.ToString("h:mm:ss tt")
            };
            
            TranscriptionReceived?.Invoke(this, new TranscriptionEventArgs(entry));
        }
        
        public void Dispose()
        {
            speakerCapture?.Dispose();
            recognizer?.Dispose();
        }
    }
}

