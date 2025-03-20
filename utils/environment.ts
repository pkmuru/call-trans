/**
 * Environment detection and configuration utilities
 */
export const environment = {
  /**
   * Check if the app is running in a development environment
   */
  isDevelopment(): boolean {
    return process.env.NODE_ENV === "development"
  },

  /**
   * Check if the app is running in a production environment
   */
  isProduction(): boolean {
    return process.env.NODE_ENV === "production"
  },

  /**
   * Get the base URL for API requests
   */
  getApiBaseUrl(): string {
    // In a real app, this would be configured based on environment
    return this.isDevelopment() ? "http://localhost:5000/api" : "/api"
  },

  /**
   * Get the current date formatted as a string
   */
  getCurrentDateFormatted(): string {
    const now = new Date()
    return now.toLocaleDateString("en-US", {
      month: "long",
      day: "numeric",
      year: "numeric",
    })
  },

  /**
   * Get the current time formatted as a string
   */
  getCurrentTimeFormatted(): string {
    const now = new Date()
    return now.toLocaleTimeString("en-US", {
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    })
  },
}

