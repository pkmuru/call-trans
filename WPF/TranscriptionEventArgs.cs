using System;

namespace MeetingTranscriptionApp
{
    /// <summary>
    /// Event arguments for transcription events
    /// </summary>
    public class TranscriptionEventArgs : EventArgs
    {
        /// <summary>
        /// The transcription entry
        /// </summary>
        public TranscriptionEntry Entry { get; }
        
        /// <summary>
        /// Creates a new instance of TranscriptionEventArgs
        /// </summary>
        /// <param name="entry">The transcription entry</param>
        public TranscriptionEventArgs(TranscriptionEntry entry)
        {
            Entry = entry;
        }
    }
    
    /// <summary>
    /// Represents a single transcription entry
    /// </summary>
    public class TranscriptionEntry
    {
        /// <summary>
        /// The name of the speaker
        /// </summary>
        public string SpeakerName { get; set; }
        
        /// <summary>
        /// The initials of the speaker
        /// </summary>
        public string SpeakerInitials { get; set; }
        
        /// <summary>
        /// The color associated with the speaker
        /// </summary>
        public string SpeakerColor { get; set; }
        
        /// <summary>
        /// The transcribed text
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// The timestamp of the transcription
        /// </summary>
        public string Timestamp { get; set; }
    }
}

