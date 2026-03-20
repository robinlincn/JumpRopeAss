import {
  Badge,
  Button,
  Card,
  Col,
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
  Upload,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { Dayjs } from 'dayjs'
import {
  DeleteOutlined,
  EditOutlined,
  ExportOutlined,
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
  BoldOutlined, 
  ItalicOutlined, 
  UnderlineOutlined, 
  AlignLeftOutlined, 
  AlignCenterOutlined, 
  AlignRightOutlined, 
  UnorderedListOutlined, 
  OrderedListOutlined, 
  LinkOutlined, 
  PictureOutlined, 
  VideoCameraOutlined, 
  UndoOutlined, 
  RedoOutlined, 
  FullscreenOutlined, 
  FullscreenExitOutlined, 
  TableOutlined
} from '@ant-design/icons'
import { useState, useRef } from 'react'
import { downloadCsv } from '../../lib/exportCsv'
import { apiFetch, apiUpload } from '../../lib/api'

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

function RichEditor({ value, onChange }: { value?: string; onChange?: (v: string) => void }) {
  const editorRef = useRef<HTMLDivElement | null>(null)
  const [fullscreen, setFullscreen] = useState(false)
  const apply = (cmd: string, arg?: string) => {
    editorRef.current?.focus()
    document.execCommand(cmd, false, arg)
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
            apply('insertHTML', `<img src="${resp.data.url}" style="max-width:100%;border-radius:8px;" />`)
          })
        }}>
          <Button size="small" icon={<PictureOutlined />} />
        </Upload>
        <Upload accept="video/*" maxCount={1} beforeUpload={() => false} showUploadList={false} onChange={(info) => {
          const f = info.file.originFileObj as File | undefined
          if (!f) return
          const url = URL.createObjectURL(f)
          apply('insertHTML', `<video src="${url}" controls style="max-width:100%;border-radius:8px;"></video>`)
        }}>
          <Button size="small" icon={<VideoCameraOutlined />} />
        </Upload>
        <Button size="small" icon={<TableOutlined />} onClick={() => apply('insertHTML', '<table style="width:100%;border-collapse:collapse;"><tr><th style="border:1px solid #ddd;padding:6px;">列1</th><th style="border:1px solid #ddd;padding:6px;">列2</th></tr><tr><td style="border:1px solid #ddd;padding:6px;">内容</td><td style="border:1px solid #ddd;padding:6px;">内容</td></tr></table>')} />
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

type FilterValues = {
  keyword?: string
  contentType?: NewsContentType | 'all'
  status?: NewsStatus | 'all'
  publishRange?: [Dayjs, Dayjs] | null
}

type EditValues = {
  title: string
  contentType: NewsContentType
  status: NewsStatus
  summary?: string
  contentHtml?: string
}

export function NewsManagePage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [editForm] = Form.useForm<EditValues>()

  const [rows, setRows] = useState<NewsRow[]>([])
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<NewsRow | null>(null)

  const loadData = async (p = page, ps = pageSize) => {
    setLoading(true)
    try {
      const query = new URLSearchParams()
      query.append('page', String(p))
      query.append('size', String(ps))
      
      const v = form.getFieldsValue()
      if (v.keyword) query.append('keyword', v.keyword)
      if (v.contentType && v.contentType !== 'all') query.append('contentType', v.contentType)
      if (v.status !== undefined && v.status !== 'all') query.append('status', String(v.status))
      
      const res = await apiFetch<any>(`/api/v1/admin/news?${query.toString()}`)
      if (res.code === 0) {
        setRows(res.data.items.map((x: any) => ({
          id: x.id,
          title: x.title,
          contentType: x.contentType,
          status: x.status,
          viewCount: x.viewCount,
          publishAt: x.publishAt?.replace('T', ' '),
          updatedAt: x.updatedAt?.replace('T', ' '),
          coverUrl: x.coverUrl,
          summary: x.summary,
          contentHtml: x.contentHtml,
          videoUrl: x.videoUrl,
          tags: x.tags ? x.tags.split(',') : [],
        })))
        setTotal(res.data.total)
      } else {
        message.error(res.message || '加载失败')
      }
    } catch (e) {
      console.error(e)
      message.error('加载失败')
    } finally {
      setLoading(false)
    }
  }

  useState(() => {
    loadData()
  })

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
            shape="circle"
            title="详情"
            icon={<EyeOutlined />}
            onClick={() => {
              setEditing(r)
              setDrawerOpen(true)
            }}
          />
          <Button
            size="small"
            shape="circle"
            title="编辑"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => {
              setEditing(r)
              editForm.setFieldsValue({
                title: r.title,
                contentType: r.contentType,
                status: r.status,
                summary: r.summary,
                contentHtml: r.contentHtml,
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
                content: r.title,
                okText: '删除',
                cancelText: '取消',
                okButtonProps: { danger: true },
                onOk: async () => {
                  const resp = await apiFetch(`/api/v1/admin/news/${r.id}`, { method: 'DELETE' })
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
            <Col xs={24}>
              <Space wrap>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    setEditing(null)
                    editForm.resetFields()
                    editForm.setFieldsValue({ contentType: 'text', status: 0, title: '', summary: '', contentHtml: '' })
                    setDrawerOpen(true)
                  }}
                >
                  新建资讯
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  loading={loading}
                  onClick={async () => {
                    await loadData()
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
                      rows,
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
                    loadData(1, pageSize)
                  }}
                >
                  重置
                </Button>
                <Button type="primary" ghost onClick={() => loadData(1, pageSize)}>
                  查询
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
          pagination={{
            current: page,
            pageSize,
            total: total,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setPageSize(ps)
              loadData(p, ps)
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
                try {
                  const v = await editForm.validateFields()
                  
                  const dto = {
                    title: v.title,
                    contentType: v.contentType,
                    status: v.status,
                    summary: v.summary,
                    contentHtml: v.contentHtml
                  }

                  if (editing) {
                    const resp = await apiFetch(`/api/v1/admin/news/${editing.id}`, {
                      method: 'PUT',
                      body: JSON.stringify(dto)
                    })
                    if (resp.code !== 0) {
                      message.error(resp.message || '保存失败')
                      return
                    }
                  } else {
                    const resp = await apiFetch('/api/v1/admin/news', {
                      method: 'POST',
                      body: JSON.stringify(dto)
                    })
                    if (resp.code !== 0) {
                      message.error(resp.message || '保存失败')
                      return
                    }
                  }

                  message.success('已保存')
                  setDrawerOpen(false)
                  loadData()
                } catch (e) {
                  console.error('Save failed', e)
                  message.error('保存失败')
                }
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
                发布时间：{editing.publishAt ?? '-'} 更新时间：{editing.updatedAt}
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
          
          <Card style={{ borderRadius: 14 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>资讯正文详情</Typography.Text>
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

