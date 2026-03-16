<script setup lang="ts">
import { useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'

const router = useRouter()

const items = [
  { id: '1', title: '2026年湖南省学生跳绳等级评定活动圆满举行', views: '1.9万', time: '03/02 21:40' },
  { id: '2', title: '2026年湖南省学生跳绳等级评定：湘潭站', views: '1.3万', time: '02/09 14:25' },
  { id: '3', title: '怀化市学生跳绳协会2025年年会圆满落幕', views: '1.4万', time: '01/28 09:45' },
]
</script>

<template>
  <AppHeader title="新闻资讯" :showBack="true" />

  <main class="app-container page page-pad">
    <div class="list">
      <div v-for="it in items" :key="it.id" class="item glass" @click="router.push(`/news/${it.id}`)">
        <div class="cover">
          <img src="/hero-jumprope.svg" alt="" />
        </div>
        <div class="body">
          <div class="row">
            <span class="chip tag">
              <img src="/icon-news.svg" alt="" width="18" height="18" />
              <span>最新资讯</span>
            </span>
            <div class="title">{{ it.title }}</div>
          </div>
          <div class="meta">
            <span class="meta-left">
              <img src="/icon-grid.svg" alt="" width="16" height="16" />
              <span>{{ it.views }}</span>
            </span>
            <span>{{ it.time }}</span>
          </div>
        </div>
      </div>
    </div>
  </main>

  <AppTabBar />
</template>

<style scoped>
.list {
  display: grid;
  gap: 12px;
}
.item {
  display: grid;
  grid-template-columns: 104px 1fr;
  gap: 12px;
  padding: 12px;
}
.cover {
  width: 104px;
  height: 74px;
  border-radius: 14px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.22);
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.12),
    rgb(var(--brand-green-rgb) / 0.10)
  );
}
.cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.9;
}
.row {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}
.tag {
  flex: none;
  padding: 6px 10px;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.3;
  font-size: 14px;
}
.meta {
  margin-top: 8px;
  display: flex;
  justify-content: space-between;
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
  align-items: center;
}
.meta-left {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
</style>

