import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, InputNumber, Modal, Row, Space, Table, Tag, Typography, message, Select } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'

type RowItem = {
  id: number
  code?: string
  name: string
  participantCount: number
  status: 0 | 1
  createdAt: string
}

type EditValues = {
  code?: string
  name: string
  participantCount: number
  status: 0 | 1
}

export function ProjectCatalogPage() {
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

  const loadData = async (p = page, s = size, kw = keyword) => {
    setLoading(true)
    try {
      const qs = new URLSearchParams()
      if (kw) qs.set('keyword', kw)
      qs.set('page', String(p))
      qs.set('size', String(s))
      const res = await apiFetch<any>(`/api/v1/admin/projects?${qs.toString()}`)
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
    loadData(1, size, keyword)
    setPage(1)
  }, [])

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '项目名称',
      dataIndex: 'name',
      render: (text: string) => (
        <Space>
          <Badge status="default" />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    { title: '编码', dataIndex: 'code', width: 120 },
    { title: '人数', dataIndex: 'participantCount', width: 100 },
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
      width: 160,
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
                code: r.code,
                name: r.name,
                participantCount: r.participantCount,
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
                  const resp = await apiFetch(`/api/v1/admin/projects/${r.id}`, { method: 'DELETE' })
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
          <Input
            allowClear
            style={{ width: 240 }}
            placeholder="项目名称关键词"
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
              form.setFieldsValue({ status: 1, participantCount: 1 })
              setDrawerOpen(true)
            }}
          >
            新建项目
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
          scroll={{ x: 1000 }}
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
        width={screens.md ? 600 : '100%'}
        title={editing ? '编辑项目' : '新建项目'}
        extra={
          <Space>
            <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
            <Button
              type="primary"
              icon={<EditOutlined />}
              onClick={async () => {
                const v = await form.validateFields()
                const dto = {
                  code: v.code,
                  name: v.name,
                  participantCount: Number(v.participantCount),
                  status: v.status,
                }
                const resp = editing
                  ? await apiFetch(`/api/v1/admin/projects/${editing.id}`, { method: 'PUT', body: JSON.stringify(dto) })
                  : await apiFetch('/api/v1/admin/projects', { method: 'POST', body: JSON.stringify(dto) })
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
        <Form form={form} layout="vertical" initialValues={{ status: 1, participantCount: 1 }}>
          <Form.Item name="name" label="项目名称" rules={[{ required: true, message: '请输入项目名称' }]}>
            <Input />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="code" label="项目编码">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="participantCount" label="人数" rules={[{ required: true, message: '请输入人数' }]}>
                <InputNumber style={{ width: '100%' }} min={1} step={1} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="status" label="状态" rules={[{ required: true }]}>
            <Select options={[{ value: 1, label: '启用' }, { value: 0, label: '停用' }]} />
          </Form.Item>
        </Form>
      </Drawer>
    </Space>
  )
}
