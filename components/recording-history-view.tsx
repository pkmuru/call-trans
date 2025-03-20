"use client"

import { useState } from "react"
import {
  Text,
  Subtitle1,
  Caption1,
  Button,
  Menu,
  MenuTrigger,
  MenuList,
  MenuItem,
  MenuPopover,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Tooltip,
  Link,
} from "@fluentui/react-components"
import {
  ArrowDownRegular,
  ArrowUpRegular,
  ChevronDownRegular,
  CalendarRegular,
  ClockRegular,
  PeopleRegular,
  DeleteRegular,
  InfoRegular,
  ArrowDownloadRegular,
} from "@fluentui/react-icons"
import HistoryTranscriptView from "@/components/history-transcript-view"
import { recordingHistory } from "@/data/recordings"

export default function RecordingHistoryView() {
  const [selectedRecording, setSelectedRecording] = useState<any>(null)
  const [sortField, setSortField] = useState("date")
  const [sortDirection, setSortDirection] = useState("desc")

  const handleSort = (field: string) => {
    if (sortField === field) {
      // Toggle direction if clicking the same field
      setSortDirection(sortDirection === "asc" ? "desc" : "asc")
    } else {
      // Set new field and default to descending
      setSortField(field)
      setSortDirection("desc")
    }
  }

  // Sort recordings based on current sort settings
  const sortedRecordings = [...recordingHistory].sort((a, b) => {
    let comparison = 0

    if (sortField === "date") {
      comparison = a.timestamp.getTime() - b.timestamp.getTime()
    } else if (sortField === "title") {
      comparison = a.title.localeCompare(b.title)
    } else if (sortField === "duration") {
      comparison = Number.parseInt(a.duration) - Number.parseInt(b.duration)
    } else if (sortField === "participants") {
      comparison = a.participants - b.participants
    }

    return sortDirection === "asc" ? comparison : -comparison
  })

  return (
    <div style={{ display: "flex", flex: 1, overflow: "hidden" }}>
      {/* Recordings list */}
      <RecordingsList
        sortedRecordings={sortedRecordings}
        selectedRecording={selectedRecording}
        setSelectedRecording={setSelectedRecording}
        sortField={sortField}
        sortDirection={sortDirection}
        handleSort={handleSort}
      />

      {/* Transcript view */}
      {selectedRecording ? <RecordingTranscriptView recording={selectedRecording} /> : <SelectRecordingPrompt />}
    </div>
  )
}

interface RecordingsListProps {
  sortedRecordings: any[]
  selectedRecording: any
  setSelectedRecording: (recording: any) => void
  sortField: string
  sortDirection: string
  handleSort: (field: string) => void
}

function RecordingsList({
  sortedRecordings,
  selectedRecording,
  setSelectedRecording,
  sortField,
  sortDirection,
  handleSort,
}: RecordingsListProps) {
  return (
    <div
      style={{
        width: "50%",
        borderRight: "1px solid #e0e0e0",
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      <div style={{ padding: "16px", borderBottom: "1px solid #e0e0e0" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
          <Subtitle1>Recording History</Subtitle1>
          <SortMenu sortField={sortField} sortDirection={sortDirection} handleSort={handleSort} />
        </div>
      </div>

      <div style={{ flex: 1, overflowY: "auto" }}>
        <Table>
          <TableHeader>
            <TableRow>
              <SortableTableHeader
                title="Title"
                field="title"
                currentSortField={sortField}
                currentSortDirection={sortDirection}
                onSort={handleSort}
              />
              <SortableTableHeader
                title="Date"
                field="date"
                currentSortField={sortField}
                currentSortDirection={sortDirection}
                onSort={handleSort}
              />
              <SortableTableHeader
                title="Duration"
                field="duration"
                currentSortField={sortField}
                currentSortDirection={sortDirection}
                onSort={handleSort}
              />
              <SortableTableHeader
                title="Participants"
                field="participants"
                currentSortField={sortField}
                currentSortDirection={sortDirection}
                onSort={handleSort}
              />
            </TableRow>
          </TableHeader>
          <TableBody>
            {sortedRecordings.map((recording) => (
              <TableRow
                key={recording.id}
                style={{
                  cursor: "pointer",
                  backgroundColor: selectedRecording?.id === recording.id ? "#f0f0f0" : "transparent",
                }}
                onClick={() => setSelectedRecording(recording)}
              >
                <TableCell>
                  <Link>{recording.title}</Link>
                </TableCell>
                <TableCell>
                  <div style={{ display: "flex", alignItems: "center" }}>
                    <CalendarRegular style={{ marginRight: "4px", fontSize: "12px" }} />
                    {recording.date}
                  </div>
                  <div style={{ display: "flex", alignItems: "center", fontSize: "12px", color: "#707070" }}>
                    <ClockRegular style={{ marginRight: "4px", fontSize: "12px" }} />
                    {recording.time}
                  </div>
                </TableCell>
                <TableCell>{recording.duration}</TableCell>
                <TableCell>
                  <div style={{ display: "flex", alignItems: "center" }}>
                    <PeopleRegular style={{ marginRight: "4px", fontSize: "12px" }} />
                    {recording.participants}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}

interface SortMenuProps {
  sortField: string
  sortDirection: string
  handleSort: (field: string) => void
}

function SortMenu({ sortField, sortDirection, handleSort }: SortMenuProps) {
  return (
    <Menu>
      <MenuTrigger disableButtonEnhancement>
        <Button appearance="subtle" icon={<ChevronDownRegular />} iconPosition="after">
          Sort by
        </Button>
      </MenuTrigger>
      <MenuPopover>
        <MenuList>
          <MenuItem
            onClick={() => handleSort("date")}
            icon={
              sortField === "date" ? sortDirection === "asc" ? <ArrowUpRegular /> : <ArrowDownRegular /> : undefined
            }
          >
            Date
          </MenuItem>
          <MenuItem
            onClick={() => handleSort("title")}
            icon={
              sortField === "title" ? sortDirection === "asc" ? <ArrowUpRegular /> : <ArrowDownRegular /> : undefined
            }
          >
            Title
          </MenuItem>
          <MenuItem
            onClick={() => handleSort("duration")}
            icon={
              sortField === "duration" ? sortDirection === "asc" ? <ArrowUpRegular /> : <ArrowDownRegular /> : undefined
            }
          >
            Duration
          </MenuItem>
          <MenuItem
            onClick={() => handleSort("participants")}
            icon={
              sortField === "participants" ? (
                sortDirection === "asc" ? (
                  <ArrowUpRegular />
                ) : (
                  <ArrowDownRegular />
                )
              ) : undefined
            }
          >
            Participants
          </MenuItem>
        </MenuList>
      </MenuPopover>
    </Menu>
  )
}

interface SortableTableHeaderProps {
  title: string
  field: string
  currentSortField: string
  currentSortDirection: string
  onSort: (field: string) => void
}

function SortableTableHeader({
  title,
  field,
  currentSortField,
  currentSortDirection,
  onSort,
}: SortableTableHeaderProps) {
  return (
    <TableHeaderCell onClick={() => onSort(field)} style={{ cursor: "pointer" }}>
      <div style={{ display: "flex", alignItems: "center" }}>
        {title}
        {currentSortField === field &&
          (currentSortDirection === "asc" ? (
            <ArrowUpRegular style={{ marginLeft: "4px" }} />
          ) : (
            <ArrowDownRegular style={{ marginLeft: "4px" }} />
          ))}
      </div>
    </TableHeaderCell>
  )
}

function RecordingTranscriptView({ recording }: { recording: any }) {
  return (
    <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>
      <div style={{ padding: "16px", borderBottom: "1px solid #e0e0e0" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <Subtitle1>{recording.title}</Subtitle1>
            <Caption1>
              {recording.date} • {recording.time} • {recording.duration}
            </Caption1>
          </div>
          <div style={{ display: "flex", gap: "8px" }}>
            <Tooltip content="Download transcript" relationship="label">
              <Button icon={<ArrowDownloadRegular />} appearance="subtle" aria-label="Download" />
            </Tooltip>
            <Tooltip content="Delete recording" relationship="label">
              <Button icon={<DeleteRegular />} appearance="subtle" aria-label="Delete" />
            </Tooltip>
          </div>
        </div>
      </div>
      <div style={{ flex: 1, padding: "16px", overflowY: "auto" }}>
        <HistoryTranscriptView recordingId={recording.id} />
      </div>
    </div>
  )
}

function SelectRecordingPrompt() {
  return (
    <div
      style={{
        flex: 1,
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        height: "100%",
        padding: "32px",
        textAlign: "center",
      }}
    >
      <div
        style={{
          width: "80px",
          height: "80px",
          borderRadius: "50%",
          backgroundColor: "#f0f0f0",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          marginBottom: "24px",
        }}
      >
        <InfoRegular style={{ fontSize: "32px", color: "#707070" }} />
      </div>
      <Text size={500} weight="semibold" style={{ marginBottom: "16px" }}>
        Select a recording
      </Text>
      <Text style={{ maxWidth: "400px" }}>
        Choose a recording from the list on the left to view its transcript and details.
      </Text>
    </div>
  )
}

