import { useSyncExternalStore } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminLayout } from '../ui/AdminLayout'
import { getToken, subscribeTokenChange } from '../../lib/auth'
import { DashboardPage } from '../../pages/DashboardPage'
import { LoginPage } from '../../pages/LoginPage'
import { NewsManagePage } from '../../pages/news/NewsManagePage'
import { BannerManagePage } from '../../pages/banner/BannerManagePage'
import { LocalAssociationManagePage } from '../../pages/locals/LocalAssociationManagePage'
import { MemberUnitManagePage } from '../../pages/members/MemberUnitManagePage'
import { SchoolManagePage } from '../../pages/schools/SchoolManageRealPage'
import { JudgeManagePage } from '../../pages/judges/JudgeManageRealPage'
import { CoachManagePage } from '../../pages/coaches/CoachManageRealPage'
import { AthleteManagePage } from '../../pages/athletes/AthleteManageRealPage'
import { ProjectCatalogPage } from '../../pages/projects/ProjectCatalogPage'
import { EventsManagePage } from '../../pages/events/EventsManagePage'
import { EntriesReviewPage } from '../../pages/entries/EntriesReviewPage'
import { IdentityReviewPage } from '../../pages/identity/IdentityReviewPage'
import { AboutManagePage } from '../../pages/about/AboutManagePage'
import { PayOrdersManagePage } from '../../pages/pay/PayOrdersManagePage'
import { CertificatesListPage } from '../../pages/cert/CertificatesListPage'
import { CertIssueReviewPage } from '../../pages/cert/CertIssueReviewPage'
import { UsersRolesPage } from '../../pages/users/UsersRolesPage'
import { SystemSettingsPage } from '../../pages/settings/SystemSettingsPage'

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
          element={<BannerManagePage />}
        />
        <Route path="content/about" element={<AboutManagePage />} />
        <Route path="data/members" element={<MemberUnitManagePage />} />
        <Route path="data/schools" element={<SchoolManagePage />} />
        <Route path="data/judges" element={<JudgeManagePage />} />
        <Route path="data/coaches" element={<CoachManagePage />} />
        <Route path="data/athletes" element={<AthleteManagePage />} />
        <Route path="data/projects" element={<ProjectCatalogPage />} />
        <Route
          path="content/locals"
          element={<LocalAssociationManagePage />}
        />
        <Route path="events/list" element={<EventsManagePage />} />
        <Route
          path="entries/list"
          element={<EntriesReviewPage />}
        />
        <Route path="pay/orders" element={<PayOrdersManagePage />} />
        <Route path="cert/list" element={<CertificatesListPage />} />
        <Route path="cert/review" element={<CertIssueReviewPage />} />
        <Route
          path="identity/review"
          element={<IdentityReviewPage />}
        />
        <Route path="users" element={<UsersRolesPage />} />
        <Route path="settings" element={<SystemSettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

