using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    /// <summary>
    /// Manages audio devices using NAudio
    /// </summary>
    public class AudioDeviceManager : IDisposable
    {
        private MMDeviceEnumerator deviceEnumerator;
        private List<AudioDeviceInfo> microphones;
        private List<AudioDeviceInfo> speakers;
        
        public AudioDeviceManager()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            microphones = new List<AudioDeviceInfo>();
            speakers = new List<AudioDeviceInfo>();
        }
        
        public async Task InitializeAsync()
        {
            await Task.Run(() => {
                RefreshDevices();
            });
        }
        
        public void RefreshDevices()
        {
            // Clear existing devices
            microphones.Clear();
            speakers.Clear();
            
            try
            {
                // Get all audio endpoints
                var captureDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                var renderDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                
                // Get default devices
                var defaultCapture = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                var defaultRender = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                
                // Add microphones
                foreach (var device in captureDevices)
                {
                    microphones.Add(new AudioDeviceInfo(
                        device.ID,
                        device.FriendlyName,
                        device.ID == defaultCapture.ID,
                        device
                    ));
                }
                
                // Add speakers
                foreach (var device in renderDevices)
                {
                    speakers.Add(new AudioDeviceInfo(
                        device.ID,
                        device.FriendlyName,
                        device.ID == defaultRender.ID,
                        device
                    ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing devices: {ex.Message}");
                // Add fallback devices if needed
            }
        }
        
        public List<AudioDeviceInfo> GetMicrophones()
        {
            return microphones;
        }
        
        public List<AudioDeviceInfo> GetSpeakers()
        {
            return speakers;
        }
        
        public MMDevice GetDefaultMicrophone()
        {
            var defaultMic = microphones.FirstOrDefault(m => m.IsDefault);
            return defaultMic != null ? defaultMic.Device : (microphones.FirstOrDefault()?.Device);
        }
        
        public MMDevice GetDefaultSpeaker()
        {
            var defaultSpeaker = speakers.FirstOrDefault(s => s.IsDefault);
            return defaultSpeaker != null ? defaultSpeaker.Device : (speakers.FirstOrDefault()?.Device);
        }
        
        public MMDevice GetMicrophoneById(string deviceId)
        {
            var mic = microphones.FirstOrDefault(m => m.Id == deviceId);
            return mic?.Device;
        }
        
        public MMDevice GetSpeakerById(string deviceId)
        {
            var speaker = speakers.FirstOrDefault(s => s.Id == deviceId);
            return speaker?.Device;
        }
        
        public void Dispose()
        {
            deviceEnumerator?.Dispose();
        }
    }
    
    /// <summary>
    /// Wrapper class for audio device information
    /// </summary>
    public class AudioDeviceInfo
    {
        /// <summary>
        /// Device ID
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Friendly name of the device
        /// </summary>
        public string FriendlyName { get; }
        
        /// <summary>
        /// Whether this is the default device
        /// </summary>
        public bool IsDefault { get; }
        
        /// <summary>
        /// The underlying MMDevice
        /// </summary>
        public MMDevice Device { get; }
        
        /// <summary>
        /// Creates a new AudioDeviceInfo
        /// </summary>
        public AudioDeviceInfo(string id, string friendlyName, bool isDefault, MMDevice device)
        {
            Id = id;
            FriendlyName = friendlyName;
            IsDefault = isDefault;
            Device = device;
        }
    }
}

