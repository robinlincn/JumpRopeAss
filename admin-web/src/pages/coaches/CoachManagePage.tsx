import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, Modal, Row, Space, Table, Tag, Typography, message, Select } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'

type OrgType = '培训机构' | '学校'

type RowItem = {
  id: number
  name: string
  gender?: '男' | '女'
  idCardNo?: string
  mobile?: string
  coachLevel?: string
  orgType?: OrgType
  orgName?: string
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
      id: 7001,
      name: '向彭',
      gender: '男',
      idCardNo: '430321199604254917',
      mobile: '13700001111',
      coachLevel: '初级',
      orgType: '培训机构',
      orgName: '星锐少儿体能中心',
      status: 1,
      updatedAt: nowStr(),
    },
  ]
}

type EditValues = {
  name: string
  gender?: '男' | '女'
  idCardNo?: string
  mobile?: string
  coachLevel?: string
  orgType?: OrgType
  orgName?: string
  status: 0 | 1
}

export function CoachManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<RowItem[]>(() => createMock())
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<RowItem | null>(null)
  const [form] = Form.useForm<EditValues>()
  const orgTypeWatch = Form.useWatch('orgType', form)
  const schoolOptions = [{ value: '宁乡市滨江小学', label: '宁乡市滨江小学' }]
  const memberOptions = [{ value: '星锐少儿体能中心', label: '星锐少儿体能中心' }, { value: '拳威体育培训中心', label: '拳威体育培训中心' }]
  useEffect(() => {
    form.setFieldsValue({ orgName: undefined })
  }, [orgTypeWatch])

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '姓名',
      dataIndex: 'name',
      render: (text: string) => (
        <Space>
          <Badge status="default" />
          <Typography.Text strong>{text}</Typography.Text>
        </Space>
      ),
    },
    { title: '性别', dataIndex: 'gender', width: 80 },
    { title: '手机号', dataIndex: 'mobile', width: 140 },
    { title: '身份证号', dataIndex: 'idCardNo', width: 180 },
    { title: '教练等级', dataIndex: 'coachLevel', width: 100 },
    { title: '机构类型', dataIndex: 'orgType', width: 110 },
    { title: '所属机构', dataIndex: 'orgName', width: 220 },
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
          <Button size="small" shape="circle" title="编辑" type="primary" icon={<EditOutlined />} onClick={() => {
            setEditing(r)
            form.setFieldsValue({ name: r.name, gender: r.gender, idCardNo: r.idCardNo, mobile: r.mobile, coachLevel: r.coachLevel, orgType: r.orgType, orgName: r.orgName, status: r.status })
            setDrawerOpen(true)
          }} />
          <Button size="small" shape="circle" title="删除" danger icon={<DeleteOutlined />} onClick={() => {
            Modal.confirm({
              title: '数据一旦删除不可恢复，您确定要删除吗？',
              okText: '删除',
              cancelText: '取消',
              onOk: () => setRows((prev) => prev.filter((x) => x.id !== r.id)),
            })
          }} />
        </Space>
      ),
    },
  ]

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Space wrap>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); form.setFieldsValue({ status: 1, orgType: '培训机构' }); setDrawerOpen(true) }}>新建教练员</Button>
          <Button icon={<ReloadOutlined />} loading={loading} onClick={async () => { setLoading(true); await new Promise((r) => setTimeout(r, 500)); setRows(createMock()); setLoading(false); message.success('已刷新') }}>刷新</Button>
        </Space>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table rowKey="id" columns={columns} dataSource={rows} loading={loading} scroll={{ x: 1300 }} />
      </Card>

      <Drawer open={drawerOpen} onClose={() => setDrawerOpen(false)} width={screens.md ? 720 : '100%'} title={editing ? '编辑教练员' : '新建教练员'} extra={
        <Space>
          <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
          <Button type="primary" icon={<EditOutlined />} onClick={async () => {
            Modal.confirm({
              title: '您确定要保存吗？',
              okText: '保存',
              cancelText: '取消',
              onOk: async () => {
                const v = await form.validateFields()
                const now = nowStr()
                if (editing) {
                  setRows((prev) => prev.map((x) => x.id === editing.id ? { ...x, ...v, updatedAt: now } : x))
                } else {
                  const id = Math.max(7000, ...rows.map((x) => x.id)) + 1
                  setRows((prev) => [{ id, updatedAt: now, ...v } as RowItem, ...prev])
                }
                message.success('已保存')
                setDrawerOpen(false)
              },
            })
          }}>保存</Button>
        </Space>
      }>
        <Form form={form} layout="vertical" initialValues={{ status: 1, orgType: '培训机构' }}>
          <Row gutter={12}>
            <Col xs={24} md={8}><Form.Item name="name" label="姓名" rules={[{ required: true, message: '请输入姓名' }]}><Input /></Form.Item></Col>
            <Col xs={24} md={8}><Form.Item name="gender" label="性别"><Select options={[{ value: '男', label: '男' }, { value: '女', label: '女' }]} /></Form.Item></Col>
            <Col xs={24} md={8}>
              <Form.Item
                name="mobile"
                label="手机号"
                rules={[
                  { required: true, message: '请输入手机号' },
                  { pattern: /^1\d{10}$/, message: '请输入有效的手机号' },
                ]}
              >
                <Input inputMode="numeric" maxLength={11} onChange={(e) => form.setFieldsValue({ mobile: e.target.value.replace(/\D/g, '') })} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item
                name="idCardNo"
                label="身份证号"
                rules={[
                  { required: true, message: '请输入身份证号' },
                  { pattern: /^\d{15}$|^\d{17}[\dXx]$/, message: '身份证号格式不正确' },
                ]}
              >
                <Input inputMode="numeric" maxLength={18} onChange={(e) => form.setFieldsValue({ idCardNo: e.target.value.replace(/[^0-9Xx]/g, '') })} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}><Form.Item name="coachLevel" label="教练等级"><Input placeholder="如：初级/中级/高级" /></Form.Item></Col>
          </Row>
          <Row gutter={12}>
            <Col xs={24} md={8}><Form.Item name="orgType" label="机构类型"><Select options={[{ value: '学校', label: '学校' }, { value: '培训机构', label: '培训机构' }]} /></Form.Item></Col>
            <Col xs={24} md={16}>
              <Form.Item name="orgName" label="所属机构" rules={[{ required: true, message: '请选择所属机构' }]}>
                <Select
                  showSearch
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={(orgTypeWatch === '学校' ? schoolOptions : memberOptions)}
                />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="status" label="状态" rules={[{ required: true }]}><Select options={[{ value: 1, label: '启用' }, { value: 0, label: '停用' }]} /></Form.Item>
        </Form>
      </Drawer>
    </Space>
  )
}
