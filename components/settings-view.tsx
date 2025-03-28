"use client"

import { useState, useEffect, useRef } from "react"
import { Text, Subtitle1, Divider, Switch, Card, CardHeader, Button, Select } from "@fluentui/react-components"
import { MicRegular, InfoRegular, ArrowSyncRegular, SettingsRegular } from "@fluentui/react-icons"
import { WebViewCommunicator } from "@/utils/webview-communicator"
import type { AudioDevice } from "@/types"

// Local storage keys
const STORAGE_KEY_MIC = "meeting-transcription-mic-id"
const STORAGE_KEY_SPEAKER = "meeting-transcription-speaker-id"
const STORAGE_KEY_MIC_ENABLED = "meeting-transcription-mic-enabled"
const STORAGE_KEY_SPEAKER_ENABLED = "meeting-transcription-speaker-enabled"

interface SettingsViewProps {
  audioDevices: {
    microphones: AudioDevice[]
    speakers: AudioDevice[]
  }
  onDeviceSelect: (type: "microphone" | "speaker", deviceId: string) => void
  onSourceToggle: (type: "microphone" | "speaker", enabled: boolean) => void
}

export default function SettingsView({ audioDevices, onDeviceSelect, onSourceToggle }: SettingsViewProps) {
  // Refs to track previous values and prevent loops
  const initializedRef = useRef(false)
  const prevMicIdRef = useRef<string | null>(null)
  const prevSpeakerIdRef = useRef<string | null>(null)

  const [micEnabled, setMicEnabled] = useState(() => {
    // Initialize from localStorage or default to true
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem(STORAGE_KEY_MIC_ENABLED)
      return saved !== null ? saved === "true" : true
    }
    return true
  })

  const [speakerEnabled, setSpeakerEnabled] = useState(() => {
    // Initialize from localStorage or default to true
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem(STORAGE_KEY_SPEAKER_ENABLED)
      return saved !== null ? saved === "true" : true
    }
    return true
  })

  const [selectedMic, setSelectedMic] = useState<string>(() => {
    // Initialize from localStorage or empty string
    if (typeof window !== "undefined") {
      return localStorage.getItem(STORAGE_KEY_MIC) || ""
    }
    return ""
  })

  const [selectedSpeaker, setSelectedSpeaker] = useState<string>(() => {
    // Initialize from localStorage or empty string
    if (typeof window !== "undefined") {
      return localStorage.getItem(STORAGE_KEY_SPEAKER) || ""
    }
    return ""
  })

  // Initialize devices only once when component mounts or when devices change
  useEffect(() => {
    // Skip if no devices available
    if (audioDevices.microphones.length === 0 && audioDevices.speakers.length === 0) {
      return
    }

    // Only run initialization once
    if (!initializedRef.current) {
      let shouldUpdateMic = false
      let shouldUpdateSpeaker = false
      let micToUse = selectedMic
      let speakerToUse = selectedSpeaker

      // For microphone
      if (audioDevices.microphones.length > 0) {
        // If we have a saved selection, check if it's still available
        if (selectedMic) {
          const deviceExists = audioDevices.microphones.some((mic) => mic.id === selectedMic)
          if (!deviceExists) {
            // If saved device is no longer available, use default or first available
            const defaultMic = audioDevices.microphones.find((mic) => mic.isDefault)
            micToUse = defaultMic?.id || audioDevices.microphones[0].id
            shouldUpdateMic = true
          }
        } else {
          // No selection yet, use default or first available
          const defaultMic = audioDevices.microphones.find((mic) => mic.isDefault)
          micToUse = defaultMic?.id || audioDevices.microphones[0].id
          shouldUpdateMic = true
        }
      }

      // For speaker
      if (audioDevices.speakers.length > 0) {
        // If we have a saved selection, check if it's still available
        if (selectedSpeaker) {
          const deviceExists = audioDevices.speakers.some((speaker) => speaker.id === selectedSpeaker)
          if (!deviceExists) {
            // If saved device is no longer available, use default or first available
            const defaultSpeaker = audioDevices.speakers.find((speaker) => speaker.isDefault)
            speakerToUse = defaultSpeaker?.id || audioDevices.speakers[0].id
            shouldUpdateSpeaker = true
          }
        } else {
          // No selection yet, use default or first available
          const defaultSpeaker = audioDevices.speakers.find((speaker) => speaker.isDefault)
          speakerToUse = defaultSpeaker?.id || audioDevices.speakers[0].id
          shouldUpdateSpeaker = true
        }
      }

      // Update state and localStorage if needed
      if (shouldUpdateMic) {
        setSelectedMic(micToUse)
        if (typeof window !== "undefined") {
          localStorage.setItem(STORAGE_KEY_MIC, micToUse)
        }
      }

      if (shouldUpdateSpeaker) {
        setSelectedSpeaker(speakerToUse)
        if (typeof window !== "undefined") {
          localStorage.setItem(STORAGE_KEY_SPEAKER, speakerToUse)
        }
      }

      // Notify parent component of initial device selections
      if (micToUse && prevMicIdRef.current !== micToUse) {
        onDeviceSelect("microphone", micToUse)
        prevMicIdRef.current = micToUse
      }

      if (speakerToUse && prevSpeakerIdRef.current !== speakerToUse) {
        onDeviceSelect("speaker", speakerToUse)
        prevSpeakerIdRef.current = speakerToUse
      }

      initializedRef.current = true
    }
  }, [audioDevices, selectedMic, selectedSpeaker, onDeviceSelect])

  // Save enabled states to localStorage when they change
  useEffect(() => {
    if (typeof window !== "undefined") {
      localStorage.setItem(STORAGE_KEY_MIC_ENABLED, micEnabled.toString())
    }
  }, [micEnabled])

  useEffect(() => {
    if (typeof window !== "undefined") {
      localStorage.setItem(STORAGE_KEY_SPEAKER_ENABLED, speakerEnabled.toString())
    }
  }, [speakerEnabled])

  const handleMicToggle = (checked: boolean) => {
    setMicEnabled(checked)
    onSourceToggle("microphone", checked)
  }

  const handleSpeakerToggle = (checked: boolean) => {
    setSpeakerEnabled(checked)
    onSourceToggle("speaker", checked)
  }

  const handleMicSelect = (deviceId: string) => {
    if (deviceId !== selectedMic) {
      setSelectedMic(deviceId)
      onDeviceSelect("microphone", deviceId)
      prevMicIdRef.current = deviceId

      // Save to localStorage
      if (typeof window !== "undefined") {
        localStorage.setItem(STORAGE_KEY_MIC, deviceId)
      }
    }
  }

  const handleSpeakerSelect = (deviceId: string) => {
    if (deviceId !== selectedSpeaker) {
      setSelectedSpeaker(deviceId)
      onDeviceSelect("speaker", deviceId)
      prevSpeakerIdRef.current = deviceId

      // Save to localStorage
      if (typeof window !== "undefined") {
        localStorage.setItem(STORAGE_KEY_SPEAKER, deviceId)
      }
    }
  }

  const refreshDevices = () => {
    WebViewCommunicator.requestAudioDevices()
    // Reset initialization flag to allow re-initialization with new devices
    initializedRef.current = false
  }

  return (
    <div style={{ flex: 1, padding: "24px", overflowY: "auto" }}>
      <div style={{ maxWidth: "800px", margin: "0 auto" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
          <Subtitle1>Audio Settings</Subtitle1>
          <Button icon={<ArrowSyncRegular />} appearance="subtle" onClick={refreshDevices}>
            Refresh Devices
          </Button>
        </div>

        <Text style={{ marginBottom: "24px" }}>
          Configure which audio sources to capture and transcribe during meetings.
        </Text>

        <Card style={{ marginBottom: "24px" }}>
          <CardHeader
            header={
              <div style={{ display: "flex", alignItems: "center" }}>
                <MicRegular style={{ marginRight: "8px" }} />
                <Text weight="semibold">Microphone Input</Text>
              </div>
            }
          />
          <div style={{ padding: "16px" }}>
            <div
              style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "16px" }}
            >
              <Text>Enable microphone transcription</Text>
              <Switch checked={micEnabled} onChange={(e, data) => handleMicToggle(data.checked)} />
            </div>

            <div style={{ marginBottom: "16px" }}>
              <Text weight="medium" style={{ display: "block", marginBottom: "8px" }}>
                Select microphone device
              </Text>
              <Select
                disabled={!micEnabled}
                value={selectedMic}
                onChange={(e, data) => handleMicSelect(data.value as string)}
              >
                {audioDevices.microphones.map((mic) => (
                  <option key={mic.id} value={mic.id}>
                    {mic.name} {mic.isDefault && "(Default)"}
                  </option>
                ))}
                {audioDevices.microphones.length === 0 && <option value="none">No microphones found</option>}
              </Select>
            </div>

            <div style={{ display: "flex", alignItems: "center" }}>
              <InfoRegular style={{ color: "#707070", marginRight: "8px" }} />
              <Text size={200} style={{ color: "#707070" }}>
                Transcribes audio from your selected microphone. This is typically your voice and others in the room.
              </Text>
            </div>
          </div>
        </Card>

        <Card style={{ marginBottom: "24px" }}>
          <CardHeader
            header={
              <div style={{ display: "flex", alignItems: "center" }}>
                <SettingsRegular style={{ marginRight: "8px" }} />
                <Text weight="semibold">System Audio Output</Text>
              </div>
            }
          />
          <div style={{ padding: "16px" }}>
            <div
              style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "16px" }}
            >
              <Text>Enable system audio transcription</Text>
              <Switch checked={speakerEnabled} onChange={(e, data) => handleSpeakerToggle(data.checked)} />
            </div>

            <div style={{ marginBottom: "16px" }}>
              <Text weight="medium" style={{ display: "block", marginBottom: "8px" }}>
                Select audio output device
              </Text>
              <Select
                disabled={!speakerEnabled}
                value={selectedSpeaker}
                onChange={(e, data) => handleSpeakerSelect(data.value as string)}
              >
                {audioDevices.speakers.map((speaker) => (
                  <option key={speaker.id} value={speaker.id}>
                    {speaker.name} {speaker.isDefault && "(Default)"}
                  </option>
                ))}
                {audioDevices.speakers.length === 0 && <option value="none">No speakers found</option>}
              </Select>
            </div>

            <div style={{ display: "flex", alignItems: "center" }}>
              <InfoRegular style={{ color: "#707070", marginRight: "8px" }} />
              <Text size={200} style={{ color: "#707070" }}>
                Transcribes audio playing through your computer, such as remote participants in video calls or audio
                from applications.
              </Text>
            </div>
          </div>
        </Card>

        <Divider style={{ margin: "24px 0" }} />

        <div style={{ marginBottom: "24px" }}>
          <Subtitle1 style={{ marginBottom: "16px" }}>Transcription Settings</Subtitle1>

          <Text style={{ marginBottom: "16px" }}>
            Additional settings for transcription will be available in a future update.
          </Text>
        </div>
      </div>
    </div>
  )
}

