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

type EntryStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
type EnrollChannel = 1 | 2 | 3

type EntryRow = {
  id: number
  eventId: number
  groupId: number
  athletePersonId: number
  enrollChannel: EnrollChannel
  enrollUserId: number
  status: EntryStatus
  auditRemark?: string
  payOrderId?: number
  createdAt: string
}

function statusText(status: EntryStatus) {
  const map: Record<EntryStatus, { text: string; color?: string }> = {
    0: { text: '待审核' },
    1: { text: '驳回', color: 'red' },
    2: { text: '待缴费', color: 'gold' },
    3: { text: '支付中', color: 'processing' },
    4: { text: '已缴费', color: 'green' },
    5: { text: '已确认', color: 'green' },
    6: { text: '已取消' },
    7: { text: '支付失败', color: 'orange' },
    8: { text: '退款中', color: 'processing' },
    9: { text: '已退款' },
  }
  return map[status].text
}

function statusTag(status: EntryStatus) {
  const map: Record<EntryStatus, { text: string; color?: string }> = {
    0: { text: '待审核' },
    1: { text: '驳回', color: 'red' },
    2: { text: '待缴费', color: 'gold' },
    3: { text: '支付中', color: 'processing' },
    4: { text: '已缴费', color: 'green' },
    5: { text: '已确认', color: 'green' },
    6: { text: '已取消' },
    7: { text: '支付失败', color: 'orange' },
    8: { text: '退款中', color: 'processing' },
    9: { text: '已退款' },
  }
  const v = map[status]
  return <Tag color={v.color}>{v.text}</Tag>
}

function channelLabel(c: EnrollChannel) {
  if (c === 1) return '运动员'
  if (c === 2) return '家长'
  return '教练(第一教练)'
}

function createMockRows(): EntryRow[] {
  return Array.from({ length: 55 }).map((_, i) => {
    const dt = randomDateWithinDays(45)
    const status: EntryStatus = (i % 7 === 0 ? 1 : i % 9 === 0 ? 2 : 0) as EntryStatus
    return {
      id: 50000 + i,
      eventId: 1000 + (i % 6),
      groupId: 200 + (i % 4),
      athletePersonId: 90000 + randomInt(1, 600),
      enrollChannel: ((i % 3) + 1) as EnrollChannel,
      enrollUserId: 70000 + randomInt(1, 300),
      status,
      auditRemark: status === 1 ? '资料不完整，请补充学校信息' : undefined,
      payOrderId: status >= 3 ? 80000 + i : undefined,
      createdAt: formatDateTime(dt),
    }
  })
}

type FilterValues = {
  eventId?: string
  status?: EntryStatus | 'all'
  enrollChannel?: EnrollChannel | 'all'
  keyword?: string
}

export function EntriesReviewPage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [rows, setRows] = useState<EntryRow[]>(() => createMockRows())
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const [drawerOpen, setDrawerOpen] = useState(false)
  const [current, setCurrent] = useState<EntryRow | null>(null)

  const values = Form.useWatch([], form)

  const filtered = useMemo(() => {
    const v = (values ?? {}) as FilterValues
    const kw = (v.keyword ?? '').trim()
    const st = v.status ?? 'all'
    const ch = v.enrollChannel ?? 'all'
    const eventId = (v.eventId ?? '').trim()

    return rows.filter((r) => {
      if (eventId && String(r.eventId) !== eventId) return false
      if (st !== 'all' && r.status !== st) return false
      if (ch !== 'all' && r.enrollChannel !== ch) return false
      if (kw) {
        const hay = `${r.id} ${r.athletePersonId} ${r.enrollUserId}`
        if (!hay.includes(kw)) return false
      }
      return true
    })
  }, [rows, values])

  const paged = useMemo(() => {
    const start = (page - 1) * pageSize
    return filtered.slice(start, start + pageSize)
  }, [filtered, page, pageSize])

  const approve = (r: EntryRow) => {
    if (r.status !== 0) {
      message.warning('当前状态不可审核通过')
      return
    }
    setRows((prev) => prev.map((x) => (x.id === r.id ? { ...x, status: 2, auditRemark: undefined } : x)))
    message.success('已通过，进入待缴费')
  }

  const reject = (r: EntryRow) => {
    if (r.status !== 0) {
      message.warning('当前状态不可审核驳回')
      return
    }
    Modal.confirm({
      title: '驳回报名',
      okText: '确认驳回',
      cancelText: '取消',
      content: (
        <Form layout="vertical" id="reject-form">
          <Form.Item
            name="reason"
            label="驳回原因"
            rules={[{ required: true, message: '请输入驳回原因' }]}
          >
            <Input.TextArea autoSize={{ minRows: 3, maxRows: 6 }} placeholder="例如：学校信息缺失" />
          </Form.Item>
        </Form>
      ),
      onOk: async () => {
        const formEl = document.getElementById('reject-form')
        if (!formEl) return
        const textarea = formEl.querySelector('textarea') as HTMLTextAreaElement | null
        const reason = textarea?.value?.trim() ?? ''
        if (!reason) {
          message.error('请输入驳回原因')
          throw new Error('reason required')
        }
        setRows((prev) => prev.map((x) => (x.id === r.id ? { ...x, status: 1, auditRemark: reason } : x)))
        message.success('已驳回')
      },
    })
  }

  const columns: ColumnsType<EntryRow> = [
    { title: '报名ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '活动ID', dataIndex: 'eventId', width: 100 },
    { title: '组别ID', dataIndex: 'groupId', width: 100 },
    { title: '运动员ID', dataIndex: 'athletePersonId', width: 120 },
    {
      title: '报名方式',
      dataIndex: 'enrollChannel',
      width: 140,
      render: (v: EnrollChannel) => <Tag>{channelLabel(v)}</Tag>,
    },
    { title: '状态', dataIndex: 'status', width: 110, render: (v: EntryStatus) => statusTag(v) },
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
          initialValues={{ status: 'all', enrollChannel: 'all' }}
          onValuesChange={() => setPage(1)}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="eventId" label="活动ID">
                <Input placeholder="例如：1000" allowClear />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="status" label="状态">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 0, label: '待审核' },
                    { value: 2, label: '待缴费' },
                    { value: 4, label: '已缴费' },
                    { value: 1, label: '驳回' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="enrollChannel" label="报名方式">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 1, label: '运动员' },
                    { value: 2, label: '家长' },
                    { value: 3, label: '教练(第一教练)' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="报名ID/运动员ID/账号ID" allowClear />
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
                      `entries_${Date.now()}.csv`,
                      filtered,
                      [
                        { title: '报名ID', value: (r) => r.id },
                        { title: '活动ID', value: (r) => r.eventId },
                        { title: '组别ID', value: (r) => r.groupId },
                        { title: '运动员ID', value: (r) => r.athletePersonId },
                        { title: '报名方式', value: (r) => channelLabel(r.enrollChannel) },
                        { title: '状态', value: (r) => statusText(r.status) },
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
          scroll={{ x: 1200 }}
        />
      </Card>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={screens.md ? 640 : '100%'}
        title="报名详情"
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
                <Descriptions.Item label="报名ID">{current.id}</Descriptions.Item>
                <Descriptions.Item label="活动ID">{current.eventId}</Descriptions.Item>
                <Descriptions.Item label="组别ID">{current.groupId}</Descriptions.Item>
                <Descriptions.Item label="运动员ID">{current.athletePersonId}</Descriptions.Item>
                <Descriptions.Item label="报名方式">{channelLabel(current.enrollChannel)}</Descriptions.Item>
                <Descriptions.Item label="状态">{statusTag(current.status)}</Descriptions.Item>
                <Descriptions.Item label="驳回原因">{current.auditRemark ?? '-'}</Descriptions.Item>
                <Descriptions.Item label="支付订单">{current.payOrderId ?? '-'}</Descriptions.Item>
                <Descriptions.Item label="提交时间">{current.createdAt}</Descriptions.Item>
              </Descriptions>
            </Card>
            <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 16 } }}>
              <Typography.Title level={5} style={{ marginTop: 0 }}>
                审核提示
              </Typography.Title>
              <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
                教练代报名仅允许第一教练员；认证未通过可按策略禁止报名；审核通过后进入待缴费状态。
              </Typography.Paragraph>
            </Card>
          </Space>
        ) : null}
      </Drawer>
    </Space>
  )
}

