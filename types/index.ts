export interface Recording {
  id: number
  title: string
  date: string
  time: string
  duration: string
  participants: number
  timestamp: Date
}

export interface Speaker {
  name: string
  initials: string
  color: string
}

export interface TranscriptEntry {
  id: number
  speaker: Speaker
  text: string
  timestamp: string
}

export interface Participant {
  id: number
  name: string
  role: string
  isSpeaking: boolean
  isMuted: boolean
  initials: string
  color: string
  isGuest: boolean
}

export interface KnownUser {
  id: number
  name: string
  role: string
  initials: string
  color: string
}

// Audio device types
export interface AudioDevice {
  id: string
  name: string
  isDefault: boolean
}

// WebView communication types
export interface WebViewMessage {
  type: string
  data?: any
}

export interface RecordingStateMessage extends WebViewMessage {
  type: "RECORDING_STATE_CHANGED"
  data: {
    isRecording: boolean
  }
}

export interface AppReadyMessage extends WebViewMessage {
  type: "APP_READY"
}

export interface RequestAudioDevicesMessage extends WebViewMessage {
  type: "REQUEST_AUDIO_DEVICES"
}

export interface AudioDevicesMessage extends WebViewMessage {
  type: "AUDIO_DEVICES"
  data: {
    microphones: AudioDevice[]
    speakers: AudioDevice[]
  }
}

export interface SetAudioDeviceMessage extends WebViewMessage {
  type: "SET_AUDIO_DEVICE"
  data: {
    deviceType: "microphone" | "speaker"
    deviceId: string
  }
}

export interface ToggleAudioSourceMessage extends WebViewMessage {
  type: "TOGGLE_AUDIO_SOURCE"
  data: {
    sourceType: "microphone" | "speaker"
    enabled: boolean
  }
}

export interface TranscriptionDataMessage extends WebViewMessage {
  type: "TRANSCRIPTION_DATA"
  data: {
    entries: TranscriptionEntryData[]
  }
}

export interface TranscriptionEntryData {
  speakerName: string
  speakerInitials: string
  speakerColor: string
  text: string
  timestamp: string
}

