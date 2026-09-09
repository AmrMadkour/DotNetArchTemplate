import { useState } from 'react'

// Points at the API project (Presentation/API); default dev port from its launchSettings.json, set in .env.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

// Reads the 'exp' claim out of a JWT without verifying its signature — client-side, just to
// decide whether to show the Quote button as usable. The server re-validates on every real call.
function readTokenExpiry(token) {
  const payload = JSON.parse(atob(token.split('.')[1]))
  return payload.exp * 1000 // exp is seconds since epoch; Date.now() is milliseconds
}

export default function LoginForm({ isLoggedIn, onLogin }) {
  const [username, setUsername] = useState('demo')
  const [password, setPassword] = useState('demo123')
  const [loginError, setLoginError] = useState(null)

  async function handleLogin(e) {
    e.preventDefault()
    setLoginError(null)

    const response = await fetch(`${API_BASE_URL}/api/Auth/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })

    if (!response.ok) {
      setLoginError(response.status === 429 ? 'Too many login attempts — try again shortly.' : 'Invalid credentials.')
      return
    }

    const { token } = await response.json()
    onLogin({ token, expiresAtMs: readTokenExpiry(token) })
  }

  return (
    <section className="card">
      <h2>1. Log in</h2>
      <form onSubmit={handleLogin}>
        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} disabled={isLoggedIn} />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} disabled={isLoggedIn} />
        </label>
        <button type="submit" disabled={isLoggedIn}>{isLoggedIn ? 'Logged in' : 'Login'}</button>
      </form>
      {loginError && <p className="error">{loginError}</p>}
    </section>
  )
}
