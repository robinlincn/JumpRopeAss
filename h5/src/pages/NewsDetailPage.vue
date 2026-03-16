<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'

const route = useRoute()
const id = computed(() => String(route.params.id || ''))
const isVideo = computed(() => id.value === '2')
</script>

<template>
  <AppHeader title="资讯详情" :showBack="true" />

  <main class="app-container page page-pad">
    <div v-if="isVideo" class="video glass">
      <div class="video-mask">
        <div class="video-chip chip">
          <img src="/icon-news.svg" alt="" width="18" height="18" />
          <span>视频资讯</span>
        </div>
        <div class="video-text">视频区域（示例）</div>
      </div>
    </div>

    <div class="title">
      {{ isVideo ? '2026年湖南省学生跳绳等级评定：郴州站圆满举行' : '近万名学生参与竞技！2025年度回望' }}
    </div>
    <div class="meta">
      <span class="chip">
        <img class="logo" src="/logo.svg" alt="" width="16" height="16" />
        <span>湖南跳绳</span>
      </span>
      <span class="chip">2026-01-13 16:21</span>
      <span class="chip">湖南</span>
    </div>

    <div class="content card card-pad">
      <p>
        本页面用于 H5 阶段验证资讯详情的排版与“图文/视频”两种内容形态。后续接入后台发布的 HTML 内容与视频地址。
      </p>
      <p>
        重点：标题、时间、来源、正文富文本、可选视频、图片自适应、长内容滚动表现。
      </p>
      <div class="img glass">
        <img src="/hero-jumprope.svg" alt="" />
        <div class="img-label chip">
          <img src="/icon-trophy.svg" alt="" width="18" height="18" />
          <span>活动海报</span>
        </div>
      </div>
      <p>
        这里展示更多正文段落……这里展示更多正文段落……这里展示更多正文段落……这里展示更多正文段落……
      </p>
    </div>
  </main>
</template>

<style scoped>
.video {
  height: 210px;
  overflow: hidden;
  margin-bottom: 12px;
  position: relative;
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
.img {
  margin: 12px 0;
  height: 160px;
  overflow: hidden;
  position: relative;
}
.img img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.9;
}
.img-label {
  position: absolute;
  left: 12px;
  bottom: 12px;
  background: rgba(255, 255, 255, 0.62);
  border-color: rgba(15, 23, 42, 0.10);
  color: rgba(15, 23, 42, 0.78);
}
.logo {
  filter: drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18));
}
</style>

