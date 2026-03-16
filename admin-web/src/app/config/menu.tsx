import {
  ApartmentOutlined,
  FileTextOutlined,
  IdcardOutlined,
  PayCircleOutlined,
  ProfileOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  TeamOutlined,
  TrophyOutlined,
} from '@ant-design/icons'
import type { ReactNode } from 'react'

export type MenuNode = {
  key: string
  path?: string
  name: string
  icon?: ReactNode
  children?: MenuNode[]
}

export const menuTree: MenuNode[] = [
  { key: 'dashboard', path: '/', name: '工作台', icon: <ProfileOutlined /> },
  {
    key: 'content',
    name: '内容管理',
    icon: <FileTextOutlined />,
    children: [
      { key: 'content_news', path: '/content/news', name: '资讯管理' },
      { key: 'content_banners', path: '/content/banners', name: 'Banner管理' },
      { key: 'content_about', path: '/content/about', name: '关于协会' },
      { key: 'content_members', path: '/content/members', name: '会员单位' },
      { key: 'content_locals', path: '/content/locals', name: '地方协会' },
    ],
  },
  {
    key: 'data',
    name: '基础数据',
    icon: <ApartmentOutlined />,
    children: [
      { key: 'data_orgs', path: '/data/orgs', name: '机构管理' },
      { key: 'data_persons', path: '/data/persons', name: '人员管理' },
    ],
  },
  {
    key: 'events',
    name: '活动赛事',
    icon: <TrophyOutlined />,
    children: [{ key: 'events_list', path: '/events/list', name: '活动列表' }],
  },
  {
    key: 'entries',
    name: '报名审核',
    icon: <ProfileOutlined />,
    children: [{ key: 'entries_list', path: '/entries/list', name: '报名列表' }],
  },
  {
    key: 'pay',
    name: '财务订单',
    icon: <PayCircleOutlined />,
    children: [{ key: 'pay_orders', path: '/pay/orders', name: '订单管理' }],
  },
  {
    key: 'cert',
    name: '证书管理',
    icon: <SafetyCertificateOutlined />,
    children: [{ key: 'cert_list', path: '/cert/list', name: '证书列表' }],
  },
  {
    key: 'identity',
    name: '认证审核',
    icon: <IdcardOutlined />,
    children: [{ key: 'identity_review', path: '/identity/review', name: '认证列表' }],
  },
  {
    key: 'security',
    name: '系统设置',
    icon: <SettingOutlined />,
    children: [{ key: 'users', path: '/users', name: '账号与角色', icon: <TeamOutlined /> }],
  },
]

export function flattenMenu(nodes: MenuNode[]): Array<MenuNode & { path: string }> {
  const out: Array<MenuNode & { path: string }> = []
  const walk = (ns: MenuNode[]) => {
    for (const n of ns) {
      if (n.path) out.push(n as MenuNode & { path: string })
      if (n.children?.length) walk(n.children)
    }
  }
  walk(nodes)
  return out
}

