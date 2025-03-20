"use client"

import { Avatar, Text, Caption1, Divider } from "@fluentui/react-components"
import { historicalTranscripts } from "@/data/transcripts"

interface HistoryTranscriptViewProps {
  recordingId: number
}

export default function HistoryTranscriptView({ recordingId }: HistoryTranscriptViewProps) {
  const transcript = historicalTranscripts[recordingId as keyof typeof historicalTranscripts] || []

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

      {transcript.length === 0 && (
        <div style={{ padding: "16px", textAlign: "center" }}>
          <Text>No transcript available for this recording.</Text>
        </div>
      )}
    </div>
  )
}

