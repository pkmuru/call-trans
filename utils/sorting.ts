/**
 * Sorts an array of objects by a specified field and direction
 * @param items Array of objects to sort
 * @param field Field to sort by
 * @param direction Sort direction ('asc' or 'desc')
 * @returns Sorted array
 */
export function sortItems<T extends Record<string, any>>(items: T[], field: keyof T, direction: "asc" | "desc"): T[] {
  return [...items].sort((a, b) => {
    let comparison = 0

    // Handle different data types
    if (a[field] instanceof Date && b[field] instanceof Date) {
      comparison = a[field].getTime() - b[field].getTime()
    } else if (typeof a[field] === "string" && typeof b[field] === "string") {
      comparison = a[field].localeCompare(b[field])
    } else if (typeof a[field] === "number" && typeof b[field] === "number") {
      comparison = a[field] - b[field]
    } else {
      // Try to convert to string for comparison
      comparison = String(a[field]).localeCompare(String(b[field]))
    }

    return direction === "asc" ? comparison : -comparison
  })
}

