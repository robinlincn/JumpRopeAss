<script setup lang="ts">
import { useRouter } from 'vue-router'

const props = defineProps<{
  title: string
  showBack?: boolean
}>()

const router = useRouter()
</script>

<template>
  <header class="header glass">
    <button v-if="props.showBack" class="back" type="button" @click="router.back()">
      <img src="/icon-back.svg" alt="" width="22" height="22" />
    </button>
    <div class="title">{{ props.title }}</div>
    <div class="right">
      <div class="logo-wrap">
        <img class="logo" src="/logo.svg" alt="" width="22" height="22" />
      </div>
    </div>
  </header>
</template>

<style scoped>
.header {
  position: sticky;
  top: 0;
  z-index: 10;
  height: 52px;
  margin: 10px 12px 0;
  display: grid;
  grid-template-columns: 48px 1fr 48px;
  align-items: center;
  padding: 0 6px;
}
.back {
  height: 44px;
  width: 44px;
  border: 0;
  background: transparent;
  display: grid;
  place-items: center;
}
.title {
  text-align: center;
  font-size: 16px;
  font-weight: 900;
  letter-spacing: -0.2px;
}
.right {
  height: 44px;
  display: grid;
  place-items: center;
  opacity: 0.92;
}
.logo-wrap {
  width: 30px;
  height: 30px;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.22);
  display: grid;
  place-items: center;
}
.logo {
  filter: drop-shadow(0 10px 16px rgba(2, 6, 23, 0.18));
}
</style>

