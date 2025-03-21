using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class AudioCaptureService : IDisposable
    {
        // NAudio components
        private WasapiCapture microphoneCapture;
        private WasapiLoopbackCapture speakerCapture;
        private BufferedWaveProvider micBuffer;
        private BufferedWaveProvider speakerBuffer;
        
        // Status
        private bool isInitialized = false;
        private bool isCapturing = false;
        
        // Events
        public event EventHandler<AudioCaptureErrorEventArgs> ErrorOccurred;
        
        public async Task InitializeAsync()
        {
            if (isInitialized) return;
            
            await Task.Run(() => {
                try
                {
                    // Initialize microphone capture
                    var enumerator = new MMDeviceEnumerator();
                    var micDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                    
                    microphoneCapture = new WasapiCapture(micDevice);
                    micBuffer = new BufferedWaveProvider(microphoneCapture.WaveFormat);
                    microphoneCapture.DataAvailable += (s, e) => {
                        if (isCapturing)
                        {
                            micBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                        }
                    };
                    
                    // Initialize speaker capture (loopback)
                    var speakerDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    speakerCapture = new WasapiLoopbackCapture(speakerDevice);
                    speakerBuffer = new BufferedWaveProvider(speakerCapture.WaveFormat);
                    speakerCapture.DataAvailable += (s, e) => {
                        if (isCapturing)
                        {
                            speakerBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                        }
                    };
                    
                    isInitialized = true;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, new AudioCaptureErrorEventArgs("Failed to initialize audio devices", ex));
                }
            });
        }
        
        public void StartCapture()
        {
            if (!isInitialized)
            {
                ErrorOccurred?.Invoke(this, new AudioCaptureErrorEventArgs("Audio capture not initialized", null));
                return;
            }
            
            try
            {
                // Clear buffers
                micBuffer.ClearBuffer();
                speakerBuffer.ClearBuffer();
                
                // Start captures
                microphoneCapture.StartRecording();
                speakerCapture.StartRecording();
                
                isCapturing = true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new AudioCaptureErrorEventArgs("Failed to start audio capture", ex));
            }
        }
        
        public void StopCapture()
        {
            if (!isCapturing) return;
            
            try
            {
                // Stop captures
                if (microphoneCapture != null && microphoneCapture.CaptureState == CaptureState.Capturing)
                {
                    microphoneCapture.StopRecording();
                }
                
                if (speakerCapture != null && speakerCapture.CaptureState == CaptureState.Capturing)
                {
                    speakerCapture.StopRecording();
                }
                
                isCapturing = false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new AudioCaptureErrorEventArgs("Failed to stop audio capture", ex));
            }
        }
        
        public BufferedWaveProvider GetMicrophoneBuffer()
        {
            return micBuffer;
        }
        
        public BufferedWaveProvider GetSpeakerBuffer()
        {
            return speakerBuffer;
        }
        
        public WaveFormat GetMicrophoneFormat()
        {
            return microphoneCapture?.WaveFormat;
        }
        
        public WaveFormat GetSpeakerFormat()
        {
            return speakerCapture?.WaveFormat;
        }
        
        public void Dispose()
        {
            StopCapture();
            microphoneCapture?.Dispose();
            speakerCapture?.Dispose();
        }
    }
    
    public class AudioCaptureErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public Exception Exception { get; }
        
        public AudioCaptureErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}

