import { clearToken, getToken } from './auth'

export type ApiResponse<T> = {
  code: number
  message: string
  data: T
}

const ENV_BASE_URL = import.meta.env.VITE_API_BASE_URL || ''
const BASE_URL = (() => {
  if (!ENV_BASE_URL) return ''
  const isLoopback = /^https?:\/\/(localhost|127\.0\.0\.1)(:\d+)?/i.test(ENV_BASE_URL)
  const isPageLoopback = typeof window !== 'undefined' && (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1')
  if (isLoopback && !isPageLoopback) return ''
  return ENV_BASE_URL
})()

export async function apiFetch<T>(path: string, options?: RequestInit): Promise<ApiResponse<T>> {
  const token = getToken()
  const headers = new Headers(options?.headers)
  if (!headers.has('Content-Type') && options?.body) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let res: Response
  try {
    res = await fetch(`${BASE_URL}${path}`, { ...options, headers })
  } catch {
    return { code: -1, message: '网络异常，无法连接到后台服务', data: null as unknown as T }
  }

  if (res.status === 401) clearToken()
  const text = await res.text()
  if (!text) return { code: res.ok ? 0 : res.status, message: res.ok ? 'OK' : res.statusText || '请求失败', data: null as unknown as T }

  try {
    return JSON.parse(text) as ApiResponse<T>
  } catch {
    return { code: res.ok ? 0 : res.status, message: '返回不是JSON', data: text as unknown as T }
  }
}

