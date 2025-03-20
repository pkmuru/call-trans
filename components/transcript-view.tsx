"use client"

import { useEffect, useState } from "react"
import { Avatar, Text, Caption1, Divider } from "@fluentui/react-components"
import type { TranscriptEntry } from "@/types"
import { WebViewCommunicator } from "@/utils/webview-communicator"

// Sample transcript data
const sampleTranscript: TranscriptEntry[] = [
  {
    id: 1,
    speaker: { name: "Guest 1", initials: "G1", color: "#4F6BED" },
    text: "Good morning everyone! Let's start our daily standup. Can everyone share what they worked on yesterday and what they plan to do today?",
    timestamp: "9:30:15 AM",
  },
  {
    id: 2,
    speaker: { name: "Sarah Chen", initials: "SC", color: "#D83B01" },
    text: "Yesterday I finished the wireframes for the new dashboard and got feedback from the team. Today I'll be working on implementing those changes and starting on the user profile redesign.",
    timestamp: "9:30:45 AM",
  },
  {
    id: 3,
    speaker: { name: "Emma Wilson", initials: "EW", color: "#8764B8" },
    text: "I completed the test cases for the payment module yesterday. Today I'll be running regression tests and documenting any issues I find.",
    timestamp: "9:31:20 AM",
  },
]

interface TranscriptViewProps {
  isRecording: boolean
}

export default function TranscriptView({ isRecording }: TranscriptViewProps) {
  const [transcript, setTranscript] = useState<TranscriptEntry[]>(sampleTranscript)

  useEffect(() => {
    // Register for transcription updates from the WPF host
    WebViewCommunicator.onReceiveTranscription((transcriptionData) => {
      if (transcriptionData && transcriptionData.entries) {
        // Add new transcription entries
        setTranscript((prev) => {
          // Create new entries with proper IDs
          const newEntries = transcriptionData.entries.map((entry: any, index: number) => ({
            id: prev.length + index + 1,
            speaker: {
              name: entry.speakerName || "Unknown Speaker",
              initials: entry.speakerInitials || "??",
              color: entry.speakerColor || "#4F6BED",
            },
            text: entry.text,
            timestamp:
              entry.timestamp ||
              new Date().toLocaleTimeString([], {
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit",
              }),
          }))

          return [...prev, ...newEntries]
        })
      }
    })
  }, [])

  return (
    <div>
      {transcript.map((entry, index) => (
        <div key={entry.id} style={{ marginBottom: "24px" }}>
          <div style={{ display: "flex", alignItems: "flex-start", marginBottom: "8px" }}>
            <Avatar
              name={entry.speaker.name}
              initials={entry.speaker.initials}
              color="colorful"
              style={{ backgroundColor: entry.speaker.color }}
              size={32}
            />
            <div style={{ marginLeft: "12px" }}>
              <div style={{ display: "flex", alignItems: "center" }}>
                <Text weight="semibold">{entry.speaker.name}</Text>
                <Caption1 style={{ marginLeft: "8px" }}>{entry.timestamp}</Caption1>
              </div>
              <Text style={{ marginTop: "4px" }}>{entry.text}</Text>
            </div>
          </div>
          {index < transcript.length - 1 && <Divider style={{ margin: "16px 0" }} />}
        </div>
      ))}

      {isRecording && transcript.length === 0 && (
        <div style={{ padding: "16px", textAlign: "center" }}>
          <Text italic>Waiting for someone to speak...</Text>
        </div>
      )}
    </div>
  )
}

