import apiClient from './api'

export const auditService = {
  // Получение событий аудита
  getEvents(params) {
    return apiClient.get('/SecurityAudit/events', { params })
  },
  
  // Получение статистики
  getStats() {
    return apiClient.get('/SecurityAudit/stats')
  },
  
  // Получение последних нарушений безопасности
  getRecentBreaches(count = 10) {
    return apiClient.get('/SecurityAudit/recent-security-breaches', { 
      params: { count } 
    })
  },
  
  // Получение неудачных попыток входа
  getFailedLogins(username) {
    return apiClient.get(`/SecurityAudit/failed-logins/${username}`)
  },
  
  // Получение сводки
  getSummary() {
    return apiClient.get('/SecurityAudit/summary')
  },
  
  
}