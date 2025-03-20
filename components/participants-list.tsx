"use client"

import { useState } from "react"
import { Avatar, Text, Caption1, Button, Input, Divider, Card, CardHeader } from "@fluentui/react-components"
import { MicOffRegular, EditRegular, SearchRegular, DismissRegular } from "@fluentui/react-icons"
import { knownUsers, initialParticipants } from "@/data/participants"
import type { KnownUser, Participant } from "@/types"

export default function ParticipantsList() {
  const [participants, setParticipants] = useState(initialParticipants)
  const [selectedParticipant, setSelectedParticipant] = useState<Participant | null>(null)
  const [searchQuery, setSearchQuery] = useState("")
  const [showAssignPanel, setShowAssignPanel] = useState(false)

  // Filter known users based on search query
  const filteredUsers = knownUsers.filter(
    (user) =>
      user.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.role.toLowerCase().includes(searchQuery.toLowerCase()),
  )

  // Handle assigning a known user to a guest participant
  const handleAssignUser = (user: KnownUser) => {
    if (!selectedParticipant) return

    setParticipants((prev) =>
      prev.map((p) =>
        p.id === selectedParticipant.id
          ? {
              ...p,
              name: user.name,
              role: user.role,
              initials: user.initials,
              color: user.color,
              isGuest: false,
            }
          : p,
      ),
    )

    setSelectedParticipant(null)
    setShowAssignPanel(false)
  }

  const openAssignPanel = (participant: Participant) => {
    setSelectedParticipant(participant)
    setShowAssignPanel(true)
  }

  const closeAssignPanel = () => {
    setSelectedParticipant(null)
    setShowAssignPanel(false)
  }

  return (
    <div style={{ position: "relative", height: "100%" }}>
      {/* Using div elements instead of List/ListItem */}
      <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
        {participants.map((participant) => (
          <div
            key={participant.id}
            style={{
              padding: "8px",
              borderRadius: "4px",
              backgroundColor: "transparent",
              transition: "background-color 0.2s",
              cursor: "default",
            }}
          >
            <div style={{ display: "flex", alignItems: "center", width: "100%" }}>
              <div style={{ position: "relative" }}>
                <Avatar
                  name={participant.name}
                  initials={participant.initials}
                  color="colorful"
                  style={{ backgroundColor: participant.color }}
                  size={32}
                />
                {participant.isSpeaking && (
                  <div
                    style={{
                      position: "absolute",
                      bottom: 0,
                      right: 0,
                      width: "10px",
                      height: "10px",
                      borderRadius: "50%",
                      backgroundColor: "#107C10",
                      border: "2px solid white",
                    }}
                  />
                )}
              </div>
              <div style={{ marginLeft: "12px", flex: 1 }}>
                <div style={{ display: "flex", alignItems: "center" }}>
                  <Text>{participant.name}</Text>
                  {participant.isGuest && (
                    <Button
                      icon={<EditRegular />}
                      appearance="subtle"
                      size="small"
                      style={{ marginLeft: "4px" }}
                      onClick={() => openAssignPanel(participant)}
                    />
                  )}
                </div>
                <Caption1>{participant.role}</Caption1>
              </div>
              {participant.isMuted && <MicOffRegular style={{ color: "#707070" }} />}
            </div>
          </div>
        ))}
      </div>

      {/* Custom slide-in panel instead of Dialog */}
      {showAssignPanel && (
        <div
          style={{
            position: "absolute",
            top: 0,
            right: 0,
            bottom: 0,
            width: "100%",
            backgroundColor: "white",
            boxShadow: "-2px 0 10px rgba(0,0,0,0.1)",
            zIndex: 100,
            padding: "16px",
            display: "flex",
            flexDirection: "column",
          }}
        >
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
            <Text weight="semibold" size={400}>
              {selectedParticipant ? `Assign Identity to ${selectedParticipant.name}` : "Assign Identity"}
            </Text>
            <Button icon={<DismissRegular />} appearance="subtle" onClick={closeAssignPanel} aria-label="Close" />
          </div>

          <Divider style={{ margin: "0 -16px 16px" }} />

          <Text style={{ marginBottom: "16px" }}>Select a person from the list or search for someone:</Text>

          <div style={{ position: "relative", marginBottom: "16px" }}>
            <SearchRegular style={{ position: "absolute", left: "8px", top: "8px", color: "#707070" }} />
            <Input
              placeholder="Search by name or role..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              style={{ paddingLeft: "32px" }}
            />
          </div>

          <div style={{ flex: 1, overflowY: "auto" }}>
            {filteredUsers.map((user) => (
              <Card
                key={user.id}
                appearance="outline"
                style={{
                  marginBottom: "8px",
                  cursor: "pointer",
                  padding: "8px",
                }}
                onClick={() => handleAssignUser(user)}
              >
                <CardHeader
                  header={
                    <div style={{ display: "flex", alignItems: "center" }}>
                      <Avatar
                        name={user.name}
                        initials={user.initials}
                        color="colorful"
                        style={{ backgroundColor: user.color }}
                        size={32}
                      />
                      <div style={{ marginLeft: "12px" }}>
                        <Text>{user.name}</Text>
                        <Caption1>{user.role}</Caption1>
                      </div>
                    </div>
                  }
                />
              </Card>
            ))}
            {filteredUsers.length === 0 && (
              <Text align="center" style={{ display: "block", padding: "16px" }}>
                No matching users found
              </Text>
            )}
          </div>

          <div style={{ marginTop: "16px" }}>
            <Button appearance="secondary" onClick={closeAssignPanel} style={{ width: "100%" }}>
              Cancel
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}

