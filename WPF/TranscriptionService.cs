using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.Recognition;
using NAudio.Wave;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class TranscriptionService
    {
        private SpeechRecognitionEngine recognizer;
        private WaveInEvent waveIn;
        private bool isRecording;
        
        // Event to notify when new transcription is available
        public event EventHandler<TranscriptionEventArgs> TranscriptionReceived;
        
        public TranscriptionService()
        {
            InitializeSpeechRecognition();
        }
        
        private void InitializeSpeechRecognition()
        {
            // Create a speech recognition engine
            recognizer = new SpeechRecognitionEngine();
            
            // Configure the recognizer
            var grammar = new DictationGrammar();
            recognizer.LoadGrammar(grammar);
            
            // Set up event handlers
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            
            // Configure audio input
            waveIn = new WaveInEvent
            {
                DeviceNumber = 0, // Default microphone
                WaveFormat = new WaveFormat(16000, 1) // 16kHz mono
            };
            
            waveIn.DataAvailable += WaveIn_DataAvailable;
        }
        
        public void StartRecording()
        {
            if (!isRecording)
            {
                isRecording = true;
                
                try
                {
                    waveIn.StartRecording();
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch (Exception ex)
                {
                    isRecording = false;
                    throw new Exception($"Failed to start recording: {ex.Message}", ex);
                }
            }
        }
        
        public void StopRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                
                try
                {
                    waveIn.StopRecording();
                    recognizer.RecognizeAsyncStop();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to stop recording: {ex.Message}", ex);
                }
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
                
                // Raise the event with the new transcription
                TranscriptionReceived?.Invoke(this, new TranscriptionEventArgs(entry));
            }
        }
        
        public void Dispose()
        {
            waveIn?.Dispose();
            recognizer?.Dispose();
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

