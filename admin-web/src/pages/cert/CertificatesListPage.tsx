import { Button, Card, Col, Form, Grid, Input, Modal, Row, Select, Space, Table, Tag, Typography, Upload, message } from 'antd'
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
  assessStatus?: number | null
  score?: number | null
  points?: number | null
  rank?: number | null
  associationName?: string | null
  certLevel?: string | null
  projectName?: string | null
  province?: string | null
  city?: string | null
  district?: string | null
  issueScene: IssueScene
  issueAt: string
  status: CertStatus
  fileUrl?: string | null
  createdAt: string
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

function areaText(p?: string | null, c?: string | null, d?: string | null) {
  return [p, c, d].filter((x) => !!String(x ?? '').trim()).join(' / ') || '-'
}

export function CertificatesListPage() {
  const screens = Grid.useBreakpoint()
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
  const [importRows, setImportRows] = useState<any[] | null>(null)
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
        assessStatus: x.assessStatus != null ? Number(x.assessStatus) : null,
        score: x.score != null ? Number(x.score) : null,
        points: x.points != null ? Number(x.points) : null,
        rank: x.rank != null ? Number(x.rank) : null,
        associationName: x.associationName ?? null,
        certLevel: x.certLevel ?? null,
        projectName: x.projectName ?? null,
        province: x.province ?? null,
        city: x.city ?? null,
        district: x.district ?? null,
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

  const columns: ColumnsType<CertRow> = [
    { title: '证书ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '证书编号', dataIndex: 'certNo', width: 220, render: (v: string) => <Typography.Text strong>{v}</Typography.Text> },
    { title: '证书类型', dataIndex: 'certTypeName', width: 180, render: (_, r) => r.certTypeName ?? '-' },
    { title: '持证人', dataIndex: 'holderName', width: 160, render: (_, r) => r.holderName ?? '-' },
    { title: '赛号', dataIndex: 'raceNo', width: 110, render: (v?: number | null) => (v != null ? String(v) : '-') },
    { title: '组别', dataIndex: 'groupName', width: 160, render: (v?: string | null) => v ?? '-' },
    { title: '性别', dataIndex: 'gender', width: 90, render: (v?: number | null) => genderText(v) },
    {
      title: '手机号',
      dataIndex: 'holderMobileMasked',
      width: 150,
      render: (_, r) => (
        <Typography.Text copyable={r.holderMobile ? { text: r.holderMobile } : false}>
          {r.holderMobileMasked || r.holderMobile || '-'}
        </Typography.Text>
      ),
    },
    {
      title: '身份证号',
      dataIndex: 'holderIdCardNoMasked',
      width: 200,
      render: (_, r) => (
        <Typography.Text copyable={r.holderIdCardNo ? { text: r.holderIdCardNo } : false}>
          {r.holderIdCardNoMasked || r.holderIdCardNo || '-'}
        </Typography.Text>
      ),
    },
    { title: '考核状态', dataIndex: 'assessStatus', width: 110, render: (v?: number | null) => (v != null ? String(v) : '-') },
    { title: '成绩', dataIndex: 'score', width: 110, render: (v?: number | null) => (v != null ? String(v) : '-') },
    { title: '分数', dataIndex: 'points', width: 110, render: (v?: number | null) => (v != null ? String(v) : '-') },
    { title: '名次', dataIndex: 'rank', width: 100, render: (v?: number | null) => (v != null ? String(v) : '-') },
    { title: '所属协会', dataIndex: 'associationName', width: 180, render: (v?: string | null) => v ?? '-' },
    { title: '证书等级', dataIndex: 'certLevel', width: 150, render: (v?: string | null) => v ?? '-' },
    { title: '项目', dataIndex: 'projectName', width: 180, render: (v?: string | null) => v ?? '-' },
    { title: '省市县', dataIndex: 'area', width: 220, render: (_, r) => areaText(r.province, r.city, r.district) },
    { title: '发证场景', dataIndex: 'issueScene', width: 120, render: (v: IssueScene) => <Tag>{sceneText(v)}</Tag> },
    { title: '发证时间', dataIndex: 'issueAt', width: 170 },
    { title: '状态', dataIndex: 'status', width: 110, render: (v: CertStatus) => statusTag(v) },
    {
      title: '二维码',
      dataIndex: 'qr',
      width: 160,
      render: (_, r) => (
        <Space direction="vertical" size={6}>
          <img
            src={certQrImgUrl(r.certNo)}
            alt=""
            width={92}
            height={92}
            style={{ borderRadius: 12, border: '1px solid rgba(0,0,0,0.06)' }}
          />
          <Typography.Text
            type="secondary"
            copyable={{ text: certQueryUrl(r.certNo) }}
            style={{ fontSize: 12, maxWidth: 140 }}
            ellipsis={{ tooltip: certQueryUrl(r.certNo) }}
          >
            复制查询链接
          </Typography.Text>
        </Space>
      ),
    },
    { title: '文件', dataIndex: 'fileUrl', width: 220, render: (v?: string | null) => (v ? <Typography.Text ellipsis={{ tooltip: v }}>{v}</Typography.Text> : '-') },
    { title: '创建时间', dataIndex: 'createdAt', width: 170 },
  ]

  const exportTemplate = () => {
    const ws = XLSX.utils.json_to_sheet([
      {
        certTypeCode: 'coach_cert',
        holderName: '张三',
        mobile: '13800000000',
        idCardNo: '4301************1234',
        issueAt: '2026-03-20 10:00:00',
        certNo: '',
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
      return {
        certTypeCode: get(['certTypeCode', '证书类型编码', '证书类型code']),
        holderName: get(['holderName', '姓名', '持证人', '持证人姓名']),
        mobile: get(['mobile', '手机号', '手机']),
        idCardNo: get(['idCardNo', '身份证号', '证件号']),
        issueAt: get(['issueAt', '发证时间', '签发时间']),
        certNo: get(['certNo', '证书编号']),
      }
    }
    setImportRows(json.map(mapRow).filter((x) => x.certTypeCode || x.holderName || x.mobile))
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
          })),
        }),
      })
      if (res.code !== 0) {
        message.error(res.message || '导入失败')
        return
      }
      message.success(`导入完成：新增${res.data?.created ?? 0}，跳过${res.data?.skipped ?? 0}`)
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
          certNo: x.certNo,
          certTypeCode: x.certTypeCode,
          certTypeName: x.certTypeName,
          holderName: x.holderName,
          holderMobile: x.holderMobile,
          holderIdCardNo: x.holderIdCardNoMasked,
          issueScene: x.issueScene,
          issueAt: x.issueAt ? String(x.issueAt).replace('T', ' ') : '',
          status: x.status,
          fileUrl: x.fileUrl,
          createdAt: x.createdAt ? String(x.createdAt).replace('T', ' ') : '',
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
            模板字段：certTypeCode（证书类型编码）/ holderName（姓名）/ mobile（手机号）/ idCardNo（可选）/ issueAt（可选）/ certNo（可选，留空自动生成）
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

