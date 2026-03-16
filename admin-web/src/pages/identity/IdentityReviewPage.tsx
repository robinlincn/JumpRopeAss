import {
  Button,
  Card,
  Col,
  Descriptions,
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
  CheckOutlined,
  CloseOutlined,
  ExportOutlined,
  ReloadOutlined,
} from '@ant-design/icons'
import { useMemo, useState } from 'react'
import { downloadCsv } from '../../lib/exportCsv'
import { formatDateTime, randomDateWithinDays, randomInt } from '../../lib/mockData'

type IdentityStatus = 0 | 1 | 2

type IdentityRow = {
  id: number
  userId: number
  realName: string
  idCardNo: string
  mobile: string
  status: IdentityStatus
  rejectReason?: string
  createdAt: string
}

function statusInfo(s: IdentityStatus) {
  if (s === 0) return { text: '待审核', color: 'processing' as const }
  if (s === 1) return { text: '已通过', color: 'green' as const }
  return { text: '已驳回', color: 'red' as const }
}

function createMockRows(): IdentityRow[] {
  const names = ['张三', '李四', '王五', '赵六', '钱七', '孙八', '周九']
  return Array.from({ length: 44 }).map((_, i) => {
    const dt = randomDateWithinDays(30)
    const status: IdentityStatus = (i % 6 === 0 ? 2 : i % 5 === 0 ? 1 : 0) as IdentityStatus
    const name = names[i % names.length]
    return {
      id: 70000 + i,
      userId: 60000 + randomInt(1, 600),
      realName: name,
      idCardNo: `4301************${randomInt(1000, 9999)}`,
      mobile: `13${randomInt(0, 9)}${randomInt(10000000, 99999999)}`,
      status,
      rejectReason: status === 2 ? '身份证号与姓名不一致' : undefined,
      createdAt: formatDateTime(dt),
    }
  })
}

type FilterValues = {
  status?: IdentityStatus | 'all'
  keyword?: string
}

export function IdentityReviewPage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [rows, setRows] = useState<IdentityRow[]>(() => createMockRows())
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const [drawerOpen, setDrawerOpen] = useState(false)
  const [current, setCurrent] = useState<IdentityRow | null>(null)

  const values = Form.useWatch([], form)

  const filtered = useMemo(() => {
    const v = (values ?? {}) as FilterValues
    const kw = (v.keyword ?? '').trim()
    const st = v.status ?? 'all'

    return rows.filter((r) => {
      if (st !== 'all' && r.status !== st) return false
      if (kw) {
        const hay = `${r.realName} ${r.mobile} ${r.userId} ${r.id}`
        if (!hay.includes(kw)) return false
      }
      return true
    })
  }, [rows, values])

  const paged = useMemo(() => {
    const start = (page - 1) * pageSize
    return filtered.slice(start, start + pageSize)
  }, [filtered, page, pageSize])

  const approve = (r: IdentityRow) => {
    if (r.status !== 0) {
      message.warning('当前状态不可审核通过')
      return
    }
    setRows((prev) => prev.map((x) => (x.id === r.id ? { ...x, status: 1, rejectReason: undefined } : x)))
    message.success('已通过认证')
  }

  const reject = (r: IdentityRow) => {
    if (r.status !== 0) {
      message.warning('当前状态不可审核驳回')
      return
    }
    Modal.confirm({
      title: '驳回认证',
      okText: '确认驳回',
      cancelText: '取消',
      content: (
        <Form layout="vertical" id="identity-reject-form">
          <Form.Item
            name="reason"
            label="驳回原因"
            rules={[{ required: true, message: '请输入驳回原因' }]}
          >
            <Input.TextArea autoSize={{ minRows: 3, maxRows: 6 }} placeholder="例如：照片不清晰/信息不一致" />
          </Form.Item>
        </Form>
      ),
      onOk: async () => {
        const formEl = document.getElementById('identity-reject-form')
        if (!formEl) return
        const textarea = formEl.querySelector('textarea') as HTMLTextAreaElement | null
        const reason = textarea?.value?.trim() ?? ''
        if (!reason) {
          message.error('请输入驳回原因')
          throw new Error('reason required')
        }
        setRows((prev) => prev.map((x) => (x.id === r.id ? { ...x, status: 2, rejectReason: reason } : x)))
        message.success('已驳回')
      },
    })
  }

  const columns: ColumnsType<IdentityRow> = [
    { title: '提交ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '账号ID', dataIndex: 'userId', width: 120 },
    { title: '姓名', dataIndex: 'realName', width: 120, render: (v: string) => <Typography.Text strong>{v}</Typography.Text> },
    { title: '手机号', dataIndex: 'mobile', width: 140 },
    {
      title: '状态',
      dataIndex: 'status',
      width: 110,
      render: (v: IdentityStatus) => <Tag color={statusInfo(v).color}>{statusInfo(v).text}</Tag>,
    },
    { title: '提交时间', dataIndex: 'createdAt', width: 170 },
    {
      title: '操作',
      key: 'action',
      width: 260,
      fixed: screens.lg ? 'right' : undefined,
      render: (_, r) => (
        <Space size={6} wrap>
          <Button
            size="small"
            onClick={() => {
              setCurrent(r)
              setDrawerOpen(true)
            }}
          >
            详情
          </Button>
          <Button size="small" type="primary" icon={<CheckOutlined />} onClick={() => approve(r)}>
            通过
          </Button>
          <Button size="small" danger icon={<CloseOutlined />} onClick={() => reject(r)}>
            驳回
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
          initialValues={{ status: 'all' }}
          onValuesChange={() => setPage(1)}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="status" label="状态">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 0, label: '待审核' },
                    { value: 1, label: '已通过' },
                    { value: 2, label: '已驳回' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={10}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="姓名/手机号/账号ID/提交ID" allowClear />
              </Form.Item>
            </Col>
            <Col xs={24}>
              <Space wrap>
                <Button
                  icon={<ReloadOutlined />}
                  loading={loading}
                  onClick={async () => {
                    setLoading(true)
                    await new Promise((r) => setTimeout(r, 450))
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
                      `identity_${Date.now()}.csv`,
                      filtered,
                      [
                        { title: '提交ID', value: (r) => r.id },
                        { title: '账号ID', value: (r) => r.userId },
                        { title: '姓名', value: (r) => r.realName },
                        { title: '手机号', value: (r) => r.mobile },
                        { title: '状态', value: (r) => statusInfo(r.status).text },
                        { title: '驳回原因', value: (r) => r.rejectReason ?? '' },
                        { title: '提交时间', value: (r) => r.createdAt },
                      ],
                    )
                  }}
                >
                  导出
                </Button>
                <Button onClick={() => form.resetFields()}>重置</Button>
                <Typography.Text type="secondary">待审核：{rows.filter((x) => x.status === 0).length}</Typography.Text>
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
        title="认证详情"
        extra={
          current ? (
            <Space>
              <Button onClick={() => setDrawerOpen(false)}>关闭</Button>
              <Button type="primary" icon={<CheckOutlined />} onClick={() => approve(current)}>
                通过
              </Button>
              <Button danger icon={<CloseOutlined />} onClick={() => reject(current)}>
                驳回
              </Button>
            </Space>
          ) : null
        }
      >
        {current ? (
          <Space direction="vertical" style={{ width: '100%' }} size={12}>
            <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 16 } }}>
              <Descriptions column={1} size="small" bordered>
                <Descriptions.Item label="提交ID">{current.id}</Descriptions.Item>
                <Descriptions.Item label="账号ID">{current.userId}</Descriptions.Item>
                <Descriptions.Item label="姓名">{current.realName}</Descriptions.Item>
                <Descriptions.Item label="身份证号">{current.idCardNo}</Descriptions.Item>
                <Descriptions.Item label="手机号">{current.mobile}</Descriptions.Item>
                <Descriptions.Item label="状态">
                  <Tag color={statusInfo(current.status).color}>{statusInfo(current.status).text}</Tag>
                </Descriptions.Item>
                <Descriptions.Item label="驳回原因">{current.rejectReason ?? '-'}</Descriptions.Item>
                <Descriptions.Item label="提交时间">{current.createdAt}</Descriptions.Item>
              </Descriptions>
            </Card>
            <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 16 } }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                审核建议
              </Typography.Title>
              <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
                认证通过后可作为报名/资料变更等关键操作门槛；驳回时需填写明确原因，便于用户修正后重新提交。
              </Typography.Paragraph>
            </Card>
          </Space>
        ) : null}
      </Drawer>
    </Space>
  )
}

