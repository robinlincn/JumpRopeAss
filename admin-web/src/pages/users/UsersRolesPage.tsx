import { Button, Card, Col, Form, Grid, Input, Modal, Row, Select, Space, Switch, Table, Tabs, Tag, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { useEffect, useMemo, useRef, useState } from 'react'
import { apiFetch } from '../../lib/api'
import { menuTree } from '../../app/config/menu'

type RoleRow = {
  id: number
  name: string
  code: string
  permissions: string[]
  createdAt: string
}

type UserRow = {
  id: number
  username: string
  status: number
  roles: Array<{ id: number; name?: string | null; code?: string | null }>
  roleIds: number[]
  createdAt: string
}

type FilterValues = {
  keyword?: string
}

type RoleFormValues = {
  name: string
  code: string
  permissions: string[]
}

type UserFormValues = {
  username: string
  status: boolean
  roleIds: number[]
  password?: string
}

type ResetPwdValues = {
  password: string
}

function permOptions() {
  const out: Array<{ value: string; label: string }> = [{ value: '*', label: '全部权限' }]
  const walk = (nodes: any[], prefix: string) => {
    for (const n of nodes) {
      const p = prefix ? `${prefix} / ${n.name}` : n.name
      if (n.path) out.push({ value: String(n.key), label: p })
      if (Array.isArray(n.children) && n.children.length) walk(n.children, p)
    }
  }
  walk(menuTree as any, '')
  const seen = new Set<string>()
  return out.filter((x) => {
    if (seen.has(x.value)) return false
    seen.add(x.value)
    return true
  })
}

export function UsersRolesPage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [roleForm] = Form.useForm<RoleFormValues>()
  const [userForm] = Form.useForm<UserFormValues>()
  const [pwdForm] = Form.useForm<ResetPwdValues>()
  const [roles, setRoles] = useState<RoleRow[]>([])
  const [users, setUsers] = useState<UserRow[]>([])
  const [loading, setLoading] = useState(false)
  const [activeTab, setActiveTab] = useState('users')
  const [roleModalOpen, setRoleModalOpen] = useState(false)
  const [userModalOpen, setUserModalOpen] = useState(false)
  const [pwdModalOpen, setPwdModalOpen] = useState(false)
  const [editingRoleId, setEditingRoleId] = useState<number | null>(null)
  const [editingUserId, setEditingUserId] = useState<number | null>(null)
  const [pwdUserId, setPwdUserId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)
  const timerRef = useRef<number | null>(null)
  const perms = useMemo(() => permOptions(), [])
  const roleOptions = useMemo(() => roles.map((r) => ({ value: r.id, label: r.name })), [roles])

  const loadData = async (kw?: string) => {
    setLoading(true)
    try {
      const keyword = (kw ?? form.getFieldValue('keyword') ?? '').trim()

      const [rolesRes, usersRes] = await Promise.all([
        apiFetch<any>('/api/v1/admin/admin-roles'),
        apiFetch<any>(`/api/v1/admin/admin-users${keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''}`),
      ])

      if (rolesRes.code !== 0) {
        message.error(rolesRes.message || '加载角色失败')
        return
      }
      if (usersRes.code !== 0) {
        message.error(usersRes.message || '加载用户失败')
        return
      }

      setRoles(
        (rolesRes.data?.items ?? []).map((x: any) => ({
          id: Number(x.id),
          name: String(x.name),
          code: String(x.code),
          permissions: Array.isArray(x.permissions) ? x.permissions.map((p: any) => String(p)) : [],
          createdAt: String(x.createdAt ?? '').replace('T', ' '),
        })),
      )

      setUsers(
        (usersRes.data?.items ?? []).map((x: any) => ({
          id: Number(x.id),
          username: String(x.username),
          status: Number(x.status),
          roles: Array.isArray(x.roles) ? x.roles.map((r: any) => ({ id: Number(r.id), name: r.name ?? null, code: r.code ?? null })) : [],
          roleIds: Array.isArray(x.roleIds) ? x.roleIds.map((n: any) => Number(n)) : [],
          createdAt: String(x.createdAt ?? '').replace('T', ' '),
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

  const openCreateRole = () => {
    setEditingRoleId(null)
    roleForm.setFieldsValue({ name: '', code: '', permissions: [] })
    setRoleModalOpen(true)
  }

  const openEditRole = (r: RoleRow) => {
    setEditingRoleId(r.id)
    roleForm.setFieldsValue({ name: r.name, code: r.code, permissions: r.permissions ?? [] })
    setRoleModalOpen(true)
  }

  const saveRole = async () => {
    const v = await roleForm.validateFields()
    setSaving(true)
    try {
      const payload = { name: v.name.trim(), code: v.code.trim(), permissions: v.permissions ?? [] }
      const res = editingRoleId
        ? await apiFetch<any>(`/api/v1/admin/admin-roles/${editingRoleId}`, { method: 'PUT', body: JSON.stringify(payload) })
        : await apiFetch<any>('/api/v1/admin/admin-roles', { method: 'POST', body: JSON.stringify(payload) })
      if (res.code !== 0) {
        message.error(res.message || '保存失败')
        return
      }
      message.success('已保存')
      setRoleModalOpen(false)
      await loadData()
    } finally {
      setSaving(false)
    }
  }

  const deleteRole = (r: RoleRow) => {
    Modal.confirm({
      title: `删除角色「${r.name}」？`,
      content: '删除后会同时移除所有用户的该角色关联。',
      okText: '删除',
      okButtonProps: { danger: true },
      cancelText: '取消',
      onOk: async () => {
        const res = await apiFetch<any>(`/api/v1/admin/admin-roles/${r.id}`, { method: 'DELETE' })
        if (res.code !== 0) {
          message.error(res.message || '删除失败')
          return
        }
        message.success('已删除')
        await loadData()
      },
    })
  }

  const openCreateUser = () => {
    setEditingUserId(null)
    userForm.setFieldsValue({ username: '', status: true, roleIds: [], password: '' })
    setUserModalOpen(true)
  }

  const openEditUser = (u: UserRow) => {
    setEditingUserId(u.id)
    userForm.setFieldsValue({ username: u.username, status: u.status === 1, roleIds: u.roleIds ?? [] })
    setUserModalOpen(true)
  }

  const saveUser = async () => {
    const v = await userForm.validateFields()
    if (!editingUserId && String(v.password ?? '').trim().length < 6) {
      message.error('密码至少6位')
      return
    }
    setSaving(true)
    try {
      const payload: any = {
        username: v.username.trim(),
        status: v.status ? 1 : 0,
        roleIds: v.roleIds ?? [],
      }
      if (!editingUserId) payload.password = String(v.password ?? '')

      const res = editingUserId
        ? await apiFetch<any>(`/api/v1/admin/admin-users/${editingUserId}`, { method: 'PUT', body: JSON.stringify(payload) })
        : await apiFetch<any>('/api/v1/admin/admin-users', { method: 'POST', body: JSON.stringify(payload) })

      if (res.code !== 0) {
        message.error(res.message || '保存失败')
        return
      }
      message.success('已保存')
      setUserModalOpen(false)
      await loadData()
    } finally {
      setSaving(false)
    }
  }

  const deleteUser = (u: UserRow) => {
    Modal.confirm({
      title: `删除账号「${u.username}」？`,
      content: '删除后无法恢复。',
      okText: '删除',
      okButtonProps: { danger: true },
      cancelText: '取消',
      onOk: async () => {
        const res = await apiFetch<any>(`/api/v1/admin/admin-users/${u.id}`, { method: 'DELETE' })
        if (res.code !== 0) {
          message.error(res.message || '删除失败')
          return
        }
        message.success('已删除')
        await loadData()
      },
    })
  }

  const openResetPwd = (u: UserRow) => {
    setPwdUserId(u.id)
    pwdForm.setFieldsValue({ password: '' })
    setPwdModalOpen(true)
  }

  const savePwd = async () => {
    const v = await pwdForm.validateFields()
    const id = pwdUserId
    if (!id) return
    const pwd = String(v.password ?? '').trim()
    if (pwd.length < 6) {
      message.error('密码至少6位')
      return
    }
    setSaving(true)
    try {
      const res = await apiFetch<any>(`/api/v1/admin/admin-users/${id}/password`, { method: 'PUT', body: JSON.stringify({ password: pwd }) })
      if (res.code !== 0) {
        message.error(res.message || '保存失败')
        return
      }
      message.success('已重置')
      setPwdModalOpen(false)
    } finally {
      setSaving(false)
    }
  }

  const roleColumns: ColumnsType<RoleRow> = [
    { title: '角色ID', dataIndex: 'id', width: 120 },
    { title: '角色名称', dataIndex: 'name', width: 200, render: (v: string) => <Typography.Text strong>{v}</Typography.Text> },
    { title: '角色编码', dataIndex: 'code', width: 180, render: (v: string) => <Tag>{v}</Tag> },
    { title: '权限', dataIndex: 'permissions', width: 420, render: (v: string[]) => (v?.length ? <Space wrap>{v.map((x) => <Tag key={x}>{x}</Tag>)}</Space> : '-') },
    { title: '创建时间', dataIndex: 'createdAt', width: 180 },
    {
      title: '操作',
      dataIndex: 'actions',
      width: 170,
      fixed: screens.lg ? 'right' : undefined,
      render: (_, r) => (
        <Space>
          <Button type="link" onClick={() => openEditRole(r)}>
            编辑
          </Button>
          <Button type="link" danger onClick={() => deleteRole(r)}>
            删除
          </Button>
        </Space>
      ),
    },
  ]

  const userColumns: ColumnsType<UserRow> = [
    { title: '用户ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '账号', dataIndex: 'username', width: 220, render: (v: string) => <Typography.Text strong>{v}</Typography.Text> },
    { title: '状态', dataIndex: 'status', width: 120, render: (v: number) => <Tag color={v === 1 ? 'green' : 'red'}>{v === 1 ? '启用' : '停用'}</Tag> },
    { title: '角色', dataIndex: 'roles', width: 420, render: (v: UserRow['roles']) => (v?.length ? <Space wrap>{v.map((x) => <Tag key={x.id}>{x.name ?? x.code ?? x.id}</Tag>)}</Space> : '-') },
    { title: '创建时间', dataIndex: 'createdAt', width: 180 },
    {
      title: '操作',
      dataIndex: 'actions',
      width: 220,
      fixed: screens.lg ? 'right' : undefined,
      render: (_, r) => (
        <Space>
          <Button type="link" onClick={() => openEditUser(r)}>
            编辑
          </Button>
          <Button type="link" onClick={() => openResetPwd(r)}>
            重置密码
          </Button>
          <Button type="link" danger onClick={() => deleteUser(r)}>
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
          onValuesChange={() => {
            const keyword = String(form.getFieldValue('keyword') ?? '')
            if (timerRef.current) window.clearTimeout(timerRef.current)
            timerRef.current = window.setTimeout(() => loadData(keyword), 300)
          }}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={10} lg={8}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="账号/用户ID" allowClear />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Space wrap>
                <Button icon={<ReloadOutlined />} loading={loading} onClick={() => loadData()}>
                  刷新
                </Button>
                <Button
                  onClick={() => {
                    form.resetFields()
                    loadData()
                  }}
                >
                  重置
                </Button>
              </Space>
            </Col>
          </Row>
        </Form>
      </Card>

      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'users',
            label: '账号',
            children: (
              <Card
                title={
                  <Space style={{ width: '100%', justifyContent: 'space-between' }} wrap>
                    <span>账号列表</span>
                    <Button type="primary" icon={<PlusOutlined />} onClick={openCreateUser}>
                      新增账号
                    </Button>
                  </Space>
                }
                style={{ borderRadius: 16 }}
                styles={{ body: { padding: 0 } }}
              >
                <Table rowKey="id" columns={userColumns} dataSource={users} loading={loading} pagination={false} scroll={{ x: 1400 }} />
              </Card>
            ),
          },
          {
            key: 'roles',
            label: '角色',
            children: (
              <Card
                title={
                  <Space style={{ width: '100%', justifyContent: 'space-between' }} wrap>
                    <span>角色列表</span>
                    <Button type="primary" icon={<PlusOutlined />} onClick={openCreateRole}>
                      新增角色
                    </Button>
                  </Space>
                }
                style={{ borderRadius: 16 }}
                styles={{ body: { padding: 0 } }}
              >
                <Table rowKey="id" columns={roleColumns} dataSource={roles} loading={loading} pagination={false} scroll={{ x: 1200 }} />
              </Card>
            ),
          },
        ]}
      />

      <Modal
        open={roleModalOpen}
        title={editingRoleId ? '编辑角色' : '新增角色'}
        onCancel={() => setRoleModalOpen(false)}
        onOk={saveRole}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={roleForm} layout="vertical">
          <Form.Item name="name" label="角色名称" rules={[{ required: true, message: '请输入角色名称' }]}>
            <Input placeholder="例如：证书管理员" />
          </Form.Item>
          <Form.Item name="code" label="角色编码" rules={[{ required: true, message: '请输入角色编码' }]}>
            <Input placeholder="例如：cert" />
          </Form.Item>
          <Form.Item name="permissions" label="权限点">
            <Select mode="multiple" options={perms} placeholder="可多选；选“全部权限”表示不限制" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        open={userModalOpen}
        title={editingUserId ? '编辑账号' : '新增账号'}
        onCancel={() => setUserModalOpen(false)}
        onOk={saveUser}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={userForm} layout="vertical">
          <Form.Item name="username" label="账号" rules={[{ required: true, message: '请输入账号' }]}>
            <Input placeholder="例如：admin2" />
          </Form.Item>
          <Form.Item name="status" label="启用" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Form.Item name="roleIds" label="角色">
            <Select mode="multiple" options={roleOptions} placeholder="可多选" />
          </Form.Item>
          {!editingUserId && (
            <Form.Item name="password" label="初始密码" rules={[{ required: true, message: '请输入初始密码' }]}>
              <Input.Password placeholder="至少6位" />
            </Form.Item>
          )}
        </Form>
      </Modal>

      <Modal
        open={pwdModalOpen}
        title="重置密码"
        onCancel={() => setPwdModalOpen(false)}
        onOk={savePwd}
        confirmLoading={saving}
        destroyOnClose
      >
        <Form form={pwdForm} layout="vertical">
          <Form.Item name="password" label="新密码" rules={[{ required: true, message: '请输入新密码' }]}>
            <Input.Password placeholder="至少6位" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  )
}

