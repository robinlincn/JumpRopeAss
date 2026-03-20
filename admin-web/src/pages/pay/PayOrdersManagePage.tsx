import { Button, Card, Col, Form, Grid, Input, Row, Select, Space, Table, Tag, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined } from '@ant-design/icons'
import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'

type BizType = 1 | 2 | 3
type PayStatus = 0 | 1 | 2 | 3

type PayOrderRow = {
  id: number
  bizType: BizType
  bizId: number
  bizName?: string | null
  userId: number
  userName?: string | null
  userMobile?: string | null
  amount: number
  status: PayStatus
  wxOutTradeNo?: string | null
  paidAt?: string | null
  createdAt: string
}

type FilterValues = {
  status?: PayStatus | 'all'
  bizType?: BizType | 'all'
  keyword?: string
}

function bizTypeTag(v: BizType) {
  const t = v === 1 ? { text: '活动报名', color: 'blue' } : v === 2 ? { text: '首次发证', color: 'purple' } : { text: '补证', color: 'geekblue' }
  return <Tag color={t.color}>{t.text}</Tag>
}

function statusTag(v: PayStatus) {
  const t = v === 0 ? { text: '待支付', color: 'default' } : v === 1 ? { text: '已支付', color: 'green' } : v === 2 ? { text: '已关闭', color: 'red' } : { text: '已退款', color: 'orange' }
  return <Tag color={t.color}>{t.text}</Tag>
}

function yuan(amountFen: number) {
  return `¥${(amountFen / 100).toFixed(2)}`
}

export function PayOrdersManagePage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm<FilterValues>()
  const [rows, setRows] = useState<PayOrderRow[]>([])
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  const loadData = async (p = page, ps = pageSize) => {
    setLoading(true)
    try {
      const v = form.getFieldsValue() as FilterValues
      const qs = new URLSearchParams()
      const keyword = (v.keyword ?? '').trim()
      const status = v.status ?? 'all'
      const bizType = v.bizType ?? 'all'
      if (keyword) qs.set('keyword', keyword)
      if (status !== 'all') qs.set('status', String(status))
      if (bizType !== 'all') qs.set('bizType', String(bizType))
      qs.set('page', String(p))
      qs.set('pageSize', String(ps))
      const res = await apiFetch<any>(`/api/v1/admin/pay-orders?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setTotal(res.data?.total ?? 0)
      const items = (res.data?.items ?? []).map((x: any) => ({
        id: Number(x.id),
        bizType: Number(x.bizType) as BizType,
        bizId: Number(x.bizId),
        bizName: x.bizName ?? null,
        userId: Number(x.userId),
        userName: x.userName ?? null,
        userMobile: x.userMobile ?? null,
        amount: Number(x.amount),
        status: Number(x.status) as PayStatus,
        wxOutTradeNo: x.wxOutTradeNo ?? null,
        paidAt: x.paidAt ? String(x.paidAt).replace('T', ' ') : null,
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

  const columns: ColumnsType<PayOrderRow> = [
    { title: '订单ID', dataIndex: 'id', width: 120, fixed: screens.lg ? 'left' : undefined },
    { title: '业务类型', dataIndex: 'bizType', width: 120, render: (v: BizType) => bizTypeTag(v) },
    { title: '业务ID', dataIndex: 'bizId', width: 120 },
    { title: '下单账号ID', dataIndex: 'userId', width: 120 },
    { title: '金额', dataIndex: 'amount', width: 120, render: (v: number) => <Typography.Text strong>{yuan(v)}</Typography.Text> },
    { title: '状态', dataIndex: 'status', width: 110, render: (v: PayStatus) => statusTag(v) },
    { title: '商户订单号', dataIndex: 'wxOutTradeNo', width: 240, render: (v?: string | null) => v ?? '-' },
    { title: '支付时间', dataIndex: 'paidAt', width: 170, render: (v?: string | null) => v ?? '-' },
    { title: '创建时间', dataIndex: 'createdAt', width: 170 },
  ]

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
        <Form
          form={form}
          layout="vertical"
          initialValues={{ status: 'all', bizType: 'all' }}
          onValuesChange={() => {
            setPage(1)
            loadData(1, pageSize)
          }}
        >
          <Row gutter={[16, 8]}>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="status" label="状态">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 0, label: '待支付' },
                    { value: 1, label: '已支付' },
                    { value: 2, label: '已关闭' },
                    { value: 3, label: '已退款' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={6}>
              <Form.Item name="bizType" label="业务类型">
                <Select
                  options={[
                    { value: 'all', label: '全部' },
                    { value: 1, label: '活动报名' },
                    { value: 2, label: '首次发证' },
                    { value: 3, label: '补证' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col xs={24} sm={12} md={8} lg={12}>
              <Form.Item name="keyword" label="关键词">
                <Input placeholder="订单ID/账号ID/业务ID/商户订单号" allowClear />
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
                    setPage(1)
                    loadData(1, pageSize)
                  }}
                >
                  重置
                </Button>
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
          scroll={{ x: 1400 }}
        />
      </Card>
    </Space>
  )
}

