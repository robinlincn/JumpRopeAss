<script setup lang="ts">
import { computed, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { getToken } from '../lib/auth'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const hasToken = computed(() => !!getToken())
const loading = ref(false)
const summary = ref<{ total: number; pending: number; waitPay: number } | null>(null)

async function refresh() {
  if (!getToken()) return
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/me/entries?page=1&pageSize=50')
    if (res.code !== 0) return
    const list: any[] = res.data?.items ?? []
    summary.value = {
      total: Number(res.data?.total ?? list.length),
      pending: list.filter((x) => Number(x.status) === 0).length,
      waitPay: list.filter((x) => Number(x.status) === 2).length,
    }
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 30000)
</script>

<template>
  <AppHeader title="我的主页" :showBack="true" />
  <main class="app-container page page-pad">
    <div v-if="!hasToken" class="card card-pad empty">未登录：请先到“我的”进行开发登录</div>

    <section v-else class="glass head">
      <div class="title">个人概览</div>
      <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '刷新' }}</button>
    </section>

    <section v-if="summary" class="grid">
      <div class="card stat">
        <div class="num">{{ summary.total }}</div>
        <div class="lbl">报名总数</div>
      </div>
      <div class="card stat">
        <div class="num">{{ summary.pending }}</div>
        <div class="lbl">待审核</div>
      </div>
      <div class="card stat">
        <div class="num">{{ summary.waitPay }}</div>
        <div class="lbl">待缴费</div>
      </div>
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
.grid {
  margin-top: 12px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
.stat {
  padding: 12px 10px;
  text-align: center;
}
.num {
  font-weight: 950;
  font-size: 18px;
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb)), rgb(var(--brand-green-rgb)));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}
.lbl {
  margin-top: 4px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.62);
}
.empty {
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
}
</style>

