import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, Modal, Row, Space, Table, Tag, Typography, message, Select } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useState } from 'react'

type RowItem = {
  id: number
  name: string
  gender?: '男' | '女'
  idCardNo?: string
  mobile?: string
  schoolName: string
  gradeName?: string
  className?: string
  trainingOrgName?: string
  firstCoachName?: string
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
      id: 8001,
      name: '唐琬欣',
      gender: '女',
      idCardNo: '430124201802280444',
      mobile: '13517379260',
      schoolName: '宁乡市滨江小学',
      gradeName: '三年级',
      className: '一班',
      trainingOrgName: '拳威体育培训中心',
      firstCoachName: '伍教练',
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
  schoolName: string
  gradeName?: string
  className?: string
  trainingOrgName?: string
  firstCoachName?: string
  status: 0 | 1
}

export function AthleteManagePage() {
  const screens = Grid.useBreakpoint()
  const [rows, setRows] = useState<RowItem[]>(() => createMock())
  const [loading, setLoading] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [editing, setEditing] = useState<RowItem | null>(null)
  const [form] = Form.useForm<EditValues>()
  const schoolOptions = [{ value: '宁乡市滨江小学', label: '宁乡市滨江小学' }]
  const memberOptions = [{ value: '拳威体育培训中心', label: '拳威体育培训中心' }]
  const coachOptions = [{ value: '伍教练', label: '伍教练' }, { value: '向彭', label: '向彭' }]

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
    { title: '学校', dataIndex: 'schoolName', width: 220 },
    { title: '年级', dataIndex: 'gradeName', width: 120 },
    { title: '班级', dataIndex: 'className', width: 120 },
    { title: '培训机构', dataIndex: 'trainingOrgName', width: 220 },
    { title: '第一教练', dataIndex: 'firstCoachName', width: 140 },
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
            form.setFieldsValue({
              name: r.name,
              gender: r.gender,
              idCardNo: r.idCardNo,
              mobile: r.mobile,
              schoolName: r.schoolName,
              gradeName: r.gradeName,
              className: r.className,
              trainingOrgName: r.trainingOrgName,
              firstCoachName: r.firstCoachName,
              status: r.status,
            })
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
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); form.resetFields(); form.setFieldsValue({ status: 1 }); setDrawerOpen(true) }}>新建运动员</Button>
          <Button icon={<ReloadOutlined />} loading={loading} onClick={async () => { setLoading(true); await new Promise((r) => setTimeout(r, 500)); setRows(createMock()); setLoading(false); message.success('已刷新') }}>刷新</Button>
        </Space>
      </Card>

      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 0 } }}>
        <Table rowKey="id" columns={columns} dataSource={rows} loading={loading} scroll={{ x: 1400 }} />
      </Card>

      <Drawer open={drawerOpen} onClose={() => setDrawerOpen(false)} width={screens.md ? 720 : '100%'} title={editing ? '编辑运动员' : '新建运动员'} extra={
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
                  const id = Math.max(8000, ...rows.map((x) => x.id)) + 1
                  setRows((prev) => [{ id, updatedAt: now, ...v } as RowItem, ...prev])
                }
                message.success('已保存')
                setDrawerOpen(false)
              },
            })
          }}>保存</Button>
        </Space>
      }>
        <Form form={form} layout="vertical" initialValues={{ status: 1 }}>
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
            <Col xs={24} md={12}>
              <Form.Item name="schoolName" label="学校" rules={[{ required: true, message: '请选择学校' }]}>
                <Select
                  showSearch
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={schoolOptions}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={12}>
            <Col xs={24} md={8}><Form.Item name="gradeName" label="年级"><Input placeholder="如：一年级/七年级" /></Form.Item></Col>
            <Col xs={24} md={8}><Form.Item name="className" label="班级"><Input placeholder="如：一班/七年级三班" /></Form.Item></Col>
            <Col xs={24} md={8}>
              <Form.Item name="trainingOrgName" label="培训机构">
                <Select
                  showSearch
                  allowClear
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={memberOptions}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={12}>
            <Col xs={24} md={8}>
              <Form.Item name="firstCoachName" label="第一教练">
                <Select
                  showSearch
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={coachOptions}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="secondCoachName" label="第二教练">
                <Select
                  showSearch
                  allowClear
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={coachOptions}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="thirdCoachName" label="第三教练">
                <Select
                  showSearch
                  allowClear
                  filterOption={(input, option) => (option?.label as string).toLowerCase().includes(input.toLowerCase())}
                  options={coachOptions}
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
