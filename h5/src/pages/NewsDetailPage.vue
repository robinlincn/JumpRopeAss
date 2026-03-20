<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { useAutoRefresh } from '../lib/useAutoRefresh'
import logoUrl from '../assets/logo.png'

const route = useRoute()
const id = computed(() => String(route.params.id || ''))
const loading = ref(false)
const item = ref<any | null>(null)
const isVideo = computed(() => String(item.value?.contentType ?? 'text') === 'video')

function timeText(v?: string | null) {
  if (!v) return '-'
  return String(v).replace('T', ' ').slice(0, 16)
}

async function refresh() {
  loading.value = true
  try {
    const res = await apiFetch<any>(`/api/v1/app/news/${id.value}`)
    if (res.code === 0) item.value = res.data
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh)
</script>

<template>
  <AppHeader title="资讯详情" :showBack="true" />

  <main class="app-container page page-pad">
    <div v-if="isVideo" class="video glass">
      <div class="video-chip chip">
        <img src="/icon-news.svg" alt="" width="18" height="18" />
        <span>视频资讯</span>
      </div>
      <video v-if="item?.videoUrl" class="video-el" :src="item.videoUrl" controls playsinline></video>
      <div v-else class="video-mask">
        <div class="video-text">暂无视频地址</div>
      </div>
    </div>
    <div v-else-if="item?.coverUrl" class="cover glass">
      <img :src="item.coverUrl" alt="" />
    </div>

    <div class="title">
      {{ item?.title || (loading ? '加载中…' : '资讯详情') }}
    </div>
    <div class="meta">
      <span class="chip">
        <img class="logo" :src="logoUrl" alt="" width="16" height="16" />
        <span>湖南跳绳</span>
      </span>
      <span class="chip">{{ timeText(item?.publishAt) }}</span>
      <span class="chip">浏览 {{ Number(item?.viewCount ?? 0) }}</span>
    </div>

    <div class="content card card-pad">
      <div v-if="item?.summary" class="summary">{{ item.summary }}</div>
      <div v-if="item?.contentHtml" class="html" v-html="item.contentHtml"></div>
      <div v-else class="empty">{{ loading ? '加载中…' : '暂无内容' }}</div>
    </div>
  </main>
</template>

<style scoped>
.cover {
  height: 210px;
  overflow: hidden;
  margin-bottom: 12px;
  position: relative;
}
.cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.92;
}
.video {
  height: 210px;
  overflow: hidden;
  margin-bottom: 12px;
  position: relative;
}
.video-el {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.video-mask {
  height: 100%;
  display: grid;
  place-items: center;
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.28),
    rgb(var(--brand-green-rgb) / 0.22)
  );
}
.video-chip {
  position: absolute;
  left: 12px;
  top: 12px;
}
.video-text {
  color: rgba(255, 255, 255, 0.92);
  font-weight: 900;
  letter-spacing: -0.2px;
}
.title {
  font-weight: 900;
  font-size: 20px;
  line-height: 1.25;
  color: rgba(15, 23, 42, 0.92);
  letter-spacing: -0.2px;
}
.meta {
  margin-top: 10px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.content {
  margin-top: 14px;
  color: rgba(15, 23, 42, 0.9);
  line-height: 1.75;
  font-size: 14px;
}
.summary {
  color: rgba(15, 23, 42, 0.72);
  font-size: 13px;
  line-height: 1.7;
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.55);
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
  color: rgba(15, 23, 42, 0.62);
  font-size: 13px;
}
.logo {
  filter: drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18));
}
</style>

