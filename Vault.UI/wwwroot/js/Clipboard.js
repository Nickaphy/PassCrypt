export async function copyText(value)
{
  await navigator.clipboard.writeText(value);
}

let clearTimer = null;
let pendingValue = null;

// Clear the clipboard
async function tryClear()
{
  try
  {
    await navigator.clipboard.writeText("");
    pendingValue = null;
  }
  catch
  {
  }
}

export function scheduleClear(value, delayMs)
{
  if (clearTimer)
  {
    clearTimeout(clearTimer);
  }

  pendingValue = value;
  clearTimer = setTimeout(() =>
  {
    clearTimer = null;
    tryClear();
  }, delayMs);
}

// clear upon refocus on page
window.addEventListener("focus", () =>
{
  if (pendingValue !== null && clearTimer === null)
  {
    tryClear();
  }
});

// If the page is closed -> go around the timer and clear it immidiately.
window.addEventListener("pagehide", () =>
{
  if (pendingValue !== null)
  {
    navigator.clipboard.writeText("").catch(() => {});
  }
});
