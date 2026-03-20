<script setup lang="ts">
import { useRouter } from 'vue-router'
import { computed, ref } from 'vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'
import { clearToken, getToken, getUserIdFromToken, setToken } from '../lib/auth'
import { useAutoRefresh } from '../lib/useAutoRefresh'
import logoUrl from '../assets/logo.png'

const router = useRouter()

const token = ref<string | null>(getToken())
const identity = ref<any | null>(null)
const entryTotal = ref(0)
const entryPending = ref(0)
const entryWaitPay = ref(0)
const latestEntries = ref<any[]>([])
const notice = ref<string>('')
const devUserId = ref('990000')
const loginOpen = ref(false)
const loggingIn = ref(false)
const refreshing = ref(false)

const userIdText = computed(() => getUserIdFromToken(token.value))

const identityBadge = computed(() => {
  const s = identity.value?.latest?.status
  if (s === 0) return '待审核'
  if (s === 1) return '已通过'
  if (s === 2) return '已驳回'
  return token.value ? '未提交' : '未登录'
})

const maskedIdCard = computed(() => {
  const v = String(identity.value?.latest?.idCardNo ?? '')
  if (v.length < 8) return v
  return `${v.slice(0, 4)}**********${v.slice(-4)}`
})

function openLogin() {
  notice.value = ''
  loginOpen.value = true
}

function logout() {
  clearToken()
  token.value = null
  identity.value = null
  entryTotal.value = 0
  entryPending.value = 0
  entryWaitPay.value = 0
  latestEntries.value = []
  notice.value = '已退出登录'
}

async function devLogin() {
  notice.value = ''
  loggingIn.value = true
  try {
    const uid = Number(devUserId.value)
    if (!Number.isFinite(uid) || uid <= 0) {
      notice.value = '请输入合法的开发登录 UserId'
      return
    }

    const res = await apiFetch<any>('/api/v1/app/auth/dev-login', { method: 'POST', body: JSON.stringify({ userId: uid }) })
    if (res.code !== 0) {
      notice.value = res.message || '登录失败'
      return
    }
    setToken(res.data?.token)
    token.value = getToken()
    loginOpen.value = false
    notice.value = '登录成功'
    await refresh()
  } finally {
    loggingIn.value = false
  }
}

async function refresh() {
  token.value = getToken()
  if (!token.value) {
    identity.value = null
    entryTotal.value = 0
    entryPending.value = 0
    entryWaitPay.value = 0
    latestEntries.value = []
    return
  }

  refreshing.value = true
  try {
    const [me, i, e] = await Promise.all([
      apiFetch<any>('/api/v1/app/me'),
      apiFetch<any>('/api/v1/app/identity/status'),
      apiFetch<any>('/api/v1/app/me/entries?page=1&pageSize=50'),
    ])

    if (me.code !== 0) {
      logout()
      notice.value = '登录已失效，请重新登录'
      return
    }

    if (i.code === 0) identity.value = i.data
    if (i.code === 0 && i.data?.latest?.status === 1) {
      try {
        localStorage.setItem(
          'h5_identity',
          JSON.stringify({
            realName: i.data.latest.realName,
            mobile: i.data.latest.mobile,
            idCardNo: i.data.latest.idCardNo,
          }),
        )
      } catch {
      }
    }

    if (e.code === 0) {
      const list: any[] = e.data?.items ?? []
      entryTotal.value = Number(e.data?.total ?? list.length)
      entryPending.value = list.filter((x) => Number(x.status) === 0).length
      entryWaitPay.value = list.filter((x) => Number(x.status) === 2).length
      latestEntries.value = list.slice(0, 3)
    }
  } finally {
    refreshing.value = false
  }
}

useAutoRefresh(refresh, 30000)

function ensureLogin(go: () => void) {
  if (!getToken()) {
    openLogin()
    notice.value = '此功能需要登录，请先完成开发登录'
    return
  }
  go()
}
</script>

<template>
  <main class="app-container page page-pad">
    <section class="profile glass">
      <div class="profile-top">
        <div class="avatar">
          <img class="logo" :src="logoUrl" alt="" width="34" height="34" />
        </div>
        <div class="info">
          <div class="name">个人中心</div>
          <div class="sub">
            <span v-if="token">已登录（UserId：{{ userIdText || '-' }}）</span>
            <span v-else>未登录（需要登录后才能报名/提交认证/查看我的活动）</span>
          </div>
        </div>
        <button v-if="!token" class="chip" type="button" @click="openLogin()">登录</button>
        <button v-else class="chip" type="button" @click="refresh()" :disabled="refreshing">{{ refreshing ? '刷新中' : '刷新' }}</button>
      </div>

      <div v-if="notice" class="notice">{{ notice }}</div>

      <div class="counts">
        <div class="count card">
          <div class="num">{{ entryTotal }}</div>
          <div class="lbl">我的报名</div>
        </div>
        <div class="count card">
          <div class="num">{{ entryPending }}</div>
          <div class="lbl">待审核</div>
        </div>
        <div class="count card">
          <div class="num">{{ entryWaitPay }}</div>
          <div class="lbl">待缴费</div>
        </div>
      </div>

      <div v-if="token" class="identity-line">
        <span class="chip">实名认证：{{ identityBadge }}</span>
        <span v-if="identity?.latest?.status === 1" class="chip">姓名：{{ identity.latest.realName }}</span>
        <span v-if="identity?.latest?.status === 1" class="chip">手机：{{ identity.latest.mobile }}</span>
        <span v-if="identity?.latest?.status === 1" class="chip">身份证：{{ maskedIdCard }}</span>
        <span v-if="identity?.latest?.rejectReason" class="chip">原因：{{ identity.latest.rejectReason }}</span>
        <button class="chip-btn pressable btn-reset" type="button" @click="router.push('/me/identity')">去认证</button>
        <button class="chip-btn pressable btn-reset" type="button" @click="logout()">退出</button>
      </div>
    </section>

    <section v-if="token" class="card card-pad mini-list">
      <div class="mini-head">
        <div class="mini-title">最近报名</div>
        <button class="btn-reset more" type="button" @click="router.push('/me/activity')">查看全部</button>
      </div>
      <div v-if="!latestEntries.length" class="mini-empty">暂无报名记录</div>
      <button
        v-for="it in latestEntries"
        :key="it.id"
        class="mini-row btn-reset pressable"
        type="button"
        @click="router.push(`/events/${it.eventId}`)"
      >
        <div class="m1">{{ it.eventTitle || `活动#${it.eventId}` }}</div>
        <div class="m2">
          <span class="chip">{{ it.status }}</span>
          <span class="meta">{{ String(it.createdAt ?? '').replace('T', ' ') }}</span>
        </div>
      </button>
    </section>

    <section class="menu">
      <button class="row glass pressable" type="button" @click="ensureLogin(() => router.push('/me/profile'))">
        <span class="left">
          <img src="/icon-grid.svg" alt="" width="22" height="22" />
          <span>信息录入</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass pressable" type="button" @click="ensureLogin(() => router.push('/me/org'))">
        <span class="left">
          <img src="/icon-grid.svg" alt="" width="22" height="22" />
          <span>绑定机构</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass pressable" type="button" @click="ensureLogin(() => router.push('/me/activity'))">
        <span class="left">
          <img src="/icon-trophy.svg" alt="" width="22" height="22" />
          <span>我的活动</span>
        </span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass pressable" type="button" @click="ensureLogin(() => router.push('/me/identity'))">
        <span class="left">
          <img src="/icon-user.svg" alt="" width="22" height="22" />
          <span>我的认证</span>
        </span>
        <span class="badge">{{ identityBadge }}</span>
        <span class="arrow">›</span>
      </button>
      <button class="row glass pressable" type="button" @click="ensureLogin(() => router.push('/me/home'))">
        <span class="left">
          <img src="/icon-news.svg" alt="" width="22" height="22" />
          <span>我的主页</span>
        </span>
        <span class="arrow">›</span>
      </button>
    </section>

    <div v-if="loginOpen" class="mask" @click="loginOpen = false">
      <div class="sheet" @click.stop>
        <div class="sheet-title">开发登录</div>
        <div class="sheet-sub">仅用于本地联调：后端 Development 环境可用</div>

        <div class="field">
          <div class="field-label">UserId</div>
          <input v-model="devUserId" class="input" inputmode="numeric" placeholder="例如：990000" />
        </div>

        <div class="actions">
          <button class="btn btn-ghost" type="button" @click="loginOpen = false">取消</button>
          <button class="btn" type="button" :disabled="loggingIn" @click="devLogin">{{ loggingIn ? '登录中…' : '登录' }}</button>
        </div>
      </div>
    </div>
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
.notice {
  margin-top: 10px;
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.65);
  color: rgba(15, 23, 42, 0.72);
  font-size: 12px;
  line-height: 1.6;
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
.identity-line {
  margin-top: 12px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.chip-btn {
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.72);
  font-size: 12px;
  font-weight: 900;
  color: rgba(15, 23, 42, 0.76);
}
.mini-list {
  margin-top: 14px;
}
.mini-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 10px;
}
.mini-title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.more {
  font-weight: 900;
  color: rgba(15, 23, 42, 0.62);
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.55);
}
.mini-row {
  width: 100%;
  text-align: left;
  margin-top: 10px;
  padding: 12px;
  border-radius: 16px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.6);
}
.m1 {
  font-weight: 900;
  letter-spacing: -0.2px;
  line-height: 1.3;
}
.m2 {
  margin-top: 8px;
  display: flex;
  justify-content: space-between;
  gap: 10px;
  flex-wrap: wrap;
  align-items: center;
}
.meta {
  color: rgba(15, 23, 42, 0.6);
  font-size: 12px;
}
.mini-empty {
  margin-top: 10px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
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
.mask {
  position: fixed;
  inset: 0;
  background: rgba(2, 6, 23, 0.42);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  display: grid;
  align-items: end;
  padding: 12px;
  z-index: 50;
}
.sheet {
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.22);
  background: rgba(255, 255, 255, 0.86);
  box-shadow: 0 22px 66px rgba(2, 6, 23, 0.30);
  padding: 14px;
}
.sheet-title {
  font-weight: 950;
  letter-spacing: -0.2px;
  font-size: 16px;
}
.sheet-sub {
  margin-top: 4px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
}
.field {
  margin-top: 12px;
}
.field-label {
  font-size: 12px;
  font-weight: 900;
  letter-spacing: -0.2px;
  color: rgba(15, 23, 42, 0.72);
}
.input {
  margin-top: 6px;
  width: 100%;
  height: 42px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.86);
  padding: 0 12px;
  outline: none;
}
.actions {
  margin-top: 14px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
</style>

