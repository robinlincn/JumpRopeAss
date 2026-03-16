import {
  Avatar,
  Breadcrumb,
  Button,
  Drawer,
  Dropdown,
  Grid,
  Layout,
  Menu,
  Space,
  Typography,
  theme,
} from 'antd'
import {
  BellOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  QuestionCircleOutlined,
  SearchOutlined,
  SettingOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { clearToken } from '../../lib/auth'
import { flattenMenu, menuTree, type MenuNode } from '../config/menu'
import { useMemo, useState } from 'react'

const { Header, Sider, Content } = Layout

function toMenuItems(nodes: MenuNode[]): any[] {
  return nodes.map((n) => ({
    key: n.path ?? n.key,
    icon: n.icon,
    label: n.name,
    children: n.children ? toMenuItems(n.children) : undefined,
  }))
}

function findBreadcrumb(nodes: MenuNode[], path: string): MenuNode[] {
  for (const n of nodes) {
    if (n.path === path) return [n]
    if (n.children?.length) {
      const child = findBreadcrumb(n.children, path)
      if (child.length) return [n, ...child]
    }
  }
  return []
}

export function AdminLayout() {
  const nav = useNavigate()
  const location = useLocation()
  const screens = Grid.useBreakpoint()
  const isMobile = !screens.md
  const [collapsed, setCollapsed] = useState(false)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const { token } = theme.useToken()

  const flat = useMemo(() => flattenMenu(menuTree), [])
  const selectedPath = useMemo(() => {
    const pathname = location.pathname
    const hit = flat
      .filter((x) => pathname === x.path || pathname.startsWith(x.path + '/'))
      .sort((a, b) => b.path.length - a.path.length)[0]
    return hit?.path ?? '/'
  }, [flat, location.pathname])

  const breadcrumbNodes = useMemo(() => findBreadcrumb(menuTree, selectedPath), [selectedPath])
  const title = breadcrumbNodes[breadcrumbNodes.length - 1]?.name ?? '工作台'
  const menuItems = useMemo(() => toMenuItems(menuTree), [])

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {!isMobile && (
        <Sider
          width={248}
          collapsible
          collapsed={collapsed}
          onCollapse={setCollapsed}
          breakpoint="lg"
          collapsedWidth={72}
          style={{
            borderRight: `1px solid ${token.colorBorderSecondary}`,
          }}
        >
          <div
            style={{
              height: 64,
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              padding: collapsed ? '0 14px' : '0 18px',
            }}
          >
            <div
              style={{
                width: 36,
                height: 36,
                borderRadius: 12,
                background: 'rgba(255,255,255,0.78)',
                border: `1px solid ${token.colorBorderSecondary}`,
                display: 'grid',
                placeItems: 'center',
                boxShadow: '0 16px 36px rgba(2, 6, 23, 0.12)',
              }}
            >
              <img
                src="/logo.svg"
                alt=""
                width={30}
                height={30}
                style={{ filter: 'drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18))' }}
              />
            </div>
            {!collapsed && (
              <div style={{ lineHeight: 1.2 }}>
                <div style={{ fontWeight: 800, color: token.colorText }}>学生跳绳协会</div>
                <div style={{ fontSize: 12, color: token.colorTextSecondary }}>后台管理</div>
              </div>
            )}
          </div>
          <Menu
            mode="inline"
            items={menuItems}
            selectedKeys={[selectedPath]}
            onClick={(e) => {
              if (typeof e.key === 'string' && e.key.startsWith('/')) nav(e.key)
            }}
            style={{ borderInlineEnd: 0 }}
          />
        </Sider>
      )}

      <Layout>
        <Header
          style={{
            padding: '0 16px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
          }}
        >
          <Space size={12} style={{ minWidth: 0 }}>
            {isMobile && (
              <Button
                type="text"
                icon={<MenuUnfoldOutlined />}
                onClick={() => setDrawerOpen(true)}
              />
            )}
            {!isMobile && (
              <Button
                type="text"
                icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                onClick={() => setCollapsed((v) => !v)}
              />
            )}
            <Breadcrumb
              items={[
                { title: '控制台' },
                ...breadcrumbNodes
                  .filter((x) => x.path)
                  .map((x) => ({
                    title: x.name,
                    onClick: () => x.path && nav(x.path),
                  })),
              ]}
            />
          </Space>

          <Space size={8}>
            <Button type="text" icon={<SearchOutlined />} />
            <Button type="text" icon={<QuestionCircleOutlined />} />
            <Button type="text" icon={<BellOutlined />} />
            <Dropdown
              menu={{
                items: [
                  { key: 'profile', label: '个人信息', icon: <UserOutlined /> },
                  { key: 'settings', label: '系统设置', icon: <SettingOutlined /> },
                  { type: 'divider' as const },
                  { key: 'logout', label: '退出登录', icon: <LogoutOutlined /> },
                ],
                onClick: (e) => {
                  if (e.key === 'logout') {
                    clearToken()
                    nav('/login', { replace: true })
                  }
                },
              }}
              placement="bottomRight"
            >
              <Space style={{ cursor: 'pointer' }}>
                <Avatar size="small" icon={<UserOutlined />} />
                <Typography.Text strong style={{ maxWidth: 120 }} ellipsis>
                  管理员
                </Typography.Text>
              </Space>
            </Dropdown>
          </Space>
        </Header>

        <Content style={{ padding: 16 }}>
          <div
            style={{
              maxWidth: 1400,
              margin: '0 auto',
              minHeight: 420,
            }}
          >
            <Typography.Title level={4} style={{ marginTop: 4, marginBottom: 12 }}>
              {title}
            </Typography.Title>
            <Outlet />
          </div>
        </Content>
      </Layout>

      <Drawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        placement="left"
        width={280}
        styles={{ body: { padding: 0 } }}
        title={
          <Space size={10}>
            <div
              style={{
                width: 32,
                height: 32,
                borderRadius: 12,
                background: 'rgba(255,255,255,0.78)',
                border: `1px solid ${token.colorBorderSecondary}`,
                display: 'grid',
                placeItems: 'center',
              }}
            >
              <img
                src="/logo.svg"
                alt=""
                width={26}
                height={26}
                style={{ filter: 'drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18))' }}
              />
            </div>
            <div style={{ lineHeight: 1.2 }}>
              <div style={{ fontWeight: 800 }}>学生跳绳协会</div>
              <div style={{ fontSize: 12, color: token.colorTextSecondary }}>后台管理</div>
            </div>
          </Space>
        }
      >
        <Menu
          mode="inline"
          items={menuItems}
          selectedKeys={[selectedPath]}
          onClick={(e) => {
            if (typeof e.key === 'string' && e.key.startsWith('/')) {
              nav(e.key)
              setDrawerOpen(false)
            }
          }}
          style={{ borderInlineEnd: 0 }}
        />
      </Drawer>
    </Layout>
  )
}

