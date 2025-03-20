"use client"

import { useState } from "react"
import { Text, Button, Subtitle1, Badge, Tooltip } from "@fluentui/react-components"
import { RecordRegular, ChevronRightRegular, ChevronLeftRegular } from "@fluentui/react-icons"
import ParticipantsList from "@/components/participants-list"
import TranscriptView from "@/components/transcript-view"

interface CurrentMeetingViewProps {
  isRecording: boolean
  toggleRecording: () => void
}

export default function CurrentMeetingView({ isRecording, toggleRecording }: CurrentMeetingViewProps) {
  const [showParticipants, setShowParticipants] = useState(true)

  const toggleParticipantsPanel = () => {
    setShowParticipants((prev) => !prev)
  }

  return (
    <>
      {/* Transcript area */}
      <div
        style={{
          flex: 1,
          padding: "16px",
          overflowY: "auto",
          display: "flex",
          flexDirection: "column",
        }}
      >
        {isRecording ? (
          <>
            <div
              style={{
                textAlign: "center",
                padding: "8px",
                backgroundColor: "#FEF6F6",
                borderRadius: "4px",
                marginBottom: "16px",
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
              }}
            >
              <div style={{ flex: 1 }}></div>
              <Text style={{ color: "#BC2F32", flex: 1, textAlign: "center" }}>● Recording in progress</Text>
              <div style={{ flex: 1, textAlign: "right" }}>
                {isRecording && (
                  <Tooltip content={showParticipants ? "Hide participants" : "Show participants"} relationship="label">
                    <Button
                      icon={showParticipants ? <ChevronRightRegular /> : <ChevronLeftRegular />}
                      appearance="subtle"
                      onClick={toggleParticipantsPanel}
                      aria-label={showParticipants ? "Hide participants" : "Show participants"}
                    />
                  </Tooltip>
                )}
              </div>
            </div>
            <TranscriptView isRecording={isRecording} />
          </>
        ) : (
          <StartRecordingPrompt toggleRecording={toggleRecording} />
        )}
      </div>

      {/* Participants sidebar - only show when recording and showParticipants is true */}
      {isRecording && showParticipants && (
        <div
          style={{
            width: "250px",
            borderLeft: "1px solid #e0e0e0",
            padding: "16px",
            transition: "width 0.3s ease-in-out",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "16px" }}>
            <Subtitle1>Participants</Subtitle1>
            <Badge appearance="filled" shape="rounded" color="informative">
              5
            </Badge>
          </div>
          <ParticipantsList />
        </div>
      )}
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

