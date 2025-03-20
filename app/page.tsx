"use client"

import { useState, useEffect } from "react"
import { FluentProvider, webLightTheme, TabList, Tab } from "@fluentui/react-components"
import { DocumentRegular, HistoryRegular } from "@fluentui/react-icons"
import MeetingHeader from "@/components/meeting-header"
import CurrentMeetingView from "@/components/current-meeting-view"
import RecordingHistoryView from "@/components/recording-history-view"
import { WebViewCommunicator } from "@/utils/webview-communicator"
import type { TranscriptionEntryData } from "@/types"

export default function Home() {
  const [isRecording, setIsRecording] = useState(false)
  const [activeTab, setActiveTab] = useState("current")

  // Initialize WebView2 communication
  useEffect(() => {
    // Set up WebView2 communication
    WebViewCommunicator.initialize()

    // Listen for transcription data from the WPF host
    WebViewCommunicator.onReceiveTranscription((transcriptionData: TranscriptionEntryData[]) => {
      console.log("Received transcription data:", transcriptionData)
      // Handle transcription data here
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
            </TabList>
          </div>

          {/* Content area */}
          <div style={{ display: "flex", flex: 1, overflow: "hidden" }}>
            {activeTab === "current" ? (
              <CurrentMeetingView isRecording={isRecording} toggleRecording={toggleRecording} />
            ) : (
              <RecordingHistoryView />
            )}
          </div>
        </div>
      </div>
    </FluentProvider>
  )
}

