<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import { apiFetch } from '../lib/api'

const route = useRoute()

const pageTitle = computed(() => (route.meta.title as string) || '人员列表')
const type = computed(() => (route.meta.type as string) || 'judges')

const searchKeyword = ref('')
const activeFilter = ref<'area' | 'level' | 'project' | null>(null)

// filter selected values
const selectedArea = ref('全部地区')
const selectedLevel = ref('全部等级')
const selectedProject = ref('全部项目')

const items = ref<any[]>([])
const loading = ref(false)
const finished = ref(false)
let page = 1
let pageSize = 15
const sentinelEl = ref<HTMLDivElement | null>(null)
let observer: IntersectionObserver | null = null

const levelOptions = ['全部等级', '一级', '二级', '三级', '国家级']
const projectOptions = ['全部项目', '跳绳', '花样跳绳', '速度跳绳']
const areaOptions = ['全部地区', '长沙', '株洲', '湘潭', '衡阳', '邵阳', '岳阳']

function toggleFilter(filter: 'area' | 'level' | 'project') {
  if (activeFilter.value === filter) {
    activeFilter.value = null
  } else {
    activeFilter.value = filter
  }
}

function selectFilter(filter: 'area' | 'level' | 'project', val: string) {
  if (filter === 'area') selectedArea.value = val
  if (filter === 'level') selectedLevel.value = val
  if (filter === 'project') selectedProject.value = val
  activeFilter.value = null
  onSearch()
}

function closeFilter() {
  activeFilter.value = null
}

function onSearch() {
  page = 1
  items.value = []
  finished.value = false
  fetchData()
}

async function fetchData() {
  if (loading.value || finished.value) return
  loading.value = true
  try {
    let url = `/api/v1/app/${type.value}?page=${page}&pageSize=${pageSize}`
    if (searchKeyword.value.trim()) {
      url += `&keyword=${encodeURIComponent(searchKeyword.value.trim())}`
    }
    if (selectedLevel.value !== '全部等级') {
      url += `&level=${encodeURIComponent(selectedLevel.value)}`
    }
    
    const data = await apiFetch<any>(url)
    const list: any[] = data.data?.items ?? []
    if (list.length === 0) {
      finished.value = true
    } else {
      items.value.push(...list)
      page += 1
    }
  } catch (e) {
    finished.value = true
  } finally {
    loading.value = false
  }
}

watch(() => route.path, () => {
  onSearch()
})

onMounted(() => {
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((e) => e.isIntersecting)) {
        fetchData()
      }
    },
    { rootMargin: '200px' },
  )
  if (sentinelEl.value) observer.observe(sentinelEl.value)
})
onBeforeUnmount(() => {
  if (observer) observer.disconnect()
})
</script>

<template>
  <div class="page-container" @click="closeFilter">
    <AppHeader :title="pageTitle" :show-back="true" />

    <!-- Search Bar -->
    <div class="search-bar glass">
      <div class="search-input-wrap">
        <svg class="search-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"></circle>
          <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
        </svg>
        <input 
          v-model="searchKeyword" 
          type="text" 
          placeholder="请输入关键字" 
          class="search-input"
          @keyup.enter="onSearch"
        />
      </div>
      <button class="search-btn btn-reset" @click="onSearch">搜索</button>
    </div>

    <!-- Filter Bar -->
    <div class="filter-bar glass" @click.stop>
      <div class="filter-item" :class="{ active: activeFilter === 'area' }" @click="toggleFilter('area')">
        <span>{{ selectedArea }}</span>
        <svg class="arrow" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </div>
      <div class="filter-item" :class="{ active: activeFilter === 'level' }" @click="toggleFilter('level')">
        <span>{{ selectedLevel }}</span>
        <svg class="arrow" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </div>
      <div class="filter-item" :class="{ active: activeFilter === 'project' }" @click="toggleFilter('project')">
        <span>{{ selectedProject }}</span>
        <svg class="arrow" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </div>

      <!-- Dropdowns -->
      <div v-if="activeFilter === 'area'" class="dropdown shadow">
        <div v-for="opt in areaOptions" :key="opt" class="dropdown-item" :class="{ selected: selectedArea === opt }" @click="selectFilter('area', opt)">
          {{ opt }}
        </div>
      </div>
      <div v-if="activeFilter === 'level'" class="dropdown shadow">
        <div v-for="opt in levelOptions" :key="opt" class="dropdown-item" :class="{ selected: selectedLevel === opt }" @click="selectFilter('level', opt)">
          {{ opt }}
        </div>
      </div>
      <div v-if="activeFilter === 'project'" class="dropdown shadow">
        <div v-for="opt in projectOptions" :key="opt" class="dropdown-item" :class="{ selected: selectedProject === opt }" @click="selectFilter('project', opt)">
          {{ opt }}
        </div>
      </div>
    </div>

    <!-- List -->
    <div class="list-container">
      <div v-for="item in items" :key="item.id" class="list-card glass">
        <img class="avatar" :src="item.avatar || '/icon-user.svg'" alt="" />
        <div class="info">
          <div class="name-row">
            <span class="name">{{ item.name }}</span>
            <span class="role-tag">{{ item.role }}</span>
          </div>
          <div class="desc-row">
            <span class="level">等级：{{ item.level || '无' }}</span>
            <span class="project">跳绳 ({{ selectedProject === '全部项目' ? '综合' : selectedProject }})</span>
          </div>
        </div>
      </div>
      <div ref="sentinelEl" class="sentinel"></div>
      <div v-if="loading" class="loading">正在加载...</div>
      <div v-else-if="finished && items.length > 0" class="loading">已无更多</div>
      <div v-else-if="finished && items.length === 0" class="empty">暂无数据</div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  min-height: 100vh;
  padding-bottom: 24px;
  display: flex;
  flex-direction: column;
}

.search-bar {
  margin: 12px 12px 0;
  border-radius: 12px;
  padding: 8px 12px;
  display: flex;
  gap: 12px;
  align-items: center;
}

.search-input-wrap {
  flex: 1;
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 4px 12px;
}

.search-icon {
  color: #666;
  margin-right: 6px;
}

.search-input {
  flex: 1;
  border: none;
  background: transparent;
  outline: none;
  font-size: 14px;
  color: #333;
}

.search-btn {
  font-size: 14px;
  color: #333;
  font-weight: 500;
  padding: 4px 8px;
}

.filter-bar {
  position: relative;
  margin: 12px 12px 0;
  border-radius: 12px;
  display: flex;
  padding: 12px 0;
}

.filter-item {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  font-size: 14px;
  color: #333;
}

.filter-item.active {
  color: var(--primary);
}

.arrow {
  transition: transform 0.2s;
}

.filter-item.active .arrow {
  transform: rotate(180deg);
}

.dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #fff;
  border-radius: 12px;
  margin-top: 8px;
  padding: 8px 0;
  z-index: 20;
  max-height: 200px;
  overflow-y: auto;
}

.dropdown-item {
  padding: 12px 24px;
  font-size: 14px;
  color: #333;
  text-align: center;
}

.dropdown-item.selected {
  color: var(--primary);
  background: rgba(var(--primary-rgb), 0.05);
}

.list-container {
  margin: 12px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.list-card {
  padding: 16px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
}

.avatar {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  object-fit: cover;
  background: #eee;
  border: 2px solid rgba(255, 255, 255, 0.5);
}

.info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.name-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.name {
  font-size: 16px;
  font-weight: bold;
  color: #111;
}

.role-tag {
  background: #e11d48;
  color: #fff;
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 12px;
}

.desc-row {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #666;
}

.level {
  color: #666;
}

.project {
  color: #666;
}

.loading, .empty {
  text-align: center;
  padding: 20px;
  color: #999;
  font-size: 14px;
}
</style>
