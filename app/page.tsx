"use client"

import { useState, useEffect } from "react"
import { FluentProvider, webLightTheme, TabList, Tab } from "@fluentui/react-components"
import { DocumentRegular, HistoryRegular, SettingsRegular } from "@fluentui/react-icons"
import MeetingHeader from "@/components/meeting-header"
import CurrentMeetingView from "@/components/current-meeting-view"
import RecordingHistoryView from "@/components/recording-history-view"
import SettingsView from "@/components/settings-view"
import { WebViewCommunicator } from "@/utils/webview-communicator"
import type { TranscriptionEntryData, AudioDevice } from "@/types"

export default function Home() {
  const [isRecording, setIsRecording] = useState(false)
  const [activeTab, setActiveTab] = useState("current")
  const [audioDevices, setAudioDevices] = useState<{
    microphones: AudioDevice[]
    speakers: AudioDevice[]
  }>({
    microphones: [],
    speakers: [],
  })

  // Initialize WebView2 communication
  useEffect(() => {
    // Set up WebView2 communication
    WebViewCommunicator.initialize()

    // Listen for transcription data from the WPF host
    WebViewCommunicator.onReceiveTranscription((transcriptionData: TranscriptionEntryData[]) => {
      console.log("Received transcription data:", transcriptionData)
      // Handle transcription data here
    })

    // Request audio devices from the WPF host
    WebViewCommunicator.requestAudioDevices()

    // Listen for audio devices from the WPF host
    WebViewCommunicator.onReceiveAudioDevices((devices) => {
      setAudioDevices(devices)
    })

    return () => {
      WebViewCommunicator.cleanup()
    }
  }, [])

  const toggleRecording = () => {
    const newRecordingState = !isRecording
    setIsRecording(newRecordingState)

    // Notify the WPF host about recording state change
    WebViewCommunicator.notifyRecordingStateChanged(newRecordingState)
  }

  const handleDeviceSelection = (type: "microphone" | "speaker", deviceId: string) => {
    WebViewCommunicator.setAudioDevice(type, deviceId)
  }

  const handleSourceToggle = (type: "microphone" | "speaker", enabled: boolean) => {
    WebViewCommunicator.toggleAudioSource(type, enabled)
  }

  return (
    <FluentProvider theme={webLightTheme}>
      <div style={{ display: "flex", height: "100vh" }}>
        {/* Main content area */}
        <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>
          {/* Header */}
          <div style={{ padding: "16px", borderBottom: "1px solid #e0e0e0" }}>
            <MeetingHeader isRecording={isRecording} toggleRecording={toggleRecording} activeTab={activeTab} />

            <TabList selectedValue={activeTab} onTabSelect={(_, data) => setActiveTab(data.value as string)}>
              <Tab value="current" icon={<DocumentRegular />}>
                Current Meeting
              </Tab>
              <Tab value="history" icon={<HistoryRegular />}>
                Recording History
              </Tab>
              <Tab value="settings" icon={<SettingsRegular />}>
                Settings
              </Tab>
            </TabList>
          </div>

          {/* Content area */}
          <div style={{ display: "flex", flex: 1, overflow: "hidden" }}>
            {activeTab === "current" ? (
              <CurrentMeetingView isRecording={isRecording} toggleRecording={toggleRecording} />
            ) : activeTab === "history" ? (
              <RecordingHistoryView />
            ) : (
              <SettingsView
                audioDevices={audioDevices}
                onDeviceSelect={handleDeviceSelection}
                onSourceToggle={handleSourceToggle}
              />
            )}
          </div>
        </div>
      </div>
    </FluentProvider>
  )
}

