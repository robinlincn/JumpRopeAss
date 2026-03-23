import { createRouter, createWebHistory } from 'vue-router'
import HomePage from '../pages/HomePage.vue'
import EventsPage from '../pages/EventsPage.vue'
import EventDetailPage from '../pages/EventDetailPage.vue'
import NewsListPage from '../pages/NewsListPage.vue'
import NewsDetailPage from '../pages/NewsDetailPage.vue'
import MePage from '../pages/MePage.vue'
import IdentitySubmitPage from '../pages/IdentitySubmitPage.vue'
import MembersPage from '../pages/MembersPage.vue'
import LocalsPage from '../pages/LocalsPage.vue'
import AboutPage from '../pages/AboutPage.vue'
import AssessmentsPage from '../pages/AssessmentsPage.vue'
import TrainingsPage from '../pages/TrainingsPage.vue'
import MeProfilePage from '../pages/MeProfilePage.vue'
import MeOrgPage from '../pages/MeOrgPage.vue'
import MeActivityPage from '../pages/MeActivityPage.vue'
import MeHomePage from '../pages/MeHomePage.vue'
import CertVerifyPage from '../pages/CertVerifyPage.vue'
import PersonnelPage from '../pages/PersonnelPage.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: HomePage },
    { path: '/judges', component: PersonnelPage, meta: { type: 'judges', title: '裁判员' } },
    { path: '/athletes', component: PersonnelPage, meta: { type: 'athletes', title: '运动员' } },
    { path: '/coaches', component: PersonnelPage, meta: { type: 'coaches', title: '教练员' } },
    { path: '/org/members', component: MembersPage },
    { path: '/org/locals', component: LocalsPage },
    { path: '/about', component: AboutPage },
    { path: '/events', component: EventsPage },
    { path: '/events/:id', component: EventDetailPage },
    { path: '/news', component: NewsListPage },
    { path: '/news/:id', component: NewsDetailPage },
    { path: '/assessments', component: AssessmentsPage },
    { path: '/trainings', component: TrainingsPage },
    { path: '/me', component: MePage },
    { path: '/me/identity', component: IdentitySubmitPage },
    { path: '/me/profile', component: MeProfilePage },
    { path: '/me/org', component: MeOrgPage },
    { path: '/me/activity', component: MeActivityPage },
    { path: '/me/home', component: MeHomePage },
    { path: '/cert/verify', component: CertVerifyPage },
  ],
})

