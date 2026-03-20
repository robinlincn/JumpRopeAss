<script setup lang="ts">
import { useRouter } from 'vue-router'
import { onBeforeUnmount, onMounted, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const router = useRouter()

type NewsItem = {
  id: number
  title: string
  coverUrl?: string | null
  summary?: string | null
  publishAt?: string | null
  viewCount: number
  contentType: string
}

const items = ref<NewsItem[]>([])
const loading = ref(false)
const finished = ref(false)
const sentinelEl = ref<HTMLDivElement | null>(null)
let observer: IntersectionObserver | null = null
let page = 1
const pageSize = 10

function reset() {
  page = 1
  items.value = []
  finished.value = false
}

function timeText(v?: string | null) {
  if (!v) return '-'
  const s = String(v).replace('T', ' ')
  return s.slice(5, 16)
}

function viewText(n: number) {
  if (n >= 10000) return `${(n / 10000).toFixed(1)}万`
  return String(n)
}

async function fetchMore() {
  if (loading.value || finished.value) return
  loading.value = true
  try {
    const res = await apiFetch<any>(`/api/v1/app/news?page=${page}&pageSize=${pageSize}`)
    if (res.code !== 0) return
    const list: any[] = res.data?.items ?? []
    if (!list.length) {
      finished.value = true
      return
    }
    items.value.push(
      ...list.map((x) => ({
        id: Number(x.id),
        title: String(x.title),
        coverUrl: x.coverUrl ?? null,
        summary: x.summary ?? null,
        publishAt: x.publishAt ?? null,
        viewCount: Number(x.viewCount ?? 0),
        contentType: String(x.contentType ?? 'text'),
      })),
    )
    page += 1
  } finally {
    loading.value = false
  }
}

useAutoRefresh(async () => {
  reset()
  await fetchMore()
}, 30000)

observer = new IntersectionObserver(
  (entries) => {
    if (entries.some((e) => e.isIntersecting)) fetchMore()
  },
  { rootMargin: '200px' },
)

onMounted(() => {
  if (sentinelEl.value) observer?.observe(sentinelEl.value)
})

onBeforeUnmount(() => {
  if (observer) observer.disconnect()
})
</script>

<template>
  <AppHeader title="新闻资讯" :showBack="true" />

  <main class="app-container page page-pad">
    <div class="list">
      <button
        v-for="it in items"
        :key="it.id"
        type="button"
        class="item glass btn-reset pressable"
        @click="router.push(`/news/${it.id}`)"
      >
        <div class="cover">
          <img :src="it.coverUrl || '/hero-jumprope.svg'" alt="" />
        </div>
        <div class="body">
          <div class="row">
            <span class="chip tag">
              <img src="/icon-news.svg" alt="" width="18" height="18" />
              <span>{{ it.contentType === 'video' ? '视频' : '资讯' }}</span>
            </span>
            <div class="title">{{ it.title }}</div>
          </div>
          <div class="meta">
            <span class="meta-left">
              <img src="/icon-grid.svg" alt="" width="16" height="16" />
              <span>{{ viewText(it.viewCount) }}</span>
            </span>
            <span>{{ timeText(it.publishAt) }}</span>
          </div>
        </div>
      </button>
      <div ref="sentinelEl" class="sentinel"></div>
    </div>
    <div v-if="loading" class="loading">正在加载...</div>
    <div v-else-if="finished" class="loading">已无更多</div>
  </main>

  <AppTabBar />
</template>

<style scoped>
.sentinel {
  height: 1px;
}
.loading {
  margin-top: 12px;
  text-align: center;
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
}
.list {
  display: grid;
  gap: 12px;
}
.item {
  display: grid;
  grid-template-columns: 104px 1fr;
  gap: 12px;
  padding: 12px;
  text-align: left;
}
.cover {
  width: 104px;
  height: 74px;
  border-radius: 14px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.22);
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.12),
    rgb(var(--brand-green-rgb) / 0.10)
  );
}
.cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.9;
}
.row {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}
.tag {
  flex: none;
  padding: 6px 10px;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.3;
  font-size: 14px;
}
.meta {
  margin-top: 8px;
  display: flex;
  justify-content: space-between;
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
  align-items: center;
}
.meta-left {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
</style>

