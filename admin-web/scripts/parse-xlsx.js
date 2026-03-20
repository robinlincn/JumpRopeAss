import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'
import xlsx from 'xlsx'

function readXlsx(filePath) {
  const wb = xlsx.readFile(filePath, { cellDates: true })
  const out = { file: path.basename(filePath), sheets: [] }
  for (const name of wb.SheetNames) {
    const ws = wb.Sheets[name]
    const rows = xlsx.utils.sheet_to_json(ws, { header: 1, defval: '' })
    const headers = Array.isArray(rows[0]) ? rows[0].map(v => String(v).trim()) : []
    const sample = Array.isArray(rows[1]) ? rows[1] : []
    out.sheets.push({ name, headers, sample })
  }
  return out
}

function main() {
  const __filename = fileURLToPath(import.meta.url)
  const __dirname = path.dirname(__filename)
  const docDir = path.resolve(__dirname, '../../Doc')
  const files = fs.readdirSync(docDir)
    .filter(f => /\.(xlsx|xls)$/i.test(f))
    .map(f => path.join(docDir, f))
  const result = files.map(readXlsx)
  console.log(JSON.stringify(result, null, 2))
}

main()
