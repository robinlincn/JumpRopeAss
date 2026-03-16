<script setup lang="ts">
import { reactive } from 'vue'
import AppHeader from '../components/AppHeader.vue'

const form = reactive({
  realName: '',
  idCardNo: '',
  mobile: '',
})

function submit() {
  alert(`已提交（示例）\n姓名：${form.realName}\n身份证：${form.idCardNo}\n手机号：${form.mobile}\n\n下一步：后台审核通过后状态变更为已认证。`)
}
</script>

<template>
  <AppHeader title="用户认证" :showBack="true" />
  <main class="app-container page page-pad">
    <div class="card card-pad">
      <div class="head">
        <div class="h-title">实名信息认证</div>
        <div class="h-sub">提交后由后台审核，通过后可作为报名与关键操作门槛</div>
      </div>

      <div class="field">
        <div class="label">真实姓名</div>
        <input v-model="form.realName" class="input" placeholder="请输入您的真实姓名" />
      </div>
      <div class="field">
        <div class="label">身份证号</div>
        <input v-model="form.idCardNo" class="input" placeholder="请输入您的身份证号" />
      </div>
      <div class="field">
        <div class="label">手机号</div>
        <input v-model="form.mobile" class="input" placeholder="请输入您的手机号" />
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
    <button class="btn" type="button" @click="submit">提交</button>
  </footer>
</template>

<style scoped>
.page {
  padding-bottom: 78px;
}
.head {
  margin-bottom: 14px;
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

