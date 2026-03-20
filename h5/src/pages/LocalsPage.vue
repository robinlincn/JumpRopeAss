<script setup lang="ts">
import { computed, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

type LocalItem = {
  id: number
  name: string
  logoUrl?: string | null
  contactName?: string | null
  contactPhone?: string | null
  intro?: string | null
}

const keyword = ref('')
const loading = ref(false)
const items = ref<LocalItem[]>([])

const filtered = computed(() => {
  const kw = keyword.value.trim()
  if (!kw) return items.value
  return items.value.filter((x) => x.name.includes(kw))
})

async function refresh() {
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/locals?page=1&pageSize=50')
    if (res.code !== 0) return
    items.value = (res.data?.items ?? []).map((x: any) => ({
      id: Number(x.id),
      name: String(x.name),
      logoUrl: x.logoUrl ?? null,
      contactName: x.contactName ?? null,
      contactPhone: x.contactPhone ?? null,
      intro: x.intro ?? null,
    }))
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 30000)
</script>

<template>
  <AppHeader title="地方协会" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="glass search">
      <input v-model="keyword" class="input" placeholder="搜索协会名称" />
      <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '刷新' }}</button>
    </section>

    <section class="list">
      <div v-for="it in filtered" :key="it.id" class="card item">
        <div class="head">
          <img class="logo" :src="it.logoUrl || '/logo.png'" alt="" />
          <div class="name">{{ it.name }}</div>
        </div>
        <div v-if="it.intro" class="intro">{{ it.intro }}</div>
        <div class="meta">
          <span class="chip">联系人：{{ it.contactName || '-' }}</span>
          <span class="chip">电话：{{ it.contactPhone || '-' }}</span>
        </div>
      </div>
      <div v-if="!filtered.length && !loading" class="empty">暂无数据</div>
    </section>
  </main>
  <AppTabBar />
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
.head {
  display: grid;
  grid-template-columns: 40px 1fr;
  gap: 10px;
  align-items: center;
}
.logo {
  width: 40px;
  height: 40px;
  border-radius: 14px;
  object-fit: cover;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.72);
}
.name {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.intro {
  margin-top: 10px;
  color: rgba(15, 23, 42, 0.72);
  font-size: 13px;
  line-height: 1.6;
}
.meta {
  margin-top: 10px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.empty {
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
  font-size: 13px;
  padding: 18px 0;
}
</style>
