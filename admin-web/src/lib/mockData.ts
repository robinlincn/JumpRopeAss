export function randomInt(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1)) + min
}

export function pad2(n: number) {
  return n < 10 ? `0${n}` : `${n}`
}

export function randomDateWithinDays(days: number) {
  const now = new Date()
  const ms = randomInt(0, days) * 24 * 3600 * 1000 + randomInt(0, 24 * 3600 * 1000)
  return new Date(now.getTime() - ms)
}

export function formatDateTime(dt: Date) {
  const y = dt.getFullYear()
  const m = pad2(dt.getMonth() + 1)
  const d = pad2(dt.getDate())
  const hh = pad2(dt.getHours())
  const mm = pad2(dt.getMinutes())
  return `${y}-${m}-${d} ${hh}:${mm}`
}

