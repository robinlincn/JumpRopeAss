import {
  Badge,
  Button,
  Card,
  Drawer,
  Form,
  Grid,
  Input,
  Modal,
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
import { useRef, useState } from 'react'

const TRANSPARENT_PIXEL =
  'data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw=='

type SchoolType = '小学' | '中学' | '高中' | '大学'

type RowItem = {
  id: number
  name: string
  address?: string
  logoUrl?: string
  motto?: string
  type?: SchoolType
  overview?: string
  history?: string
  honors?: string
  status: 0 | 1
  updatedAt: string
}

function nowStr() {
  const d = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

function createMock(): RowItem[] {
  return [
    {
      id: 5001,
      name: '滨江小学',
      address: '湖南省长沙市',
      logoUrl: '/logo.png',
      motto: '勤学善思',
      type: '小学',
      overview: '',
      history: '',
      honors: '',
      status: 1,
      updatedAt: nowStr(),
    },
  ]
}

function RichEditor({ value, onChange }: { value?: string; onChange?: (v: string) => void }) {
  const editorRef = useRef<HTMLDivElement | null>(null)
  const [fullscreen, setFullscreen] = useState(false)
  const apply = (cmd: string, arg?: string) => {
    document.execCommand(cmd, false, arg)
    const v = editorRef.current?.innerHTML ?? ''
    onChange?.(v)
  }
  const insertHTML = (html: string) => {
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
            const reader = new FileReader()
            reader.onload = () => {
              const src = String(reader.result)
              insertHTML(`<img src="${src}" style="max-width:100%;border-radius:8px;" />`)
            }
            reader.readAsDataURL(f)
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
  motto?: string
  type?: SchoolType
  logoFileList?: UploadFile[]
  logoUrl?: string
  overview?: string
  history?: string
  honors?: string
  status: 0 | 1
}

export function SchoolManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<RowItem[]>(() => createMock())
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<RowItem | null>(null)
  const [form] = Form.useForm<EditValues>()

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '学校名称',
      dataIndex: 'name',
      render: (text: string) => (
        <Space>
          <Badge status="default" />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    { title: '学校地址', dataIndex: 'address', width: 220 },
    { title: '校训', dataIndex: 'motto', width: 160 },
    { title: '类型', dataIndex: 'type', width: 100, render: (v?: SchoolType) => <Tag>{v ?? '-'}</Tag> },
    {
      title: '校徽',
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
    { title: '更新时间', dataIndex: 'updatedAt', width: 170 },
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
              form.setFieldsValue({
                name: r.name,
                address: r.address,
                motto: r.motto,
                type: r.type,
                logoUrl: r.logoUrl,
                overview: r.overview,
                history: r.history,
                honors: r.honors,
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
                title: '您确定要删除吗？',
                okText: '删除',
                cancelText: '取消',
                onOk: () => setRows((prev) => prev.filter((x) => x.id !== r.id)),
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
              form.setFieldsValue({ status: 1, type: '小学' })
              setDrawerOpen(true)
            }}
          >
            新建学校
          </Button>
          <Button
            icon={<ReloadOutlined />}
            loading={loading}
            onClick={async () => {
              setLoading(true)
              await new Promise((r) => setTimeout(r, 500))
              setRows(createMock())
              setLoading(false)
              message.success('已刷新')
            }}
          >
            刷新
          </Button>
        </Space>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table rowKey="id" columns={columns} dataSource={rows} loading={loading} scroll={{ x: 1300 }} />
      </Card>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 720 : '100%'}
        title={editing ? '编辑学校' : '新建学校'}
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
                    const v = await form.validateFields()
                    const now = nowStr()
                    if (editing) {
                      setRows((prev) =>
                        prev.map((x) =>
                          x.id === editing.id
                            ? {
                                ...x,
                                name: v.name,
                                address: v.address,
                                motto: v.motto,
                                type: v.type,
                                logoUrl: v.logoUrl,
                                overview: v.overview,
                                history: v.history,
                                honors: v.honors,
                                status: v.status,
                                updatedAt: now,
                              }
                            : x,
                        ),
                      )
                    } else {
                      const id = Math.max(5000, ...rows.map((x) => x.id)) + 1
                      setRows((prev) => [
                        {
                          id,
                          name: v.name,
                          address: v.address,
                          motto: v.motto,
                          type: v.type,
                          logoUrl: v.logoUrl,
                          overview: v.overview,
                          history: v.history,
                          honors: v.honors,
                          status: v.status,
                          updatedAt: now,
                        },
                        ...prev,
                      ])
                    }
                    message.success('已保存')
                    setDrawerOpen(false)
                  },
                })
              }}
            >
              保存
            </Button>
          </Space>
        }
      >
        <Form form={form} layout="vertical" initialValues={{ status: 1, type: '小学' }}>
          <Form.Item name="logoFileList" label="学校校徽" valuePropName="fileList" getValueFromEvent={(e) => e?.fileList}>
            <Upload
              listType="picture-card"
              maxCount={1}
              accept="image/*"
              beforeUpload={() => false}
              onChange={(info) => {
                const f = info.file.originFileObj as File | undefined
                if (!f) return
                const reader = new FileReader()
                reader.onload = () => {
                  form.setFieldsValue({ logoUrl: String(reader.result) })
                }
                reader.readAsDataURL(f)
              }}
            >
              <div>
                <PlusOutlined />
                <div style={{ marginTop: 8 }}>上传校徽</div>
              </div>
            </Upload>
          </Form.Item>
          <Form.Item name="logoUrl" hidden rules={[{ required: true, message: '请上传学校校徽' }]}>
            <Input />
          </Form.Item>

          <Form.Item name="name" label="学校名称" rules={[{ required: true, message: '请输入学校名称' }]}>
            <Input placeholder="请输入学校名称" />
          </Form.Item>
          <Form.Item name="address" label="学校地址">
            <Input placeholder="请输入学校地址" />
          </Form.Item>
          <Form.Item name="motto" label="学校校训">
            <Input placeholder="请输入学校校训" />
          </Form.Item>
          <Form.Item name="type" label="学校类型" rules={[{ required: true, message: '请选择学校类型' }]}>
            <Select
              options={[
                { value: '小学', label: '小学' },
                { value: '中学', label: '中学' },
                { value: '高中', label: '高中' },
                { value: '大学', label: '大学' },
              ]}
            />
          </Form.Item>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>学校概述</Typography.Text>
              <Form.Item name="overview" valuePropName="value" rules={[{ required: true, message: '请输入学校概述' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>学校历史</Typography.Text>
              <Form.Item name="history" valuePropName="value" rules={[{ required: true, message: '请输入学校历史' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>

          <Card style={{ borderRadius: 14 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>学校荣誉</Typography.Text>
              <Form.Item name="honors" valuePropName="value" rules={[{ required: true, message: '请输入学校荣誉' }]} noStyle>
                <RichEditor />
              </Form.Item>
            </Space>
          </Card>
        </Form>
      </Drawer>
    </Space>
  )
}
