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
  Upload,
  message,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { UploadFile } from 'antd/es/upload/interface'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { apiFetch, apiUpload } from '../../lib/api'

type MediaType = 'image' | 'video'
type BannerStatus = 0 | 1
type ThemePosition = 'home' | 'news' | 'events' | 'identity' | 'custom'

const TRANSPARENT_PIXEL =
  'data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw=='

type BannerRow = {
  id: number
  title?: string
  position: ThemePosition
  mediaType: MediaType
  imageUrl?: string
  videoUrl?: string
  sortNo: number
  status: BannerStatus
  createdAt: string
}

const positionLabel: Record<ThemePosition, string> = {
  home: '首页横幅',
  news: '资讯页面横幅',
  events: '活动页面横幅',
  identity: '认证页面横幅',
  custom: '自定义主题页面',
}

const positionSizeHint: Record<ThemePosition, string> = {
  home: '建议尺寸 1125×540（2:1），居中裁切显示',
  news: '建议尺寸 1125×540（2:1），与首页一致',
  events: '建议尺寸 1125×540（2:1），与首页一致',
  identity: '建议尺寸 750×300（2.5:1），信息密度较低',
  custom: '根据页面设计自适配，建议长宽比约 2:1',
}

type EditValues = {
  title?: string
  position: ThemePosition
  mediaType: MediaType
  sortNo: number
  status: BannerStatus
  imageFileList?: UploadFile[]
  imageUrl?: string
  videoUrl?: string
}

export function BannerManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<BannerRow[]>([])
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<BannerRow | null>(null)
  const [form] = Form.useForm<EditValues>()

  const loadData = async () => {
    setLoading(true)
    try {
      const res = await apiFetch<any>('/api/v1/admin/banners')
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      const list = (res.data ?? []) as any[]
      setRows(
        list.map((x) => ({
          id: x.id,
          title: x.title,
          position: x.position,
          mediaType: x.mediaType === 'video' ? 'video' : 'image',
          imageUrl: x.imageUrl,
          videoUrl: x.videoUrl,
          sortNo: x.sortNo,
          status: x.status,
          createdAt: x.createdAt?.replace('T', ' '),
        })),
      )
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

  const columns: ColumnsType<BannerRow> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '标题',
      dataIndex: 'title',
      render: (text: string, r) => (
        <Space>
          <Badge status={r.mediaType === 'video' ? 'processing' : 'default'} />
          <Typography.Text strong>{text ?? '-'}</Typography.Text>
        </Space>
      ),
    },
    {
      title: '位置',
      dataIndex: 'position',
      width: 160,
      render: (v: ThemePosition) => <Tag>{positionLabel[v]}</Tag>,
    },
    {
      title: '类型',
      dataIndex: 'mediaType',
      width: 100,
      render: (v: MediaType) => <Tag>{v === 'video' ? '视频' : '图片'}</Tag>,
    },
    {
      title: '预览',
      dataIndex: 'preview',
      width: 180,
      render: (_, r) =>
        r.mediaType === 'image' && r.imageUrl ? (
          <img
            src={r.imageUrl}
            alt=""
            onError={(e) => {
              e.currentTarget.src = TRANSPARENT_PIXEL
            }}
            style={{ width: 120, height: 60, objectFit: 'cover', borderRadius: 8 }}
          />
        ) : r.mediaType === 'video' && r.videoUrl ? (
          <video src={r.videoUrl} style={{ width: 140, height: 80, borderRadius: 8 }} />
        ) : (
          <Typography.Text type="secondary">-</Typography.Text>
        ),
    },
    { title: '排序', dataIndex: 'sortNo', width: 90 },
    {
      title: '状态',
      dataIndex: 'status',
      width: 100,
      render: (v: BannerStatus) => <Tag color={v === 1 ? 'green' : 'default'}>{v === 1 ? '启用' : '停用'}</Tag>,
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
              form.setFieldsValue({
                title: r.title,
                position: r.position,
                mediaType: r.mediaType,
                sortNo: r.sortNo,
                status: r.status,
                imageFileList: r.imageUrl ? [{ uid: '-1', name: 'image.png', status: 'done', url: r.imageUrl }] : [],
                imageUrl: r.imageUrl,
                videoUrl: r.videoUrl,
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
                content: r.title ?? `${r.id}`,
                okText: '删除',
                cancelText: '取消',
                okButtonProps: { danger: true },
                onOk: async () => {
                  const resp = await apiFetch(`/api/v1/admin/banners/${r.id}`, { method: 'DELETE' })
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

  const values = Form.useWatch([], form) as EditValues | undefined
  const sizeHint = positionSizeHint[(values?.position ?? 'home') as ThemePosition]

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
              form.setFieldsValue({ position: 'home', mediaType: 'image', sortNo: 1, status: 1 })
              setDrawerOpen(true)
            }}
          >
            新建Banner
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
        </Space>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table rowKey="id" columns={columns} dataSource={rows} loading={loading} scroll={{ x: 1100 }} />
      </Card>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 640 : '100%'}
        title={editing ? '编辑Banner' : '新建Banner'}
        extra={
          <Space>
            <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                let v: EditValues
                try {
                  v = await form.validateFields()
                } catch {
                  message.error('请完善必填项')
                  return
                }
                if (v.mediaType === 'image' && !v.imageUrl) {
                  message.error('请上传图片')
                  return
                }
                const dto = {
                  title: v.title,
                  position: v.position,
                  mediaType: v.mediaType,
                  imageUrl: v.mediaType === 'image' ? v.imageUrl : null,
                  videoUrl: v.mediaType === 'video' ? v.videoUrl : null,
                  sortNo: Number(v.sortNo),
                  status: v.status,
                }
                const resp = editing
                  ? await apiFetch(`/api/v1/admin/banners/${editing.id}`, { method: 'PUT', body: JSON.stringify(dto) })
                  : await apiFetch('/api/v1/admin/banners', { method: 'POST', body: JSON.stringify(dto) })
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
        <Form form={form} layout="vertical" initialValues={{ position: 'home', mediaType: 'image', sortNo: 1, status: 1 }}>
          <Form.Item name="title" label="标题">
            <Input placeholder="可选，用于后台辨识" />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="position" label="主题页面" rules={[{ required: true }]}>
                <Select
                  options={[
                    { value: 'home', label: positionLabel.home },
                    { value: 'news', label: positionLabel.news },
                    { value: 'events', label: positionLabel.events },
                    { value: 'identity', label: positionLabel.identity },
                    { value: 'custom', label: positionLabel.custom },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="mediaType" label="类型" rules={[{ required: true }]}>
                <Select
                  options={[
                    { value: 'image', label: '图片' },
                    { value: 'video', label: '视频' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>

          <Card style={{ borderRadius: 14, marginBottom: 12 }} styles={{ body: { padding: 14 } }}>
            <Space direction="vertical" size={6} style={{ width: '100%' }}>
              <Typography.Text strong>尺寸建议</Typography.Text>
              <Typography.Text type="secondary">{sizeHint}</Typography.Text>
            </Space>
          </Card>

          {((values?.mediaType ?? 'image') as MediaType) === 'image' ? (
            <Form.Item
              name="imageFileList"
              label="图片"
              valuePropName="fileList"
              getValueFromEvent={(e) => e?.fileList}
              rules={[
                {
                  validator: async (_, value) => {
                    const hasFile = Array.isArray(value) && value.length > 0
                    if (hasFile) return
                    const url = form.getFieldValue('imageUrl')
                    if (url) return
                    throw new Error('请上传图片')
                  },
                },
              ]}
            >
              <Upload
                listType="picture-card"
                maxCount={1}
                accept="image/*"
                beforeUpload={() => false}
                onChange={(info) => {
                  if (info.fileList.length === 0) {
                    form.setFieldsValue({ imageUrl: undefined })
                    return
                  }
                  const f = info.fileList[0]?.originFileObj as File | undefined
                  if (!f) return
                  if (f.size > 2 * 1024 * 1024) {
                    message.error('图片过大，请选择小于2MB的图片')
                    form.setFieldsValue({ imageFileList: [], imageUrl: undefined })
                    return
                  }
                  const fd = new FormData()
                  fd.append('file', f)
                  apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
                    if (resp.code !== 0) {
                      message.error(resp.message || '上传失败')
                      form.setFieldsValue({ imageFileList: [], imageUrl: undefined })
                      return
                    }
                    form.setFieldsValue({ imageUrl: resp.data.url })
                    form.validateFields(['imageFileList']).catch(() => undefined)
                  })
                }}
              >
                <div>
                  <PlusOutlined />
                  <div style={{ marginTop: 8 }}>上传图片</div>
                </div>
              </Upload>
            </Form.Item>
          ) : (
            <Form.Item name="videoUrl" label="视频地址" rules={[{ required: true, message: '请输入视频地址' }]}>
              <Input placeholder="请输入视频地址（URL）" />
            </Form.Item>
          )}
          <Form.Item name="imageUrl" hidden>
            <Input />
          </Form.Item>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="sortNo" label="排序" rules={[{ required: true }]}>
                <Input type="number" placeholder="越小越靠前" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="status" label="状态" rules={[{ required: true }]}>
                <Select
                  options={[
                    { value: 1, label: '启用' },
                    { value: 0, label: '停用' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Drawer>
    </Space>
  )
}
