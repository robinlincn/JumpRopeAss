<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const spriteUrl = '/icons.svg?v=1'

const items = [
  { key: 'home', path: '/', label: '首页', icon: 'jr-home' },
  { key: 'events', path: '/events', label: '赛事', icon: 'jr-event' },
  { key: 'cert', path: '/cert/verify', label: '证书', icon: 'jr-cert' },
  { key: 'assess', path: '/assessments', label: '评定', icon: 'jr-assessment' },
  { key: 'train', path: '/trainings', label: '培训', icon: 'jr-training' },
  { key: 'me', path: '/me', label: '我的', icon: 'jr-me' },
]

const activePath = computed(() => {
  const hit = items.find((x) => route.path === x.path || route.path.startsWith(x.path + '/'))
  return hit?.path ?? '/'
})

const visibleItems = computed(() => items.filter((x) => x.key !== 'assess' && x.key !== 'train'))
</script>

<template>
  <nav class="tabbar glass" aria-label="底部导航" :style="{ gridTemplateColumns: `repeat(${visibleItems.length}, 1fr)` }">
    <button
      v-for="it in visibleItems"
      :key="it.path"
      class="tab"
      :class="[`k-${it.key}`, { active: activePath === it.path }]"
      type="button"
      :aria-current="activePath === it.path ? 'page' : undefined"
      @click="router.push(it.path)"
    >
      <span class="ico" aria-hidden="true">
        <svg class="ico-svg" width="22" height="22" viewBox="0 0 24 24">
          <use :href="`${spriteUrl}#${it.icon}`"></use>
        </svg>
      </span>
      <span class="label">{{ it.label }}</span>
    </button>
  </nav>
</template>

<style scoped>
.tabbar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
  display: grid;
  /* grid-template-columns is set dynamically via inline style */
  gap: 8px;
  height: 66px;
  margin: 0;
  padding: 6px;
  padding-bottom: calc(6px + env(safe-area-inset-bottom, 0px));
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
  min-height: 54px;
  transition:
    transform 160ms var(--ease-out),
    background 180ms var(--ease-out),
    border-color 180ms var(--ease-out),
    color 180ms var(--ease-out);
}
.tab:active {
  transform: translateY(1px) scale(0.99);
}
.tab.active {
  color: rgba(15, 23, 42, 0.92);
  font-weight: 800;
  background: rgba(255, 255, 255, 0.55);
  border: 1px solid rgba(15, 23, 42, 0.08);
}
.ico {
  width: 44px;
  height: 34px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  border: 1px solid rgba(255, 255, 255, 0.18);
  background: rgba(255, 255, 255, 0.22);
  transition:
    background 180ms var(--ease-out),
    border-color 180ms var(--ease-out),
    box-shadow 220ms var(--ease-out);
}
.ico-svg {
  display: block;
  color: rgba(15, 23, 42, 0.58);
}
.tab.active .ico {
  border-color: rgba(15, 23, 42, 0.06);
  background: linear-gradient(135deg, var(--tab-accent-soft), rgba(255, 255, 255, 0.62));
  box-shadow: 0 14px 26px rgba(2, 6, 23, 0.10);
}
.tab.active .ico-svg {
  color: var(--tab-accent);
}
.tab.k-home {
  --tab-accent: rgb(var(--brand-blue-rgb));
  --tab-accent-soft: rgb(var(--brand-blue-rgb) / 0.18);
}
.tab.k-events {
  --tab-accent: rgb(var(--brand-cyan-rgb));
  --tab-accent-soft: rgb(var(--brand-cyan-rgb) / 0.18);
}
.tab.k-cert {
  --tab-accent: rgb(var(--brand-blue-rgb));
  --tab-accent-soft: rgb(var(--brand-blue-rgb) / 0.14);
}
.tab.k-assess {
  --tab-accent: rgb(var(--brand-navy-rgb));
  --tab-accent-soft: rgb(var(--brand-navy-rgb) / 0.16);
}
.tab.k-train {
  --tab-accent: rgb(var(--brand-yellow-rgb));
  --tab-accent-soft: rgb(var(--brand-yellow-rgb) / 0.16);
}
.tab.k-me {
  --tab-accent: rgb(var(--brand-green-rgb));
  --tab-accent-soft: rgb(var(--brand-green-rgb) / 0.16);
}
.label {
  line-height: 1;
}
</style>

