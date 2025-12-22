import { defineStore } from 'pinia'
import { ref } from 'vue'
import { auditService } from '@/services/api-auditService'

export const useAuditStore = defineStore('audit', () => {
  const events = ref([])
  const stats = ref(null)
  const recentBreaches = ref([])
  const summary = ref(null)
  const isLoading = ref(false)
  const error = ref(null)
  const filters = ref({
    from: null,
    to: null,
    user: '',
    action: '',
    isSuccessful: null
  })

  const fetchEvents = async (customFilters = {}) => {
    isLoading.value = true
    error.value = null
    
    try {
      const params = {
        ...filters.value,
        ...customFilters
      }
      
      // Убираем null/undefined значения
      Object.keys(params).forEach(key => {
        if (params[key] === null || params[key] === undefined || params[key] === '') {
          delete params[key]
        }
      })
      
      const response = await auditService.getEvents(params)
      events.value = response.data
    } catch (err) {
      console.error('Error fetching audit events:', err)
      error.value = err.response?.data?.message || 'Ошибка загрузки событий аудита'
    } finally {
      isLoading.value = false
    }
  }

  const fetchStats = async () => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await auditService.getStats()
      stats.value = response.data
    } catch (err) {
      console.error('Error fetching audit stats:', err)
      error.value = err.response?.data?.message || 'Ошибка загрузки статистики'
    } finally {
      isLoading.value = false
    }
  }

  const fetchRecentBreaches = async (count = 10) => {
    try {
      const response = await auditService.getRecentBreaches(count)
      recentBreaches.value = response.data
    } catch (err) {
      console.error('Error fetching recent breaches:', err)
    }
  }

  const fetchSummary = async () => {
    try {
      const response = await auditService.getSummary()
      summary.value = response.data
    } catch (err) {
      console.error('Error fetching audit summary:', err)
    }
  }

  const updateFilters = (newFilters) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = {
      from: null,
      to: null,
      user: '',
      action: '',
      isSuccessful: null
    }
  }

  const refreshAll = async () => {
    await Promise.all([
      fetchStats(),
      fetchEvents(),
      fetchRecentBreaches(),
      fetchSummary()
    ])
  }

  return {
    events,
    stats,
    recentBreaches,
    summary,
    isLoading,
    error,
    filters,
    fetchEvents,
    fetchStats,
    fetchRecentBreaches,
    fetchSummary,
    updateFilters,
    resetFilters,
    refreshAll
  }
})