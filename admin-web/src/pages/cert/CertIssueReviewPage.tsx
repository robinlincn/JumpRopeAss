import { Button, Card, Col, Form, Grid, Input, Row, Select, Space, Table, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { ReloadOutlined } from '@ant-design/icons'
import { useEffect, useMemo, useState } from 'react'
import { apiFetch } from '../../lib/api'

type CertType = { id: number; name: string }

type IssueRow = {
  entryId: number
  eventId: number
  eventTitle: string
  groupId: number
  groupName: string | null
  athletePersonId: number
  athleteName: string | null
  athleteMobile: string | null
  enrollUserId: number
  createdAt: string
  certNo: string
}

export function CertIssueReviewPage() {
  const screens = Grid.useBreakpoint()
  const [form] = Form.useForm()
  const [loading, setLoading] = useState(false)
  const [typesLoading, setTypesLoading] = useState(false)
  const [items, setItems] = useState<IssueRow[]>([])
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [certTypes, setCertTypes] = useState<CertType[]>([])
  const [certTypeId, setCertTypeId] = useState<number | undefined>(undefined)
  const [issuing, setIssuing] = useState<Record<number, boolean>>({})

  const loadCertTypes = async () => {
    setTypesLoading(true)
    try {
      const res = await apiFetch<any>('/api/v1/admin/cert-types')
      if (res.code !== 0) {
        message.error(res.message || '加载证书类型失败')
        return
      }
      const list: CertType[] = (res.data?.items ?? []).map((x: any) => ({ id: Number(x.id), name: String(x.name) }))
      setCertTypes(list)
      if (!certTypeId && list.length) setCertTypeId(list[0].id)
    } finally {
      setTypesLoading(false)
    }
  }

  const load = async (p = page, ps = pageSize) => {
    const { keyword, eventId } = form.getFieldsValue()
    setLoading(true)
    try {
      const qs = new URLSearchParams()
      qs.set('page', String(p))
      qs.set('pageSize', String(ps))
      if (keyword) qs.set('keyword', String(keyword).trim())
      if (eventId) qs.set('eventId', String(eventId).trim())
      const res = await apiFetch<any>(`/api/v1/admin/cert-issues?${qs.toString()}`)
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setItems(res.data.items ?? [])
      setTotal(Number(res.data.total ?? 0))
      setPage(Number(res.data.page ?? p))
      setPageSize(Number(res.data.pageSize ?? ps))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCertTypes()
    load(1, pageSize)
  }, [])

  const columns = useMemo<ColumnsType<IssueRow>>(
    () => [
      { title: '报名ID', dataIndex: 'entryId', width: 110, fixed: screens.lg ? 'left' : undefined },
      { title: '活动', dataIndex: 'eventTitle', width: 260 },
      { title: '组别', dataIndex: 'groupName', width: 160, render: (v) => v || '-' },
      { title: '选手', dataIndex: 'athleteName', width: 140, render: (v) => v || '-' },
      { title: '手机号', dataIndex: 'athleteMobile', width: 140, render: (v) => v || '-' },
      { title: '证书号', dataIndex: 'certNo', width: 200, render: (v) => <Typography.Text copyable>{v}</Typography.Text> },
      { title: '确认时间', dataIndex: 'createdAt', width: 180, render: (v) => String(v).replace('T', ' ') },
      {
        title: '操作',
        dataIndex: 'op',
        width: 160,
        fixed: screens.lg ? 'right' : undefined,
        render: (_, row) => (
          <Button
            type="primary"
            loading={!!issuing[row.entryId]}
            onClick={async () => {
              const ct = certTypeId
              if (!ct) {
                message.warning('请先选择证书类型')
                return
              }
              setIssuing((s) => ({ ...s, [row.entryId]: true }))
              try {
                const res = await apiFetch<any>(`/api/v1/admin/cert-issues/${row.entryId}/approve`, {
                  method: 'POST',
                  body: JSON.stringify({ certTypeId: ct }),
                })
                if (res.code !== 0) {
                  message.error(res.message || '发证失败')
                  return
                }
                message.success(res.data?.created ? '已生成证书' : '证书已存在')
                load(1, pageSize)
              } finally {
                setIssuing((s) => ({ ...s, [row.entryId]: false }))
              }
            }}
          >
            生成证书
          </Button>
        ),
      },
    ],
    [screens.lg, certTypeId, issuing, pageSize],
  )

  return (
    <Space direction="vertical" size={16} style={{ width: '100%' }}>
      <Card styles={{ body: { padding: 16 } }}>
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Row gutter={[12, 12]}>
            <Col flex="auto">
              <Typography.Title level={5} style={{ margin: 0 }}>
                发证审核
              </Typography.Title>
              <Typography.Text type="secondary">仅展示：活动已结束 + 报名已确认 + 尚未生成证书</Typography.Text>
            </Col>
            <Col>
              <Space>
                <Select
                  style={{ width: 220 }}
                  loading={typesLoading}
                  placeholder="选择证书类型"
                  value={certTypeId}
                  onChange={(v) => setCertTypeId(v)}
                  options={certTypes.map((x) => ({ label: x.name, value: x.id }))}
                />
                <Button icon={<ReloadOutlined />} onClick={() => load(1, pageSize)} loading={loading}>
                  刷新
                </Button>
              </Space>
            </Col>
          </Row>

          <Form form={form} layout={screens.md ? 'inline' : 'vertical'} onFinish={() => load(1, pageSize)}>
            <Form.Item name="keyword" label="关键字">
              <Input placeholder="姓名/ID" allowClear style={{ width: screens.md ? 240 : '100%' }} />
            </Form.Item>
            <Form.Item name="eventId" label="活动ID">
              <Input placeholder="可选" allowClear style={{ width: screens.md ? 180 : '100%' }} />
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit">
                查询
              </Button>
            </Form.Item>
          </Form>
        </Space>
      </Card>

      <Card styles={{ body: { padding: 0 } }}>
        <Table<IssueRow>
          rowKey="entryId"
          columns={columns}
          dataSource={items}
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            onChange: (p, ps) => load(p, ps),
          }}
          scroll={{ x: 1350 }}
        />
      </Card>
    </Space>
  )
}
