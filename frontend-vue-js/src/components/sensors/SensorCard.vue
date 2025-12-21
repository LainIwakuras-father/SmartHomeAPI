<template>
  <article class="sensor-card group hover:shadow-lg transition-shadow">
    <div class="flex justify-between items-start mb-3">
      <div>
        <h3 class="font-semibold text-lg">{{ sensor.name || `Датчик ${sensor.sensorId}` }}</h3>
        <p class="text-sm text-gray-500">{{ sensor.type }} • {{ sensor.location }}</p>
      </div>
      <SensorStatusIndicator :status="sensor.status" />
    </div>
    
    <div v-if="sensor.latest" class="mt-4">
      <div class="flex items-center justify-between">
        <span class="text-sm text-gray-600">Последнее значение</span>
        <span class="text-2xl font-bold text-indigo-700">
          {{ sensor.latest.value }} {{ sensor.unit || '' }}
        </span>
      </div>
      <p class="text-xs text-gray-400 mt-1">
        {{ formatTime(sensor.latest.time) }}
      </p>
    </div>
    
    <div class="card-actions mt-4">
      <button @click="$emit('refresh')" class="btn btn-xs btn-ghost">
        <Icon name="refresh" class="w-3 h-3" />
      </button>
      <router-link 
        :to="`/sensors/${sensor.id}`" 
        class="btn btn-xs btn-primary ml-auto"
      >
        Детали
      </router-link>
    </div>
  </article>
</template>

<script setup>
defineProps({
  sensor: {
    type: Object,
    required: true
  }
})

defineEmits(['refresh'])

const formatTime = (isoString) => {
  return new Date(isoString).toLocaleTimeString('ru-RU', {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.sensor-card {
  @apply bg-white rounded-xl p-4 border border-gray-200;
}
.card-actions {
  @apply flex items-center opacity-0 group-hover:opacity-100 transition-opacity;
}
</style>