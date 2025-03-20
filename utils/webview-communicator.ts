/**
 * Utility for communication between the React app and the WebView2 host
 */
export class WebViewCommunicator {
  private static transcriptionCallback: ((data: any) => void) | null = null

  /**
   * Initialize communication with the WebView2 host
   */
  static initialize(): void {
    // Add event listener for messages from the WebView2 host
    window.addEventListener("message", this.handleHostMessage)

    // Notify the host that the web app is ready
    this.sendMessageToHost({ type: "APP_READY" })

    // Add a global function that the host can call directly
    ;(window as any).receiveTranscriptionFromHost = (transcriptionData: string) => {
      try {
        const parsedData = JSON.parse(transcriptionData)
        if (this.transcriptionCallback) {
          this.transcriptionCallback(parsedData)
        }
      } catch (error) {
        console.error("Error parsing transcription data:", error)
      }
    }
  }

  /**
   * Clean up event listeners
   */
  static cleanup(): void {
    window.removeEventListener("message", this.handleHostMessage)
  }

  /**
   * Handle messages from the WebView2 host
   */
  private static handleHostMessage(event: MessageEvent): void {
    const message = event.data

    if (typeof message !== "object" || message === null) {
      return
    }

    switch (message.type) {
      case "TRANSCRIPTION_DATA":
        if (WebViewCommunicator.transcriptionCallback) {
          WebViewCommunicator.transcriptionCallback(message.data)
        }
        break

      // Add more message types as needed

      default:
        console.log("Received unknown message type:", message.type)
    }
  }

  /**
   * Send a message to the WebView2 host
   */
  private static sendMessageToHost(message: any): void {
    // Check if we're running in a WebView2 environment
    if (window.chrome && (window.chrome as any).webview) {
      // Use the WebView2-specific postMessage API
      ;(window.chrome as any).webview.postMessage(message)
    } else {
      // Fallback for development in a browser
      console.log("Would send to host:", message)
    }
  }

  /**
   * Notify the host when recording state changes
   */
  static notifyRecordingStateChanged(isRecording: boolean): void {
    this.sendMessageToHost({
      type: "RECORDING_STATE_CHANGED",
      data: { isRecording },
    })
  }

  /**
   * Register a callback to receive transcription data
   */
  static onReceiveTranscription(callback: (data: any) => void): void {
    this.transcriptionCallback = callback
  }
}

