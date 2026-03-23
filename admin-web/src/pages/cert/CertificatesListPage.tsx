import { Button, Card, Col, Form, Input, Modal, Row, Select, Space, Table, Tag, Typography, Upload, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { DownloadOutlined, ReloadOutlined, UploadOutlined } from '@ant-design/icons'
import { useEffect, useMemo, useState } from 'react'
import type { Key } from 'react'
import { apiFetch } from '../../lib/api'
import * as XLSX from 'xlsx'

type CertStatus = 1 | 2
type IssueScene = 1 | 2

type CertTypeOption = { id: number; name: string; code: string; status: number }

type CertRow = {
  id: number
  certNo: string
  certTypeId: number
  certTypeCode?: string | null
  certTypeName?: string | null
  raceNo?: number | null
  groupName?: string | null
  holderPersonId: number
  holderName?: string | null
  gender?: number | null
  holderMobile?: string | null
  holderMobileMasked?: string | null
  holderIdCardNo?: string | null
  holderIdCardNoMasked?: string | null
  assessStatus?: string | null
  score?: number | null
  points?: number | null
  rank?: string | null
  associationName?: string | null
  certLevel?: string | null
  projectName?: string | null
  province?: string | null
  city?: string | null
  district?: string | null
  issuerName?: string | null
  eventDate?: string | null
  issueDate?: string | null
  validPeriod?: string | null
  coachName?: string | null
  location?: string | null
  titleName?: string | null
  unitName?: string | null
  eventName?: string | null
  issueOrg?: string | null
  referrerName?: string | null
  referrerPhone?: string | null
  issueScene: IssueScene
  issueAt: string
  status: CertStatus
  fileUrl?: string | null
  createdAt: string
}

type ImportRow = {
  certTypeCode: string
  holderName: string
  mobile: string
  idCardNo: string
  issueAt: string
  certNo: string
  raceNo: string
  groupName: string
  gender: string
  rank: string
  resultStatus: string
  score: string
  associationName: string
  roleName: string
  projectName: string
  level: string
  province: string
  city: string
  district: string
  points: string
  signPerson: string
  activityDate: string
  issueDate: string
  validPeriodText: string
  coachName: string
  location: string
  title: string
  orgName: string
  activityName: string
  issuerOrg: string
  recommender: string
  recommenderPhone: string
}

type FilterValues = {
  certTypeId?: number | 'all'
  status?: CertStatus | 'all'
  keyword?: string
}

function sceneText(v: IssueScene) {
  return v === 1 ? '首次' : '补证'
}

function statusTag(v: CertStatus) {
  const t = v === 1 ? { text: '有效', color: 'green' } : { text: '作废', color: 'red' }
  return <Tag color={t.color}>{t.text}</Tag>
}

function genderText(v?: number | null) {
  if (v === 1) return '男'
  if (v === 2) return '女'
  return '-'
}

export function CertificatesListPage() {
  const [form] = Form.useForm<FilterValues>()
  const [types, setTypes] = useState<CertTypeOption[]>([])
  const [rows, setRows] = useState<CertRow[]>([])
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [selectedRowKeys, setSelectedRowKeys] = useState<Key[]>([])
  const [importOpen, setImportOpen] = useState(false)
  const [printOpen, setPrintOpen] = useState(false)
  const [importing, setImporting] = useState(false)
  const [exporting, setExporting] = useState(false)
  const [resetting, setResetting] = useState(false)
  const [importRows, setImportRows] = useState<ImportRow[] | null>(null)
  const [printBgDataUrl, setPrintBgDataUrl] = useState<string | null>(null)
  const [queryPrefix, setQueryPrefix] = useState<string>(() => {
    const k = 'admin_cert_query_prefix'
    try {
      const v = localStorage.getItem(k)
      if (v) return v
    } catch {
    }
    return `${window.location.origin.replace(/\/$/, '')}/#/cert/verify?certNo=`
  })

  const loadSystemSettings = async () => {
    const k = 'admin_cert_query_prefix'
    try {
      const v = localStorage.getItem(k)
      if (v) return
    } catch {
    }

    const res = await apiFetch<any>('/api/v1/admin/system-settings')
    if (res.code !== 0) return
    const p = String(res.data?.system?.certQueryPrefix ?? '').trim()
    if (!p) return
    setQueryPrefix(p)
  }

  const loadTypes = async () => {
    const res = await apiFetch<any>('/api/v1/admin/cert-types')
    if (res.code !== 0) return
    setTypes((res.data?.items ?? []).map((x: any) => ({ id: Number(x.id), name: String(x.name), code: String(x.code), status: Number(x.status) })))
  }

  const loadData = async (p = page, ps = pageSize) => {
    setLoading(true)
    try {
      const v = form.getFieldsValue() as FilterValues
      const qs = new URLSearchParams()
      const keyword = (v.keyword ?? '').trim()
      const status = v.status ?? 'all'
      const certTypeId = v.certTypeId ?? 'all'
      if (keyword) qs.set('keyword', keyword)
      if (status !== 'all') qs.set('status', String(status))
      if (certTypeId !== 'all') qs.set('certTypeId', String(certTypeId))
      qs.set('page', String(p))
      qs.set('pageSize', String(ps))
      const res = await apiFetch<any>(`/api/v1/admin/certificates?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setTotal(res.data?.total ?? 0)
      const items = (res.data?.items ?? []).map((x: any) => ({
        id: Number(x.id),
        certNo: String(x.certNo),
        certTypeId: Number(x.certTypeId),
        certTypeCode: x.certTypeCode ?? null,
        certTypeName: x.certTypeName ?? null,
        raceNo: x.raceNo != null ? Number(x.raceNo) : null,
        groupName: x.groupName ?? null,
        holderPersonId: Number(x.holderPersonId),
        holderName: x.holderName ?? null,
        gender: x.gender != null ? Number(x.gender) : null,
        holderMobile: x.holderMobile ?? null,
        holderMobileMasked: x.holderMobileMasked ?? null,
        holderIdCardNo: x.holderIdCardNo ?? null,
        holderIdCardNoMasked: x.holderIdCardNoMasked ?? null,
        assessStatus: x.assessStatus ?? null,
        score: x.score != null ? Number(x.score) : null,
        points: x.points != null ? Number(x.points) : null,
        rank: x.rank ?? null,
        associationName: x.associationName ?? null,
        certLevel: x.certLevel ?? null,
        projectName: x.projectName ?? null,
        province: x.province ?? null,
        city: x.city ?? null,
        district: x.district ?? null,
        issuerName: x.issuerName ?? null,
        eventDate: x.eventDate ?? null,
        issueDate: x.issueDate ?? null,
        validPeriod: x.validPeriod ?? null,
        coachName: x.coachName ?? null,
        location: x.location ?? null,
        titleName: x.titleName ?? null,
        unitName: x.unitName ?? null,
        eventName: x.eventName ?? null,
        issueOrg: x.issueOrg ?? null,
        referrerName: x.referrerName ?? null,
        referrerPhone: x.referrerPhone ?? null,
        issueScene: Number(x.issueScene) as IssueScene,
        issueAt: String(x.issueAt ?? '').replace('T', ' '),
        status: Number(x.status) as CertStatus,
        fileUrl: x.fileUrl ?? null,
        createdAt: String(x.createdAt ?? '').replace('T', ' '),
      }))
      setRows(items)
    } catch (e) {
      console.error(e)
      message.error('加载失败')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadTypes()
    loadData()
    loadSystemSettings()
  }, [])

  const selectedRows = useMemo(() => {
    const s = new Set(selectedRowKeys.map((x) => Number(x)))
    return rows.filter((r) => s.has(r.id))
  }, [selectedRowKeys, rows])

  const certQueryUrl = (certNo: string) => `${queryPrefix}${encodeURIComponent(certNo)}`
  const certQrImgUrl = (certNo: string) =>
    `https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=${encodeURIComponent(certQueryUrl(certNo))}`

  const columns: ColumnsType<CertRow> = useMemo(
    () => [
      { title: '赛号', dataIndex: 'raceNo', key: 'raceNo', width: 80, align: 'center' },
      { title: '组别', dataIndex: 'groupName', key: 'groupName', width: 140 },
      { title: '姓名', dataIndex: 'holderName', key: 'holderName', width: 100 },
      { title: '性别', dataIndex: 'gender', key: 'gender', width: 60, render: genderText, align: 'center' },
      { title: '手机号', dataIndex: 'holderMobile', key: 'holderMobile', width: 120 },
      { title: '身份证号', dataIndex: 'holderIdCardNo', key: 'holderIdCardNo', width: 180 },
      { title: '名次', dataIndex: 'rank', key: 'rank', width: 80, align: 'center' },
      {
        title: '考核状态',
        dataIndex: 'assessStatus',
        key: 'assessStatus',
        width: 100,
        render: (v) => (v ? String(v) : '-'),
      },
      { title: '成绩', dataIndex: 'score', key: 'score', width: 80 },
      { title: '所属协会', dataIndex: 'associationName', key: 'associationName', width: 140 },
      { title: '角色', dataIndex: 'certTypeName', key: 'certTypeName', width: 100 },
      { title: '项目', dataIndex: 'projectName', key: 'projectName', width: 140 },
      { title: '等级', dataIndex: 'certLevel', key: 'certLevel', width: 100 },
      { title: '省', dataIndex: 'province', key: 'province', width: 80 },
      { title: '市', dataIndex: 'city', key: 'city', width: 80 },
      { title: '区', dataIndex: 'district', key: 'district', width: 80 },
      { title: '分数', dataIndex: 'points', key: 'points', width: 80 },
      { title: '签证人', dataIndex: 'issuerName', key: 'issuerName', width: 100 },
      {
        title: '活动日期',
        dataIndex: 'eventDate',
        key: 'eventDate',
        width: 120,
        render: (v) => (v ? v.substring(0, 10) : '-'),
      },
      {
        title: '发证日期',
        dataIndex: 'issueDate',
        key: 'issueDate',
        width: 120,
        render: (v) => (v ? v.substring(0, 10) : '-'),
      },
      { title: '有效期', dataIndex: 'validPeriod', key: 'validPeriod', width: 180 },
      { title: '教练员', dataIndex: 'coachName', key: 'coachName', width: 100 },
      { title: '地点', dataIndex: 'location', key: 'location', width: 140 },
      { title: '证书编号', dataIndex: 'certNo', key: 'certNo', width: 180 },
      { title: '称号', dataIndex: 'titleName', key: 'titleName', width: 120 },
      { title: '所属单位', dataIndex: 'unitName', key: 'unitName', width: 180 },
      { title: '活动名称', dataIndex: 'eventName', key: 'eventName', width: 200 },
      { title: '发证机构', dataIndex: 'issueOrg', key: 'issueOrg', width: 180 },
      { title: '推荐人', dataIndex: 'referrerName', key: 'referrerName', width: 100 },
      { title: '推荐人电话', dataIndex: 'referrerPhone', key: 'referrerPhone', width: 120 },
      {
        title: '场景',
        dataIndex: 'issueScene',
        key: 'issueScene',
        width: 80,
        render: (v) => sceneText(v),
        align: 'center',
        fixed: 'right',
      },
      {
        title: '状态',
        dataIndex: 'status',
        key: 'status',
        width: 80,
        render: (v) => statusTag(v),
        align: 'center',
        fixed: 'right',
      },
    ],
    [],
  )

  const exportTemplate = () => {
    const ws = XLSX.utils.json_to_sheet([
      {
        赛号: '',
        组别: 'U12',
        姓名: '张三',
        性别: '男',
        手机号: '13800000000',
        身份证号: '4301************12',
        名次: '1',
        考核状态: '通过',
        成绩: '80',
        所属协会: '湖南省跳绳运动协会',
        角色: '学员',
        项目: '30秒单摇跳速度',
        等级: '一段',
        省: '湖南省',
        市: '长沙市',
        区: '',
        分数: '80',
        签证人: '向彭',
        活动日期: '2026-03-20',
        发证日期: '2026-03-20',
        有效期: '',
        教练员: '李四',
        地点: '湖南长沙',
        证书编号: '',
        称号: '',
        所属单位: '测试单位',
        活动名称: '2026年测试活动',
        发证机构: '《湖南省跳绳运动协会》',
        推荐人: '',
        推荐人电话: '',
      },
    ])
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'template')
    XLSX.writeFile(wb, `证书导入模板.xlsx`)
  }

  const parseImportFile = async (file: File) => {
    const buf = await file.arrayBuffer()
    const wb = XLSX.read(buf, { type: 'array' })
    const sheetName = wb.SheetNames[0]
    const ws = wb.Sheets[sheetName]
    const json = XLSX.utils.sheet_to_json<Record<string, any>>(ws, { defval: '' })
    const norm = (k: string) => k.trim().toLowerCase()
    const mapRow = (r: Record<string, any>) => {
      const keys = Object.keys(r)
      const get = (cands: string[]) => {
        for (const cand of cands) {
          const k = keys.find((x) => norm(x) === norm(cand))
          if (k) return String(r[k] ?? '').trim()
        }
        return ''
      }
      
      const roleName = get(['角色'])
      let typeCode = get(['certTypeCode', '证书类型编码', '证书类型code'])
      if (!typeCode) typeCode = roleName
      if (typeCode === '教练员') typeCode = 'coach_cert'
      if (typeCode === '学员' || typeCode === '运动员') typeCode = 'athlete_level'
      if (typeCode === '裁判员') typeCode = 'judge_cert'

      return {
        certTypeCode: typeCode,
        holderName: get(['holderName', '姓名', '持证人', '持证人姓名', '姓名（中文）']),
        mobile: get(['mobile', '手机号', '手机']),
        idCardNo: get(['idCardNo', '身份证号', '证件号']),
        issueAt: get(['issueAt', '发证日期', '发证时间', '签发时间']),
        certNo: get(['certNo', '证书编号']),
        raceNo: get(['赛号']),
        groupName: get(['组别']),
        gender: get(['性别']),
        rank: get(['名次']),
        resultStatus: get(['考核状态', '考核结果']),
        score: get(['成绩']),
        associationName: get(['所属协会']),
        roleName,
        projectName: get(['项目']),
        level: get(['等级']),
        province: get(['省']),
        city: get(['市']),
        district: get(['区']),
        points: get(['分数']),
        signPerson: get(['签证人']),
        activityDate: get(['活动日期']),
        issueDate: get(['发证日期']),
        validPeriodText: get(['有效期']),
        coachName: get(['教练员']),
        location: get(['地点']),
        title: get(['称号']),
        orgName: get(['所属单位']),
        activityName: get(['活动名称']),
        issuerOrg: get(['发证机构']),
        recommender: get(['推荐人']),
        recommenderPhone: get(['推荐人电话']),
      }
    }
    const mapped = json.map(mapRow)
    setImportRows(mapped.filter((x) => x.certTypeCode && (x.holderName || x.idCardNo || x.mobile)))
  }

  const doImport = async () => {
    if (!importRows?.length) {
      message.warning('请先选择并解析导入文件')
      return
    }
    setImporting(true)
    try {
      const res = await apiFetch<any>('/api/v1/admin/certificates/import', {
        method: 'POST',
        body: JSON.stringify({
          items: importRows.map((x) => ({
            certTypeCode: x.certTypeCode,
            holderName: x.holderName,
            mobile: x.mobile,
            idCardNo: x.idCardNo || null,
            issueAt: x.issueAt || null,
            certNo: x.certNo || null,
            raceNo: x.raceNo || null,
            groupName: x.groupName || null,
            gender: x.gender || null,
            rank: x.rank || null,
            resultStatus: x.resultStatus || null,
            score: x.score || null,
            associationName: x.associationName || null,
            roleName: x.roleName || null,
            projectName: x.projectName || null,
            level: x.level || null,
            province: x.province || null,
            city: x.city || null,
            district: x.district || null,
            points: x.points || null,
            signPerson: x.signPerson || null,
            activityDate: x.activityDate || null,
            issueDate: x.issueDate || null,
            validPeriodText: x.validPeriodText || null,
            coachName: x.coachName || null,
            location: x.location || null,
            title: x.title || null,
            orgName: x.orgName || null,
            activityName: x.activityName || null,
            issuerOrg: x.issuerOrg || null,
            recommender: x.recommender || null,
            recommenderPhone: x.recommenderPhone || null,
          })),
        }),
      })
      if (res.code !== 0) {
        message.error(res.message || '导入失败')
        return
      }
      const created = Number(res.data?.created ?? 0)
      const skipped = Number(res.data?.skipped ?? 0)
      const errorCount = Number(res.data?.errorCount ?? 0)
      const errors: string[] = Array.isArray(res.data?.errors) ? res.data.errors : []

      if (errorCount > 0) {
        Modal.error({
          title: `导入存在错误：成功${created}，跳过${skipped}，错误${errorCount}`,
          width: 720,
          content: (
            <div style={{ whiteSpace: 'pre-wrap' }}>
              {(errors.slice(0, 30).join('\n') || '未返回错误明细') + (errorCount > 30 ? `\n...（仅展示前30条）` : '')}
            </div>
          ),
        })
        if (created > 0) await loadData(1, pageSize)
        return
      }

      message.success(`导入完成：新增${created}，跳过${skipped}`)
      setImportOpen(false)
      setImportRows(null)
      await loadData(1, pageSize)
    } finally {
      setImporting(false)
    }
  }

  const fetchAllForExport = async () => {
    const v = form.getFieldsValue() as FilterValues
    const qsBase = new URLSearchParams()
    const keyword = (v.keyword ?? '').trim()
    const status = v.status ?? 'all'
    const certTypeId = v.certTypeId ?? 'all'
    if (keyword) qsBase.set('keyword', keyword)
    if (status !== 'all') qsBase.set('status', String(status))
    if (certTypeId !== 'all') qsBase.set('certTypeId', String(certTypeId))

    const out: any[] = []
    const max = 5000
    const ps = 50
    for (let p = 1; p <= 200; p++) {
      const qs = new URLSearchParams(qsBase)
      qs.set('page', String(p))
      qs.set('pageSize', String(ps))
      const res = await apiFetch<any>(`/api/v1/admin/certificates?${qs.toString()}`)
      if (res.code !== 0) throw new Error(res.message || '导出加载失败')
      const items = res.data?.items ?? []
      out.push(...items)
      if (out.length >= max) break
      if (items.length < ps) break
    }
    return out.slice(0, max)
  }

  const doExport = async () => {
    setExporting(true)
    try {
      const all = await fetchAllForExport()
      const ws = XLSX.utils.json_to_sheet(
        all.map((x: any) => ({
          赛号: x.raceNo ?? '',
          组别: x.groupName ?? '',
          姓名: x.holderName ?? '',
          性别: x.gender === 1 ? '男' : x.gender === 2 ? '女' : '',
          手机号: x.holderMobile ?? '',
          身份证号: x.holderIdCardNoMasked ?? x.holderIdCardNo ?? '',
          名次: x.rank ?? '',
          考核状态: x.assessStatus ?? '',
          成绩: x.score ?? '',
          所属协会: x.associationName ?? '',
          角色: x.certTypeName ?? '',
          项目: x.projectName ?? '',
          等级: x.certLevel ?? '',
          省: x.province ?? '',
          市: x.city ?? '',
          区: x.district ?? '',
          分数: x.points ?? '',
          签证人: x.issuerName ?? '',
          活动日期: x.eventDate ? String(x.eventDate).substring(0, 10) : '',
          发证日期: x.issueDate ? String(x.issueDate).substring(0, 10) : x.issueAt ? String(x.issueAt).substring(0, 10) : '',
          有效期: x.validPeriod ?? '',
          教练员: x.coachName ?? '',
          地点: x.location ?? '',
          证书编号: x.certNo ?? '',
          称号: x.titleName ?? '',
          所属单位: x.unitName ?? '',
          活动名称: x.eventName ?? '',
          发证机构: x.issueOrg ?? '',
          推荐人: x.referrerName ?? '',
          推荐人电话: x.referrerPhone ?? '',
        })),
      )
      const wb = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(wb, ws, 'certificates')
      XLSX.writeFile(wb, `证书导出_${new Date().toISOString().slice(0, 10)}.xlsx`)
      message.success(`已导出 ${all.length} 条`)
    } catch (e: any) {
      message.error(e?.message || '导出失败')
    } finally {
      setExporting(false)
    }
  }

  const doReset = async () => {
    if (!selectedRowKeys.length) {
      message.warning('请先勾选需要重置的证书')
      return
    }
    Modal.confirm({
      title: '重置证书',
      content: `将作废所选证书，并生成新的证书编号（补证场景）。数量：${selectedRowKeys.length}`,
      okText: '确认重置',
      cancelText: '取消',
      onOk: async () => {
        setResetting(true)
        try {
          const res = await apiFetch<any>('/api/v1/admin/certificates/reset', {
            method: 'POST',
            body: JSON.stringify({ ids: selectedRowKeys.map((x) => Number(x)) }),
          })
          if (res.code !== 0) {
            message.error(res.message || '重置失败')
            return
          }
          message.success(`已重置：作废${res.data?.voided ?? 0}，新建${res.data?.created ?? 0}`)
          setSelectedRowKeys([])
          await loadData(1, pageSize)
        } finally {
          setResetting(false)
        }
      },
    })
  }

  const doClearCertificates = async () => {
    Modal.confirm({
      title: '清空证书表',
      content: '将删除证书表中的所有记录，用于导入测试。此操作不可恢复。',
      okText: '确认清空',
      okButtonProps: { danger: true },
      cancelText: '取消',
      onOk: async () => {
        const res = await apiFetch<any>('/api/v1/admin/certificates/clear', { method: 'POST' })
        if (res.code !== 0) {
          message.error(res.message || '清空失败')
          return
        }
        message.success(`已清空证书表：删除${Number(res.data?.deleted ?? 0)}条`)
        setSelectedRowKeys([])
        setPage(1)
        await loadData(1, pageSize)
      },
    })
  }

  const buildPrintHtml = (items: CertRow[], bg: string | null) => {
    const pages = items
      .map((c) => {
        const qr = certQrImgUrl(c.certNo)
        const issueAt = c.issueAt || ''
        const holder = c.holderName || ''
        const type = c.certTypeName || ''
        return `
<div class="page">
  <div class="bg"></div>
  <div class="field certNo">${c.certNo}</div>
  <div class="field holder">${holder}</div>
  <div class="field type">${type}</div>
  <div class="field issueAt">${issueAt}</div>
  <img class="qr" src="${qr}" alt="qr" />
</div>`
      })
      .join('\n')

    const bgCss = bg ? `background-image: url('${bg}');` : `background: white;`
    return `<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <title>证书批量打印</title>
    <style>
      @page { size: A4; margin: 0; }
      body { margin: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif; }
      .page { width: 210mm; height: 297mm; position: relative; page-break-after: always; overflow: hidden; }
      .bg { position: absolute; inset: 0; ${bgCss} background-size: cover; background-position: center; }
      .field { position: absolute; color: rgba(0,0,0,0.88); font-weight: 700; letter-spacing: 0.2px; }
      .certNo { left: 30mm; top: 40mm; font-size: 16pt; }
      .holder { left: 30mm; top: 70mm; font-size: 22pt; }
      .type { left: 30mm; top: 86mm; font-size: 14pt; font-weight: 600; }
      .issueAt { left: 30mm; top: 102mm; font-size: 12pt; font-weight: 600; }
      .qr { position: absolute; right: 18mm; bottom: 18mm; width: 34mm; height: 34mm; border-radius: 6mm; background: white; padding: 2mm; }
    </style>
  </head>
  <body>
    ${pages}
    <script>window.onload = () => { setTimeout(() => window.print(), 200) }</script>
  </body>
</html>`
  }

  const openPrint = () => {
    const items = selectedRows.length ? selectedRows : []
    if (!items.length) {
      message.warning('请先勾选需要打印的证书')
      return
    }
    const html = buildPrintHtml(items, printBgDataUrl)
    const w = window.open('', '_blank')
    if (!w) {
      message.error('无法打开新窗口，请检查浏览器拦截设置')
      return
    }
    w.document.open()
    w.document.write(html)
    w.document.close()
  }

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Form
          form={form}
          layout="vertical"
          initialValues={{ status: 'all', certTypeId: 'all' }}
          onValuesChange={() => {
            setPage(1)
            loadData(1, pageSize)
          }}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="certTypeId" label="证书类型">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    ...types.filter((x) => x.status === 1).map((x) => ({ value: x.id, label: x.name })),
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="status" label="状态">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 1, label: '有效' },
                    { value: 2, label: '作废' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={12}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="证书编号/证书ID/持证人ID" allowClear />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Space wrap>
                <Button onClick={() => setPrintOpen(true)} disabled={!selectedRowKeys.length}>
                  打印证书
                </Button>
                <Button danger loading={resetting} onClick={doReset} disabled={!selectedRowKeys.length}>
                  重置证书
                </Button>
                <Button danger onClick={doClearCertificates}>
                  清空证书表
                </Button>
                <Button icon={<UploadOutlined />} onClick={() => setImportOpen(true)}>
                  导入
                </Button>
                <Button icon={<DownloadOutlined />} loading={exporting} onClick={doExport}>
                  导出
                </Button>
                <Button icon={<ReloadOutlined />} loading={loading} onClick={() => loadData()}>
                  刷新
                </Button>
                <Button
                  onClick={() => {
                    form.resetFields()
                    setPage(1)
                    loadData(1, pageSize)
                  }}
                >
                  重置
                </Button>
              </Space>
            </Col>
          </Row>
        </Form>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table
          rowKey="id"
          columns={columns}
          dataSource={rows}
          loading={loading}
          rowSelection={{
            selectedRowKeys,
            onChange: (keys) => setSelectedRowKeys(keys),
          }}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setPageSize(ps)
              loadData(p, ps)
            },
          }}
          scroll={{ x: 3200 }}
        />
      </Card>

      <Modal
        title="导入证书（按模板）"
        open={importOpen}
        onCancel={() => {
          setImportOpen(false)
          setImportRows(null)
        }}
        onOk={doImport}
        okText="开始导入"
        cancelText="取消"
        confirmLoading={importing}
        width={720}
      >
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Typography.Text type="secondary">
            模板字段与 Doc/运动员报名.xlsx 一致；至少需要：角色、姓名、证书编号（可选）、发证日期（可选）。手机号/身份证号可为空，但建议至少保留其一便于匹配人员。
          </Typography.Text>
          <Space wrap>
            <Button onClick={exportTemplate}>下载模板</Button>
            <Upload
              accept=".xlsx,.xls"
              maxCount={1}
              beforeUpload={(file) => {
                parseImportFile(file as File)
                return false
              }}
              onRemove={() => setImportRows(null)}
            >
              <Button icon={<UploadOutlined />}>选择Excel</Button>
            </Upload>
          </Space>
          <Typography.Text>已解析：{importRows?.length ?? 0} 条</Typography.Text>
        </Space>
      </Modal>

      <Modal
        title="批量打印证书"
        open={printOpen}
        onCancel={() => setPrintOpen(false)}
        onOk={() => {
          try {
            localStorage.setItem('admin_cert_query_prefix', queryPrefix)
          } catch {
          }
          setPrintOpen(false)
          openPrint()
        }}
        okText="打开打印预览"
        cancelText="取消"
        width={720}
      >
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Typography.Text type="secondary">将按所选证书逐页打印（A4）。可上传背景模板图，系统会把姓名/编号/类型/时间/二维码叠加到模板上。</Typography.Text>
          <div>
            <Typography.Text strong>查询链接前缀</Typography.Text>
            <Input
              value={queryPrefix}
              onChange={(e) => setQueryPrefix(e.target.value)}
              placeholder="例如：https://xxx.com/#/cert/verify?certNo="
              style={{ marginTop: 8 }}
            />
          </div>
          <div>
            <Typography.Text strong>证书背景模板（图片）</Typography.Text>
            <Space style={{ marginTop: 8 }} wrap>
              <Upload
                accept="image/*"
                maxCount={1}
                beforeUpload={(file) => {
                  const f = file as File
                  const reader = new FileReader()
                  reader.onload = () => setPrintBgDataUrl(String(reader.result))
                  reader.readAsDataURL(f)
                  return false
                }}
                onRemove={() => {
                  setPrintBgDataUrl(null)
                  return true
                }}
              >
                <Button icon={<UploadOutlined />}>选择图片</Button>
              </Upload>
              <Button onClick={() => setPrintBgDataUrl(null)}>不使用背景</Button>
            </Space>
            {printBgDataUrl ? (
              <div style={{ marginTop: 12, borderRadius: 12, overflow: 'hidden', border: '1px solid rgba(0,0,0,0.06)' }}>
                <img src={printBgDataUrl} alt="" style={{ width: '100%', display: 'block' }} />
              </div>
            ) : null}
          </div>
          <Typography.Text>当前选择：{selectedRowKeys.length} 条</Typography.Text>
        </Space>
      </Modal>
    </Space>
  )
}
