<script setup lang="ts">
import { useRouter } from 'vue-router'
import AppTabBar from '../components/AppTabBar.vue'

const router = useRouter()
</script>

<template>
  <main class="app-container page page-pad">
    <section class="profile glass">
      <div class="profile-top">
        <div class="avatar">
          <img class="logo" src="/logo.svg" alt="" width="34" height="34" />
        </div>
        <div class="info">
          <div class="name">个人中心</div>
          <div class="sub">暂未关联人员信息</div>
        </div>
        <button class="chip" type="button" @click="router.push('/me/identity')">去认证</button>
      </div>
      <div class="counts">
        <div class="count card">
          <div class="num">0</div>
          <div class="lbl">学员</div>
        </div>
        <div class="count card">
          <div class="num">0</div>
          <div class="lbl">教练</div>
        </div>
        <div class="count card">
          <div class="num">0</div>
          <div class="lbl">裁判</div>
        </div>
      </div>
    </section>

    <section class="menu">
      <button class="row glass" type="button" @click="router.push('/me/profile')">
        <span class="left">
          <img src="/icon-grid.svg" alt="" width="22" height="22" />
          <span>信息录入</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass" type="button" @click="router.push('/me/org')">
        <span class="left">
          <img src="/icon-grid.svg" alt="" width="22" height="22" />
          <span>绑定机构</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass" type="button" @click="router.push('/me/activity')">
        <span class="left">
          <img src="/icon-trophy.svg" alt="" width="22" height="22" />
          <span>我的活动</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass" type="button" @click="router.push('/me/identity')">
        <span class="left">
          <img src="/icon-user.svg" alt="" width="22" height="22" />
          <span>我的认证</span>
        </span>
        <span class="badge">未认证</span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass" type="button" @click="router.push('/me/home')">
        <span class="left">
          <img src="/icon-news.svg" alt="" width="22" height="22" />
          <span>我的主页</span>
        </span>
        <span class="arrow">›</span>
      </button>
    </section>
  </main>

  <AppTabBar />
</template>

<style scoped>
.profile {
  padding: 14px;
}
.profile-top {
  display: flex;
  align-items: center;
  gap: 12px;
}
.avatar {
  width: 48px;
  height: 48px;
  border-radius: 16px;
  background: linear-gradient(
    135deg,
    rgb(var(--brand-blue-rgb) / 0.16),
    rgb(var(--brand-green-rgb) / 0.12)
  );
  border: 1px solid rgba(255, 255, 255, 0.22);
  display: grid;
  place-items: center;
}
.logo {
  filter: drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18));
}
.info {
  flex: 1;
  min-width: 0;
}
.name {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.sub {
  margin-top: 2px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
}
.counts {
  margin-top: 14px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
.count {
  padding: 12px 10px;
  text-align: center;
}
.lbl {
  margin-top: 4px;
  font-size: 12px;
  color: rgba(15, 23, 42, 0.62);
}
.num {
  font-weight: 950;
  font-size: 18px;
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb)), rgb(var(--brand-green-rgb)));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}
.menu {
  margin-top: 14px;
  display: grid;
  gap: 10px;
}
.row {
  border: 0;
  padding: 14px 12px;
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: 10px;
  align-items: center;
  text-align: left;
  cursor: pointer;
}
.left {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  font-weight: 900;
  letter-spacing: -0.2px;
}
.arrow {
  opacity: 0.5;
  font-size: 18px;
}
.badge {
  background: linear-gradient(
    135deg,
    rgb(var(--brand-red-rgb) / 0.14),
    rgb(var(--brand-blue-rgb) / 0.12)
  );
  border: 1px solid rgba(15, 23, 42, 0.08);
  color: rgba(15, 23, 42, 0.8);
  font-weight: 800;
  border-radius: 10px;
  padding: 4px 10px;
  font-size: 12px;
}
</style>

