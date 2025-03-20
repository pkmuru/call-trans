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

