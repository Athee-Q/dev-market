let checkoutScriptPromise = null;

/**
 * Loads Razorpay's Checkout widget script on demand (not in index.html — most pages never need
 * it) and caches the in-flight/resolved promise so repeat "Pay Now" clicks don't re-fetch it.
 */
export function loadRazorpayCheckout() {
  if (window.Razorpay) return Promise.resolve();
  if (checkoutScriptPromise) return checkoutScriptPromise;

  checkoutScriptPromise = new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    script.async = true;
    script.onload = () => resolve();
    script.onerror = () => {
      checkoutScriptPromise = null; // allow retrying on a later click
      reject(new Error('Failed to load Razorpay Checkout.'));
    };
    document.body.appendChild(script);
  });

  return checkoutScriptPromise;
}
