const TOKEN_KEY = 'jumpropeass_admin_token'

const emitter = new EventTarget()
const EVENT_NAME = 'token-change'

if (typeof window !== 'undefined') {
  window.addEventListener('storage', (e) => {
    if (e.key === TOKEN_KEY) emitter.dispatchEvent(new Event(EVENT_NAME))
  })
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token)
  emitter.dispatchEvent(new Event(EVENT_NAME))
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY)
  emitter.dispatchEvent(new Event(EVENT_NAME))
}

export function subscribeTokenChange(listener: () => void) {
  emitter.addEventListener(EVENT_NAME, listener)
  return () => emitter.removeEventListener(EVENT_NAME, listener)
}

