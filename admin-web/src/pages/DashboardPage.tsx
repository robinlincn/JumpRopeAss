import {
  Card,
  Col,
  Flex,
  Progress,
  Row,
  Space,
  Statistic,
  Typography,
} from 'antd'
import {
  CheckCircleOutlined,
  DollarOutlined,
  FileTextOutlined,
  IdcardOutlined,
  TrophyOutlined,
} from '@ant-design/icons'

export function DashboardPage() {
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
              <Space style={{ marginTop: 14 }}>
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
                value={0}
                prefix={<TrophyOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">赛事/评定/培训统一入口</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="待审核认证"
                value={0}
                prefix={<IdcardOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">实名信息后台审核</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="今日支付订单"
                value={0}
                prefix={<DollarOutlined style={{ marginRight: 6 }} />}
              />
              <Typography.Text type="secondary">对账与导出一键完成</Typography.Text>
            </Card>
          </Col>
          <Col xs={24} sm={12}>
            <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
              <Statistic
                title="资讯发布"
                value={0}
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
                <Typography.Text type="secondary">0/0</Typography.Text>
              </Flex>
              <Progress percent={0} showInfo={false} />
            </div>
            <div>
              <Flex justify="space-between" style={{ marginBottom: 6 }}>
                <Typography.Text>认证审核</Typography.Text>
                <Typography.Text type="secondary">0/0</Typography.Text>
              </Flex>
              <Progress percent={0} showInfo={false} />
            </div>
            <div>
              <Flex justify="space-between" style={{ marginBottom: 6 }}>
                <Typography.Text>证书发放</Typography.Text>
                <Typography.Text type="secondary">0/0</Typography.Text>
              </Flex>
              <Progress percent={0} showInfo={false} />
            </div>
          </Space>
        </Card>
      </Col>

      <Col xs={24} lg={16}>
        <Card styles={{ body: { padding: 18 } }} style={{ borderRadius: 16 }}>
          <Typography.Title level={5} style={{ marginTop: 0 }}>
            提醒
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
            这里将展示：报名截止提醒、待缴费提醒、证书补发申请、支付异常等运营信息。
          </Typography.Paragraph>
        </Card>
      </Col>
    </Row>
  )
}

