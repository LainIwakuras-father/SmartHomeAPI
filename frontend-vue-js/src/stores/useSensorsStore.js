import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { sensorsService, telemetryService } from '@/services/api'

export const useSensorsStore = defineStore('sensors', () => {
  const sensors = ref([])
  const latestReadings = ref({})
  const isLoading = ref(false)
  const errors = ref({})

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
      errors.value.fetch = err.response?.data || 'Ошибка загрузки датчиков'
    } finally {
      isLoading.value = false
    }
  }

  const fetchLatestReading = async (sensorId) => {
    try {
      const { data } = await telemetryService.getLatest(sensorId)
      latestReadings.value[sensorId] = data
    } catch (err) {
      console.error(`Failed to fetch latest for ${sensorId}:`, err)
    }
  }

  const fetchAllLatestReadings = async () => {
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
    errors,
    fetchSensors,
    fetchLatestReading,
    fetchAllLatestReadings
  }
})