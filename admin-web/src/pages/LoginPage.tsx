import { Button, Card, Flex, Form, Input, Typography, message } from 'antd'
import { useNavigate } from 'react-router-dom'
import { apiFetch } from '../lib/api'
import { setToken } from '../lib/auth'
import logoUrl from '../assets/logo.png'

type LoginResp = {
  token: string
}

export function LoginPage() {
  const nav = useNavigate()

  return (
    <div
      style={{
        minHeight: '100vh',
        background:
          'radial-gradient(1200px 600px at 18% 8%, rgba(31, 110, 201, 0.22), transparent 60%), radial-gradient(980px 560px at 92% 16%, rgba(21, 133, 77, 0.16), transparent 58%), radial-gradient(900px 520px at 60% 92%, rgba(229, 57, 53, 0.10), transparent 62%), #f6f7fb',
        padding: 16,
        display: 'grid',
        placeItems: 'center',
      }}
    >
      <Card style={{ width: 920, maxWidth: '100%', overflow: 'hidden' }} styles={{ body: { padding: 0 } }}>
        <Flex wrap="wrap" style={{ minHeight: 420 }}>
          <div
            style={{
              flex: '1 1 420px',
              padding: 28,
              background:
                'linear-gradient(135deg, rgba(31, 110, 201, 0.96), rgba(21, 133, 77, 0.90))',
              color: '#fff',
              display: 'grid',
              alignContent: 'start',
              gap: 12,
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <div
                style={{
                  width: 44,
                  height: 44,
                  borderRadius: 14,
                  background: 'rgba(255,255,255,0.78)',
                  border: '1px solid rgba(255,255,255,0.22)',
                  display: 'grid',
                  placeItems: 'center',
                  boxShadow: '0 18px 42px rgba(2, 6, 23, 0.22)',
                }}
              >
                <img
                  src={logoUrl}
                  alt=""
                  width={34}
                  height={34}
                  style={{ filter: 'drop-shadow(0 12px 18px rgba(2, 6, 23, 0.18))' }}
                />
              </div>
              <div>
                <div style={{ fontWeight: 800, fontSize: 16 }}>湖南省学生跳绳协会</div>
                <div style={{ opacity: 0.85, fontSize: 12 }}>后台管理系统</div>
              </div>
            </div>
            <div style={{ fontSize: 26, fontWeight: 900, lineHeight: 1.2 }}>
              一站式赛事与证书管理
            </div>
            <div style={{ opacity: 0.9, lineHeight: 1.6 }}>
              资讯发布、报名审核、缴费对账、证书发放与补证、基础数据留痕追溯。
            </div>
            <img
              src="/admin-hero.svg"
              alt=""
              style={{ width: '100%', maxWidth: 420, marginTop: 8, opacity: 0.95 }}
            />
          </div>

          <div style={{ flex: '1 1 360px', padding: 28, display: 'grid', alignContent: 'center' }}>
        <Typography.Title level={4} style={{ marginTop: 0 }}>
          后台登录
        </Typography.Title>
        <Typography.Paragraph type="secondary" style={{ marginTop: 4 }}>
          开发环境账号：admin / 123456
        </Typography.Paragraph>
        <Form
          layout="vertical"
          onFinish={async (values) => {
            const resp = await apiFetch<LoginResp>('/api/v1/admin/auth/login', {
              method: 'POST',
              body: JSON.stringify(values),
            })
            if (resp.code !== 0 || !resp.data?.token) {
              message.error(resp.message || '登录失败')
              return
            }
            setToken(resp.data.token)
            nav('/', { replace: true })
          }}
        >
          <Form.Item
            name="username"
            label="账号"
            rules={[{ required: true, message: '请输入账号' }]}
          >
            <Input placeholder="请输入账号" autoComplete="username" />
          </Form.Item>
          <Form.Item
            name="password"
            label="密码"
            rules={[{ required: true, message: '请输入密码' }]}
          >
            <Input.Password placeholder="请输入密码" autoComplete="current-password" />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            登录
          </Button>
        </Form>
        </div>
        </Flex>
      </Card>
    </div>
  )
}

