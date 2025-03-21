"use client"

import { useState, useEffect } from "react"
import { Text, Subtitle1, Divider, Select, Option, Switch, Card, CardHeader, Button } from "@fluentui/react-components"
import { MicRegular, VolumeRegular, InfoRegular, ArrowSyncRegular } from "@fluentui/react-icons"
import { WebViewCommunicator } from "@/utils/webview-communicator"
import type { AudioDevice } from "@/types"

interface SettingsViewProps {
  audioDevices: {
    microphones: AudioDevice[]
    speakers: AudioDevice[]
  }
  onDeviceSelect: (type: "microphone" | "speaker", deviceId: string) => void
  onSourceToggle: (type: "microphone" | "speaker", enabled: boolean) => void
}

export default function SettingsView({ audioDevices, onDeviceSelect, onSourceToggle }: SettingsViewProps) {
  const [micEnabled, setMicEnabled] = useState(true)
  const [speakerEnabled, setSpeakerEnabled] = useState(true)
  const [selectedMic, setSelectedMic] = useState<string>("")
  const [selectedSpeaker, setSelectedSpeaker] = useState<string>("")

  useEffect(() => {
    // Set initial selected devices if available
    if (audioDevices.microphones.length > 0 && !selectedMic) {
      const defaultMic = audioDevices.microphones.find((mic) => mic.isDefault)
      setSelectedMic(defaultMic?.id || audioDevices.microphones[0].id)
    }

    if (audioDevices.speakers.length > 0 && !selectedSpeaker) {
      const defaultSpeaker = audioDevices.speakers.find((speaker) => speaker.isDefault)
      setSelectedSpeaker(defaultSpeaker?.id || audioDevices.speakers[0].id)
    }
  }, [audioDevices, selectedMic, selectedSpeaker])

  const handleMicToggle = (checked: boolean) => {
    setMicEnabled(checked)
    onSourceToggle("microphone", checked)
  }

  const handleSpeakerToggle = (checked: boolean) => {
    setSpeakerEnabled(checked)
    onSourceToggle("speaker", checked)
  }

  const handleMicSelect = (deviceId: string) => {
    setSelectedMic(deviceId)
    onDeviceSelect("microphone", deviceId)
  }

  const handleSpeakerSelect = (deviceId: string) => {
    setSelectedSpeaker(deviceId)
    onDeviceSelect("speaker", deviceId)
  }

  const refreshDevices = () => {
    WebViewCommunicator.requestAudioDevices()
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
                  <Option key={mic.id} value={mic.id} text={mic.name}>
                    {mic.name} {mic.isDefault && "(Default)"}
                  </Option>
                ))}
                {audioDevices.microphones.length === 0 && (
                  <Option value="none" text="No microphones found">
                    No microphones found
                  </Option>
                )}
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
                <VolumeRegular style={{ marginRight: "8px" }} />
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
                  <Option key={speaker.id} value={speaker.id} text={speaker.name}>
                    {speaker.name} {speaker.isDefault && "(Default)"}
                  </Option>
                ))}
                {audioDevices.speakers.length === 0 && (
                  <Option value="none" text="No speakers found">
                    No speakers found
                  </Option>
                )}
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

