import {
  Badge,
  Button,
  Card,
  Col,
  DatePicker,
  Drawer,
  Form,
  Grid,
  Input,
  Modal,
  Row,
  Select,
  Space,
  Table,
  Tag,
  Typography,
  message,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  DeleteOutlined,
  EditOutlined,
  ExportOutlined,
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons'
import { useMemo, useState } from 'react'
import { downloadCsv } from '../../lib/exportCsv'
import { formatDateTime, randomDateWithinDays, randomInt } from '../../lib/mockData'

type NewsStatus = 0 | 1 | 2
type NewsContentType = 'text' | 'video'

type NewsRow = {
  id: number
  title: string
  contentType: NewsContentType
  status: NewsStatus
  viewCount: number
  publishAt?: string
  updatedAt: string
  coverUrl?: string
  summary?: string
  contentHtml?: string
  videoUrl?: string
  tags?: string[]
}

function statusLabel(status: NewsStatus) {
  if (status === 1) return { text: '已发布', color: 'green' as const }
  if (status === 0) return { text: '草稿', color: 'default' as const }
  return { text: '已下线', color: 'red' as const }
}

function contentTypeLabel(t: NewsContentType) {
  return t === 'video' ? '视频' : '图文'
}

function createMockRows(): NewsRow[] {
  const titles = [
    '2026年湖南省学生跳绳等级评定活动圆满举行',
    '湖南省学生跳绳协会年度工作会议顺利召开',
    '全省校园跳绳推广活动启动',
    '关于开展教练员继续教育的通知',
    '2026年春季等级评定报名开启',
    '赛事回顾：公开赛精彩瞬间',
  ]
  return Array.from({ length: 38 }).map((_, i) => {
    const id = 1000 + i
    const dt = randomDateWithinDays(90)
    const pub = randomInt(0, 10) > 2 ? formatDateTime(randomDateWithinDays(60)) : undefined
    const status: NewsStatus = pub ? 1 : 0
    const contentType: NewsContentType = randomInt(0, 10) > 7 ? 'video' : 'text'
    return {
      id,
      title: titles[i % titles.length] + (i % 3 === 0 ? `（第${Math.floor(i / 3) + 1}期）` : ''),
      contentType,
      status: status === 1 && randomInt(0, 10) > 8 ? 2 : status,
      viewCount: randomInt(200, 28000),
      publishAt: pub,
      updatedAt: formatDateTime(dt),
      coverUrl: '',
      summary: '用于演示的资讯摘要，后续接入后台富文本编辑与资源管理。',
      contentHtml: '<p>这是一个演示内容。</p>',
      videoUrl: contentType === 'video' ? 'https://example.com/video.mp4' : undefined,
      tags: ['最新资讯'],
    }
  })
}

type FilterValues = {
  keyword?: string
  contentType?: NewsContentType | 'all'
  status?: NewsStatus | 'all'
  publishRange?: [any, any]
}

type EditValues = {
  title: string
  contentType: NewsContentType
  status: NewsStatus
  summary?: string
}

export function NewsManagePage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [editForm] = Form.useForm<EditValues>()

  const [rows, setRows] = useState<NewsRow[]>(() => createMockRows())
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<NewsRow | null>(null)

  const values = Form.useWatch([], form)

  const filtered = useMemo(() => {
    const v = (values ?? {}) as FilterValues
    const kw = (v.keyword ?? '').trim()
    const ct = v.contentType ?? 'all'
    const st = v.status ?? 'all'
    const range = v.publishRange

    return rows.filter((r) => {
      if (kw && !r.title.includes(kw)) return false
      if (ct !== 'all' && r.contentType !== ct) return false
      if (st !== 'all' && r.status !== st) return false
      if (range?.length === 2 && range[0] && range[1]) {
        if (!r.publishAt) return false
        const start = typeof range[0]?.toDate === 'function' ? range[0].toDate() : new Date(range[0])
        const end = typeof range[1]?.toDate === 'function' ? range[1].toDate() : new Date(range[1])
        const pub = new Date(r.publishAt.replace(' ', 'T'))
        if (Number.isNaN(pub.getTime())) return false
        if (pub.getTime() < start.getTime() || pub.getTime() > end.getTime()) return false
      }
      return true
    })
  }, [rows, values])

  const paged = useMemo(() => {
    const start = (page - 1) * pageSize
    return filtered.slice(start, start + pageSize)
  }, [filtered, page, pageSize])

  const columns: ColumnsType<NewsRow> = [
    {
      title: 'ID',
      dataIndex: 'id',
      width: 90,
    },
    {
      title: '标题',
      dataIndex: 'title',
      ellipsis: true,
      render: (text: string, r) => (
        <Space size={8}>
          <Badge status={r.contentType === 'video' ? 'processing' : 'default'} />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    {
      title: '类型',
      dataIndex: 'contentType',
      width: 100,
      render: (v: NewsContentType) => <Tag>{contentTypeLabel(v)}</Tag>,
    },
    {
      title: '状态',
      dataIndex: 'status',
      width: 110,
      render: (v: NewsStatus) => {
        const s = statusLabel(v)
        return <Tag color={s.color}>{s.text}</Tag>
      },
    },
    {
      title: '发布时间',
      dataIndex: 'publishAt',
      width: 170,
      render: (v?: string) => <Typography.Text type="secondary">{v ?? '-'}</Typography.Text>,
    },
    {
      title: '浏览量',
      dataIndex: 'viewCount',
      width: 110,
    },
    {
      title: '更新时间',
      dataIndex: 'updatedAt',
      width: 170,
    },
    {
      title: '操作',
      key: 'action',
      width: 220,
      fixed: screens.lg ? 'right' : undefined,
      render: (_, r) => (
        <Space size={6} wrap>
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => {
              setEditing(r)
              setDrawerOpen(true)
            }}
          >
            详情
          </Button>
          <Button
            size="small"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => {
              setEditing(r)
              editForm.setFieldsValue({
                title: r.title,
                contentType: r.contentType,
                status: r.status,
                summary: r.summary,
              })
              setDrawerOpen(true)
            }}
          >
            编辑
          </Button>
          <Button
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={() => {
              Modal.confirm({
                title: '确认删除该资讯？',
                content: r.title,
                okText: '删除',
                cancelText: '取消',
                okButtonProps: { danger: true },
                onOk: () => setRows((prev) => prev.filter((x) => x.id !== r.id)),
              })
            }}
          >
            删除
          </Button>
        </Space>
      ),
    },
  ]

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Form
          form={form}
          layout="vertical"
          initialValues={{ contentType: 'all', status: 'all' }}
          onValuesChange={() => setPage(1)}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={8} lg={7}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="标题关键词" allowClear />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={5}>
              <Form.Item name="contentType" label="类型">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 'text', label: '图文' },
                    { value: 'video', label: '视频' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={5}>
              <Form.Item name="status" label="状态">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 1, label: '已发布' },
                    { value: 0, label: '草稿' },
                    { value: 2, label: '已下线' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={7}>
              <Form.Item name="publishRange" label="发布时间">
                <DatePicker.RangePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Space wrap>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    setEditing(null)
                    editForm.resetFields()
                    editForm.setFieldsValue({ contentType: 'text', status: 0, title: '' })
                    setDrawerOpen(true)
                  }}
                >
                  新建资讯
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  loading={loading}
                  onClick={async () => {
                    setLoading(true)
                    await new Promise((r) => setTimeout(r, 500))
                    setRows(createMockRows())
                    setLoading(false)
                    message.success('已刷新')
                  }}
                >
                  刷新
                </Button>
                <Button
                  icon={<ExportOutlined />}
                  onClick={() => {
                    downloadCsv(
                      `news_${Date.now()}.csv`,
                      filtered,
                      [
                        { title: 'ID', value: (r) => r.id },
                        { title: '标题', value: (r) => r.title },
                        { title: '类型', value: (r) => contentTypeLabel(r.contentType) },
                        { title: '状态', value: (r) => statusLabel(r.status).text },
                        { title: '发布时间', value: (r) => r.publishAt ?? '' },
                        { title: '浏览量', value: (r) => r.viewCount },
                        { title: '更新时间', value: (r) => r.updatedAt },
                      ],
                    )
                  }}
                >
                  导出
                </Button>
                <Button
                  onClick={() => {
                    form.resetFields()
                    setPage(1)
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
          dataSource={paged}
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total: filtered.length,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setPageSize(ps)
            },
          }}
          scroll={{ x: 1100 }}
        />
      </Card>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 640 : '100%'}
        title={editing ? '资讯详情/编辑' : '新建资讯'}
        extra={
          <Space>
            <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                const v = await editForm.validateFields()
                const now = formatDateTime(new Date())

                if (editing) {
                  setRows((prev) =>
                    prev.map((x) =>
                      x.id === editing.id
                        ? {
                            ...x,
                            title: v.title,
                            contentType: v.contentType,
                            status: v.status,
                            summary: v.summary,
                            publishAt: v.status === 1 ? (x.publishAt ?? now) : x.publishAt,
                            updatedAt: now,
                          }
                        : x,
                    ),
                  )
                } else {
                  const id = Math.max(...rows.map((x) => x.id)) + 1
                  setRows((prev) => [
                    {
                      id,
                      title: v.title,
                      contentType: v.contentType,
                      status: v.status,
                      viewCount: 0,
                      publishAt: v.status === 1 ? now : undefined,
                      updatedAt: now,
                      summary: v.summary,
                      contentHtml: '',
                      tags: [],
                    },
                    ...prev,
                  ])
                }

                message.success('已保存')
                setDrawerOpen(false)
              }}
            >
              保存
            </Button>
          </Space>
        }
      >
        {editing && (
          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text type="secondary">预览信息</Typography.Text>
              <Typography.Title level={5} style={{ margin: 0 }}>
                {editing.title}
              </Typography.Title>
              <Space wrap>
                <Tag>{contentTypeLabel(editing.contentType)}</Tag>
                <Tag color={statusLabel(editing.status).color}>{statusLabel(editing.status).text}</Tag>
                <Typography.Text type="secondary">浏览量：{editing.viewCount}</Typography.Text>
              </Space>
              <Typography.Text type="secondary">
                发布时间：{editing.publishAt ?? '-'}　更新时间：{editing.updatedAt}
              </Typography.Text>
            </Space>
          </Card>
        )}

        <Form
          form={editForm}
          layout="vertical"
          initialValues={{ contentType: 'text', status: 0 }}
        >
          <Form.Item
            name="title"
            label="标题"
            rules={[{ required: true, message: '请输入标题' }]}
          >
            <Input placeholder="请输入资讯标题" />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="contentType" label="类型" rules={[{ required: true }]}>
                <Select
                  options={[
                    { value: 'text', label: '图文' },
                    { value: 'video', label: '视频' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="status" label="状态" rules={[{ required: true }]}>
                <Select
                  options={[
                    { value: 0, label: '草稿' },
                    { value: 1, label: '已发布' },
                    { value: 2, label: '已下线' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="summary" label="摘要">
            <Input.TextArea placeholder="用于列表展示的摘要" autoSize={{ minRows: 3, maxRows: 6 }} />
          </Form.Item>
        </Form>
      </Drawer>
    </Space>
  )
}

