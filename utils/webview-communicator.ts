import type {
  WebViewMessage,
  RecordingStateMessage,
  AppReadyMessage,
  TranscriptionDataMessage,
  TranscriptionEntryData,
  RequestAudioDevicesMessage,
  AudioDevicesMessage,
  SetAudioDeviceMessage,
  ToggleAudioSourceMessage,
  AudioDevice,
} from "@/types"

/**
 * Type guard to check if a message is a TranscriptionDataMessage
 */
function isTranscriptionDataMessage(message: WebViewMessage): message is TranscriptionDataMessage {
  return message.type === "TRANSCRIPTION_DATA" && message.data && Array.isArray(message.data.entries)
}

/**
 * Type guard to check if a message is an AudioDevicesMessage
 */
function isAudioDevicesMessage(message: WebViewMessage): message is AudioDevicesMessage {
  return (
    message.type === "AUDIO_DEVICES" &&
    message.data &&
    Array.isArray(message.data.microphones) &&
    Array.isArray(message.data.speakers)
  )
}

/**
 * Utility for communication between the React app and the WebView2 host
 */
export class WebViewCommunicator {
  private static transcriptionCallback: ((data: TranscriptionEntryData[]) => void) | null = null
  private static audioDevicesCallback:
    | ((data: { microphones: AudioDevice[]; speakers: AudioDevice[] }) => void)
    | null = null

  /**
   * Initialize communication with the WebView2 host
   */
  static initialize(): void {
    // Add event listener for messages from the WebView2 host
    window.addEventListener("message", this.handleHostMessage)

    // Notify the host that the web app is ready
    this.sendMessageToHost({ type: "APP_READY" } as AppReadyMessage)

    // Add a global function that the host can call directly
    ;(window as any).receiveTranscriptionFromHost = (transcriptionData: string) => {
      try {
        const parsedData = JSON.parse(transcriptionData) as TranscriptionEntryData[]
        if (this.transcriptionCallback) {
          this.transcriptionCallback(parsedData)
        }
      } catch (error) {
        console.error("Error parsing transcription data:", error)
      }
    }
    ;(window as any).receiveAudioDevicesFromHost = (audioDevicesData: string) => {
      try {
        const parsedData = JSON.parse(audioDevicesData) as {
          microphones: AudioDevice[]
          speakers: AudioDevice[]
        }
        if (this.audioDevicesCallback) {
          this.audioDevicesCallback(parsedData)
        }
      } catch (error) {
        console.error("Error parsing audio devices data:", error)
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
    const message = event.data as WebViewMessage

    if (typeof message !== "object" || message === null) {
      return
    }

    switch (message.type) {
      case "TRANSCRIPTION_DATA":
        if (isTranscriptionDataMessage(message) && WebViewCommunicator.transcriptionCallback) {
          WebViewCommunicator.transcriptionCallback(message.data.entries)
        }
        break

      case "AUDIO_DEVICES":
        if (isAudioDevicesMessage(message) && WebViewCommunicator.audioDevicesCallback) {
          WebViewCommunicator.audioDevicesCallback(message.data)
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
  private static sendMessageToHost(message: WebViewMessage): void {
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
    const message: RecordingStateMessage = {
      type: "RECORDING_STATE_CHANGED",
      data: { isRecording },
    }
    this.sendMessageToHost(message)
  }

  /**
   * Request audio devices from the host
   */
  static requestAudioDevices(): void {
    const message: RequestAudioDevicesMessage = {
      type: "REQUEST_AUDIO_DEVICES",
    }
    this.sendMessageToHost(message)
  }

  /**
   * Set the audio device to use
   */
  static setAudioDevice(deviceType: "microphone" | "speaker", deviceId: string): void {
    const message: SetAudioDeviceMessage = {
      type: "SET_AUDIO_DEVICE",
      data: {
        deviceType,
        deviceId,
      },
    }
    this.sendMessageToHost(message)
  }

  /**
   * Toggle an audio source on/off
   */
  static toggleAudioSource(sourceType: "microphone" | "speaker", enabled: boolean): void {
    const message: ToggleAudioSourceMessage = {
      type: "TOGGLE_AUDIO_SOURCE",
      data: {
        sourceType,
        enabled,
      },
    }
    this.sendMessageToHost(message)
  }

  /**
   * Register a callback to receive transcription data
   */
  static onReceiveTranscription(callback: (data: TranscriptionEntryData[]) => void): void {
    this.transcriptionCallback = callback
  }

  /**
   * Register a callback to receive audio devices data
   */
  static onReceiveAudioDevices(
    callback: (data: { microphones: AudioDevice[]; speakers: AudioDevice[] }) => void,
  ): void {
    this.audioDevicesCallback = callback
  }
}

