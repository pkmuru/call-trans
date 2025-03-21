using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingTranscriptionApp
{
    public class AudioDeviceManager
    {
        private MMDeviceEnumerator deviceEnumerator;
        private List<MMDevice> microphones;
        private List<MMDevice> speakers;
        
        public AudioDeviceManager()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            microphones = new List<MMDevice>();
            speakers = new List<MMDevice>();
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
            DisposeDevices();
            
            // Get all audio endpoints
            var captureDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var renderDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            
            // Get default devices
            var defaultCapture = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            var defaultRender = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            
            // Add microphones
            foreach (var device in captureDevices)
            {
                device.IsDefault = device.ID == defaultCapture.ID;
                microphones.Add(device);
            }
            
            // Add speakers
            foreach (var device in renderDevices)
            {
                device.IsDefault = device.ID == defaultRender.ID;
                speakers.Add(device);
            }
        }
        
        public List<MMDevice> GetMicrophones()
        {
            return microphones;
        }
        
        public List<MMDevice> GetSpeakers()
        {
            return speakers;
        }
        
        public MMDevice GetDefaultMicrophone()
        {
            return microphones.FirstOrDefault(m => m.IsDefault) ?? microphones.FirstOrDefault();
        }
        
        public MMDevice GetDefaultSpeaker()
        {
            return speakers.FirstOrDefault(s => s.IsDefault) ?? speakers.FirstOrDefault();
        }
        
        public MMDevice GetMicrophoneById(string deviceId)
        {
            return microphones.FirstOrDefault(m => m.ID == deviceId);
        }
        
        public MMDevice GetSpeakerById(string deviceId)
        {
            return speakers.FirstOrDefault(s => s.ID == deviceId);
        }
        
        private void DisposeDevices()
        {
            foreach (var device in microphones)
            {
                device.Dispose();
            }
            
            foreach (var device in speakers)
            {
                device.Dispose();
            }
            
            microphones.Clear();
            speakers.Clear();
        }
        
        public void Dispose()
        {
            DisposeDevices();
            deviceEnumerator.Dispose();
        }
    }
    
    // Extension method to add IsDefault property to MMDevice
    public static class MMDeviceExtensions
    {
        public static bool IsDefault { get; set; }
        
        public static void SetIsDefault(this MMDevice device, bool isDefault)
        {
            device.IsDefault = isDefault;
        }
    }
}

