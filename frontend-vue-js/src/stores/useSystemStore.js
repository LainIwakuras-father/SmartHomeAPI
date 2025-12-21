import { defineStore } from 'pinia'
import { ref } from 'vue'
import { statusService } from '@/services/api'

export const useSystemStore = defineStore('system', () => {
  const status = ref(null)
  const isLoading = ref(false)
  const error = ref(null)

  const fetchStatus = async () => {
    isLoading.value = true
    error.value = null
    try {
      const { data } = await statusService.getStatus()
      status.value = data
    } catch (err) {
      error.value = err.response?.data?.message || 'Ошибка загрузки статуса'
      console.error('System status error:', err)
    } finally {
      isLoading.value = false
    }
  }

  return {
    status,
    isLoading,
    error,
    fetchStatus
  }
})