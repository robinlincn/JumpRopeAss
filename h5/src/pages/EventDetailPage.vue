<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'

const route = useRoute()
const router = useRouter()

const id = computed(() => String(route.params.id || ''))

const groups = [
  { code: 'A1', name: '少儿组单人作品赛' },
  { code: 'A2', name: '少儿组3-6人团体作品赛' },
  { code: 'A3', name: '少儿组7-15人团体作品赛' },
  { code: 'A4', name: '精英组作品赛' },
]
</script>

<template>
  <AppHeader title="赛事详情" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="cover glass">
      <img class="cover-img" src="/hero-jumprope.svg" alt="" />
      <div class="cover-overlay">
        <div class="cover-chip">
          <img src="/icon-trophy.svg" alt="" width="20" height="20" />
          <span>公开赛 · 开放报名</span>
        </div>
        <div class="cover-title">2025年湖南省学生跳绳公开赛：益阳站报名</div>
        <div class="cover-meta">
          <span class="chip">报名：2025-09-29 ~ 2025-10-26</span>
          <span class="chip">比赛：2025-07-28</span>
        </div>
      </div>
    </section>

    <section class="card card-pad">
      <div class="block">
        <div class="label">报名时间</div>
        <div class="value">2025-09-29 ~ 2025-10-26</div>
      </div>
      <div class="block">
        <div class="label">比赛时间</div>
        <div class="value">2025-07-28</div>
      </div>
      <div class="block">
        <div class="label">参赛组别</div>
        <div class="value">
          <div v-for="g in groups" :key="g.code" class="group">
            <span class="chip">{{ g.code }}</span>
            <span>{{ g.name }}</span>
          </div>
        </div>
      </div>
      <div class="block">
        <div class="label">比赛地点</div>
        <div class="value">益阳市赫山区清溪剧院</div>
      </div>
    </section>
  </main>

  <footer class="bottom">
    <div class="left">{{ id }}号赛事 · 403人参与</div>
    <button class="btn" type="button" @click="router.push('/events')">报名已结束</button>
  </footer>
</template>

<style scoped>
.page {
  padding-bottom: 86px;
}
.cover {
  position: relative;
  height: 220px;
  overflow: hidden;
}
.cover-img {
  position: absolute;
  inset: -30px;
  width: calc(100% + 60px);
  height: calc(100% + 60px);
  object-fit: cover;
  opacity: 0.9;
  filter: saturate(1.08) contrast(1.02);
}
.cover-overlay {
  position: absolute;
  inset: 0;
  padding: 16px;
  display: grid;
  align-content: end;
  gap: 10px;
  background: linear-gradient(180deg, rgba(2, 6, 23, 0.10), rgba(2, 6, 23, 0.42));
}
.cover-chip {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.22);
  background: rgba(255, 255, 255, 0.14);
  color: rgba(255, 255, 255, 0.92);
  width: fit-content;
  font-size: 12px;
  font-weight: 800;
}
.cover-title {
  color: #fff;
  font-weight: 950;
  letter-spacing: -0.2px;
  font-size: 18px;
  line-height: 1.25;
  text-shadow: 0 10px 28px rgba(2, 6, 23, 0.42);
}
.cover-meta {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.cover-meta .chip {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.2);
  color: rgba(255, 255, 255, 0.9);
}
.block {
  margin-top: 12px;
}
.label {
  font-weight: 900;
  letter-spacing: -0.2px;
  font-size: 13px;
}
.value {
  margin-top: 6px;
  color: rgba(15, 23, 42, 0.72);
  font-size: 13px;
  line-height: 1.7;
}
.group {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 8px;
}
.bottom {
  position: fixed;
  left: 12px;
  right: 12px;
  bottom: 12px;
  height: 66px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 14px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.76);
  border: 1px solid rgba(15, 23, 42, 0.10);
  box-shadow: 0 18px 44px rgba(2, 6, 23, 0.18);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
}
.left {
  font-size: 13px;
  color: rgba(15, 23, 42, 0.72);
  font-weight: 800;
}
.btn {
  padding: 12px 14px;
}
</style>

