import { useEffect, useState } from 'react'
import LoginForm from './LoginForm'
import QuoteForm from './QuoteForm'
import './App.css'

function loadStoredSession() {
  const raw = sessionStorage.getItem('session')
  if (!raw) return null

  const session = JSON.parse(raw)
  if (session.expiresAtMs <= Date.now()) {
    sessionStorage.removeItem('session')
    return null
  }
  return session
}

export default function App() {
  const [session, setSession] = useState(loadStoredSession)

  // Once a stored token's expiry passes while the page is open, drop it so the Quote button disables again.
  useEffect(() => {
    if (!session) return
    const msUntilExpiry = session.expiresAtMs - Date.now()
    const timer = setTimeout(() => setSession(null), msUntilExpiry)
    return () => clearTimeout(timer)
  }, [session])

  function handleLogin(newSession) {
    sessionStorage.setItem('session', JSON.stringify(newSession))
    setSession(newSession)
  }

  function handleSessionExpired() {
    sessionStorage.removeItem('session')
    setSession(null)
  }

  const isLoggedIn = session !== null

  return (
    <main className="page">
      <h1>Order Quote Demo</h1>

      <LoginForm isLoggedIn={isLoggedIn} onLogin={handleLogin} />

      <QuoteForm isLoggedIn={isLoggedIn} token={session?.token} onSessionExpired={handleSessionExpired} />
    </main>
  )
}
