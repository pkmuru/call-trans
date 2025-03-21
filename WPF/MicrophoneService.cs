using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class MicrophoneService : IDisposable
    {
        private readonly SpeechConfig speechConfig;
        private SpeechRecognizer recognizer;
        private WasapiCapture microphoneCapture;
        private BufferedWaveProvider micBuffer;
        private MMDevice currentDevice;
        private bool isRecording;
        
        // Speaker color for visual distinction
        private readonly string MicColor = "#4F6BED"; // Blue for microphone
        
        // Event for transcription results
        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        
        public MicrophoneService(SpeechConfig speechConfig)
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
            microphoneCapture?.Dispose();
            
            // Set new device
            currentDevice = device;
            
            // Initialize capture for new device
            microphoneCapture = new WasapiCapture(device);
            micBuffer = new BufferedWaveProvider(microphoneCapture.WaveFormat);
            microphoneCapture.DataAvailable += MicrophoneCapture_DataAvailable;
        }
        
        private void MicrophoneCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (isRecording)
            {
                micBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }
        
        public async Task StartRecordingAsync(CancellationToken cancellationToken)
        {
            if (microphoneCapture == null || currentDevice == null)
            {
                throw new InvalidOperationException("Microphone device not set");
            }
            
            try
            {
                // Clear buffer
                micBuffer.ClearBuffer();
                
                // Start capture
                microphoneCapture.StartRecording();
                
                // Start recognition
                _ = Task.Run(() => StartRecognitionAsync(cancellationToken));
                
                isRecording = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start microphone recording: {ex.Message}", ex);
            }
        }
        
        public async Task StopRecordingAsync()
        {
            try
            {
                isRecording = false;
                
                // Stop capture
                if (microphoneCapture != null && microphoneCapture.CaptureState == CaptureState.Capturing)
                {
                    microphoneCapture.StopRecording();
                }
                
                // Stop recognizer
                if (recognizer != null)
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to stop microphone recording: {ex.Message}", ex);
            }
        }
        
        private async Task StartRecognitionAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Create an audio stream from the microphone buffer
                var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(
                    (uint)microphoneCapture.WaveFormat.SampleRate,
                    (byte)microphoneCapture.WaveFormat.BitsPerSample,
                    (byte)microphoneCapture.WaveFormat.Channels));
                
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
                    int bytesRead = micBuffer.Read(buffer, 0, buffer.Length);
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
                Console.WriteLine($"Error in microphone recognition: {ex.Message}");
            }
        }
        
        private void ProcessRecognizedSpeech(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            var entry = new TranscriptionEntry
            {
                SpeakerName = currentDevice?.FriendlyName ?? "Microphone",
                SpeakerInitials = "MIC",
                SpeakerColor = MicColor,
                Text = text,
                Timestamp = DateTime.Now.ToString("h:mm:ss tt")
            };
            
            TranscriptionReceived?.Invoke(this, new TranscriptionEventArgs(entry));
        }
        
        public void Dispose()
        {
            microphoneCapture?.Dispose();
            recognizer?.Dispose();
        }
    }
}

