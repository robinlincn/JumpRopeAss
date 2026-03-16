<script setup lang="ts">
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'

const props = defineProps<{ title: string }>()
</script>

<template>
  <AppHeader :title="props.title" :showBack="true" />
  <main class="app-container page page-pad">
    <div class="card card-pad">
      <div class="title">{{ props.title }}</div>
      <div class="desc">
        该页面用于结构与视觉验证，后续将补齐列表、详情、编辑与导出等完整能力。
      </div>
      <div class="actions">
        <button class="btn" type="button">开始使用</button>
        <button class="btn btn-ghost" type="button">查看说明</button>
      </div>
    </div>
  </main>
  <AppTabBar />
</template>

<style scoped>
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.desc {
  margin-top: 8px;
  color: rgba(15, 23, 42, 0.65);
  font-size: 13px;
  line-height: 1.7;
}
.actions {
  margin-top: 14px;
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}
</style>

