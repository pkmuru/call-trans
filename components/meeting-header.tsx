"use client"

import { Caption1, Button } from "@fluentui/react-components"
import { MicRegular, StopRegular } from "@fluentui/react-icons"

interface MeetingHeaderProps {
  isRecording: boolean
  toggleRecording: () => void
  activeTab: string
}

export default function MeetingHeader({ isRecording, toggleRecording, activeTab }: MeetingHeaderProps) {
  return (
    <div style={{ display: "flex", flexDirection: "column", marginBottom: "16px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <div>
          <Caption1>March 20, 2025 • 9:30 AM</Caption1>
        </div>
        {activeTab === "current" && isRecording && (
          <div style={{ display: "flex", gap: "8px" }}>
            <Button icon={<StopRegular />} appearance="primary" onClick={toggleRecording}>
              Stop Recording
            </Button>
            <Button icon={<MicRegular />} appearance="subtle" aria-label="Mute" />
          </div>
        )}
      </div>
    </div>
  )
}

