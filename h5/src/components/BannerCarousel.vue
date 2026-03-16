<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'

type BannerItem = {
  src: string
  title: string
  subtitle: string
}

const props = defineProps<{
  items: BannerItem[]
}>()

const index = ref(0)
let timer: number | undefined

function next() {
  if (!props.items.length) return
  index.value = (index.value + 1) % props.items.length
}

onMounted(() => {
  timer = window.setInterval(next, 3800)
})

onUnmounted(() => {
  if (timer) window.clearInterval(timer)
})
</script>

<template>
  <div class="wrap">
    <div
      class="track"
      :style="{
        width: `${props.items.length * 100}%`,
        transform: `translateX(-${index * (100 / Math.max(props.items.length, 1))}%)`,
      }"
    >
      <div v-for="it in props.items" :key="it.src" class="slide">
        <img class="img" :src="it.src" alt="" />
        <div class="overlay"></div>
        <div class="text">
          <div class="t1">{{ it.title }}</div>
          <div class="t2">{{ it.subtitle }}</div>
        </div>
      </div>
    </div>

    <div class="dots">
      <button
        v-for="(_, i) in props.items"
        :key="i"
        class="dot"
        :class="{ active: i === index }"
        type="button"
        @click="index = i"
      ></button>
    </div>
  </div>
</template>

<style scoped>
.wrap {
  position: relative;
  overflow: hidden;
  border-radius: 18px;
  border: 1px solid rgba(255, 255, 255, 0.22);
  background: rgba(255, 255, 255, 0.18);
}
.track {
  display: flex;
  transition: transform 520ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.slide {
  position: relative;
  width: 100%;
  min-height: 210px;
}
.img {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transform: scale(1.02);
  filter: saturate(1.06) contrast(1.04);
}
.overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(180deg, rgba(2, 6, 23, 0.08), rgba(2, 6, 23, 0.38));
}
.text {
  position: absolute;
  left: 14px;
  right: 14px;
  bottom: 14px;
  color: rgba(255, 255, 255, 0.96);
  text-shadow: 0 12px 30px rgba(2, 6, 23, 0.55);
}
.t1 {
  font-weight: 950;
  letter-spacing: -0.2px;
  font-size: 18px;
  line-height: 1.2;
}
.t2 {
  margin-top: 6px;
  font-size: 12px;
  opacity: 0.92;
  line-height: 1.45;
}
.dots {
  position: absolute;
  right: 12px;
  top: 12px;
  display: flex;
  gap: 6px;
}
.dot {
  width: 8px;
  height: 8px;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.38);
  background: rgba(255, 255, 255, 0.22);
}
.dot.active {
  width: 18px;
  background: rgba(255, 255, 255, 0.82);
  border-color: rgba(255, 255, 255, 0.65);
}
@media (min-width: 768px) {
  .slide {
    min-height: 290px;
  }
  .t1 {
    font-size: 20px;
  }
}
</style>

