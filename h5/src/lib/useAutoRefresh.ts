import { onBeforeUnmount, onMounted } from 'vue'

export function useAutoRefresh(refresh: () => void | Promise<void>, intervalMs?: number) {
  let timer: number | null = null

  const onVisible = () => {
    if (document.visibilityState === 'visible') refresh()
  }
  const onOnline = () => refresh()

  onMounted(() => {
    refresh()
    document.addEventListener('visibilitychange', onVisible)
    window.addEventListener('online', onOnline)
    if (intervalMs && intervalMs > 0) timer = window.setInterval(() => refresh(), intervalMs)
  })

  onBeforeUnmount(() => {
    document.removeEventListener('visibilitychange', onVisible)
    window.removeEventListener('online', onOnline)
    if (timer) window.clearInterval(timer)
  })
}

