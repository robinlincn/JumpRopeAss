import { createRouter, createWebHistory } from 'vue-router'
import HomePage from '../pages/HomePage.vue'
import EventsPage from '../pages/EventsPage.vue'
import EventDetailPage from '../pages/EventDetailPage.vue'
import NewsListPage from '../pages/NewsListPage.vue'
import NewsDetailPage from '../pages/NewsDetailPage.vue'
import MePage from '../pages/MePage.vue'
import IdentitySubmitPage from '../pages/IdentitySubmitPage.vue'
import PlaceholderPage from '../pages/PlaceholderPage.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: HomePage },
    { path: '/org/members', component: PlaceholderPage, props: { title: '会员单位' } },
    { path: '/org/locals', component: PlaceholderPage, props: { title: '地方协会' } },
    { path: '/about', component: PlaceholderPage, props: { title: '关于协会' } },
    { path: '/events', component: EventsPage },
    { path: '/events/:id', component: EventDetailPage },
    { path: '/news', component: NewsListPage },
    { path: '/news/:id', component: NewsDetailPage },
    { path: '/assessments', component: PlaceholderPage, props: { title: '评定' } },
    { path: '/trainings', component: PlaceholderPage, props: { title: '培训' } },
    { path: '/me', component: MePage },
    { path: '/me/identity', component: IdentitySubmitPage },
    { path: '/me/profile', component: PlaceholderPage, props: { title: '信息录入' } },
    { path: '/me/org', component: PlaceholderPage, props: { title: '绑定机构' } },
    { path: '/me/activity', component: PlaceholderPage, props: { title: '我的活动' } },
    { path: '/me/home', component: PlaceholderPage, props: { title: '我的主页' } },
  ],
})

