<script setup lang="ts">
import { useRouter } from 'vue-router'
import { computed, onMounted, onBeforeUnmount, ref } from 'vue'
import AppTabBar from '../components/AppTabBar.vue'
import BannerCarousel from '../components/BannerCarousel.vue'
import logoUrl from '../assets/logo.png'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const router = useRouter()

const quick = [
  { label: '会员单位', path: '/org/members', iconType: 'img', icon: '/icon-user.svg' },
  { label: '地方协会', path: '/org/locals', iconType: 'sprite', icon: 'social-icon' },
  { label: '关于协会', path: '/about', iconType: 'sprite', icon: 'documentation-icon' },
  { label: '新闻资讯', path: '/news', iconType: 'img', icon: '/icon-news.svg' },
]

type BannerItem = {
  id: number
  title: string
  imageUrl?: string | null
  linkType: number
  linkValue?: string | null
}

const banners = ref<{ src: string; title: string; subtitle: string; linkType: number; linkValue?: string | null }[]>([])
const stats = ref<{ judges: number; athletes: number; coaches: number; events: number } | null>(null)
const statCards = computed(() => {
  const s = stats.value
  return [
    { label: '裁判员', value: s?.judges ?? 0, path: '/judges' },
    { label: '运动员', value: s?.athletes ?? 0, path: '/athletes' },
    { label: '教练员', value: s?.coaches ?? 0, path: '/coaches' },
  ]
})

type EventItem = {
  id: number
  title: string
  coverUrl?: string
  eventDate?: string
  location?: string
}

type NewsItem = {
  id: number
  title: string
  publishAt?: string | null
  viewCount: number
  coverUrl?: string | null
  contentType: string
}

const events = ref<EventItem[]>([])
const news = ref<NewsItem[]>([])
const loading = ref(false)
const finished = ref(false)
const sentinelEl = ref<HTMLDivElement | null>(null)
let observer: IntersectionObserver | null = null
let page = 1
let pageSize = 5

function resetList() {
  page = 1
  pageSize = 5
  events.value = []
  finished.value = false
}

async function fetchEvents() {
  if (loading.value || finished.value) return
  loading.value = true
  try {
    const url = `/api/v1/app/events?page=${page}&pageSize=${pageSize}&status=1`
    const data = await apiFetch<any>(url)
    const items: any[] = data.data?.items ?? []
    if (items.length === 0) {
      finished.value = true
    } else {
      events.value.push(
        ...items.map((x) => ({
          id: Number(x.id),
          title: x.title,
          coverUrl: x.coverUrl,
          eventDate: x.eventDate ?? x.signupStartAt,
          location: x.location,
        })),
      )
      page += 1
      pageSize = 10
    }
  } catch (e) {
    finished.value = true
  } finally {
    loading.value = false
  }
}

const isHomeBaseLoading = ref(true)

let homeBaseCache: any = null
let isFetchingHomeBase = false

async function fetchHomeBase() {
  if (homeBaseCache) {
    applyHomeBaseData(homeBaseCache)
    isHomeBaseLoading.value = false
    // 依然在后台静默更新
  }

  if (isFetchingHomeBase) return
  isFetchingHomeBase = true

  try {
    const res = await apiFetch<any>('/api/v1/app/home/aggregate')
    if (res.code === 0 && res.data) {
      homeBaseCache = res.data
      applyHomeBaseData(homeBaseCache)
    } else {
      console.error('Failed to fetch home base data:', res)
    }
  } catch (e) {
    console.error('Error fetching home base data:', e)
  } finally {
    isHomeBaseLoading.value = false
    isFetchingHomeBase = false
  }
}

function applyHomeBaseData(data: any) {
  if (data.banners && Array.isArray(data.banners)) {
    banners.value = data.banners.map((x: BannerItem) => ({
      src: x.imageUrl || '/banner-1.svg',
      title: x.title || '推荐',
      subtitle: x.linkType === 1 ? '赛事推荐' : x.linkType === 2 ? '资讯推荐' : '快捷入口',
      linkType: x.linkType,
      linkValue: x.linkValue,
    }))
  }
  if (data.stats) {
    stats.value = {
      judges: Number(data.stats.judges ?? 0),
      athletes: Number(data.stats.athletes ?? 0),
      coaches: Number(data.stats.coaches ?? 0),
      events: Number(data.stats.events ?? 0),
    }
  }
  if (data.news && Array.isArray(data.news)) {
    news.value = data.news.map((x: any) => ({
      id: Number(x.id),
      title: String(x.title),
      publishAt: x.publishAt ?? null,
      viewCount: Number(x.viewCount ?? 0),
      coverUrl: x.coverUrl ?? null,
      contentType: String(x.contentType ?? 'text'),
    }))
  }
}

function timeText(v?: string | null) {
  if (!v) return '-'
  return String(v).replace('T', ' ').slice(5, 16)
}

function viewText(n: number) {
  if (n >= 10000) return `${(n / 10000).toFixed(1)}万`
  return String(n)
}

useAutoRefresh(async () => {
  await fetchHomeBase()
  resetList()
  await fetchEvents()
}, 30000)

onMounted(() => {
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((e) => e.isIntersecting)) {
        fetchEvents()
      }
    },
    { rootMargin: '200px' },
  )
  if (sentinelEl.value) observer.observe(sentinelEl.value)
})
onBeforeUnmount(() => {
  if (observer) observer.disconnect()
})
</script>

<template>
  <main class="app-container page">
    <section class="hero glass">
      <div class="hero-banner" :class="{ 'skeleton-banner': isHomeBaseLoading && banners.length === 0 }">
        <BannerCarousel v-if="banners.length > 0" :items="banners" />
        <div v-else-if="!isHomeBaseLoading" class="empty-banner">暂无推荐</div>
      </div>
      <div class="hero-left">
        <div class="brand">
          <div class="brand-logo-wrap">
            <img class="brand-logo" :src="logoUrl" alt="" width="38" height="38" />
          </div>
          <div>
            <div class="brand-name">湖南省学生跳绳协会</div>
            <div class="brand-sub">朝气蓬勃 · 青春活力 · 专业赛事</div>
          </div>
        </div>

        <div class="hero-title">让每一次跃起，都更有力量</div>
        <div class="hero-desc">
          资讯、赛事、评定、培训与证书，一站式服务。支持运动员/家长/第一教练员报名与审核缴费闭环。
        </div>

        
      </div>
    </section>

    <section class="quick glass">
      <button
        v-for="q in quick"
        :key="q.path"
        class="quick-item"
        type="button"
        @click="router.push(q.path)"
      >
        <span class="quick-ico" aria-hidden="true">
          <img v-if="q.iconType === 'img'" :src="q.icon" alt="" width="22" height="22" />
          <svg v-else class="quick-sprite" width="22" height="22">
            <use :href="`/icons.svg#${q.icon}`"></use>
          </svg>
        </span>
        <span>{{ q.label }}</span>
      </button>
    </section>

    <section class="stats">
      <div v-for="s in statCards" :key="s.label" class="stat card pressable" @click="router.push(s.path)" style="cursor: pointer;">
        <div class="stat-value">{{ s.value }}</div>
        <div class="stat-label">{{ s.label }}</div>
      </div>
    </section>

    <section class="latest">
      <div class="section-head">
        <div class="h">
          <div class="t">最新资讯</div>
          <div class="s">来自后台资讯 · 实时同步</div>
        </div>
        <button class="btn-reset more" type="button" @click="router.push('/news')">更多</button>
      </div>
      <div class="news-mini" :class="{ 'skeleton-news-container': isHomeBaseLoading && news.length === 0 }">
        <template v-if="news.length > 0">
          <button
            v-for="it in news"
            :key="it.id"
            class="mini card btn-reset pressable"
            type="button"
            @click="router.push(`/news/${it.id}`)"
          >
            <div class="mini-left">
              <div class="mini-top">
                <span class="chip">{{ it.contentType === 'video' ? '视频' : '图文' }}</span>
                <span class="meta">{{ timeText(it.publishAt) }} · {{ viewText(it.viewCount) }}</span>
              </div>
              <div class="mini-title">{{ it.title }}</div>
            </div>
            <img class="mini-cover" :src="it.coverUrl || '/hero-jumprope.svg'" alt="" loading="lazy" />
          </button>
        </template>
        <template v-else-if="isHomeBaseLoading">
          <div v-for="i in 3" :key="i" class="mini card skeleton-news"></div>
        </template>
      </div>
    </section>

    <section class="latest">
      <div class="latest-grid">
        <button
          v-for="ev in events"
          :key="ev.id"
          class="news-card card btn-reset pressable"
          type="button"
          @click="router.push(`/events/${ev.id}`)"
        >
          <div class="news-top">
            <span class="chip">最新赛事</span>
            <span class="meta">{{ ev.eventDate || '时间待定' }}</span>
          </div>
          <div class="news-title">{{ ev.title }}</div>
          <div class="news-desc">{{ ev.location || '地点待定' }}</div>
          <div class="news-foot">
            <img src="/icon-trophy.svg" alt="" width="24" height="24" />
            <span>查看活动</span>
          </div>
        </button>
        <div ref="sentinelEl" class="sentinel"></div>
      </div>
      <div v-if="loading" class="loading">正在加载...</div>
      <div v-else-if="finished" class="loading">已无更多</div>
    </section>
  </main>

  <AppTabBar />
</template>

<style scoped>
.empty-banner {
  height: 180px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #666;
  font-size: 14px;
}

.skeleton-banner {
  height: 180px;
  border-radius: 16px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
}

.skeleton-news {
  height: 84px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
  border: none;
}

@keyframes skeleton-loading {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

.page {
  padding: 14px 12px 88px;
}

.hero {
  padding: 16px;
  overflow: hidden;
  display: grid;
  gap: 14px;
  grid-template-areas: 'banner' 'content';
}

.hero-banner {
  grid-area: banner;
}

.hero-left {
  grid-area: content;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
}

.brand-logo-wrap {
  width: 44px;
  height: 44px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.22);
  display: grid;
  place-items: center;
  box-shadow: 0 14px 32px rgba(2, 6, 23, 0.22);
}

.brand-logo {
  border-radius: 12px;
  filter: drop-shadow(0 10px 18px rgba(2, 6, 23, 0.25));
}

.brand-name {
  font-weight: 950;
  letter-spacing: -0.2px;
}

.brand-sub {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.9);
}

.hero-left {
  color: rgba(255, 255, 255, 0.96);
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.92),
    rgb(var(--brand-green-rgb) / 0.86)
  );
  border-radius: 18px;
  padding: 16px;
  border: 1px solid rgba(255, 255, 255, 0.18);
}

.hero-title {
  margin-top: 12px;
  font-size: 24px;
  font-weight: 950;
  letter-spacing: -0.4px;
  line-height: 1.2;
}

.hero-desc {
  margin-top: 10px;
  color: rgba(255, 255, 255, 0.88);
  line-height: 1.6;
  font-size: 13px;
}

.hero-cta {
  margin-top: 14px;
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.hero-chips {
  margin-top: 12px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.hero-chips .chip {
  background: rgba(255, 255, 255, 0.18);
  border-color: rgba(255, 255, 255, 0.26);
  color: rgba(255, 255, 255, 0.92);
}

.quick {
  margin-top: 14px;
  padding: 12px 8px;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 6px;
}

.quick-item {
  border: 0;
  background: transparent;
  display: grid;
  justify-items: center;
  gap: 8px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.82);
  padding: 10px 6px;
  border-radius: 16px;
  transition:
    transform 160ms var(--ease-out),
    background 180ms var(--ease-out),
    border-color 180ms var(--ease-out);
}
.quick-item:active {
  transform: translateY(1px) scale(0.99);
}
.quick-ico {
  width: 44px;
  height: 44px;
  border-radius: 16px;
  display: grid;
  place-items: center;
  border: 1px solid rgba(255, 255, 255, 0.26);
  background:
    radial-gradient(40px 40px at 20% 18%, rgb(var(--brand-cyan-rgb) / 0.22), transparent 70%),
    radial-gradient(48px 48px at 88% 30%, rgb(var(--brand-blue-rgb) / 0.20), transparent 72%),
    radial-gradient(44px 44px at 42% 92%, rgb(var(--brand-green-rgb) / 0.16), transparent 70%),
    rgba(255, 255, 255, 0.58);
  box-shadow: 0 16px 34px rgba(2, 6, 23, 0.10);
}
.quick-sprite {
  display: block;
}
.quick-item:hover .quick-ico {
  background:
    radial-gradient(46px 46px at 20% 18%, rgb(var(--brand-cyan-rgb) / 0.24), transparent 70%),
    radial-gradient(56px 56px at 88% 30%, rgb(var(--brand-blue-rgb) / 0.22), transparent 72%),
    radial-gradient(50px 50px at 42% 92%, rgb(var(--brand-green-rgb) / 0.18), transparent 70%),
    rgba(255, 255, 255, 0.70);
  box-shadow: 0 18px 42px rgba(2, 6, 23, 0.12);
}

.stats {
  margin-top: 14px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
.loading {
  margin: 10px 12px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.55);
}
.sentinel {
  height: 1px;
}

.stat {
  padding: 12px 10px;
  text-align: center;
}

.stat-value {
  font-weight: 950;
  font-size: 18px;
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb)), rgb(var(--brand-green-rgb)));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.stat-label {
  margin-top: 4px;
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
}

.latest {
  margin-top: 14px;
  padding-bottom: 12px;
}

.section-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}
.h {
  min-width: 0;
}
.t {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.s {
  margin-top: 2px;
  color: rgba(15, 23, 42, 0.56);
  font-size: 12px;
}
.more {
  font-weight: 900;
  color: rgba(15, 23, 42, 0.62);
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.55);
}
.news-mini {
  display: grid;
  gap: 10px;
  margin-bottom: 10px;
}
.mini {
  padding: 12px;
  text-align: left;
  width: 100%;
  display: grid;
  grid-template-columns: 1fr 86px;
  gap: 12px;
  align-items: center;
}
.mini-left {
  min-width: 0;
}
.mini-top {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: center;
  flex-wrap: wrap;
}
.mini-title {
  margin-top: 8px;
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.3;
  font-size: 14px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.mini-cover {
  width: 86px;
  height: 62px;
  border-radius: 14px;
  object-fit: cover;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.6);
}

.latest-grid {
  display: grid;
  gap: 12px;
}

.news-card {
  padding: 14px;
  text-align: left;
  width: 100%;
}

.news-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.meta {
  font-size: 12px;
  color: rgba(15, 23, 42, 0.6);
}

.news-title {
  margin-top: 10px;
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.25;
  font-size: 16px;
}

.news-desc {
  margin-top: 8px;
  color: rgba(15, 23, 42, 0.65);
  line-height: 1.65;
  font-size: 13px;
}

.news-foot {
  margin-top: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: rgba(15, 23, 42, 0.72);
  font-weight: 900;
  font-size: 13px;
}

@media (min-width: 768px) {
  .page {
    padding: 18px 18px 0;
  }
  .hero {
    grid-template-areas: 'content banner';
    grid-template-columns: 1.2fr 0.8fr;
    align-items: start;
  }
  .hero-left {
    padding: 18px;
  }
  .hero-title {
    font-size: 28px;
  }
  .latest-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (min-width: 1024px) {
  .quick-item:hover {
    background: rgba(255, 255, 255, 0.55);
    border: 1px solid rgba(15, 23, 42, 0.08);
  }
}
</style>



