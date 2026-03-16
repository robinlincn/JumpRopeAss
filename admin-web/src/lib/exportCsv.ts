export type CsvColumn<T> = {
  title: string
  value: (row: T) => string | number | null | undefined
}

function escapeCsvValue(value: string) {
  if (value.includes('"') || value.includes(',') || value.includes('\n') || value.includes('\r')) {
    return `"${value.replaceAll('"', '""')}"`
  }
  return value
}

export function toCsv<T>(rows: T[], columns: CsvColumn<T>[]) {
  const header = columns.map((c) => escapeCsvValue(c.title)).join(',')
  const body = rows
    .map((r) =>
      columns
        .map((c) => {
          const raw = c.value(r)
          const s = raw === null || raw === undefined ? '' : String(raw)
          return escapeCsvValue(s)
        })
        .join(','),
    )
    .join('\n')

  return `${header}\n${body}\n`
}

export function downloadTextFile(filename: string, content: string, mime = 'text/plain;charset=utf-8') {
  const blob = new Blob([content], { type: mime })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

export function downloadCsv<T>(filename: string, rows: T[], columns: CsvColumn<T>[]) {
  const csv = toCsv(rows, columns)
  downloadTextFile(filename, csv, 'text/csv;charset=utf-8')
}

