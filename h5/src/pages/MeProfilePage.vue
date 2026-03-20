<script setup lang="ts">
import { computed, ref } from 'vue'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'
import { getToken } from '../lib/auth'
import { useAutoRefresh } from '../lib/useAutoRefresh'

const hasToken = computed(() => !!getToken())
const loading = ref(false)
const identity = ref<any | null>(null)

function statusText(s?: number) {
  if (s === 0) return '待审核'
  if (s === 1) return '已通过'
  if (s === 2) return '已驳回'
  return '未提交'
}

async function refresh() {
  if (!getToken()) return
  loading.value = true
  try {
    const res = await apiFetch<any>('/api/v1/app/identity/status')
    if (res.code === 0) identity.value = res.data?.latest ?? null
  } finally {
    loading.value = false
  }
}

useAutoRefresh(refresh, 30000)
</script>

<template>
  <AppHeader title="信息录入" :showBack="true" />
  <main class="app-container page page-pad">
    <div v-if="!hasToken" class="card card-pad empty">未登录：请先到“我的”进行开发登录</div>

    <section v-else class="card card-pad">
      <div class="row">
        <div class="title">实名认证状态</div>
        <button class="btn btn-ghost" type="button" @click="refresh">{{ loading ? '加载中' : '刷新' }}</button>
      </div>
      <div class="grid">
        <div class="cell">
          <div class="lbl">状态</div>
          <div class="val">{{ statusText(identity?.status) }}</div>
        </div>
        <div class="cell">
          <div class="lbl">提交时间</div>
          <div class="val">{{ String(identity?.createdAt ?? '-').replace('T', ' ') }}</div>
        </div>
        <div class="cell" v-if="identity?.rejectReason">
          <div class="lbl">驳回原因</div>
          <div class="val">{{ identity.rejectReason }}</div>
        </div>
      </div>
      <div class="tips">
        本页作为“个人资料/录入信息”的数据承接点，当前以实名认证为核心入口；后续可在同一页扩展学员/教练/裁判资料表单。
      </div>
    </section>
  </main>
</template>

<style scoped>
.row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.title {
  font-weight: 950;
  letter-spacing: -0.2px;
}
.grid {
  margin-top: 12px;
  display: grid;
  gap: 10px;
}
.cell {
  padding: 12px;
  border-radius: 14px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.6);
}
.lbl {
  font-size: 12px;
  color: rgba(15, 23, 42, 0.62);
}
.val {
  margin-top: 6px;
  font-weight: 900;
  color: rgba(15, 23, 42, 0.92);
}
.tips {
  margin-top: 12px;
  color: rgba(15, 23, 42, 0.62);
  font-size: 12px;
  line-height: 1.6;
}
.empty {
  text-align: center;
  color: rgba(15, 23, 42, 0.62);
}
</style>

