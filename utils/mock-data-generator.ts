import type { TranscriptEntry, Speaker, Recording } from "@/types"

/**
 * Utility to generate mock data for development and testing
 */
export const mockDataGenerator = {
  /**
   * Generate a random speaker
   */
  generateSpeaker(): Speaker {
    const speakers: Speaker[] = [
      { name: "Alex Johnson", initials: "AJ", color: "#4F6BED" },
      { name: "Sarah Chen", initials: "SC", color: "#D83B01" },
      { name: "Michael Rodriguez", initials: "MR", color: "#107C10" },
      { name: "Emma Wilson", initials: "EW", color: "#8764B8" },
      { name: "David Kim", initials: "DK", color: "#FFB900" },
    ]

    return speakers[Math.floor(Math.random() * speakers.length)]
  },

  /**
   * Generate a random transcript entry
   */
  generateTranscriptEntry(id: number): TranscriptEntry {
    const speaker = this.generateSpeaker()
    const phrases = [
      "I think we should focus on improving the user experience.",
      "The latest metrics show a significant increase in user engagement.",
      "We need to address the performance issues before the next release.",
      "I've completed the tasks assigned to me for this sprint.",
      "Can we discuss the timeline for the upcoming features?",
      "The feedback from the beta testers has been mostly positive.",
      "We should prioritize fixing the critical bugs reported by users.",
    ]

    const now = new Date()

    return {
      id,
      speaker,
      text: phrases[Math.floor(Math.random() * phrases.length)],
      timestamp: now.toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
      }),
    }
  },

  /**
   * Generate a random recording
   */
  generateRecording(id: number): Recording {
    const date = new Date()
    date.setDate(date.getDate() - Math.floor(Math.random() * 30))

    return {
      id,
      title: `Meeting ${date.toLocaleDateString("en-US", { month: "short", day: "numeric" })}`,
      date: date.toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" }),
      time: date.toLocaleTimeString("en-US", { hour: "numeric", minute: "2-digit", hour12: true }),
      duration: `${Math.floor(Math.random() * 60) + 15} minutes`,
      participants: Math.floor(Math.random() * 10) + 2,
      timestamp: date,
    }
  },

  /**
   * Generate a list of mock transcript entries
   */
  generateTranscript(count: number): TranscriptEntry[] {
    return Array.from({ length: count }, (_, i) => this.generateTranscriptEntry(i + 1))
  },

  /**
   * Generate a list of mock recordings
   */
  generateRecordings(count: number): Recording[] {
    return Array.from({ length: count }, (_, i) => this.generateRecording(i + 1))
  },
}

