<template>
  <div class="app-desktop-shell">
    <router-view v-slot="{ Component, route }">
      <transition name="page" mode="out-in">
        <div :key="route.fullPath">
          <component :is="Component" />
        </div>
      </transition>
    </router-view>
  </div>
</template>
