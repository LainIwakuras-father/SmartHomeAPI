<template>
  <section class="card">
    <div class="card-header">
      <div class="header-icon">📡</div>
      <h2 class="card-title">Датчики</h2>
      <button 
        class="btn btn-sm btn-outline ml-auto" 
        :disabled="store.isLoading"
        @click="refreshAll"
      >
        <span class="refresh-icon">🔄</span>
        Обновить
      </button>
    </div>
    
    <div v-if="store.isLoading && store.sensors.length === 0" class="loading-skeleton">
      <div v-for="i in 5" :key="i" class="skeleton-item"></div>
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
  await store.fetchLatestReading(sensor.sensorId)
}

const refreshAll = async () => {
  await store.fetchAllLatestReadings()
}

onMounted(async () => {
  await store.fetchSensors()
  await store.fetchAllLatestReadings()
})
</script>

<style scoped>
.card {
  @apply bg-white rounded-xl shadow-sm border border-gray-200;
}

.card-header {
  @apply flex items-center gap-3 px-6 py-4 border-b border-gray-200;
}

.header-icon {
  @apply text-xl;
}

.card-title {
  @apply text-lg font-semibold text-gray-800 flex-1;
}

.btn {
  @apply px-3 py-1.5 rounded-lg border transition-colors text-sm font-medium;
}

.btn-outline {
  @apply border-gray-300 text-gray-700 hover:bg-gray-50;
}

.btn:disabled {
  @apply opacity-50 cursor-not-allowed;
}

.loading-skeleton {
  @apply p-6 space-y-3;
}

.skeleton-item {
  @apply h-16 bg-gray-200 rounded-lg animate-pulse;
}
</style>