<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

type Item = {
  id: number
  title: string
  dateText: string
  locationText: string
  statusLabel: string
  statusKey: 'all' | 'not_started' | 'signup' | 'ended'
}

const { defaultType, hideTabs, title } = defineProps<{ defaultType?: number; hideTabs?: boolean; title?: string }>()

const router = useRouter()
const tabs = [
  { key: 'all', label: '全部' },
  { key: 'not_started', label: '未开始' },
  { key: 'signup', label: '报名中' },
  { key: 'ended', label: '已结束' },
] as const

const active = ref<(typeof tabs)[number]['key']>('all')

const items = ref<Item[]>([])

const eventType = computed(() => defaultType ?? 1)

function toDateText(v?: string | null) {
  if (!v) return '-'
  return String(v).slice(0, 10)
}

function statusKeyOf(x: any): Item['statusKey'] {
  const now = Date.now()
  const start = x.signupStartAt ? Date.parse(x.signupStartAt) : NaN
  const end = x.signupEndAt ? Date.parse(x.signupEndAt) : NaN
  if (!Number.isNaN(start) && now < start) return 'not_started'
  if (!Number.isNaN(start) && !Number.isNaN(end) && now >= start && now <= end) return 'signup'
  if (!Number.isNaN(end) && now > end) return 'ended'
  return x.status === 1 ? 'signup' : 'ended'
}

function statusLabelOf(key: Item['statusKey']) {
  if (key === 'not_started') return '报名未开始'
  if (key === 'signup') return '报名进行中'
  if (key === 'ended') return '报名已结束'
  return '全部'
}

async function refresh() {
  const res = await apiFetch<any>(`/api/v1/app/events?page=1&pageSize=50&type=${eventType.value}&status=1`)
  if (res.code !== 0) return
  const list: any[] = res.data?.items ?? []
  items.value = list.map((x) => {
    const key = statusKeyOf(x)
    return {
      id: Number(x.id),
      title: String(x.title),
      dateText: toDateText(x.eventDate ?? x.eventStartAt ?? x.signupStartAt),
      locationText: x.location || '-',
      statusKey: key,
      statusLabel: statusLabelOf(key),
    }
  })
}

useAutoRefresh(refresh, 30000)

const filtered = computed(() => {
  if (active.value === 'all') return items.value
  return items.value.filter((x) => x.statusKey === active.value)
})
</script>

<template>
  <AppHeader :title="title || (eventType === 1 ? '赛事列表' : eventType === 2 ? '评定列表' : '培训列表')" :showBack="true" />

  <main class="app-container page page-pad">
    <div v-if="!hideTabs" class="tabs glass">
      <button
        v-for="t in tabs"
        :key="t.key"
        type="button"
        class="tab pressable"
        :class="{ active: active === t.key }"
        @click="active = t.key"
      >
        {{ t.label }}
      </button>
    </div>

    <div class="list">
      <button
        v-for="it in filtered"
        :key="it.id"
        type="button"
        class="item card btn-reset pressable"
        @click="router.push(`/events/${it.id}`)"
      >
        <div class="badge">{{ it.statusLabel }}</div>
        <div class="row">
          <img class="medal" src="/icon-trophy.svg" alt="" width="22" height="22" />
          <div class="title">{{ it.title }}</div>
        </div>
        <div class="meta">比赛日期：{{ it.dateText }} · {{ it.locationText }}</div>
      </button>
    </div>
  </main>

  <AppTabBar v-if="!hideTabs" />
</template>

<style scoped>
.tabs {
  display: flex;
  gap: 10px;
  overflow-x: auto;
  padding: 10px 10px;
}
.tab {
  border: 0;
  background: rgba(255, 255, 255, 0.25);
  border: 1px solid rgba(15, 23, 42, 0.06);
  font-size: 13px;
  color: rgba(15, 23, 42, 0.62);
  padding: 8px 12px;
  white-space: nowrap;
  border-radius: 999px;
}
.tab.active {
  color: rgba(15, 23, 42, 0.92);
  font-weight: 900;
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.18),
    rgb(var(--brand-green-rgb) / 0.14)
  );
}
.list {
  margin-top: 14px;
  display: grid;
  gap: 12px;
}
.item {
  padding: 12px;
  text-align: left;
  width: 100%;
}
.badge {
  display: inline-block;
  background: rgba(15, 23, 42, 0.06);
  color: rgba(15, 23, 42, 0.72);
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 999px;
}
.row {
  margin-top: 10px;
  display: grid;
  grid-template-columns: 28px 1fr;
  gap: 8px;
  align-items: start;
}
.medal {
  margin-top: 1px;
  opacity: 0.92;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.35;
}
.meta {
  margin-top: 10px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
}
</style>

