const TOKEN_KEY = 'h5_token'

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function setToken(token: string) {
  try {
    localStorage.setItem(TOKEN_KEY, token)
  } catch {
  }
}

export function clearToken() {
  try {
    localStorage.removeItem(TOKEN_KEY)
  } catch {
  }
}

function base64UrlDecode(input: string) {
  const s = input.replace(/-/g, '+').replace(/_/g, '/')
  const pad = s.length % 4 ? '='.repeat(4 - (s.length % 4)) : ''
  const decoded = atob(s + pad)
  try {
    return decodeURIComponent(
      Array.from(decoded)
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    )
  } catch {
    return decoded
  }
}

export function getUserIdFromToken(token?: string | null): string | null {
  const t = token ?? getToken()
  if (!t) return null
  const parts = t.split('.')
  if (parts.length < 2) return null
  try {
    const payload = JSON.parse(base64UrlDecode(parts[1])) as any
    const nameId =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
      payload['nameid'] ??
      payload['sub'] ??
      null
    return nameId ? String(nameId) : null
  } catch {
    return null
  }
}
