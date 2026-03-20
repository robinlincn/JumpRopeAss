<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import AppTabBar from '../components/AppTabBar.vue'
import { apiFetch } from '../lib/api'

const route = useRoute()
const certNo = computed(() => String(route.query.certNo || ''))

const loading = ref(false)
const item = ref<any | null>(null)
const list = ref<any[]>([])

const mobile = ref<string>('')
const idCardNo = ref<string>('')
const certNoInput = ref<string>('')

function statusText(v: number) {
  return v === 1 ? '有效' : v === 2 ? '作废' : String(v)
}

function printCert() {
  window.print()
}

function openCert(no: string) {
  const v = String(no || '').trim()
  if (!v) return
  window.location.hash = `#/cert/verify?certNo=${encodeURIComponent(v)}`
}

async function load() {
  const no = certNo.value.trim()
  if (!no) {
    item.value = null
    return
  }
  loading.value = true
  try {
    const res = await apiFetch<any>(`/api/v1/app/certificates/by-no/${encodeURIComponent(no)}`)
    if (res.code !== 0) {
      item.value = null
      return
    }
    item.value = res.data?.item ?? null
  } finally {
    loading.value = false
  }
}

async function search() {
  const c = certNoInput.value.trim()
  const m = mobile.value.trim()
  const idn = idCardNo.value.trim()
  if (!c && !m && !idn) {
    alert('请输入手机号或身份证号或证书编号')
    return
  }
  if (c) {
    openCert(c)
    return
  }

  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/certificates/search', {
      method: 'POST',
      body: JSON.stringify({
        certNo: null,
        mobile: m || null,
        idCardNo: idn || null,
      }),
    })
    if (res.code !== 0) {
      list.value = []
      alert(res.message || '查询失败')
      return
    }
    list.value = res.data?.items ?? []
  } finally {
    loading.value = false
  }
}

watch(mobile, (v) => {
  if (String(v || '').trim()) {
    idCardNo.value = ''
    certNoInput.value = ''
  }
})

watch(idCardNo, (v) => {
  if (String(v || '').trim()) {
    mobile.value = ''
    certNoInput.value = ''
  }
})

watch(certNoInput, (v) => {
  if (String(v || '').trim()) {
    mobile.value = ''
    idCardNo.value = ''
  }
})

onMounted(() => {
  certNoInput.value = certNo.value.trim()
  load()
})
</script>

<template>
  <AppHeader title="证书查询" :showBack="true" />
  <main class="app-container page page-pad">
    <section class="card card-pad">
      <div class="head">
        <div class="h-title">证书信息</div>
        <div class="h-sub">手机号 / 身份证号 / 证书编号（三选一）查询电子证书</div>
      </div>

      <div class="form">
        <div class="field">
          <div class="label">证书编号</div>
          <input v-model="certNoInput" class="input" placeholder="例如：coach_cert-20260320-123456" />
        </div>
        <div class="split">
          <div class="hr"></div>
          <div class="or">或</div>
          <div class="hr"></div>
        </div>
        <div class="field">
          <div class="label">手机号</div>
          <input v-model="mobile" class="input" inputmode="numeric" placeholder="请输入手机号" />
        </div>
        <div class="field">
          <div class="label">身份证号</div>
          <input v-model="idCardNo" class="input" placeholder="请输入身份证号" />
        </div>
        <button class="btn2" type="button" :disabled="loading" @click="search">{{ loading ? '查询中…' : '查询' }}</button>
      </div>

      <div v-if="!loading && list.length" class="list">
        <button
          v-for="c in list"
          :key="c.certNo"
          class="row2 pressable btn-reset"
          type="button"
          @click="openCert(String(c.certNo))"
        >
          <div class="r1">
            <div class="r-title">{{ c.certTypeName || '证书' }}</div>
            <div class="r-sub">{{ String(c.issueAt || '').replace('T', ' ') || '-' }}</div>
          </div>
          <div class="r2">
            <div class="r-no">{{ c.certNo }}</div>
            <div class="r-st">{{ statusText(Number(c.status ?? 0)) }}</div>
          </div>
        </button>
      </div>

      <div v-if="certNo && loading" class="empty">加载中…</div>
      <div v-else-if="certNo && !item" class="empty">未查询到证书</div>
      <div v-else-if="certNo && item" class="kv">
        <div class="row">
          <div class="k">类型</div>
          <div class="v">{{ item.certTypeName || '-' }}</div>
        </div>
        <div class="row">
          <div class="k">持证人</div>
          <div class="v">{{ item.holderName || '-' }}</div>
        </div>
        <div class="row">
          <div class="k">手机号</div>
          <div class="v">{{ item.holderMobileMasked || '-' }}</div>
        </div>
        <div class="row">
          <div class="k">身份证号</div>
          <div class="v">{{ item.holderIdCardNoMasked || '-' }}</div>
        </div>
        <div class="row">
          <div class="k">发证时间</div>
          <div class="v">{{ String(item.issueAt || '').replace('T', ' ') || '-' }}</div>
        </div>
        <div class="row">
          <div class="k">状态</div>
          <div class="v">{{ statusText(Number(item.status ?? 0)) }}</div>
        </div>
        <div class="row" v-if="item.fileUrl">
          <div class="k">文件</div>
          <div class="v">
            <a :href="item.fileUrl" target="_blank" rel="noreferrer">{{ item.fileUrl }}</a>
          </div>
        </div>
      </div>
    </section>
  </main>

  <footer v-if="certNo" class="bottom">
    <button class="btn" type="button" @click="printCert">打印</button>
  </footer>

  <AppTabBar />
</template>

<style scoped>
.page {
  padding-bottom: calc(78px + 66px);
}
.head {
  margin-bottom: 12px;
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
.value {
  font-weight: 900;
}
.kv {
  margin-top: 12px;
  display: grid;
  gap: 10px;
}
.row {
  display: grid;
  grid-template-columns: 74px 1fr;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.65);
}
.k {
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
  font-weight: 900;
}
.v {
  color: rgba(15, 23, 42, 0.92);
  font-weight: 900;
  overflow: hidden;
  text-overflow: ellipsis;
}
.empty {
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
  font-size: 13px;
  padding: 18px 0;
}
.form {
  margin-bottom: 10px;
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
.btn2 {
  width: 100%;
  height: 44px;
  border-radius: 14px;
}
.split {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  align-items: center;
  gap: 10px;
  margin: 6px 0 14px;
}
.hr {
  height: 1px;
  background: rgba(15, 23, 42, 0.08);
}
.or {
  font-size: 12px;
  font-weight: 900;
  color: rgba(15, 23, 42, 0.55);
}
.list {
  margin-top: 12px;
  display: grid;
  gap: 10px;
}
.row2 {
  width: 100%;
  text-align: left;
  padding: 12px;
  border-radius: 16px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.6);
  display: grid;
  gap: 10px;
}
.r1 {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: baseline;
  flex-wrap: wrap;
}
.r-title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.r-sub {
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
}
.r2 {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: center;
  flex-wrap: wrap;
}
.r-no {
  font-weight: 900;
  color: rgba(15, 23, 42, 0.86);
}
.r-st {
  font-size: 12px;
  color: rgba(15, 23, 42, 0.62);
}
.bottom {
  position: fixed;
  left: 12px;
  right: 12px;
  bottom: calc(12px + 66px + env(safe-area-inset-bottom, 0px));
  height: 66px;
  background: rgba(255, 255, 255, 0.76);
  border: 1px solid rgba(15, 23, 42, 0.10);
  border-radius: 18px;
  box-shadow: 0 18px 44px rgba(2, 6, 23, 0.18);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
  z-index: 1001;
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
