import {
  Badge,
  Button,
  Card,
  Col,
  Drawer,
  Form,
  Grid,
  Input,
  InputNumber,
  Modal,
  Row,
  Space,
  Table,
  Tag,
  Typography,
  Upload,
  message,
  Select,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { UploadFile } from 'antd/es/upload/interface'
import {
  PlusOutlined,
  ReloadOutlined,
  DeleteOutlined,
  EditOutlined,
  PictureOutlined,
  VideoCameraOutlined,
  BoldOutlined,
  ItalicOutlined,
  UnderlineOutlined,
  StrikethroughOutlined,
  AlignLeftOutlined,
  AlignCenterOutlined,
  AlignRightOutlined,
  UnorderedListOutlined,
  OrderedListOutlined,
  LinkOutlined,
  CheckSquareOutlined,
  HighlightOutlined,
  TableOutlined,
  UndoOutlined,
  RedoOutlined,
  FullscreenOutlined,
  FullscreenExitOutlined,
} from '@ant-design/icons'
import { useEffect, useRef, useState } from 'react'
import { apiFetch, apiUpload } from '../../lib/api'

const TRANSPARENT_PIXEL =
  'data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw=='

type RowItem = {
  id: number
  name: string
  address?: string
  logoUrl?: string
  contactName?: string
  contactPhone?: string
  intro?: string
  contentHtml?: string
  sortNo: number
  status: 0 | 1
  createdAt: string
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
    <Card style={{ borderRadius: 10 }} styles={{ body: { padding: 10 } }}>
      <Space wrap style={{ marginBottom: 8 }}>
        <Select
          size="small"
          style={{ width: 100 }}
          placeholder="H"
          onChange={(v) => apply('formatBlock', v)}
          options={[
            { value: 'h1', label: 'H1' },
            { value: 'h2', label: 'H2' },
            { value: 'h3', label: 'H3' },
            { value: 'p', label: '正文' },
          ]}
        />
        <Button size="small" icon={<BoldOutlined />} onClick={() => apply('bold')} />
        <Button size="small" icon={<ItalicOutlined />} onClick={() => apply('italic')} />
        <Button size="small" icon={<UnderlineOutlined />} onClick={() => apply('underline')} />
        <Button size="small" icon={<StrikethroughOutlined />} onClick={() => apply('strikeThrough')} />
        <Button size="small" icon={<AlignLeftOutlined />} onClick={() => apply('justifyLeft')} />
        <Button size="small" icon={<AlignCenterOutlined />} onClick={() => apply('justifyCenter')} />
        <Button size="small" icon={<AlignRightOutlined />} onClick={() => apply('justifyRight')} />
        <Button size="small" icon={<HighlightOutlined />} onClick={() => apply('foreColor', '#ff6b00')} />
        <Button size="small" icon={<UnorderedListOutlined />} onClick={() => apply('insertUnorderedList')} />
        <Button size="small" icon={<OrderedListOutlined />} onClick={() => apply('insertOrderedList')} />
        <Button
          size="small"
          icon={<LinkOutlined />}
          onClick={() => {
            const url = window.prompt('输入链接地址') || ''
            if (url) apply('createLink', url)
          }}
        />
        <Button size="small" icon={<CheckSquareOutlined />} onClick={() => insertHTML('<span>☑</span> ')} />
        <Upload
          accept="image/*"
          maxCount={1}
          beforeUpload={() => false}
          showUploadList={false}
          onChange={(info) => {
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
          }}
        >
          <Button size="small" icon={<PictureOutlined />} />
        </Upload>
        <Upload
          accept="video/*"
          maxCount={1}
          beforeUpload={() => false}
          showUploadList={false}
          onChange={(info) => {
            const f = info.file.originFileObj as File | undefined
            if (!f) return
            const url = URL.createObjectURL(f)
            insertHTML(`<video src="${url}" controls style="max-width:100%;border-radius:8px;"></video>`)
          }}
        >
          <Button size="small" icon={<VideoCameraOutlined />} />
        </Upload>
        <Button
          size="small"
          icon={<TableOutlined />}
          onClick={() =>
            insertHTML(
              '<table style="width:100%;border-collapse:collapse;"><tr><th style="border:1px solid #ddd;padding:6px;">列1</th><th style="border:1px solid #ddd;padding:6px;">列2</th></tr><tr><td style="border:1px solid #ddd;padding:6px;">内容</td><td style="border:1px solid #ddd;padding:6px;">内容</td></tr></table>',
            )
          }
        />
        <Button size="small" icon={<UndoOutlined />} onClick={() => apply('undo')} />
        <Button size="small" icon={<RedoOutlined />} onClick={() => apply('redo')} />
        <Button
          size="small"
          icon={fullscreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />}
          onClick={() => setFullscreen((v) => !v)}
        />
      </Space>
      <div
        ref={editorRef}
        contentEditable
        onInput={(e) => {
          const v = (e.target as HTMLDivElement).innerHTML
          onChange?.(v)
        }}
        style={{
          minHeight: 220,
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

type EditValues = {
  name: string
  address?: string
  logoFileList?: UploadFile[]
  logoUrl?: string
  contactPhone?: string
  contactName?: string
  intro?: string
  contentHtml?: string
  sortNo: number
  status: 0 | 1
}

export function MemberUnitManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<RowItem[]>([])
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<RowItem | null>(null)
  const [form] = Form.useForm<EditValues>()
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(20)
  const [total, setTotal] = useState(0)

  const loadData = async (p = page, s = size) => {
    setLoading(true)
    try {
      const qs = new URLSearchParams()
      qs.set('page', String(p))
      qs.set('size', String(s))
      const res = await apiFetch<any>(`/api/v1/admin/members?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setRows(res.data.items ?? [])
      setTotal(res.data.total ?? 0)
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

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '单位名称',
      dataIndex: 'name',
      render: (text: string) => (
        <Space>
          <Badge status="default" />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    { title: '单位地址', dataIndex: 'address', width: 220 },
    { title: '负责人', dataIndex: 'contactName', width: 120 },
    { title: '联系电话', dataIndex: 'contactPhone', width: 140 },
    {
      title: 'Logo',
      dataIndex: 'logoUrl',
      width: 140,
      render: (v?: string) =>
        v ? (
          <img
            src={v}
            alt=""
            onError={(e) => {
              e.currentTarget.src = TRANSPARENT_PIXEL
            }}
            style={{ width: 100, height: 50, objectFit: 'cover', borderRadius: 8 }}
          />
        ) : (
          <Typography.Text type="secondary">-</Typography.Text>
        ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      width: 100,
      render: (v: 0 | 1) => <Tag color={v === 1 ? 'green' : 'default'}>{v === 1 ? '启用' : '停用'}</Tag>,
    },
    { title: '创建时间', dataIndex: 'createdAt', width: 170 },
    {
      title: '操作',
      key: 'action',
      width: 220,
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
              const logoFileList: UploadFile[] = r.logoUrl ? [{ uid: '-1', name: 'logo.png', status: 'done', url: r.logoUrl }] : []
              form.setFieldsValue({
                logoFileList,
                name: r.name,
                address: r.address,
                contactName: r.contactName,
                contactPhone: r.contactPhone,
                logoUrl: r.logoUrl,
                intro: r.intro,
                contentHtml: r.contentHtml,
                sortNo: r.sortNo,
                status: r.status,
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
                  const resp = await apiFetch(`/api/v1/admin/members/${r.id}`, { method: 'DELETE' })
                  if (resp.code !== 0) {
                    message.error(resp.message || '删除失败')
                    return
                  }
                  message.success('已删除')
                  loadData()
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
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditing(null)
              form.resetFields()
              form.setFieldsValue({ status: 1, sortNo: 1 })
              setDrawerOpen(true)
            }}
          >
            新建会员单位
          </Button>
          <Button
            icon={<ReloadOutlined />}
            loading={loading}
            onClick={async () => {
              message.success('已刷新')
              loadData()
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
          scroll={{ x: 1200 }}
          pagination={{
            current: page,
            pageSize: size,
            total,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setSize(ps)
              loadData(p, ps)
            },
          }}
        />
      </Card>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 720 : '100%'}
        title={editing ? '编辑会员单位' : '新建会员单位'}
        extra={
          <Space>
            <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                const v = await form.validateFields()
                const dto = {
                  name: v.name,
                  logoUrl: v.logoUrl,
                  contactName: v.contactName,
                  contactPhone: v.contactPhone,
                  address: v.address,
                  intro: v.intro,
                  contentHtml: v.contentHtml,
                  sortNo: Number(v.sortNo),
                  status: v.status,
                }
                const resp = editing
                  ? await apiFetch(`/api/v1/admin/members/${editing.id}`, { method: 'PUT', body: JSON.stringify(dto) })
                  : await apiFetch('/api/v1/admin/members', { method: 'POST', body: JSON.stringify(dto) })
                if (resp.code !== 0) {
                  message.error(resp.message || '保存失败')
                  return
                }
                message.success('已保存')
                setDrawerOpen(false)
                loadData()
              }}
            >
              保存
            </Button>
          </Space>
        }
      >
        <Form form={form} layout="vertical" initialValues={{ status: 1 }}>
          <Form.Item name="logoFileList" label="单位Logo" valuePropName="fileList" getValueFromEvent={(e) => e?.fileList}>
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
                const f = info.file.originFileObj as File | undefined
                if (!f) return
                if (f.size > 2 * 1024 * 1024) {
                  message.error('图片过大，请选择小于2MB的图片')
                  form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                  return
                }
                const fd = new FormData()
                fd.append('file', f)
                apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
                  if (resp.code !== 0) {
                    message.error(resp.message || '上传失败')
                    form.setFieldsValue({ logoFileList: [], logoUrl: undefined })
                    return
                  }
                  form.setFieldsValue({ logoUrl: resp.data.url })
                })
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

          <Form.Item name="name" label="单位名称" rules={[{ required: true, message: '请输入单位名称' }]}>
            <Input placeholder="请输入单位名称" />
          </Form.Item>
          <Form.Item name="address" label="单位地址">
            <Input placeholder="请输入单位地址" />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="sortNo" label="排序" rules={[{ required: true, message: '请输入排序' }]}>
                <InputNumber style={{ width: '100%' }} min={0} step={1} placeholder="越小越靠前" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="contactName" label="负责人姓名">
                <Input placeholder="请输入负责人姓名" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12} style={{ display: 'none' }} />
          </Row>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="contactPhone" label="负责人联系电话">
                <Input placeholder="请输入负责人联系电话" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="status" label="状态" rules={[{ required: true }]}>
                <Select options={[{ value: 1, label: '启用' }, { value: 0, label: '停用' }]} />
              </Form.Item>
            </Col>
          </Row>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>简介</Typography.Text>
              <Form.Item name="intro" rules={[{ required: true, message: '请输入简介' }]} noStyle>
                <Input.TextArea rows={3} placeholder="用于列表展示的简介" />
              </Form.Item>
            </Space>
          </Card>

          <Card style={{ borderRadius: 14 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>详情内容</Typography.Text>
              <Form.Item name="contentHtml" valuePropName="value" noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>
        </Form>
      </Drawer>
    </Space>
  )
}
