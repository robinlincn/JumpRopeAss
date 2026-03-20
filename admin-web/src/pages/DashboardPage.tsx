import {
  Badge,
  Card,
  Col,
  Flex,
  Progress,
  Row,
  Space,
  Statistic,
  Table,
  Tag,
  Typography,
  message,
} from 'antd'
import {
  CheckCircleOutlined,
  DollarOutlined,
  FileTextOutlined,
  IdcardOutlined,
  ReloadOutlined,
  TrophyOutlined,
} from '@ant-design/icons'
import { useEffect, useMemo, useState } from 'react'
import { apiFetch } from '../lib/api'

export function DashboardPage() {
  const [loading, setLoading] = useState(false)
  const [data, setData] = useState<any>(null)

  const loadData = async () => {
    setLoading(true)
    try {
      const res = await apiFetch<any>('/api/v1/admin/dashboard')
      if (res.code !== 0) {
        message.error(res.message || '加载失败')
        return
      }
      setData(res.data)
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

  const kpis = data?.kpis ?? {}
  const progress = data?.progress ?? {}
  const reminders: any[] = data?.reminders ?? []
  const latestEntries: any[] = data?.latest?.entries ?? []
  const latestOrders: any[] = data?.latest?.orders ?? []

  const progressPercent = (done?: number, total?: number) => {
    const t = Number(total ?? 0)
    const d = Number(done ?? 0)
    if (t <= 0) return 0
    return Math.max(0, Math.min(100, Math.round((d / t) * 100)))
  }

  const reminderBadge = (t: string) => {
    if (t === 'event_signup_end') return <Badge color="#faad14" text="报名将截止" />
    if (t === 'entry_wait_pay') return <Badge color="#1677ff" text="待缴费" />
    return <Badge color="#ff4d4f" text="订单关闭" />
  }

  const latestEntryStatusTag = (s: number) => {
    const map: Record<number, { text: string; color?: string }> = {
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
    const v = map[s] ?? { text: String(s) }
    return <Tag color={v.color}>{v.text}</Tag>
  }

  const latestOrderStatusTag = (s: number) => {
    const map: Record<number, { text: string; color?: string }> = {
      0: { text: '待支付' },
      1: { text: '已支付', color: 'green' },
      2: { text: '已关闭', color: 'red' },
      3: { text: '已退款', color: 'orange' },
    }
    const v = map[s] ?? { text: String(s) }
    return <Tag color={v.color}>{v.text}</Tag>
  }

  const entryCols = useMemo(
    () => [
      { title: '报名ID', dataIndex: 'id', width: 110 },
      { title: '活动', dataIndex: 'eventTitle', width: 220, render: (v: string) => v ?? '-' },
      { title: '运动员', dataIndex: 'athleteName', width: 140, render: (v: string) => v ?? '-' },
      { title: '状态', dataIndex: 'status', width: 110, render: (v: number) => latestEntryStatusTag(v) },
    ],
    [],
  )

  const orderCols = useMemo(
    () => [
      { title: '订单ID', dataIndex: 'id', width: 110 },
      { title: '金额(元)', dataIndex: 'amount', width: 110, render: (v: number) => (Number(v) / 100).toFixed(2) },
      { title: '状态', dataIndex: 'status', width: 110, render: (v: number) => latestOrderStatusTag(v) },
      { title: '创建时间', dataIndex: 'createdAt', width: 170, render: (v: string) => String(v ?? '').replace('T', ' ') },
    ],
    [],
  )

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={12}>
        <Card
          styles={{ body: { padding: 18 } }}
          style={{
            borderRadius: 16,
            background:
              'linear-gradient(135deg, rgba(31, 110, 201, 0.10), rgba(21, 133, 77, 0.08), rgba(245, 158, 11, 0.06))',
          }}
        >
          <Flex align="center" justify="space-between" gap={16} wrap="wrap">
            <div>
              <Typography.Title level={4} style={{ marginTop: 0, marginBottom: 8 }}>
                欢迎使用后台管理
              </Typography.Title>
              <Typography.Paragraph type="secondary" style={{ margin: 0 }}>
                以“审核与留痕”为中心，统一管理资讯、赛事、缴费与证书。
              </Typography.Paragraph>
              <Space style={{ marginTop: 14 }} wrap>
                <Space>
                  <TrophyOutlined />
                  <Typography.Text>活动管理</Typography.Text>
                </Space>
                <Space>
                  <FileTextOutlined />
                  <Typography.Text>内容发布</Typography.Text>
                </Space>
                <Space>
                  <CheckCircleOutlined />
                  <Typography.Text>证书发放</Typography.Text>
                </Space>
                <Tag
                  color="processing"
                  style={{ cursor: 'pointer', userSelect: 'none' }}
                  onClick={() => {
                    if (loading) return
                    loadData()
                  }}
                >
                  <ReloadOutlined style={{ marginRight: 6 }} />
                  刷新
                </Tag>
              </Space>
            </div>
            <img
              src="/admin-hero.svg"
              alt=""
              style={{ width: 260, maxWidth: '100%', opacity: 0.95 }}
            />
          </Flex>
        </Card>
      </Col>

      <Col xs={24} lg={12}>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="待审核报名"
                value={Number(kpis.pendingEntries ?? 0)}
                prefix={<TrophyOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">赛事/评定/培训统一入口</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="待审核认证"
                value={Number(kpis.pendingIdentities ?? 0)}
                prefix={<IdcardOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">实名信息后台审核</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="今日支付订单"
                value={Number(kpis.todayPaidOrders ?? 0)}
                prefix={<DollarOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">对账与导出一键完成</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="资讯发布"
                value={Number(kpis.newsTotal ?? 0)}
                prefix={<FileTextOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">图文/视频两种形态</Typography.Text>
            </Card>
          </Col>
        </Row>
      </Col>

      <Col xs={24} lg={8}>
        <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
          <Typography.Title level={5} style={{ marginTop: 0 }}>
            今日进度
          </Typography.Title>
          <Space direction="vertical" style={{ width: '100%' }} size={12}>
            <div>
              <Flex justify="space-between" style={{ marginBottom: 6 }}>
                <Typography.Text>报名审核</Typography.Text>
                <Typography.Text type="secondary">
                  {Number(progress.entry?.done ?? 0)}/{Number(progress.entry?.total ?? 0)}
                </Typography.Text>
              </Flex>
              <Progress percent={progressPercent(progress.entry?.done, progress.entry?.total)} showInfo={false} />
            </div>
            <div>
              <Flex justify="space-between" style={{ marginBottom: 6 }}>
                <Typography.Text>认证审核</Typography.Text>
                <Typography.Text type="secondary">
                  {Number(progress.identity?.done ?? 0)}/{Number(progress.identity?.total ?? 0)}
                </Typography.Text>
              </Flex>
              <Progress percent={progressPercent(progress.identity?.done, progress.identity?.total)} showInfo={false} />
            </div>
            <div>
              <Flex justify="space-between" style={{ marginBottom: 6 }}>
                <Typography.Text>证书发放</Typography.Text>
                <Typography.Text type="secondary">
                  {Number(progress.cert?.done ?? 0)}/{Number(progress.cert?.total ?? 0)}
                </Typography.Text>
              </Flex>
              <Progress percent={progressPercent(progress.cert?.done, progress.cert?.total)} showInfo={false} />
            </div>
          </Space>
        </Card>
      </Col>

      <Col xs={24} lg={16}>
        <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
          <Typography.Title level={5} style={{ marginTop: 0 }}>
            提醒
          </Typography.Title>
          {reminders.length ? (
            <Space direction="vertical" style={{ width: '100%' }} size={10}>
              {reminders.map((r) => (
                <Flex key={`${r.type}-${r.refId}`} justify="space-between" align="center" gap={12} wrap="wrap">
                  <Space size={10} wrap>
                    {reminderBadge(String(r.type))}
                    <Typography.Text>{r.title}</Typography.Text>
                    <Typography.Text type="secondary">#{r.refId}</Typography.Text>
                  </Space>
                  <Typography.Text type="secondary">{String(r.at ?? '').replace('T', ' ')}</Typography.Text>
                </Flex>
              ))}
            </Space>
          ) : (
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
              暂无提醒：报名截止、待缴费、订单关闭等信息会在此展示。
            </Typography.Paragraph>
          )}
        </Card>
      </Col>

      <Col xs={24} lg={12}>
        <Card styles={{ body: { padding: 12 } }} style={{ borderRadius: 16 }}>
          <Typography.Title level={5} style={{ margin: 6 }}>
            最新报名
          </Typography.Title>
          <Table
            rowKey="id"
            columns={entryCols as any}
            dataSource={latestEntries.map((x) => ({
              ...x,
              createdAt: String(x.createdAt ?? '').replace('T', ' '),
            }))}
            size="small"
            pagination={false}
            loading={loading}
            scroll={{ x: 620 }}
          />
        </Card>
      </Col>

      <Col xs={24} lg={12}>
        <Card styles={{ body: { padding: 12 } }} style={{ borderRadius: 16 }}>
          <Typography.Title level={5} style={{ margin: 6 }}>
            最新订单
          </Typography.Title>
          <Table
            rowKey="id"
            columns={orderCols as any}
            dataSource={latestOrders.map((x) => ({
              ...x,
              createdAt: String(x.createdAt ?? '').replace('T', ' '),
            }))}
            size="small"
            pagination={false}
            loading={loading}
            scroll={{ x: 620 }}
          />
        </Card>
      </Col>
    </Row>
  )
}

