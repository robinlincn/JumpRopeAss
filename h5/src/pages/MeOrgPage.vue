<script setup lang="ts">
import { ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

type OrgItem = {
  id: number
  name: string
  orgType: number
  status: number
}

const keyword = ref('')
const loading = ref(false)
const items = ref<OrgItem[]>([])

function typeText(v: number) {
  return v === 1 ? '地方协会' : v === 2 ? '会员单位' : '机构'
}

async function refresh() {
  loading.value = true
  try {
    const qs = new URLSearchParams()
    if (keyword.value.trim()) qs.set('keyword', keyword.value.trim())
    qs.set('page', '1')
    qs.set('pageSize', '50')
    const res = await apiFetch<any>(`/api/v1/app/orgs?${qs.toString()}`)
    if (res.code !== 0) return
    items.value = (res.data?.items ?? []).map((x: any) => ({
      id: Number(x.id),
      name: String(x.name),
      orgType: Number(x.orgType),
      status: Number(x.status),
    }))
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 60000)
</script>

<template>
  <AppHeader title="绑定机构" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="glass search">
      <input v-model="keyword" class="input" placeholder="搜索机构名称" />
      <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '查询' }}</button>
    </section>

    <section class="list">
      <div v-for="it in items" :key="it.id" class="card item">
        <div class="row">
          <div class="name">{{ it.name }}</div>
          <span class="chip">{{ typeText(it.orgType) }}</span>
        </div>
        <div class="meta">
          <span class="chip">ID：{{ it.id }}</span>
          <span class="chip">{{ it.status === 1 ? '启用' : '停用' }}</span>
        </div>
      </div>
      <div v-if="!items.length && !loading" class="empty">暂无机构数据（可在后台机构管理中新增）</div>
    </section>
  </main>
</template>

<style scoped>
.search {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px;
  padding: 12px;
}
.input {
  height: 40px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.82);
  padding: 0 12px;
  outline: none;
}
.list {
  margin-top: 12px;
  display: grid;
  gap: 12px;
}
.item {
  padding: 12px;
}
.row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 10px;
}
.name {
  font-weight: 950;
  letter-spacing: -0.2px;
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

