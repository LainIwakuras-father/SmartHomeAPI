import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { sensorsService, telemetryService } from '@/services/api'

export const useSensorsStore = defineStore('sensors', () => {
  const sensors = ref([])
  const latestReadings = ref({})
  const isLoading = ref(false)
  const error = ref(null)

  // Геттер для датчиков с последними показаниями
  const sensorsWithReadings = computed(() => {
    return sensors.value.map(sensor => ({
      ...sensor,
      latest: latestReadings.value[sensor.sensorId] || null
    }))
  })

  const fetchSensors = async () => {
    isLoading.value = true
    try {
      const { data } = await sensorsService.getAll()
      sensors.value = data
    } catch (err) {
      console.error('Error fetching sensors:', err)
      error.value = err.response?.data || 'Ошибка загрузки датчиков'
    } finally {
      isLoading.value = false
    }
  }

  const fetchLatestReading = async (sensorName) => {
    try {
      const { data } = await telemetryService.getLatest(sensorName)
      if (data) {
        latestReadings.value[sensorName] = data
      }
    } catch (err) {
      console.error(`Error fetching latest for ${sensorName}:`, err)
      // Не выбрасываем ошибку, чтобы не ломать весь список
    }
  }

  const fetchAllLatestReadings = async () => {
    if (sensors.value.length === 0) {
      await fetchSensors()
    }
    
    const promises = sensors.value.map(sensor => 
      fetchLatestReading(sensor.sensorId)
    )
    await Promise.allSettled(promises)
  }

  return {
    sensors,
    sensorsWithReadings,
    latestReadings,
    isLoading,
    error,
    fetchSensors,
    fetchLatestReading,
    fetchAllLatestReadings
  }
})