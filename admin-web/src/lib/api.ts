import { getToken } from './auth'

export type ApiResponse<T> = {
  code: number
  message: string
  data: T
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

export async function apiFetch<T>(
  path: string,
  options?: RequestInit,
): Promise<ApiResponse<T>> {
  const token = getToken()
  const headers = new Headers(options?.headers)
  headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers })
  const json = (await res.json()) as ApiResponse<T>
  return json
}

