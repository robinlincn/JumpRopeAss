<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'

type Item = {
  id: string
  title: string
  date: string
  statusLabel: string
  status: 'all' | 'not_started' | 'signup' | 'soon' | 'running' | 'ended'
}

const router = useRouter()
const tabs = [
  { key: 'all', label: '全部' },
  { key: 'not_started', label: '未开始' },
  { key: 'signup', label: '报名中' },
  { key: 'soon', label: '即将开始' },
  { key: 'running', label: '进行中' },
  { key: 'ended', label: '已结束' },
] as const

const active = ref<(typeof tabs)[number]['key']>('all')

const items = ref<Item[]>([
  { id: '1', title: '2025年湖南省学生跳绳公开赛：益阳站报名', date: '2025-07-28', statusLabel: '比赛已结束', status: 'ended' },
  { id: '2', title: '2025年湖南省学生跳绳公开赛：湘潭站报名', date: '2025-07-05', statusLabel: '比赛已结束', status: 'ended' },
  { id: '3', title: '2025年湖南省学生跳绳公开赛：张家界站报名', date: '2025-10-07', statusLabel: '比赛已结束', status: 'ended' },
  { id: '4', title: '2025年湖南省学生跳绳酷跑军赛', date: '2025-10-18', statusLabel: '比赛已结束', status: 'ended' },
])

const filtered = computed(() => {
  if (active.value === 'all') return items.value
  return items.value.filter((x) => x.status === active.value)
})
</script>

<template>
  <AppHeader title="赛事列表" />

  <main class="app-container page page-pad">
    <div class="tabs glass">
      <button
        v-for="t in tabs"
        :key="t.key"
        type="button"
        class="tab"
        :class="{ active: active === t.key }"
        @click="active = t.key"
      >
        {{ t.label }}
      </button>
    </div>

    <div class="list">
      <div
        v-for="it in filtered"
        :key="it.id"
        class="item card"
        @click="router.push(`/events/${it.id}`)"
      >
        <div class="badge">{{ it.statusLabel }}</div>
        <div class="row">
          <img class="medal" src="/icon-trophy.svg" alt="" width="22" height="22" />
          <div class="title">{{ it.title }}</div>
        </div>
        <div class="meta">比赛日期：{{ it.date }}</div>
      </div>
    </div>
  </main>

  <AppTabBar />
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

