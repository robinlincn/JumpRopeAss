import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, Modal, Row, Space, Table, Tag, Typography, Upload, Select, DatePicker, message, ConfigProvider } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { UploadFile } from 'antd/es/upload/interface'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined, BoldOutlined, ItalicOutlined, UnderlineOutlined, AlignLeftOutlined, AlignCenterOutlined, AlignRightOutlined, UnorderedListOutlined, OrderedListOutlined, LinkOutlined, PictureOutlined, VideoCameraOutlined, UndoOutlined, RedoOutlined, FullscreenOutlined, FullscreenExitOutlined, TableOutlined } from '@ant-design/icons'
import { useEffect, useRef, useState } from 'react'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import 'dayjs/locale/zh-cn'
import zhCN from 'antd/es/locale/zh_CN'
dayjs.locale('zh-cn')

import { apiFetch, apiUpload } from '../../lib/api'

type EventType = '赛事' | '评定' | '培训'

type Contact = { name: string; phone: string }

type RowItem = {
  id: number
  coverUrl?: string
  logoUrl?: string
  name: string
  type: EventType
  bannerUrl?: string
  slogan?: string
  address?: string
  lng?: number
  lat?: number
  hostOrg?: string
  coOrgs?: string[]
  contacts?: Contact[]
  projects?: string[]
  signupStart?: string
  signupEnd?: string
  eventStart?: string
  eventEnd?: string
  overview?: string
  status: number
  updatedAt: string
}

type EditValues = {
  coverFileList?: UploadFile[]
  coverUrl?: string
  logoFileList?: UploadFile[]
  logoUrl?: string
  name: string
  type: EventType
  bannerFileList?: UploadFile[]
  bannerUrl?: string
  slogan?: string
  address?: string
  lng?: number
  lat?: number
  hostOrg: string
  coOrgs?: string[]
  contacts?: Contact[]
  projects?: string[]
  signupRange?: Dayjs[]
  eventRange?: Dayjs[]
  overview?: string
  status?: number
}

function RichEditor({ value, onChange }: { value?: string; onChange?: (v: string) => void }) {
  const editorRef = useRef<HTMLDivElement | null>(null)
  const [fullscreen, setFullscreen] = useState(false)
  const apply = (cmd: string, arg?: string) => {
    editorRef.current?.focus()
    document.execCommand(cmd, false, arg)
    const v = editorRef.current?.innerHTML ?? ''
    onChange?.(v)
  }
  const insertHTML = (html: string) => {
    editorRef.current?.focus()
    document.execCommand('insertHTML', false, html)
    const v = editorRef.current?.innerHTML ?? ''
    onChange?.(v)
  }
  return (
    <Card style={{ borderRadius: 12 }} styles={{ body: { padding: 10 } }}>
      <Space wrap style={{ marginBottom: 8 }}>
        <Button size="small" icon={<BoldOutlined />} onClick={() => apply('bold')} />
        <Button size="small" icon={<ItalicOutlined />} onClick={() => apply('italic')} />
        <Button size="small" icon={<UnderlineOutlined />} onClick={() => apply('underline')} />
        <Button size="small" icon={<AlignLeftOutlined />} onClick={() => apply('justifyLeft')} />
        <Button size="small" icon={<AlignCenterOutlined />} onClick={() => apply('justifyCenter')} />
        <Button size="small" icon={<AlignRightOutlined />} onClick={() => apply('justifyRight')} />
        <Button size="small" icon={<UnorderedListOutlined />} onClick={() => apply('insertUnorderedList')} />
        <Button size="small" icon={<OrderedListOutlined />} onClick={() => apply('insertOrderedList')} />
        <Button size="small" icon={<LinkOutlined />} onClick={() => {
          const url = window.prompt('输入链接地址') || ''
          if (url) apply('createLink', url)
        }} />
        <Upload accept="image/*" maxCount={1} beforeUpload={() => false} showUploadList={false} onChange={(info) => {
          const f = info.file.originFileObj as File | undefined
          if (!f) return
          if (f.size > 2 * 1024 * 1024) {
            message.error('图片过大，请选择小于2MB的图片')
            return
          }
          const fd = new FormData()
          fd.append('file', f)
          apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
            if (resp.code !== 0) {
              message.error(resp.message || '上传失败')
              return
            }
            insertHTML(`<img src="${resp.data.url}" style="max-width:100%;border-radius:8px;" />`)
          })
        }}>
          <Button size="small" icon={<PictureOutlined />} />
        </Upload>
        <Upload accept="video/*" maxCount={1} beforeUpload={() => false} showUploadList={false} onChange={(info) => {
          const f = info.file.originFileObj as File | undefined
          if (!f) return
          const url = URL.createObjectURL(f)
          insertHTML(`<video src="${url}" controls style="max-width:100%;border-radius:8px;"></video>`)
        }}>
          <Button size="small" icon={<VideoCameraOutlined />} />
        </Upload>
        <Button size="small" icon={<TableOutlined />} onClick={() => insertHTML('<table style="width:100%;border-collapse:collapse;"><tr><th style="border:1px solid #ddd;padding:6px;">列1</th><th style="border:1px solid #ddd;padding:6px;">列2</th></tr><tr><td style="border:1px solid #ddd;padding:6px;">内容</td><td style="border:1px solid #ddd;padding:6px;">内容</td></tr></table>')} />
        <Button size="small" icon={<UndoOutlined />} onClick={() => apply('undo')} />
        <Button size="small" icon={<RedoOutlined />} onClick={() => apply('redo')} />
        <Button size="small" icon={fullscreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />} onClick={() => setFullscreen((v) => !v)} />
      </Space>
      <div
        ref={editorRef}
        contentEditable
        onInput={(e) => onChange?.((e.target as HTMLDivElement).innerHTML)}
        style={{
          minHeight: 200,
          border: '1px solid #E5E7EB',
          borderRadius: 8,
          padding: 12,
          ...(fullscreen ? { position: 'fixed', inset: 20, zIndex: 1000, background: '#fff', overflow: 'auto' } : {}),
        }}
        dangerouslySetInnerHTML={{ __html: value || '' }}
      />
    </Card>
  )
}

export function EventsManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<RowItem[]>([])
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<RowItem | null>(null)
  const [form] = Form.useForm<EditValues>()
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(20)
  const [total, setTotal] = useState(0)
  const uploadingCountRef = useRef(0)

  const startUpload = () => {
    uploadingCountRef.current += 1
  }

  const finishUpload = () => {
    uploadingCountRef.current = Math.max(0, uploadingCountRef.current - 1)
  }

  const parseJson = <T,>(v: any, fallback: T): T => {
    if (!v) return fallback
    if (typeof v === 'string') {
      try {
        return JSON.parse(v) as T
      } catch {
        return fallback
      }
    }
    return v as T
  }

  const typeLabel = (t: number): EventType => (t === 1 ? '赛事' : t === 2 ? '评定' : '培训')
  const typeValue = (t: EventType): number => (t === '赛事' ? 1 : t === '评定' ? 2 : 3)

  const statusLabel = (s: number) =>
    s === 0 ? '草稿' : s === 1 ? '报名中' : s === 2 ? '报名截止' : s === 3 ? '进行中' : s === 4 ? '已结束' : '下线'

  const statusColor = (s: number) =>
    s === 1 ? 'green' : s === 0 ? 'default' : s === 2 ? 'gold' : s === 3 ? 'blue' : s === 4 ? 'default' : 'red'

  const loadData = async (p = page, ps = size, kw = keyword) => {
    setLoading(true)
    try {
      const qs = new URLSearchParams()
      if (kw) qs.set('keyword', kw)
      qs.set('page', String(p))
      qs.set('size', String(ps))
      const res = await apiFetch<any>(`/api/v1/admin/events?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setTotal(res.data.total ?? 0)
      const items = (res.data.items ?? []).map((x: any) => ({
          id: x.id,
          coverUrl: x.coverUrl,
          logoUrl: x.logoUrl,
          name: x.title,
          type: typeLabel(x.eventType),
          bannerUrl: x.bannerUrl,
          slogan: x.slogan,
          address: x.location,
          hostOrg: x.hostOrg,
          coOrgs: parseJson<string[]>(x.coOrgs, []),
          contacts: parseJson<Contact[]>(x.contacts, [{ name: '', phone: '' }]),
          projects: parseJson<string[]>(x.projects, []),
          signupStart: x.signupStartAt?.replace('T', ' '),
          signupEnd: x.signupEndAt?.replace('T', ' '),
          eventStart: x.eventStartAt?.replace('T', ' '),
          eventEnd: x.eventEndAt?.replace('T', ' '),
          overview: x.descriptionHtml,
          status: x.status ?? 0,
          updatedAt: x.updatedAt?.replace('T', ' '),
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
    loadData()
  }, [])

  const assocOptions = [
    { value: '长沙市跳绳协会', label: '长沙市跳绳协会' },
    { value: '宁乡市滨江小学', label: '宁乡市滨江小学' },
    { value: '星锐少儿体能中心', label: '星锐少儿体能中心' },
  ]
  const projectOptions = [
    { value: '300个定数计时赛', label: '300个定数计时赛' },
    { value: '两人交互速度赛', label: '两人交互速度赛' },
  ]

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '活动名称',
      dataIndex: 'name',
      render: (text: string, r) => (
        <Space>
          <Badge status={r.type === '赛事' ? 'processing' : 'default'} />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    { title: '类型', dataIndex: 'type', width: 100, render: (v: EventType) => <Tag>{v}</Tag> },
    { title: '主办单位', dataIndex: 'hostOrg', width: 200 },
    {
      title: '状态',
      dataIndex: 'status',
      width: 120,
      render: (v: number) => <Tag color={statusColor(v)}>{statusLabel(v)}</Tag>,
    },
    {
      title: '报名时间',
      dataIndex: 'signupRange',
      width: 220,
      render: (_, r) => (
        <Typography.Text type="secondary">{r.signupStart && r.signupEnd ? `${r.signupStart} ~ ${r.signupEnd}` : '-'}</Typography.Text>
      ),
    },
    {
      title: '活动时间',
      dataIndex: 'eventRange',
      width: 220,
      render: (_, r) => (
        <Typography.Text type="secondary">{r.eventStart && r.eventEnd ? `${r.eventStart} ~ ${r.eventEnd}` : '-'}</Typography.Text>
      ),
    },
    { title: '更新时间', dataIndex: 'updatedAt', width: 170 },
    {
      title: '操作',
      key: 'action',
      width: 200,
      fixed: screens.lg ? 'right' : undefined,
      render: (_, r) => (
        <Space size={6} wrap>
          <Button
            size="small"
            shape="circle"
            title="编辑"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => {
              setEditing(r)
              const coverFileList: UploadFile[] = r.coverUrl ? [{ uid: '-0', name: 'cover.png', status: 'done', url: r.coverUrl }] : []
              const logoFileList: UploadFile[] = r.logoUrl ? [{ uid: '-1', name: 'logo.png', status: 'done', url: r.logoUrl }] : []
              const bannerFileList: UploadFile[] = r.bannerUrl ? [{ uid: '-2', name: 'banner.png', status: 'done', url: r.bannerUrl }] : []
              form.setFieldsValue({
                coverFileList,
                logoFileList,
                bannerFileList,
                name: r.name,
                type: r.type,
                slogan: r.slogan,
                address: r.address,
                hostOrg: r.hostOrg!,
                coOrgs: r.coOrgs ?? [],
                contacts: r.contacts && r.contacts.length ? r.contacts : [{ name: '', phone: '' }],
                projects: r.projects ?? [],
                overview: r.overview,
                status: r.status,
                coverUrl: r.coverUrl,
                logoUrl: r.logoUrl,
                bannerUrl: r.bannerUrl,
                signupRange: r.signupStart && r.signupEnd ? [dayjs(r.signupStart), dayjs(r.signupEnd)] : undefined,
                eventRange: r.eventStart && r.eventEnd ? [dayjs(r.eventStart), dayjs(r.eventEnd)] : undefined,
              })
              setDrawerOpen(true)
            }}
          />
          <Button
            size="small"
            shape="circle"
            title="删除"
            danger
            icon={<DeleteOutlined />}
            onClick={() => {
              Modal.confirm({
                title: '数据一旦删除不可恢复，您确定要删除吗？',
                okText: '删除',
                cancelText: '取消',
                onOk: async () => {
                  const resp = await apiFetch(`/api/v1/admin/events/${r.id}`, { method: 'DELETE' })
                  if (resp.code !== 0) {
                    message.error(resp.message || '删除失败')
                    return
                  }
                  message.success('已删除')
                  loadData(1, size, keyword)
                },
              })
            }}
          />
        </Space>
      ),
    },
  ]

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Space wrap>
          <Input
            allowClear
            style={{ width: 260 }}
            placeholder="活动名称关键词"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            onPressEnter={() => {
              setPage(1)
              loadData(1, size, keyword)
            }}
          />
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditing(null)
              form.resetFields()
              form.setFieldsValue({ type: '赛事', status: 1, coOrgs: [], contacts: [{ name: '', phone: '' }], projects: [] })
              setDrawerOpen(true)
            }}
          >
            新建活动
          </Button>
          <Button
            icon={<ReloadOutlined />}
            loading={loading}
            onClick={async () => {
              await loadData(page, size, keyword)
              message.success('已刷新')
            }}
          >
            刷新
          </Button>
        </Space>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table
          rowKey="id"
          columns={columns}
          dataSource={rows}
          loading={loading}
          scroll={{ x: 1450 }}
          pagination={{
            current: page,
            pageSize: size,
            total,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setSize(ps)
              loadData(p, ps, keyword)
            },
          }}
        />
      </Card>

      <ConfigProvider locale={zhCN}>
      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 800 : '100%'}
        title={editing ? '编辑活动' : '新建活动'}
        extra={
          <Space>
            <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                Modal.confirm({
                  title: '您确定要保存吗？',
                  okText: '保存',
                  cancelText: '取消',
                  onOk: async () => {
                    let v: EditValues
                    try {
                      v = await form.validateFields()
                    } catch (err: any) {
                      const field = err?.errorFields?.[0]
                      const msg = field?.errors?.[0] || '请完善必填项'
                      message.error(msg)
                      if (field?.name) form.scrollToField(field.name)
                      return
                    }
                    if (uploadingCountRef.current > 0) {
                      message.error('图片上传中，请稍后保存')
                      return
                    }
                    if (!v.logoUrl) {
                      message.error('请上传活动Logo')
                      form.scrollToField('logoFileList')
                      return
                    }
                    if (!v.bannerUrl) {
                      message.error('请上传活动Banner图')
                      form.scrollToField('bannerFileList')
                      return
                    }

                    const fmt = (d?: Dayjs) => (d ? d.format('YYYY-MM-DDTHH:mm:ss') : undefined)
                    const signupStart = fmt(v.signupRange?.[0])
                    const signupEnd = fmt(v.signupRange?.[1])
                    const eventStart = fmt(v.eventRange?.[0])
                    const eventEnd = fmt(v.eventRange?.[1])

                    const dto = {
                      eventType: typeValue(v.type),
                      title: v.name,
                      coverUrl: v.coverUrl,
                      logoUrl: v.logoUrl,
                      bannerUrl: v.bannerUrl,
                      slogan: v.slogan,
                      location: v.address,
                      hostOrg: v.hostOrg,
                      coOrgs: v.coOrgs,
                      contacts: v.contacts,
                      projects: v.projects,
                      signupStartAt: signupStart,
                      signupEndAt: signupEnd,
                      eventStartAt: eventStart,
                      eventEndAt: eventEnd,
                      descriptionHtml: v.overview,
                      status: v.status ?? 1,
                    }

                    if (editing) {
                      const resp = await apiFetch(`/api/v1/admin/events/${editing.id}`, {
                        method: 'PUT',
                        body: JSON.stringify(dto),
                      })
                      if (resp.code !== 0) {
                        message.error(resp.message || '保存失败')
                        return
                      }
                    } else {
                      const resp = await apiFetch('/api/v1/admin/events', {
                        method: 'POST',
                        body: JSON.stringify(dto),
                      })
                      if (resp.code !== 0) {
                        message.error(resp.message || '保存失败')
                        return
                      }
                    }
                    message.success('已保存')
                    setDrawerOpen(false)
                    setPage(1)
                    loadData(1, size, keyword)
                  },
                })
              }}
            >
              保存
            </Button>
          </Space>
        }
      >
        <Form
          form={form}
          layout="vertical"
          requiredMark
          initialValues={{ type: '赛事', status: 1, coOrgs: [], contacts: [{ name: '', phone: '' }], projects: [] }}
        >
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="coverFileList" label="活动封面图" valuePropName="fileList" getValueFromEvent={(e) => e?.fileList}>
                <Upload
                  listType="picture-card"
                  maxCount={1}
                  accept="image/*"
                  beforeUpload={() => false}
                  onChange={(info) => {
                    if (info.fileList.length === 0) {
                      form.setFieldsValue({ coverUrl: undefined })
                      return
                    }
                    if (info.file.url) {
                      form.setFieldsValue({ coverUrl: info.file.url })
                      return
                    }
                    const f = info.file.originFileObj as File | undefined
                    if (!f) return
                    if (f.size > 2 * 1024 * 1024) {
                      message.error('图片过大，请选择小于2MB的图片')
                      form.setFieldsValue({ coverFileList: [], coverUrl: undefined })
                      return
                    }
                    const fd = new FormData()
                    fd.append('file', f)
                    startUpload()
                    apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd)
                      .then((resp) => {
                        if (resp.code !== 0) {
                          message.error(resp.message || '上传失败')
                          form.setFieldsValue({ coverFileList: [], coverUrl: undefined })
                          return
                        }
                        form.setFieldsValue({ coverUrl: resp.data.url })
                      })
                      .finally(() => finishUpload())
                  }}
                >
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>上传封面</div>
                  </div>
                </Upload>
              </Form.Item>
              <Form.Item name="coverUrl" hidden>
                <Input />
              </Form.Item>
              <Form.Item name="logoFileList" label="活动Logo" valuePropName="fileList" getValueFromEvent={(e) => e?.fileList}>
                <Upload
                  listType="picture-card"
                  maxCount={1}
                  accept="image/*"
                  beforeUpload={() => false}
                  onChange={(info) => {
                    if (info.fileList.length === 0) {
                      form.setFieldsValue({ logoUrl: undefined })
                      return
                    }
                    if (info.file.url) {
                      form.setFieldsValue({ logoUrl: info.file.url })
                      return
                    }
                    const f = info.file.originFileObj as File | undefined
                    if (!f) return
                    if (f.size > 2 * 1024 * 1024) {
                      message.error('图片过大，请选择小于2MB的图片')
                      form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                      return
                    }
                    const fd = new FormData()
                    fd.append('file', f)
                    startUpload()
                    apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd)
                      .then((resp) => {
                        if (resp.code !== 0) {
                          message.error(resp.message || '上传失败')
                          form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                          return
                        }
                        form.setFieldsValue({ logoUrl: resp.data.url })
                      })
                      .finally(() => finishUpload())
                  }}
                >
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>上传Logo</div>
                  </div>
                </Upload>
              </Form.Item>
              <Form.Item name="logoUrl" hidden>
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="name" label="活动名称" rules={[{ required: true, message: '请输入活动名称' }]}>
                <Input />
              </Form.Item>
              <Form.Item name="type" label="活动类型" rules={[{ required: true }]}>
                <Select options={[{ value: '赛事', label: '赛事' }, { value: '评定', label: '评定' }, { value: '培训', label: '培训' }]} />
              </Form.Item>
              <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
                <Select
                  options={[
                    { value: 0, label: '草稿' },
                    { value: 1, label: '报名中' },
                    { value: 2, label: '报名截止' },
                    { value: 3, label: '进行中' },
                    { value: 4, label: '已结束' },
                    { value: 5, label: '下线' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item
                name="bannerFileList"
                label="活动Banner图（建议1125×540，约2:1）"
                valuePropName="fileList"
                getValueFromEvent={(e) => e?.fileList}
              >
                <Upload
                  listType="picture-card"
                  maxCount={1}
                  accept="image/*"
                  beforeUpload={() => false}
                  onChange={(info) => {
                    if (info.fileList.length === 0) {
                      form.setFieldsValue({ bannerUrl: undefined })
                      return
                    }
                    if (info.file.url) {
                      form.setFieldsValue({ bannerUrl: info.file.url })
                      return
                    }
                    const f = info.file.originFileObj as File | undefined
                    if (!f) return
                    if (f.size > 2 * 1024 * 1024) {
                      message.error('图片过大，请选择小于2MB的图片')
                      form.setFieldsValue({ bannerFileList: [], bannerUrl: undefined })
                      return
                    }
                    const fd = new FormData()
                    fd.append('file', f)
                    startUpload()
                    apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd)
                      .then((resp) => {
                        if (resp.code !== 0) {
                          message.error(resp.message || '上传失败')
                          form.setFieldsValue({ bannerFileList: [], bannerUrl: undefined })
                          return
                        }
                        form.setFieldsValue({ bannerUrl: resp.data.url })
                      })
                      .finally(() => finishUpload())
                  }}
                >
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>上传Banner</div>
                  </div>
                </Upload>
              </Form.Item>
              <Form.Item name="bannerUrl" hidden>
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="slogan" label="活动标语">
                <Input />
              </Form.Item>
              <Form.Item name="address" label="活动地址" rules={[{ required: true, message: '请输入活动地址' }]}>
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="hostOrg" label="主办单位" rules={[{ required: true, message: '请选择主办单位' }]}>
                <Select
                  showSearch
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={assocOptions}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="coOrgs" label="协办单位">
                <Select
                  mode="multiple"
                  showSearch
                  allowClear
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={assocOptions}
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.List
            name="contacts"
            rules={[
              {
                validator: async (_: unknown, value?: Contact[]) => {
                  if (!value || value.length === 0) {
                    return Promise.reject(new Error('请至少填写一个联系人'));
                  }
                  const hasComplete = value.some((c) => c && c.name && c.phone)
                  if (!hasComplete) {
                    return Promise.reject(new Error('请至少完整填写一个联系人与电话'));
                  }
                  return Promise.resolve()
                },
              },
            ]}
          >
            {(fields, { add, remove }) => (
              <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
                <Space direction="vertical" size={8} style={{ width: '100%' }}>
                  <Typography.Text strong><span style={{ color: '#ff4d4f', marginRight: 4 }}>*</span>联系人与电话（可多个）</Typography.Text>
                  {fields.map((field) => (
                    <Row gutter={8} key={field.key}>
                      <Col xs={24} md={8}>
                        <Form.Item {...field} name={[field.name, 'name']} rules={[{ required: true, message: '请输入联系人姓名' }]}>
                          <Input placeholder="联系人姓名" />
                        </Form.Item>
                      </Col>
                      <Col xs={24} md={12}>
                        <Form.Item
                          {...field}
                          name={[field.name, 'phone']}
                          rules={[
                            { required: true, message: '请输入联系电话' },
                          ]}
                        >
                          <Input placeholder="联系电话" maxLength={32} />
                        </Form.Item>
                      </Col>
                      {fields.length > 1 && field.name !== 0 && (
                        <Col xs={24} md={4}>
                          <Button danger onClick={() => remove(field.name)}>删除</Button>
                        </Col>
                      )}
                    </Row>
                  ))}
                  <Button type="dashed" onClick={() => add({ name: '', phone: '' })}>新增联系人</Button>
                </Space>
              </Card>
            )}
          </Form.List>

          <Row gutter={12}>
            <Col xs={24}>
              <Form.Item name="projects" label="比赛项目（不选表示包含所有）">
                <Select
                  mode="multiple"
                  showSearch
                  allowClear
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={projectOptions}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item
                name="signupRange"
                label="报名开始/截止"
                rules={[{ required: true, message: '请选择报名开始和截止时间' }]}
              >
                <DatePicker.RangePicker showTime style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                name="eventRange"
                label="活动开始/结束"
                rules={[{ required: true, message: '请选择活动开始和结束时间' }]}
              >
                <DatePicker.RangePicker showTime style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>

          <Card style={{ borderRadius: 14 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong><span style={{ color: '#ff4d4f', marginRight: 4 }}>*</span>活动概述</Typography.Text>
              <Form.Item name="overview" valuePropName="value" rules={[{ required: true, message: '请输入活动概述' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>
        </Form>
      </Drawer>
      </ConfigProvider>
    </Space>
  )
}
