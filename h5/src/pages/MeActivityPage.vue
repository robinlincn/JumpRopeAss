<script setup lang="ts">
import { computed, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { getToken } from '../lib/auth'
import { useAutoRefresh } from '../lib/useAutoRefresh'

type EntryItem = {
  id: number
  eventId: number
  eventTitle?: string | null
  groupName?: string | null
  status: number
  createdAt: string
}

const loading = ref(false)
const items = ref<EntryItem[]>([])
const total = ref(0)
const hasToken = computed(() => !!getToken())

function statusLabel(s: number) {
  const map: Record<number, string> = {
    0: '待审核',
    1: '已驳回',
    2: '待缴费',
    3: '支付中',
    4: '已缴费',
    5: '已确认',
    6: '已取消',
    7: '支付失败',
    8: '退款中',
    9: '已退款',
  }
  return map[s] ?? String(s)
}

async function refresh() {
  if (!getToken()) return
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/me/entries?page=1&pageSize=30')
    if (res.code !== 0) return
    total.value = Number(res.data?.total ?? 0)
    items.value = (res.data?.items ?? []).map((x: any) => ({
      id: Number(x.id),
      eventId: Number(x.eventId),
      eventTitle: x.eventTitle ?? null,
      groupName: x.groupName ?? null,
      status: Number(x.status),
      createdAt: String(x.createdAt ?? '').replace('T', ' '),
    }))
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 30000)
</script>

<template>
  <AppHeader title="我的活动" :showBack="true" />
  <main class="app-container page page-pad">
    <div v-if="!hasToken" class="card card-pad empty">未登录：请先到“我的”进行开发登录</div>

    <section v-else class="list">
      <div class="glass head">
        <div class="title">报名记录</div>
        <div class="right">
          <span class="chip">共 {{ total }} 条</span>
          <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '刷新' }}</button>
        </div>
      </div>

      <div v-for="it in items" :key="it.id" class="card item">
        <div class="row">
          <div class="name">{{ it.eventTitle || `活动#${it.eventId}` }}</div>
          <span class="chip">{{ statusLabel(it.status) }}</span>
        </div>
        <div class="meta">
          <span class="chip">组别：{{ it.groupName || '-' }}</span>
          <span class="chip">提交：{{ it.createdAt }}</span>
        </div>
      </div>

      <div v-if="!items.length && !loading" class="empty">暂无报名记录</div>
    </section>
  </main>
</template>

<style scoped>
.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 12px;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.right {
  display: flex;
  align-items: center;
  gap: 8px;
}
.list {
  display: grid;
  gap: 12px;
}
.item {
  padding: 12px;
}
.row {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: flex-start;
}
.name {
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.35;
}
.meta {
  margin-top: 10px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  color: rgba(15, 23, 42, 0.72);
}
.empty {
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
  font-size: 13px;
  padding: 18px 0;
}
</style>

