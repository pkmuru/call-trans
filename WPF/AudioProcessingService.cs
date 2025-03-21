using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class AudioProcessingService : IDisposable
    {
        // Azure Speech config
        private readonly SpeechConfig speechConfig;
        
        // Audio source type
        public enum AudioSourceType
        {
            Microphone,
            SystemAudio
        }
        
        // Event for transcription results
        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        
        public AudioProcessingService(string speechKey, string speechRegion)
        {
            // Create speech configuration
            speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";
        }
        
        public async Task ProcessAudioStreamAsync(
            AudioSourceType sourceType, 
            WaveFormat waveFormat, 
            BufferedWaveProvider buffer, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Create an audio stream
                var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(
                    (uint)waveFormat.SampleRate,
                    (byte)waveFormat.BitsPerSample,
                    (byte)waveFormat.Channels));
                
                // Create audio config from the stream
                var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                
                // Create speech recognizer
                using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
                
                // Set up event handlers
                recognizer.Recognized += (s, e) => {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        // Determine speaker info based on source type
                        string speakerName = sourceType == AudioSourceType.Microphone ? "Microphone" : "Speaker";
                        string speakerInitials = sourceType == AudioSourceType.Microphone ? "MIC" : "SPK";
                        string speakerColor = sourceType == AudioSourceType.Microphone ? "#4F6BED" : "#D83B01";
                        
                        // Create transcription entry
                        var entry = new TranscriptionEntry
                        {
                            SpeakerName = speakerName,
                            SpeakerInitials = speakerInitials,
                            SpeakerColor = speakerColor,
                            Text = e.Result.Text,
                            Timestamp = DateTime.Now.ToString("h:mm:ss tt")
                        };
                        
                        // Raise event
                        TranscriptionReceived?.Invoke(this, new TranscriptionEventArgs(entry));
                    }
                };
                
                // Start continuous recognition
                await recognizer.StartContinuousRecognitionAsync();
                
                // Process audio data in a loop
                byte[] audioBuffer = new byte[16000];
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = buffer.Read(audioBuffer, 0, audioBuffer.Length);
                    if (bytesRead > 0)
                    {
                        audioInputStream.Write(audioBuffer, bytesRead);
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
                Console.WriteLine($"Error processing audio: {ex.Message}");
                throw;
            }
        }
        
        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
    
    public class TranscriptionEventArgs : EventArgs
    {
        public TranscriptionEntry Entry { get; }
        
        public TranscriptionEventArgs(TranscriptionEntry entry)
        {
            Entry = entry;
        }
    }
}

