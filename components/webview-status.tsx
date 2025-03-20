"use client"

import { useEffect, useState } from "react"
import { Text, Badge } from "@fluentui/react-components"
import { webViewDetection } from "@/utils/webview-detection"

export default function WebViewStatus() {
  const [isWebView, setIsWebView] = useState<boolean | null>(null)
  const [webViewVersion, setWebViewVersion] = useState<string | null>(null)

  useEffect(() => {
    // Check if running in WebView2
    setIsWebView(webViewDetection.isRunningInWebView2())
    setWebViewVersion(webViewDetection.getWebView2Version())
  }, [])

  if (isWebView === null) {
    return null
  }

  return (
    <div style={{ padding: "8px", backgroundColor: "#f5f5f5", borderRadius: "4px", marginBottom: "16px" }}>
      <Text>
        Environment:{" "}
        {isWebView ? (
          <>
            <Badge appearance="filled" color="success">
              WebView2
            </Badge>
            {webViewVersion && <span style={{ marginLeft: "8px" }}>Version: {webViewVersion}</span>}
          </>
        ) : (
          <Badge appearance="filled" color="informative">
            Browser
          </Badge>
        )}
      </Text>
    </div>
  )
}

