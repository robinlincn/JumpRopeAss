<script setup lang="ts">
import { computed, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const loading = ref(false)
const item = ref<any | null>(null)

const sections = computed(() => {
  const it = item.value
  return [
    { key: 'overviewHtml', title: '协会概况', html: it?.overviewHtml || '' },
    { key: 'historyHtml', title: '发展历程', html: it?.historyHtml || '' },
    { key: 'honorsHtml', title: '荣誉资质', html: it?.honorsHtml || '' },
  ].filter((x) => x.html)
})

async function refresh() {
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/about')
    if (res.code !== 0) return
    item.value = res.data?.item ?? null
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 60000)
</script>

<template>
  <AppHeader title="关于协会" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="glass head">
      <div class="brand">
        <img class="logo" :src="item?.logoUrl || '/logo.png'" alt="" />
        <div>
          <div class="name">{{ item?.name || item?.title || '湖南省学生跳绳协会' }}</div>
          <div class="addr">{{ item?.address || '—' }}</div>
        </div>
      </div>
      <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '刷新' }}</button>
    </section>

    <section v-if="sections.length" class="list">
      <div v-for="s in sections" :key="s.key" class="card item">
        <div class="title">{{ s.title }}</div>
        <div class="html" v-html="s.html"></div>
      </div>
    </section>

    <div v-else class="empty card card-pad">
      {{ loading ? '加载中…' : '暂无内容，请先在后台维护“关于协会”页面内容' }}
    </div>
  </main>
  <AppTabBar />
</template>

<style scoped>
.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px;
}
.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}
.logo {
  width: 44px;
  height: 44px;
  border-radius: 16px;
  object-fit: cover;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.76);
}
.name {
  font-weight: 950;
  letter-spacing: -0.2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.addr {
  margin-top: 2px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
}
.list {
  margin-top: 12px;
  display: grid;
  gap: 12px;
}
.item {
  padding: 14px;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.html {
  margin-top: 10px;
  color: rgba(15, 23, 42, 0.86);
  font-size: 14px;
  line-height: 1.75;
}
.html :deep(img) {
  max-width: 100%;
  border-radius: 12px;
  border: 1px solid rgba(15, 23, 42, 0.10);
}
.html :deep(p) {
  margin: 0 0 12px;
}
.empty {
  margin-top: 12px;
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
}
</style>
