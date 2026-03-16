import { Card, Empty, Space, Typography } from 'antd'
import { PlusOutlined } from '@ant-design/icons'

export function PlaceholderPage(props: { title: string }) {
  return (
    <Card style={{ borderRadius: 16 }} styles={{ body: { padding: 18 } }}>
      <Space direction="vertical" size={12} style={{ width: '100%' }}>
        <Typography.Title level={5} style={{ marginTop: 0 }}>
          {props.title}
        </Typography.Title>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            <Typography.Text type="secondary">
              页面结构已就绪，下一步补齐：列表筛选、表单编辑、导出与权限控制。
            </Typography.Text>
          }
        />
        <Typography.Paragraph style={{ marginBottom: 0 }}>
          <Space>
            <PlusOutlined />
            <Typography.Text type="secondary">将按 Ant Design Pro 的列表-详情-编辑模式落地</Typography.Text>
          </Space>
        </Typography.Paragraph>
      </Space>
    </Card>
  )
}

