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
  EyeOutlined,
} from '@ant-design/icons'
import { useState, useEffect } from 'react'
import { downloadCsv } from '../../lib/exportCsv'
import { apiFetch } from '../../lib/api'

type EntryStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
type EnrollChannel = 1 | 2 | 3

type EntryRow = {
  id: number
  eventId: number
  eventTitle?: string | null
  groupId: number
  groupName?: string | null
  athletePersonId: number
  athleteName?: string | null
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

type FilterValues = {
  eventId?: string
  status?: EntryStatus | 'all'
  enrollChannel?: EnrollChannel | 'all'
  keyword?: string
}

export function EntriesReviewPage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [rows, setRows] = useState<EntryRow[]>([])
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  const [drawerOpen, setDrawerOpen] = useState(false)
  const [current, setCurrent] = useState<EntryRow | null>(null)

  const loadData = async (p = page, ps = pageSize) => {
    setLoading(true)
    try {
      const v = form.getFieldsValue() as FilterValues
      const qs = new URLSearchParams()
      const eventId = (v.eventId ?? '').trim()
      const keyword = (v.keyword ?? '').trim()
      const status = v.status ?? 'all'
      const enrollChannel = v.enrollChannel ?? 'all'
      if (eventId) qs.set('eventId', eventId)
      if (keyword) qs.set('keyword', keyword)
      if (status !== 'all') qs.set('status', String(status))
      if (enrollChannel !== 'all') qs.set('enrollChannel', String(enrollChannel))
      qs.set('page', String(p))
      qs.set('pageSize', String(ps))
      const res = await apiFetch<any>(`/api/v1/admin/entries?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setTotal(res.data?.total ?? 0)
      const items = (res.data?.items ?? []).map((x: any) => ({
        id: Number(x.id),
        eventId: Number(x.eventId),
        eventTitle: x.eventTitle ?? null,
        groupId: Number(x.groupId),
        groupName: x.groupName ?? null,
        athletePersonId: Number(x.athletePersonId),
        athleteName: x.athleteName ?? null,
        enrollChannel: Number(x.enrollChannel) as EnrollChannel,
        enrollUserId: Number(x.enrollUserId),
        status: Number(x.status) as EntryStatus,
        auditRemark: x.auditRemark ?? undefined,
        payOrderId: x.payOrderId != null ? Number(x.payOrderId) : undefined,
        createdAt: String(x.createdAt ?? '').replace('T', ' '),
      }))
      setRows(items)
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

  const approve = (r: EntryRow) => {
    if (r.status !== 0) {
      message.warning('当前状态不可审核通过')
      return
    }
    Modal.confirm({
      title: '通过报名审核',
      okText: '确认通过',
      cancelText: '取消',
      content: '通过后进入待缴费状态',
      onOk: async () => {
        const res = await apiFetch<any>(`/api/v1/admin/entries/${r.id}/approve`, {
          method: 'POST',
          body: JSON.stringify({ remark: null }),
        })
        if (res.code !== 0) {
          message.error(res.message || '操作失败')
          return
        }
        message.success('已通过，进入待缴费')
        await loadData()
      },
    })
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
        const res = await apiFetch<any>(`/api/v1/admin/entries/${r.id}/reject`, {
          method: 'POST',
          body: JSON.stringify({ reason }),
        })
        if (res.code !== 0) {
          message.error(res.message || '操作失败')
          return
        }
        message.success('已驳回')
        await loadData()
      },
    })
  }

  const columns: ColumnsType<EntryRow> = [
    { title: '报名ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '活动', dataIndex: 'eventTitle', width: 260, render: (_, r) => r.eventTitle ?? '-' },
    { title: '组别', dataIndex: 'groupName', width: 200, render: (_, r) => r.groupName ?? '-' },
    { title: '运动员', dataIndex: 'athleteName', width: 160, render: (_, r) => r.athleteName ?? '-' },
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
            shape="circle"
            title="详情"
            icon={<EyeOutlined />}
            onClick={() => {
              setCurrent(r)
              setDrawerOpen(true)
            }}
          />
          <Button size="small" shape="circle" title="通过" type="primary" icon={<CheckOutlined />} onClick={() => approve(r)} />
          <Button size="small" shape="circle" title="驳回" danger icon={<CloseOutlined />} onClick={() => reject(r)} />
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
          onValuesChange={() => {
            setPage(1)
            loadData(1, pageSize)
          }}
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
                      `entries_${Date.now()}.csv`,
                      rows,
                      [
                        { title: '报名ID', value: (r) => r.id },
                        { title: '活动', value: (r) => r.eventTitle ?? '' },
                        { title: '组别', value: (r) => r.groupName ?? '' },
                        { title: '运动员', value: (r) => r.athleteName ?? '' },
                        { title: '报名方式', value: (r) => channelLabel(r.enrollChannel) },
                        { title: '状态', value: (r) => statusText(r.status) },
                        { title: '提交时间', value: (r) => r.createdAt },
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
          dataSource={rows}
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            onChange: (p, ps) => {
              setPage(p)
              setPageSize(ps)
              loadData(p, ps)
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
                <Descriptions.Item label="活动">{current.eventTitle ?? '-'}</Descriptions.Item>
                <Descriptions.Item label="组别">{current.groupName ?? '-'}</Descriptions.Item>
                <Descriptions.Item label="运动员">{current.athleteName ?? '-'}</Descriptions.Item>
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

