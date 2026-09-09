import { useState } from 'react'

// Points at the API project (Presentation/API); default dev port from its launchSettings.json, set in .env.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export default function QuoteForm({ isLoggedIn, token, onSessionExpired }) {
  const [productId, setProductId] = useState('widget-1')
  const [unitPrice, setUnitPrice] = useState(10)
  const [quantity, setQuantity] = useState(2)
  const [couponCode, setCouponCode] = useState('')
  const [quote, setQuote] = useState(null)
  const [quoteError, setQuoteError] = useState(null)

  async function handleQuote(e) {
    e.preventDefault()
    setQuoteError(null)
    setQuote(null)

    const response = await fetch(`${API_BASE_URL}/api/Orders/quote`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
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
      onSessionExpired()
      setQuoteError('Your session expired — please log in again.')
      return
    }
    if (!response.ok) {
      setQuoteError(await response.text())
      return
    }

    setQuote(await response.json())
  }

  return (
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
  )
}
