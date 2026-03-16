import { useSyncExternalStore } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminLayout } from '../ui/AdminLayout'
import { getToken, subscribeTokenChange } from '../../lib/auth'
import { DashboardPage } from '../../pages/DashboardPage'
import { LoginPage } from '../../pages/LoginPage'
import { PlaceholderPage } from '../../pages/PlaceholderPage'
import { NewsManagePage } from '../../pages/news/NewsManagePage'
import { EntriesReviewPage } from '../../pages/entries/EntriesReviewPage'
import { IdentityReviewPage } from '../../pages/identity/IdentityReviewPage'

export function AppRouter() {
  const token = useSyncExternalStore(subscribeTokenChange, getToken, getToken)
  const authed = !!token

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={authed ? <AdminLayout /> : <Navigate to="/login" replace />}
      >
        <Route index element={<DashboardPage />} />
        <Route path="content/news" element={<NewsManagePage />} />
        <Route
          path="content/banners"
          element={<PlaceholderPage title="Banner管理" />}
        />
        <Route path="content/about" element={<PlaceholderPage title="关于协会" />} />
        <Route
          path="content/members"
          element={<PlaceholderPage title="会员单位" />}
        />
        <Route
          path="content/locals"
          element={<PlaceholderPage title="地方协会" />}
        />
        <Route path="data/orgs" element={<PlaceholderPage title="机构管理" />} />
        <Route
          path="data/persons"
          element={<PlaceholderPage title="人员管理" />}
        />
        <Route path="events/list" element={<PlaceholderPage title="活动管理" />} />
        <Route
          path="entries/list"
          element={<EntriesReviewPage />}
        />
        <Route path="pay/orders" element={<PlaceholderPage title="订单管理" />} />
        <Route path="cert/list" element={<PlaceholderPage title="证书管理" />} />
        <Route
          path="identity/review"
          element={<IdentityReviewPage />}
        />
        <Route path="users" element={<PlaceholderPage title="账号与角色" />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

