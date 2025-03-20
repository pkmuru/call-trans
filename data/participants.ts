import type { KnownUser, Participant } from "@/types"

// Sample known users for lookup
export const knownUsers: KnownUser[] = [
  { id: 101, name: "Alex Johnson", role: "Product Manager", initials: "AJ", color: "#4F6BED" },
  { id: 102, name: "Sarah Chen", role: "UX Designer", initials: "SC", color: "#D83B01" },
  { id: 103, name: "Michael Rodriguez", role: "Developer", initials: "MR", color: "#107C10" },
  { id: 104, name: "Emma Wilson", role: "QA Engineer", initials: "EW", color: "#8764B8" },
  { id: 105, name: "David Kim", role: "Product Owner", initials: "DK", color: "#FFB900" },
  { id: 106, name: "Lisa Park", role: "Marketing", initials: "LP", color: "#038387" },
  { id: 107, name: "James Wilson", role: "Sales", initials: "JW", color: "#CA5010" },
  { id: 108, name: "Olivia Martinez", role: "Customer Support", initials: "OM", color: "#881798" },
]

// Initial participants with some as "Guest" speakers
export const initialParticipants: Participant[] = [
  {
    id: 1,
    name: "Guest 1",
    role: "Unknown",
    isSpeaking: true,
    isMuted: false,
    initials: "G1",
    color: "#4F6BED",
    isGuest: true,
  },
  {
    id: 2,
    name: "Sarah Chen",
    role: "UX Designer",
    isSpeaking: false,
    isMuted: false,
    initials: "SC",
    color: "#D83B01",
    isGuest: false,
  },
  {
    id: 3,
    name: "Guest 2",
    role: "Unknown",
    isSpeaking: false,
    isMuted: true,
    initials: "G2",
    color: "#107C10",
    isGuest: true,
  },
  {
    id: 4,
    name: "Emma Wilson",
    role: "QA Engineer",
    isSpeaking: false,
    isMuted: false,
    initials: "EW",
    color: "#8764B8",
    isGuest: false,
  },
  {
    id: 5,
    name: "Guest 3",
    role: "Unknown",
    isSpeaking: false,
    isMuted: true,
    initials: "G3",
    color: "#FFB900",
    isGuest: true,
  },
]

