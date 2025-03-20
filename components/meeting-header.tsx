"use client"

import { Title3, Caption1, Button } from "@fluentui/react-components"
import { MicRegular, StopRegular } from "@fluentui/react-icons"
import WebViewStatus from "@/components/webview-status"
import { webViewDetection } from "@/utils/webview-detection"

interface MeetingHeaderProps {
  isRecording: boolean
  toggleRecording: () => void
  activeTab: string
}

export default function MeetingHeader({ isRecording, toggleRecording, activeTab }: MeetingHeaderProps) {
  const isDevelopment = webViewDetection.isDevelopmentMode()

  return (
    <div style={{ display: "flex", flexDirection: "column", marginBottom: "16px" }}>
      {isDevelopment && <WebViewStatus />}

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <div>
          <Title3>Team Standup Meeting</Title3>
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

