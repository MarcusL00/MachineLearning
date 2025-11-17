// Attach HTMX lifecycle listeners globally.
// This works even if #results-container and #results-loading are injected later as partials.

document.body.addEventListener('htmx:beforeRequest', function (evt) {
  try {
    const trigger = evt.detail && evt.detail.elt
    if (!trigger) return

    if (
      trigger.id === 'model-form' ||
      (trigger.closest && trigger.closest('#model-form'))
    ) {
      const resultsContainer = document.getElementById('results-container')
      if (resultsContainer) {
        resultsContainer.innerHTML = `
          <div id="results-loading" class="htmx-indicator htmx-request" aria-hidden="false" style="align-items: center; gap: 8px; margin-bottom: 12px">
            <span class="loader" aria-hidden="true" style="color: var(--primary-color)"></span>
          </div>
        `
      }

      const indicator = document.getElementById('results-loading')
      if (indicator) {
        indicator.classList.add('htmx-request')
        indicator.setAttribute('aria-hidden', 'false')
      }
    }
  } catch (e) {
    console.error(e)
  }
})

document.body.addEventListener('htmx:afterRequest', function (evt) {
  try {
    const trigger = evt.detail && evt.detail.elt
    if (!trigger) return

    if (
      trigger.id === 'model-form' ||
      (trigger.closest && trigger.closest('#model-form'))
    ) {
      const indicator = document.getElementById('results-loading')
      if (indicator) {
        indicator.classList.remove('htmx-request')
        indicator.setAttribute('aria-hidden', 'true')
      }
    }
  } catch (e) {
    console.error(e)
  }
})
