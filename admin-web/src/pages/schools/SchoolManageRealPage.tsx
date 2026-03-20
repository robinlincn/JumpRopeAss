import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, Modal, Row, Space, Table, Tag, Typography, message, Select } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'

type RowItem = {
  id: number
  name: string
  shortName?: string
  province?: string
  city?: string
  district?: string
  address?: string
  contactName?: string
  contactPhone?: string
  status: 0 | 1
  createdAt: string
  updatedAt: string
}

type EditValues = {
  name: string
  shortName?: string
  province?: string
  city?: string
  district?: string
  address?: string
  contactName?: string
  contactPhone?: string
  status: 0 | 1
}

export function SchoolManagePage() {
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
      const res = await apiFetch<any>(`/api/v1/admin/schools?${qs.toString()}`)
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
      title: '学校名称',
      dataIndex: 'name',
      width: 280,
      render: (text: string, r) => (
        <Space size={6} style={{ maxWidth: 260 }}>
          <Badge status="default" />
          <Typography.Text strong ellipsis={{ tooltip: text }} style={{ maxWidth: r.shortName ? 150 : 210 }}>
            {text}
          </Typography.Text>
          {r.shortName ? <Tag>{r.shortName}</Tag> : null}
        </Space>
      ),
    },
    { title: '省', dataIndex: 'province', width: 120 },
    { title: '市', dataIndex: 'city', width: 120 },
    { title: '区县', dataIndex: 'district', width: 120 },
    { title: '地址', dataIndex: 'address', width: 240 },
    { title: '联系人', dataIndex: 'contactName', width: 120 },
    { title: '联系电话', dataIndex: 'contactPhone', width: 140 },
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
                shortName: r.shortName,
                province: r.province,
                city: r.city,
                district: r.district,
                address: r.address,
                contactName: r.contactName,
                contactPhone: r.contactPhone,
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
                  const resp = await apiFetch(`/api/v1/admin/schools/${r.id}`, { method: 'DELETE' })
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
            style={{ width: 260 }}
            placeholder="学校名称关键词"
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
              form.setFieldsValue({ status: 1 })
              setDrawerOpen(true)
            }}
          >
            新建学校
          </Button>
          <Button
            icon={<ReloadOutlined />}
            loading={loading}
            onClick={() => {
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
        title={editing ? '编辑学校' : '新建学校'}
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
                  shortName: v.shortName,
                  province: v.province,
                  city: v.city,
                  district: v.district,
                  address: v.address,
                  contactName: v.contactName,
                  contactPhone: v.contactPhone,
                  status: v.status,
                }
                const resp = editing
                  ? await apiFetch(`/api/v1/admin/schools/${editing.id}`, { method: 'PUT', body: JSON.stringify(dto) })
                  : await apiFetch('/api/v1/admin/schools', { method: 'POST', body: JSON.stringify(dto) })
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
          <Row gutter={12}>
            <Col xs={24} md={16}>
              <Form.Item name="name" label="学校名称" rules={[{ required: true, message: '请输入学校名称' }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="shortName" label="学校简称">
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={8}>
              <Form.Item name="province" label="省">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="city" label="市">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="district" label="区县">
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="address" label="详细地址">
            <Input />
          </Form.Item>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="contactName" label="联系人">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="contactPhone" label="联系电话">
                <Input />
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
