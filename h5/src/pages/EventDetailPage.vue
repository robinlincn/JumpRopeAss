<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { getToken } from '../lib/auth'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const route = useRoute()
const router = useRouter()

const id = computed(() => String(route.params.id || ''))

const loading = ref(false)
const detail = ref<any | null>(null)
const groups = ref<{ id: number; code: string; name: string; feeAmount: number; quota: number }[]>([])
const myEntry = ref<any | null>(null)
const enrollOpen = ref(false)
const paying = ref(false)

const selectedGroupId = ref<number | null>(null)
const enrollChannel = ref<1 | 2 | 3>(1)
const athleteName = ref<string>('')
const athleteMobile = ref<string>('')
const athleteGender = ref<0 | 1 | 2>(0)
const athleteIdCardNo = ref<string>('')
const athleteBirthday = ref<string>('')

function dateText(v?: string | null) {
  if (!v) return '-'
  return String(v).slice(0, 10)
}

function feeText(fen: number) {
  return `¥${(Number(fen) / 100).toFixed(2)}`
}

function signupText() {
  return `${dateText(detail.value?.signupStartAt)} ~ ${dateText(detail.value?.signupEndAt)}`
}

function entryStatusText(s?: number) {
  const m: Record<number, string> = {
    0: '待审核',
    1: '已驳回',
    2: '待缴费',
    3: '支付中',
    4: '已缴费',
    5: '已确认',
    6: '已取消',
    7: '支付失败',
    8: '退款中',
    9: '已退款',
  }
  if (typeof s !== 'number') return '未报名'
  return m[s] ?? String(s)
}

function primaryBtnText() {
  const s = Number(myEntry.value?.status ?? -1)
  if (!myEntry.value) return '立即报名'
  if (s === 0) return '等待审核'
  if (s === 1) return '重新报名'
  if (s === 2 || s === 3) return '去支付'
  if (s === 4 || s === 5) return '报名成功'
  return '查看报名'
}

function canPrimaryClick() {
  const s = Number(myEntry.value?.status ?? -1)
  if (!myEntry.value) return true
  if (s === 1) return true
  if (s === 2 || s === 3) return true
  return false
}

function initForm() {
  const idn = (() => {
    try {
      return JSON.parse(localStorage.getItem('h5_identity') || 'null')
    } catch {
      return null
    }
  })() as any | null

  const cached = (() => {
    try {
      return JSON.parse(localStorage.getItem('h5_enroll_athlete') || '{}')
    } catch {
      return {}
    }
  })() as any

  const preferredName = String(cached.name || idn?.realName || '')
  const preferredMobile = String(cached.mobile || idn?.mobile || '')
  const preferredIdCard = String(cached.idCardNo || idn?.idCardNo || '')

  athleteName.value = preferredName
  athleteMobile.value = preferredMobile
  athleteGender.value = Number(cached.gender || 0) as any
  athleteIdCardNo.value = preferredIdCard
  athleteBirthday.value = String(cached.birthday || '')
  enrollChannel.value = Number(cached.channel || 1) as any

  if (!selectedGroupId.value && groups.value.length) selectedGroupId.value = groups.value[0].id
}

function persistForm() {
  try {
    localStorage.setItem(
      'h5_enroll_athlete',
      JSON.stringify({
        name: athleteName.value,
        mobile: athleteMobile.value,
        gender: athleteGender.value,
        idCardNo: athleteIdCardNo.value,
        birthday: athleteBirthday.value,
        channel: enrollChannel.value,
      }),
    )
  } catch {
  }
}

async function refresh() {
  loading.value = true
  try {
    const reqs: Promise<any>[] = [
      apiFetch<any>(`/api/v1/app/events/${id.value}`),
      apiFetch<any>(`/api/v1/app/events/${id.value}/groups`),
    ]
    if (getToken()) reqs.push(apiFetch<any>(`/api/v1/app/events/${id.value}/my-entry`))
    const [d, g, m] = await Promise.all(reqs)
    if (d?.code === 0) detail.value = d.data
    if (g?.code === 0) groups.value = (g.data?.items ?? []).map((x: any) => ({ id: Number(x.id), code: String(x.code), name: String(x.name), feeAmount: Number(x.feeAmount), quota: Number(x.quota) }))
    if (m?.code === 0) myEntry.value = m.data?.item ?? null
    if (!selectedGroupId.value && groups.value.length) selectedGroupId.value = groups.value[0].id
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh)

async function submitEnroll() {
  if (!getToken()) {
    alert('未登录：请先到“我的”进行开发登录')
    router.push('/me')
    return
  }
  if (!selectedGroupId.value) {
    alert('请选择组别')
    return
  }
  if (!athleteName.value.trim()) {
    alert('请填写姓名')
    return
  }

  persistForm()
  paying.value = true
  try {
    const res = await apiFetch<any>(`/api/v1/app/events/${id.value}/entries`, {
      method: 'POST',
      body: JSON.stringify({
        groupId: selectedGroupId.value,
        enrollChannel: enrollChannel.value,
        athlete: {
          fullName: athleteName.value,
          mobile: athleteMobile.value || null,
          gender: athleteGender.value,
          idCardNo: athleteIdCardNo.value || null,
          birthday: athleteBirthday.value || null,
        },
      }),
    })
    if (res.code !== 0) {
      alert(res.message || '报名失败')
      return
    }
    enrollOpen.value = false
    await refresh()

    const entryStatus = Number(res.data?.entry?.status ?? -1)
    if (entryStatus === 0) {
      alert('报名已提交：等待后台审核通过后即可缴费')
      return
    }
    if (entryStatus === 3 && res.data?.payOrder?.id) {
      const ok = confirm(`已生成订单：金额 ${feeText(Number(res.data.payOrder.amount))}\n是否立即支付（开发环境模拟）？`)
      if (!ok) return
      await devPay(Number(res.data.payOrder.id))
    }
  } finally {
    paying.value = false
  }
}

async function createPayOrder() {
  if (!getToken()) {
    alert('未登录：请先到“我的”进行开发登录')
    router.push('/me')
    return
  }
  const entryId = Number(myEntry.value?.id ?? 0)
  if (!entryId) return
  paying.value = true
  try {
    const existingOrderId = Number(myEntry.value?.payOrderId ?? 0)
    if (existingOrderId) {
      const ok = confirm('已存在待支付订单，是否立即支付（开发环境模拟）？')
      if (!ok) return
      await devPay(existingOrderId)
      return
    }
    const res = await apiFetch<any>(`/api/v1/app/event-entries/${entryId}/pay-order`, { method: 'POST', body: '{}' })
    if (res.code !== 0) {
      alert(res.message || '生成订单失败')
      return
    }
    const payOrderId = Number(res.data?.payOrder?.id ?? res.data?.payOrderId ?? 0)
    if (!payOrderId) return
    const ok = confirm(`已生成订单：金额 ${feeText(Number(res.data?.payOrder?.amount ?? 0))}\n是否立即支付（开发环境模拟）？`)
    if (!ok) return
    await devPay(payOrderId)
  } finally {
    paying.value = false
  }
}

async function devPay(orderId: number) {
  const res = await apiFetch<any>(`/api/v1/app/pay-orders/${orderId}/dev-pay`, { method: 'POST', body: '{}' })
  if (res.code !== 0) {
    alert(res.message || '支付失败')
    return
  }
  alert('支付成功：报名已缴费')
  await refresh()
}

function openEnroll() {
  initForm()
  if (!groups.value.length) {
    alert('当前活动暂无可报名组别，请联系管理员先维护活动组别')
    return
  }
  enrollOpen.value = true
}
</script>

<template>
  <AppHeader title="赛事详情" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="cover glass">
      <img class="cover-img" :src="detail?.bannerUrl || detail?.coverUrl || '/hero-jumprope.svg'" alt="" />
      <div class="cover-overlay">
        <div class="cover-chip">
          <img src="/icon-trophy.svg" alt="" width="20" height="20" />
          <span>{{ detail?.eventType === 2 ? '评定' : detail?.eventType === 3 ? '培训' : '赛事' }} · {{ detail?.status === 1 ? '开放' : '未开放' }}</span>
        </div>
        <div class="cover-title">{{ detail?.title || '加载中…' }}</div>
        <div class="cover-meta">
          <span class="chip">报名：{{ signupText() }}</span>
          <span class="chip">比赛：{{ dateText(detail?.eventDate || detail?.eventStartAt) }}</span>
        </div>
      </div>
    </section>

    <section class="card card-pad">
      <div class="block">
        <div class="label">报名时间</div>
        <div class="value">{{ signupText() }}</div>
      </div>
      <div class="block">
        <div class="label">比赛时间</div>
        <div class="value">{{ dateText(detail?.eventDate || detail?.eventStartAt) }}</div>
      </div>
      <div class="block">
        <div class="label">参赛组别</div>
        <div class="value">
          <div v-for="g in groups" :key="g.id" class="group">
            <span class="chip">{{ g.code }}</span>
            <span>{{ g.name }}</span>
            <span class="chip">{{ feeText(g.feeAmount) }}</span>
          </div>
        </div>
      </div>
      <div class="block">
        <div class="label">比赛地点</div>
        <div class="value">{{ detail?.location || '-' }}</div>
      </div>
      <div class="block" v-if="myEntry">
        <div class="label">我的报名</div>
        <div class="value">
          <div class="enroll-row">
            <span class="chip">{{ entryStatusText(Number(myEntry?.status)) }}</span>
            <span class="chip" v-if="myEntry?.groupName">组别：{{ myEntry.groupName }}</span>
            <span class="chip" v-if="myEntry?.athleteName">选手：{{ myEntry.athleteName }}</span>
          </div>
          <div class="tips" v-if="myEntry?.auditRemark">备注：{{ myEntry.auditRemark }}</div>
        </div>
      </div>
      <div class="block" v-if="detail?.descriptionHtml">
        <div class="label">活动说明</div>
        <div class="value html" v-html="detail.descriptionHtml"></div>
      </div>
    </section>
  </main>

  <div v-if="enrollOpen" class="mask" @click="enrollOpen = false">
    <div class="sheet" @click.stop>
      <div class="sheet-title">报名信息</div>
      <div class="sheet-sub">提交后进入审核与缴费流程</div>

      <div class="field">
        <div class="field-label">组别</div>
        <div class="group-grid">
          <button
            v-for="g in groups"
            :key="g.id"
            type="button"
            class="group-opt pressable btn-reset"
            :class="{ active: selectedGroupId === g.id }"
            @click="selectedGroupId = g.id"
          >
            <div class="g1">
              <span class="chip">{{ g.code }}</span>
              <span class="gname">{{ g.name }}</span>
            </div>
            <div class="g2">
              <span class="meta">报名费</span>
              <span class="price">{{ feeText(g.feeAmount) }}</span>
            </div>
          </button>
        </div>
      </div>

      <div class="field">
        <div class="field-label">报名身份</div>
        <div class="chips">
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: enrollChannel === 1 }" @click="enrollChannel = 1">运动员</button>
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: enrollChannel === 2 }" @click="enrollChannel = 2">家长</button>
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: enrollChannel === 3 }" @click="enrollChannel = 3">教练</button>
        </div>
      </div>

      <div class="field">
        <div class="field-label">姓名</div>
        <input v-model="athleteName" class="input" placeholder="请输入运动员姓名" />
      </div>
      <div class="field">
        <div class="field-label">手机号</div>
        <input v-model="athleteMobile" class="input" placeholder="用于联系（必填）" />
      </div>
      <div class="field">
        <div class="field-label">性别</div>
        <div class="chips">
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: athleteGender === 0 }" @click="athleteGender = 0">未知</button>
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: athleteGender === 1 }" @click="athleteGender = 1">男</button>
          <button type="button" class="chip-btn pressable btn-reset" :class="{ active: athleteGender === 2 }" @click="athleteGender = 2">女</button>
        </div>
      </div>
      <div class="field">
        <div class="field-label">身份证号</div>
        <input v-model="athleteIdCardNo" class="input" placeholder="用于核验/证书发放（可选）" />
      </div>
      <div class="field">
        <div class="field-label">出生日期</div>
        <input v-model="athleteBirthday" class="input" type="date" />
      </div>

      <div class="actions">
        <button class="btn btn-ghost" type="button" @click="enrollOpen = false">取消</button>
        <button class="btn" type="button" :disabled="paying || !groups.length" @click="submitEnroll">{{ paying ? '提交中…' : '提交报名' }}</button>
      </div>
    </div>
  </div>

  <footer class="bottom">
    <div class="left">{{ id }}号 · {{ Number(detail?.entryCount ?? 0) }}人参与</div>
    <button
      class="btn"
      type="button"
      :disabled="!canPrimaryClick() || paying"
      @click="() => {
        if (!myEntry) openEnroll()
        else if (Number(myEntry?.status) === 1) openEnroll()
        else if (Number(myEntry?.status) === 2 || Number(myEntry?.status) === 3) createPayOrder()
      }"
    >
      {{ paying ? '处理中…' : primaryBtnText() }}
    </button>
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
.value.html :deep(p) {
  margin: 0 0 10px;
}
.value.html :deep(img) {
  max-width: 100%;
  border-radius: 12px;
  border: 1px solid rgba(15, 23, 42, 0.10);
}
.group {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 8px;
}
.enroll-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.tips {
  margin-top: 8px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
  line-height: 1.6;
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
.chips {
  margin-top: 6px;
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.chip-btn {
  padding: 8px 10px;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.72);
  font-size: 12px;
  color: rgba(15, 23, 42, 0.78);
  font-weight: 800;
}
.chip-btn.active {
  border-color: rgba(31, 110, 201, 0.30);
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb) / 0.16), rgb(var(--brand-green-rgb) / 0.12));
  color: rgba(15, 23, 42, 0.92);
}
.group-grid {
  margin-top: 8px;
  display: grid;
  gap: 10px;
}
.group-opt {
  padding: 12px;
  border-radius: 18px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.72);
  text-align: left;
}
.group-opt.active {
  border-color: rgba(31, 110, 201, 0.26);
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb) / 0.14), rgb(var(--brand-green-rgb) / 0.10));
}
.g1 {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.gname {
  font-weight: 900;
  letter-spacing: -0.2px;
}
.g2 {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.price {
  font-weight: 950;
  color: rgba(15, 23, 42, 0.92);
}
.actions {
  margin-top: 14px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
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

