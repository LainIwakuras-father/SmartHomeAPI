// Форматирование дат и значений
export const formatDateTime = (isoString, options = {}) => {
  const defaultOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }
  
  return new Date(isoString).toLocaleString('ru-RU', {
    ...defaultOptions,
    ...options
  })
}

export const formatNumber = (value, decimals = 2) => {
  return Number(value).toFixed(decimals)
}

export const formatUptime = (seconds) => {
  const days = Math.floor(seconds / 86400)
  const hours = Math.floor((seconds % 86400) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  
  if (days > 0) return `${days}д ${hours}ч`
  if (hours > 0) return `${hours}ч ${minutes}м`
  return `${minutes}м`
}