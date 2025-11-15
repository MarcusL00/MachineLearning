function attachCsvListener() {
  const fileInput = document.getElementById('csv-file')
  const featuresSelect = document.getElementById('features-select')
  const targetSelect = document.getElementById('target-select')

  if (!fileInput) return // partial not present yet

  // Avoid attaching twice
  if (fileInput.dataset.listenerAttached) return
  fileInput.dataset.listenerAttached = 'true'

  fileInput.addEventListener('change', (event) => {
    const file = event.target.files[0]
    if (!file) return

    Papa.parse(file, {
      header: true,
      complete: function (results) {
        const headers = results.meta.fields

        // Clear existing options
        featuresSelect.innerHTML = ''
        targetSelect.innerHTML = ''

        // Populate both selects
        headers.forEach((header) => {
          if (header === '') {
            return // Skip empty headers
          }

          const opt1 = document.createElement('option')
          opt1.value = header
          opt1.textContent = header
          featuresSelect.appendChild(opt1)

          const opt2 = document.createElement('option')
          opt2.value = header
          opt2.textContent = header
          targetSelect.appendChild(opt2)
        })

        featuresSelect.disabled = false
        targetSelect.disabled = false
      },
    })
  })
}

// Run once on initial load
document.addEventListener('DOMContentLoaded', attachCsvListener)

// Run again whenever HTMX swaps in new content
document.body.addEventListener('htmx:afterSwap', attachCsvListener)
