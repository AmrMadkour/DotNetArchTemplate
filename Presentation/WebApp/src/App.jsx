import { useEffect, useState } from 'react'
import './App.css'

// Points at the API project (Presentation/API), default dev port from its launchSettings.json.
const API_BASE_URL = 'http://localhost:5023'

// Reads the 'exp' claim out of a JWT without verifying its signature — client-side, just to
// decide whether to show the Quote button as usable. The server re-validates on every real call.
function readTokenExpiry(token) {
  const payload = JSON.parse(atob(token.split('.')[1]))
  return payload.exp * 1000 // exp is seconds since epoch; Date.now() is milliseconds
}

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
  const [username, setUsername] = useState('demo')
  const [password, setPassword] = useState('demo123')
  const [loginError, setLoginError] = useState(null)

  const [productId, setProductId] = useState('widget-1')
  const [unitPrice, setUnitPrice] = useState(10)
  const [quantity, setQuantity] = useState(2)
  const [couponCode, setCouponCode] = useState('')
  const [quote, setQuote] = useState(null)
  const [quoteError, setQuoteError] = useState(null)

  // Once a stored token's expiry passes while the page is open, drop it so the Quote button disables again.
  useEffect(() => {
    if (!session) return
    const msUntilExpiry = session.expiresAtMs - Date.now()
    const timer = setTimeout(() => setSession(null), msUntilExpiry)
    return () => clearTimeout(timer)
  }, [session])

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
    const newSession = { token, expiresAtMs: readTokenExpiry(token) }
    sessionStorage.setItem('session', JSON.stringify(newSession))
    setSession(newSession)
  }

  async function handleQuote(e) {
    e.preventDefault()
    setQuoteError(null)
    setQuote(null)

    const response = await fetch(`${API_BASE_URL}/api/Orders/quote`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${session.token}`
      },
      body: JSON.stringify({
        items: [{ productId, unitPrice: Number(unitPrice), quantity: Number(quantity) }],
        couponCode: couponCode || null
      })
    })

    if (response.status === 429) {
      setQuoteError('Too many requests — try again shortly.')
      return
    }
    if (response.status === 401) {
      sessionStorage.removeItem('session')
      setSession(null)
      setQuoteError('Your session expired — please log in again.')
      return
    }
    if (!response.ok) {
      setQuoteError(await response.text())
      return
    }

    setQuote(await response.json())
  }

  const isLoggedIn = session !== null

  return (
    <main className="page">
      <h1>Order Quote Demo</h1>

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

      <section className="card">
        <h2>2. Get a quote</h2>
        <form onSubmit={handleQuote}>
          <label>
            Product ID
            <input value={productId} onChange={(e) => setProductId(e.target.value)} />
          </label>
          <label>
            Unit price
            <input type="number" value={unitPrice} onChange={(e) => setUnitPrice(e.target.value)} />
          </label>
          <label>
            Quantity
            <input type="number" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
          </label>
          <label>
            Coupon code (optional)
            <input value={couponCode} onChange={(e) => setCouponCode(e.target.value)} />
          </label>
          <button type="submit" disabled={!isLoggedIn}>Get Quote</button>
        </form>
        {!isLoggedIn && <p className="hint">Log in first — the quote button stays disabled until you hold a valid token.</p>}
        {quoteError && <p className="error">{quoteError}</p>}
        {quote && (
          <ul className="quote">
            <li>Subtotal: {quote.subtotal}</li>
            <li>Discount: {quote.discountAmount}</li>
            <li>Total: {quote.total}</li>
            {quote.appliedCoupon && <li>Coupon applied: {quote.appliedCoupon}</li>}
          </ul>
        )}
      </section>
    </main>
  )
}
