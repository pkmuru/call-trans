/**
 * Utility to detect if the app is running in a WebView2 environment
 */
export const webViewDetection = {
  /**
   * Check if the app is running in a WebView2 environment
   */
  isRunningInWebView2(): boolean {
    return !!(window.chrome && (window.chrome as any).webview)
  },

  /**
   * Get the WebView2 version if available
   */
  getWebView2Version(): string | null {
    try {
      if (this.isRunningInWebView2()) {
        const userAgent = navigator.userAgent
        const match = userAgent.match(/Edg\/(\d+\.\d+\.\d+\.\d+)/)
        return match ? match[1] : null
      }
      return null
    } catch (error) {
      console.error("Error getting WebView2 version:", error)
      return null
    }
  },

  /**
   * Check if the app is running in development mode
   */
  isDevelopmentMode(): boolean {
    return process.env.NODE_ENV === "development"
  },
}

