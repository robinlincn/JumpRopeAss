import { Badge, Button, Card, Col, Drawer, Form, Grid, Input, Modal, Row, Space, Table, Tag, Typography, message, Select, Upload } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { UploadFile } from 'antd/es/upload/interface'
import { PlusOutlined, ReloadOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { apiFetch, apiUpload } from '../../lib/api'

type RowItem = {
  id: number
  fullName: string
  mobile?: string
  gender: 0 | 1 | 2
  avatarUrl?: string
  idCardNo?: string
  schoolId: number
  schoolName?: string | null
  trainingOrgId?: number | null
  trainingOrgName?: string | null
  firstCoachPersonId?: number | null
  firstCoachName?: string | null
  gradeName?: string | null
  className?: string | null
  status: 0 | 1
  createdAt: string
}

type EditValues = {
  fullName: string
  mobile?: string
  gender?: 0 | 1 | 2
  avatarFileList?: UploadFile[]
  avatarUrl?: string
  idCardNo?: string
  schoolId: number
  trainingOrgId?: number
  firstCoachPersonId?: number
  gradeName?: string
  className?: string
  status: 0 | 1
}

export function AthleteManagePage() {
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

  const [schoolOptions, setSchoolOptions] = useState<{ value: number; label: string }[]>([])
  const [orgOptions, setOrgOptions] = useState<{ value: number; label: string }[]>([])
  const [coachOptions, setCoachOptions] = useState<{ value: number; label: string }[]>([])

  const genderLabel = (g?: 0 | 1 | 2) => (g === 1 ? '男' : g === 2 ? '女' : '-')

  const loadData = async (p = page, s = size, kw = keyword) => {
    setLoading(true)
    try {
      const qs = new URLSearchParams()
      if (kw) qs.set('keyword', kw)
      qs.set('page', String(p))
      qs.set('size', String(s))
      const res = await apiFetch<any>(`/api/v1/admin/athletes?${qs.toString()}`)
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

  const loadSchools = async (kw?: string) => {
    const qs = new URLSearchParams()
    if (kw) qs.set('keyword', kw)
    const res = await apiFetch<any>(`/api/v1/admin/lookups/schools?${qs.toString()}`)
    if (res.code !== 0) return
    setSchoolOptions((res.data ?? []).map((x: any) => ({ value: x.id, label: x.name })))
  }

  const loadTrainingOrgs = async (kw?: string) => {
    const qs = new URLSearchParams()
    qs.set('type', '1')
    if (kw) qs.set('keyword', kw)
    const res = await apiFetch<any>(`/api/v1/admin/lookups/orgs?${qs.toString()}`)
    if (res.code !== 0) return
    setOrgOptions((res.data ?? []).map((x: any) => ({ value: x.id, label: x.orgName })))
  }

  const loadCoaches = async (kw?: string) => {
    const qs = new URLSearchParams()
    if (kw) qs.set('keyword', kw)
    const res = await apiFetch<any>(`/api/v1/admin/lookups/coaches?${qs.toString()}`)
    if (res.code !== 0) return
    setCoachOptions((res.data ?? []).map((x: any) => ({ value: x.id, label: `${x.fullName}${x.mobile ? `（${x.mobile}）` : ''}` })))
  }

  useEffect(() => {
    loadData()
    loadSchools()
    loadTrainingOrgs()
    loadCoaches()
  }, [])

  const columns: ColumnsType<RowItem> = [
    { title: 'ID', dataIndex: 'id', width: 90 },
    {
      title: '头像',
      dataIndex: 'avatarUrl',
      width: 80,
      render: (url?: string) =>
        url ? (
          <img
            src={url}
            style={{ width: 34, height: 34, borderRadius: 999, objectFit: 'cover', display: 'block' }}
            onError={(e) => {
              e.currentTarget.style.display = 'none'
            }}
          />
        ) : (
          <Typography.Text type="secondary">-</Typography.Text>
        ),
    },
    {
      title: '姓名',
      dataIndex: 'fullName',
      width: 160,
      render: (text: string) => (
        <Space size={6} style={{ maxWidth: 150 }}>
          <Badge status="default" />
          <Typography.Text strong ellipsis={{ tooltip: text }} style={{ maxWidth: 120 }}>
            {text}
          </Typography.Text>
        </Space>
      ),
    },
    { title: '性别', dataIndex: 'gender', width: 80, render: (v: 0 | 1 | 2) => genderLabel(v) },
    { title: '手机号', dataIndex: 'mobile', width: 140 },
    { title: '身份证号', dataIndex: 'idCardNo', width: 180 },
    { title: '学校', dataIndex: 'schoolName', width: 200 },
    { title: '培训机构', dataIndex: 'trainingOrgName', width: 200 },
    { title: '第一教练', dataIndex: 'firstCoachName', width: 140 },
    { title: '年级', dataIndex: 'gradeName', width: 120 },
    { title: '班级', dataIndex: 'className', width: 120 },
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
      width: 180,
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
              const avatarFileList: UploadFile[] = r.avatarUrl ? [{ uid: '-1', name: 'avatar.png', status: 'done', url: r.avatarUrl }] : []
              form.setFieldsValue({
                fullName: r.fullName,
                mobile: r.mobile,
                gender: r.gender,
                avatarFileList,
                avatarUrl: r.avatarUrl,
                idCardNo: r.idCardNo,
                schoolId: r.schoolId,
                trainingOrgId: r.trainingOrgId ?? undefined,
                firstCoachPersonId: r.firstCoachPersonId ?? undefined,
                gradeName: r.gradeName ?? undefined,
                className: r.className ?? undefined,
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
                  const resp = await apiFetch(`/api/v1/admin/athletes/${r.id}`, { method: 'DELETE' })
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
            placeholder="姓名/手机号关键词"
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
              form.setFieldsValue({ status: 1, gender: 0 } as any)
              setDrawerOpen(true)
            }}
          >
            新建运动员
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
          scroll={{ x: 1500 }}
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
        width={screens.md ? 860 : '100%'}
        title={editing ? '编辑运动员' : '新建运动员'}
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
                const dto = {
                  fullName: v.fullName,
                  mobile: v.mobile,
                  gender: v.gender,
                  idCardNo: v.idCardNo,
                  avatarUrl: v.avatarUrl,
                  schoolId: Number(v.schoolId),
                  trainingOrgId: v.trainingOrgId ? Number(v.trainingOrgId) : null,
                  firstCoachPersonId: v.firstCoachPersonId ? Number(v.firstCoachPersonId) : null,
                  gradeName: v.gradeName,
                  className: v.className,
                  status: v.status,
                }
                const resp = editing
                  ? await apiFetch(`/api/v1/admin/athletes/${editing.id}`, { method: 'PUT', body: JSON.stringify(dto) })
                  : await apiFetch('/api/v1/admin/athletes', { method: 'POST', body: JSON.stringify(dto) })
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
        <Form form={form} layout="vertical" initialValues={{ status: 1, gender: 0 }}>
          <Form.Item name="avatarFileList" label="头像" valuePropName="fileList" getValueFromEvent={(e) => e?.fileList}>
            <Upload
              listType="picture-card"
              maxCount={1}
              accept="image/*"
              beforeUpload={() => false}
              onChange={(info) => {
                if (info.fileList.length === 0) {
                  form.setFieldsValue({ avatarUrl: undefined })
                  return
                }
                const f = info.fileList[0]?.originFileObj as File | undefined
                if (!f) return
                if (f.size > 2 * 1024 * 1024) {
                  message.error('头像图片过大，请选择小于2MB的图片')
                  form.setFieldsValue({ avatarFileList: [], avatarUrl: undefined })
                  return
                }
                const fd = new FormData()
                fd.append('file', f)
                apiUpload<{ url: string }>('/api/v1/admin/uploads/image', fd).then((resp) => {
                  if (resp.code !== 0) {
                    message.error(resp.message || '上传失败')
                    form.setFieldsValue({ avatarFileList: [], avatarUrl: undefined })
                    return
                  }
                  form.setFieldsValue({ avatarUrl: resp.data.url })
                })
              }}
            >
              <div>上传</div>
            </Upload>
          </Form.Item>
          <Form.Item name="avatarUrl" hidden>
            <Input />
          </Form.Item>
          <Row gutter={12}>
            <Col xs={24} md={8}>
              <Form.Item name="fullName" label="姓名" rules={[{ required: true, message: '请输入姓名' }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="gender" label="性别">
                <Select options={[{ value: 1, label: '男' }, { value: 2, label: '女' }, { value: 0, label: '未知' }]} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item name="mobile" label="手机号" rules={[{ required: true, message: '请输入手机号' }]}>
                <Input maxLength={32} />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="idCardNo" label="身份证号" rules={[{ required: true, message: '请输入身份证号' }]}>
                <Input maxLength={32} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="schoolId" label="所属学校" rules={[{ required: true, message: '请选择所属学校' }]}>
                <Select
                  showSearch
                  filterOption={false}
                  options={schoolOptions}
                  onDropdownVisibleChange={(open) => {
                    if (open) loadSchools()
                  }}
                  onSearch={(v) => loadSchools(v)}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="trainingOrgId" label="培训机构">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  options={orgOptions}
                  onDropdownVisibleChange={(open) => {
                    if (open) loadTrainingOrgs()
                  }}
                  onSearch={(v) => loadTrainingOrgs(v)}
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="firstCoachPersonId" label="第一教练">
                <Select
                  allowClear
                  showSearch
                  filterOption={false}
                  options={coachOptions}
                  onDropdownVisibleChange={(open) => {
                    if (open) loadCoaches()
                  }}
                  onSearch={(v) => loadCoaches(v)}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col xs={24} md={12}>
              <Form.Item name="gradeName" label="年级">
                <Input placeholder="如：一年级/七年级" />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item name="className" label="班级">
                <Input placeholder="如：一班/七年级三班" />
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
