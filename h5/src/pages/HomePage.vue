<script setup lang="ts">
import { useRouter } from 'vue-router'
import AppTabBar from '../components/AppTabBar.vue'
import BannerCarousel from '../components/BannerCarousel.vue'

const router = useRouter()

const quick = [
  { label: '会员单位', path: '/org/members', icon: '/icon-grid.svg' },
  { label: '地方协会', path: '/org/locals', icon: '/icon-grid.svg' },
  { label: '关于协会', path: '/about', icon: '/icon-news.svg' },
  { label: '新闻资讯', path: '/news', icon: '/icon-news.svg' },
]

const banners = [
  {
    src: '/banner-1.svg',
    title: '校园跳绳公开赛 · 热血开赛',
    subtitle: '多地联动，团体与个人项目齐开，报名、审核、缴费一站式完成。',
  },
  {
    src: '/banner-2.svg',
    title: '等级评定 · 专业认证',
    subtitle: '标准化评定流程，后台审核留痕，电子证书随时可查。',
  },
  {
    src: '/banner-3.svg',
    title: '培训计划 · 提升教练力',
    subtitle: '教练员继续教育与教学资源同步更新，带动更多学生跃动起来。',
  },
]

const stats = [
  { label: '裁判员', value: 266 },
  { label: '评定学员', value: 20789 },
  { label: '教练员', value: 366 },
]
</script>

<template>
  <main class="app-container page">
    <section class="hero glass">
      <div class="hero-banner">
        <BannerCarousel :items="banners" />
      </div>
      <div class="hero-left">
        <div class="brand">
          <div class="brand-logo-wrap">
            <img class="brand-logo" src="/logo.svg" alt="" width="38" height="38" />
          </div>
          <div>
            <div class="brand-name">湖南省学生跳绳协会</div>
            <div class="brand-sub">朝气蓬勃 · 青春活力 · 专业赛事</div>
          </div>
        </div>

        <div class="hero-title">让每一次跃起，都更有力量</div>
        <div class="hero-desc">
          资讯、赛事、评定、培训与证书，一站式服务。支持运动员/家长/第一教练员报名与审核缴费闭环。
        </div>

        <div class="hero-cta">
          <button class="btn" type="button" @click="router.push('/events')">立即查看赛事</button>
          <button class="btn btn-ghost" type="button" @click="router.push('/me')">进入个人中心</button>
        </div>

        <div class="hero-chips">
          <span class="chip">报名审核</span>
          <span class="chip">微信缴费</span>
          <span class="chip">电子证书</span>
          <span class="chip">留痕追溯</span>
        </div>
      </div>
    </section>

    <section class="quick glass">
      <button
        v-for="q in quick"
        :key="q.path"
        class="quick-item"
        type="button"
        @click="router.push(q.path)"
      >
        <img :src="q.icon" alt="" width="28" height="28" />
        <span>{{ q.label }}</span>
      </button>
    </section>

    <section class="stats">
      <div v-for="s in stats" :key="s.label" class="stat card">
        <div class="stat-value">{{ s.value }}</div>
        <div class="stat-label">{{ s.label }}</div>
      </div>
    </section>

    <section class="latest">
      <div class="section-head">
        <div>
          <div class="h-title">最新信息</div>
          <div class="h-sub">及时掌握协会动态与活动进展</div>
        </div>
        <button class="chip" type="button" @click="router.push('/news')">查看更多</button>
      </div>

      <div class="latest-grid">
        <div class="news-card card" @click="router.push('/news/1')">
          <div class="news-top">
            <span class="chip">最新资讯</span>
            <span class="meta">阅读量 1.9万 · 03/02 21:40</span>
          </div>
          <div class="news-title">2026年湖南省学生跳绳等级评定活动圆满举行</div>
          <div class="news-desc">全省多地同步开展，运动员热情高涨，现场气氛燃动。</div>
          <div class="news-foot">
            <img src="/icon-news.svg" alt="" width="24" height="24" />
            <span>查看详情</span>
          </div>
        </div>

        <div class="news-card glass" @click="router.push('/assessments')">
          <div class="news-top">
            <span class="chip">最新评定</span>
            <span class="meta">评定日期 2026-03-02 · 报名已结束</span>
          </div>
          <div class="news-title">2026年3月，长沙市地区等级评定</div>
          <div class="news-desc">评定通过后可生成电子证书，支持首次/补证定价。</div>
          <div class="news-foot">
            <img src="/icon-trophy.svg" alt="" width="24" height="24" />
            <span>查看活动</span>
          </div>
        </div>
      </div>
    </section>
  </main>

  <AppTabBar />
</template>

<style scoped>
.page {
  padding: 14px 12px 0;
}

.hero {
  padding: 16px;
  overflow: hidden;
  display: grid;
  gap: 14px;
  grid-template-areas: 'banner' 'content';
}

.hero-banner {
  grid-area: banner;
}

.hero-left {
  grid-area: content;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
}

.brand-logo-wrap {
  width: 44px;
  height: 44px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.22);
  display: grid;
  place-items: center;
  box-shadow: 0 14px 32px rgba(2, 6, 23, 0.22);
}

.brand-logo {
  border-radius: 12px;
  filter: drop-shadow(0 10px 18px rgba(2, 6, 23, 0.25));
}

.brand-name {
  font-weight: 950;
  letter-spacing: -0.2px;
}

.brand-sub {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.9);
}

.hero-left {
  color: rgba(255, 255, 255, 0.96);
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.92),
    rgb(var(--brand-green-rgb) / 0.86)
  );
  border-radius: 18px;
  padding: 16px;
  border: 1px solid rgba(255, 255, 255, 0.18);
}

.hero-title {
  margin-top: 12px;
  font-size: 24px;
  font-weight: 950;
  letter-spacing: -0.4px;
  line-height: 1.2;
}

.hero-desc {
  margin-top: 10px;
  color: rgba(255, 255, 255, 0.88);
  line-height: 1.6;
  font-size: 13px;
}

.hero-cta {
  margin-top: 14px;
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.hero-chips {
  margin-top: 12px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.hero-chips .chip {
  background: rgba(255, 255, 255, 0.18);
  border-color: rgba(255, 255, 255, 0.26);
  color: rgba(255, 255, 255, 0.92);
}

.quick {
  margin-top: 14px;
  padding: 12px 8px;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 6px;
}

.quick-item {
  border: 0;
  background: transparent;
  display: grid;
  justify-items: center;
  gap: 8px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.82);
  padding: 10px 6px;
  border-radius: 16px;
}

.stats {
  margin-top: 14px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.stat {
  padding: 12px 10px;
  text-align: center;
}

.stat-value {
  font-weight: 950;
  font-size: 18px;
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb)), rgb(var(--brand-green-rgb)));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.stat-label {
  margin-top: 4px;
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
}

.latest {
  margin-top: 14px;
  padding-bottom: 12px;
}

.section-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.latest-grid {
  display: grid;
  gap: 12px;
}

.news-card {
  padding: 14px;
}

.news-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.meta {
  font-size: 12px;
  color: rgba(15, 23, 42, 0.6);
}

.news-title {
  margin-top: 10px;
  font-weight: 950;
  letter-spacing: -0.2px;
  line-height: 1.25;
  font-size: 16px;
}

.news-desc {
  margin-top: 8px;
  color: rgba(15, 23, 42, 0.65);
  line-height: 1.65;
  font-size: 13px;
}

.news-foot {
  margin-top: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: rgba(15, 23, 42, 0.72);
  font-weight: 900;
  font-size: 13px;
}

@media (min-width: 768px) {
  .page {
    padding: 18px 18px 0;
  }
  .hero {
    grid-template-areas: 'content banner';
    grid-template-columns: 1.2fr 0.8fr;
    align-items: start;
  }
  .hero-left {
    padding: 18px;
  }
  .hero-title {
    font-size: 28px;
  }
  .latest-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (min-width: 1024px) {
  .quick-item:hover {
    background: rgba(255, 255, 255, 0.55);
    border: 1px solid rgba(15, 23, 42, 0.08);
  }
}
</style>



