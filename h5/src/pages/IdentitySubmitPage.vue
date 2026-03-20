<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { getToken } from '../lib/auth'

const token = ref<string | null>(getToken())

const form = reactive({
  realName: '',
  idCardNo: '',
  mobile: '',
})

const submitting = ref(false)
const loading = ref(false)
const latest = ref<any | null>(null)

const isApproved = computed(() => Number(latest.value?.status ?? -1) === 1)
const maskedIdCard = computed(() => {
  const v = String(latest.value?.idCardNo ?? '')
  if (v.length < 8) return v
  return `${v.slice(0, 4)}**********${v.slice(-4)}`
})

async function fetchStatus() {
  token.value = getToken()
  if (!token.value) return
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/identity/status')
    if (res.code !== 0) return
    latest.value = res.data?.latest ?? null
    if (isApproved.value && latest.value) {
      form.realName = String(latest.value.realName || '')
      form.mobile = String(latest.value.mobile || '')
      form.idCardNo = String(latest.value.idCardNo || '')
      try {
        localStorage.setItem(
          'h5_identity',
          JSON.stringify({
            realName: latest.value.realName,
            mobile: latest.value.mobile,
            idCardNo: latest.value.idCardNo,
          }),
        )
      } catch {
      }
    }
  } finally {
    loading.value = false
  }
}

async function submit() {
  token.value = getToken()
  if (!token.value) {
    alert('未登录：请先到“我的”进行开发登录，再提交认证')
    return
  }
  if (isApproved.value) {
    alert('已实名认证通过，无需重复提交')
    return
  }
  if (!form.realName.trim() || !form.idCardNo.trim() || !form.mobile.trim()) {
    alert('请完整填写信息')
    return
  }
  submitting.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/identity/submit', {
      method: 'POST',
      body: JSON.stringify({
        realName: form.realName,
        idCardNo: form.idCardNo,
        mobile: form.mobile,
      }),
    })
    if (res.code !== 0) {
      alert(res.message || '提交失败')
      return
    }
    alert('提交成功：已进入待审核状态')
    await fetchStatus()
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  fetchStatus()
})
</script>

<template>
  <AppHeader title="用户认证" :showBack="true" />
  <main class="app-container page page-pad">
    <div v-if="!token" class="card card-pad notice">
      未登录：请先到“我的”完成登录后再查看/提交认证
    </div>

    <div v-if="isApproved && latest" class="card card-pad approved">
      <div class="head">
        <div class="h-title">已实名认证</div>
        <div class="h-sub">该信息将用于报名与证书发放</div>
      </div>
      <div class="kv">
        <div class="k">姓名</div>
        <div class="v">{{ latest.realName }}</div>
      </div>
      <div class="kv">
        <div class="k">手机号</div>
        <div class="v">{{ latest.mobile }}</div>
      </div>
      <div class="kv">
        <div class="k">身份证号</div>
        <div class="v">{{ maskedIdCard }}</div>
      </div>
      <div class="tips">
        <span class="chip">
          <img src="/icon-user.svg" alt="" width="16" height="16" />
          <span>如需修改，请联系管理员处理</span>
        </span>
      </div>
    </div>

    <div class="card card-pad">
      <div class="head">
        <div class="h-title row">
          <span>实名信息认证</span>
          <button class="btn-reset more" type="button" @click="fetchStatus" :disabled="loading">刷新状态</button>
        </div>
        <div class="h-sub">提交后由后台审核，通过后可作为报名与关键操作门槛</div>
      </div>

      <div class="field">
        <div class="label">真实姓名</div>
        <input v-model="form.realName" class="input" placeholder="请输入您的真实姓名" :disabled="isApproved" />
      </div>
      <div class="field">
        <div class="label">身份证号</div>
        <input v-model="form.idCardNo" class="input" placeholder="请输入您的身份证号" :disabled="isApproved" />
      </div>
      <div class="field">
        <div class="label">手机号</div>
        <input v-model="form.mobile" class="input" placeholder="请输入您的手机号" :disabled="isApproved" />
      </div>
      <div class="tips">
        <span class="chip">
          <img src="/icon-user.svg" alt="" width="16" height="16" />
          <span>信息仅用于身份核验与证书发放</span>
        </span>
      </div>
    </div>
  </main>

  <footer class="bottom">
    <button class="btn" type="button" @click="submit" :disabled="submitting || loading || isApproved">
      {{ isApproved ? '已通过' : submitting ? '提交中…' : loading ? '加载中…' : '提交' }}
    </button>
  </footer>
</template>

<style scoped>
.page {
  padding-bottom: 78px;
}
.approved {
  margin-bottom: 12px;
  border-color: rgba(31, 110, 201, 0.18);
  background: linear-gradient(135deg, rgb(var(--brand-blue-rgb) / 0.10), rgb(var(--brand-green-rgb) / 0.08));
}
.kv {
  display: grid;
  grid-template-columns: 74px 1fr;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.65);
  margin-top: 10px;
}
.k {
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
  font-weight: 900;
}
.v {
  color: rgba(15, 23, 42, 0.92);
  font-weight: 900;
}
.head {
  margin-bottom: 14px;
}
.row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.notice {
  margin-bottom: 12px;
  color: rgba(15, 23, 42, 0.72);
  font-size: 13px;
  line-height: 1.6;
}
.more {
  font-weight: 900;
  color: rgba(15, 23, 42, 0.62);
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.55);
}
.field {
  margin-bottom: 12px;
}
.label {
  font-size: 13px;
  font-weight: 900;
  letter-spacing: -0.2px;
  margin-bottom: 6px;
}
.input {
  width: 100%;
  height: 44px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.10);
  background: rgba(255, 255, 255, 0.82);
  padding: 0 12px;
  font-size: 14px;
  outline: none;
}
.input:focus {
  border-color: rgb(var(--brand-blue-rgb) / 0.45);
  box-shadow: 0 0 0 4px rgb(var(--brand-blue-rgb) / 0.14);
}
.tips {
  margin-top: 8px;
}
.bottom {
  position: fixed;
  left: 12px;
  right: 12px;
  bottom: 12px;
  height: 66px;
  background: rgba(255, 255, 255, 0.76);
  border: 1px solid rgba(15, 23, 42, 0.10);
  border-radius: 18px;
  box-shadow: 0 18px 44px rgba(2, 6, 23, 0.18);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
  display: grid;
  place-items: center;
  padding: 0 12px;
}
.btn {
  width: 100%;
  height: 44px;
  border-radius: 14px;
  padding: 0;
}
</style>

