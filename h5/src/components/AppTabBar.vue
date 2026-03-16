<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const items = [
  { path: '/', label: '首页', icon: '/icon-grid.svg' },
  { path: '/events', label: '赛事', icon: '/icon-trophy.svg' },
  { path: '/assessments', label: '评定', icon: '/icon-trophy.svg' },
  { path: '/trainings', label: '培训', icon: '/icon-news.svg' },
  { path: '/me', label: '我的', icon: '/icon-user.svg' },
]

const activePath = computed(() => {
  const hit = items.find((x) => route.path === x.path || route.path.startsWith(x.path + '/'))
  return hit?.path ?? '/'
})
</script>

<template>
  <nav class="tabbar glass">
    <button
      v-for="it in items"
      :key="it.path"
      class="tab"
      :class="{ active: activePath === it.path }"
      type="button"
      @click="router.push(it.path)"
    >
      <img class="icon" :src="it.icon" alt="" width="22" height="22" />
      <span class="label">{{ it.label }}</span>
    </button>
  </nav>
</template>

<style scoped>
.tabbar {
  position: sticky;
  bottom: 0;
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  height: 66px;
  margin: 12px 12px 12px;
  padding: 6px;
}
.tab {
  appearance: none;
  border: 0;
  background: transparent;
  display: grid;
  justify-items: center;
  gap: 4px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.55);
  border-radius: 14px;
}
.tab.active {
  color: rgba(15, 23, 42, 0.92);
  font-weight: 800;
  background: rgba(255, 255, 255, 0.55);
  border: 1px solid rgba(15, 23, 42, 0.08);
}
.label {
  line-height: 1;
}
.icon {
  opacity: 0.9;
}
</style>

