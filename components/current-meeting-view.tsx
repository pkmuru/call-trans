"use client"

import { Text, Button, Subtitle1, Badge } from "@fluentui/react-components"
import { RecordRegular } from "@fluentui/react-icons"
import ParticipantsList from "@/components/participants-list"
import TranscriptView from "@/components/transcript-view"

interface CurrentMeetingViewProps {
  isRecording: boolean
  toggleRecording: () => void
}

export default function CurrentMeetingView({ isRecording, toggleRecording }: CurrentMeetingViewProps) {
  return (
    <>
      {/* Transcript area */}
      <div style={{ flex: 1, padding: "16px", overflowY: "auto" }}>
        {isRecording ? (
          <>
            <div
              style={{
                textAlign: "center",
                padding: "8px",
                backgroundColor: "#FEF6F6",
                borderRadius: "4px",
                marginBottom: "16px",
              }}
            >
              <Text style={{ color: "#BC2F32" }}>● Recording in progress</Text>
            </div>
            <TranscriptView isRecording={isRecording} />
          </>
        ) : (
          <StartRecordingPrompt toggleRecording={toggleRecording} />
        )}
      </div>

      {/* Participants sidebar */}
      <div style={{ width: "250px", borderLeft: "1px solid #e0e0e0", padding: "16px" }}>
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "16px" }}>
          <Subtitle1>Participants</Subtitle1>
          <Badge appearance="filled" shape="rounded" color="informative">
            5
          </Badge>
        </div>
        <ParticipantsList />
      </div>
    </>
  )
}

function StartRecordingPrompt({ toggleRecording }: { toggleRecording: () => void }) {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        height: "100%",
        padding: "32px",
        textAlign: "center",
      }}
    >
      <div
        style={{
          width: "80px",
          height: "80px",
          borderRadius: "50%",
          backgroundColor: "#f0f0f0",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          marginBottom: "24px",
        }}
      >
        <RecordRegular style={{ fontSize: "32px", color: "#707070" }} />
      </div>
      <Text size={500} weight="semibold" style={{ marginBottom: "16px" }}>
        Ready to start recording
      </Text>
      <Text style={{ marginBottom: "32px", maxWidth: "500px" }}>
        Click the button below to start recording and transcribing your meeting. The app will automatically detect
        speakers and transcribe the conversation.
      </Text>
      <Button icon={<RecordRegular />} appearance="primary" size="large" onClick={toggleRecording}>
        Start Recording
      </Button>
    </div>
  )
}

