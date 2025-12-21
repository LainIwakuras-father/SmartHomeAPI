<template>
  <section class="card">
    <div class="card-header">
      <Icon name="sensors" class="w-6 h-6 text-green-600" />
      <h2 class="card-title">Датчики</h2>
      <button 
        @click="refreshAll" 
        class="btn btn-sm btn-outline ml-auto"
        :disabled="store.isLoading"
      >
        <Icon name="refresh" class="w-4 h-4" />
        Обновить
      </button>
    </div>
    
    <div v-if="store.isLoading && store.sensors.length === 0" class="loading-skeleton">
      <div v-for="i in 3" :key="i" class="skeleton-item"></div>
    </div>
    
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <SensorCard 
        v-for="sensor in store.sensorsWithReadings" 
        :key="sensor.sensorId"
        :sensor="sensor"
        @refresh="refreshSensor(sensor)"
      />
    </div>
  </section>
</template>

<script setup>
import { onMounted } from 'vue'
import { useSensorsStore } from '@/stores/useSensorsStore'
import SensorCard from '@/components/sensors/SensorCard.vue'

const store = useSensorsStore()

const refreshSensor = async (sensor) => {
  await store.fetchLatestReading(sensor.sensorId) // Передаем sensorId (строку)
}

const refreshAll = async () => {
  await store.fetchAllLatestReadings()
}

onMounted(async () => {
  await store.fetchSensors()
  await store.fetchAllLatestReadings()
})
</script>