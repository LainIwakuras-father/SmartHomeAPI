<!-- <template>
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
</style> -->


<template>
  <div class="sensor-card">
    <div class="sensor-header" @click="toggleExpanded">
      <div class="sensor-info">
        <Icon :name="getSensorIcon(sensor.sensorId)" class="sensor-icon" />
        <div>
          <h3 class="sensor-name">{{ getSensorName(sensor.sensorId) }}</h3>
          <p class="sensor-id">ID: {{ sensor.id }}</p>
        </div>
      </div>
      <div class="sensor-controls">
        <span :class="['status-indicator', getStatusClass()]"></span>
        <button class="toggle-btn">
          <Icon :name="isExpanded ? 'chevron-up' : 'chevron-down'" />
        </button>
      </div>
    </div>
    
    <div v-if="isExpanded" class="sensor-details">
      <div v-if="sensor.latest" class="sensor-readings">
        <div class="reading">
          <span class="reading-label">Текущее значение:</span>
          <span class="reading-value">{{ formatValue(sensor.latest.value) }}</span>
        </div>
        
        <div class="reading">
          <span class="reading-label">Единицы измерения:</span>
          <span class="reading-unit">{{ getUnit(sensor.sensorId) }}</span>
        </div>
      </div>
      <div v-else class="no-data">
        <Icon name="warning" class="mr-2 text-yellow-500" />
        <span>Нет данных</span>
      </div>
      
      <div class="sensor-actions">
        <button 
          @click.stop="refreshSensor" 
          class="action-btn refresh-btn"
          :disabled="isRefreshing"
        >
          <Icon name="refresh" class="mr-2" />
          {{ isRefreshing ? 'Обновление...' : 'Обновить' }}
        </button>
        <button class="action-btn details-btn">
          <Icon name="chart" class="mr-2" />
          График
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  sensor: Object
})

const emit = defineEmits(['refresh'])

const isExpanded = ref(false)
const isRefreshing = ref(false)

const toggleExpanded = () => {
  isExpanded.value = !isExpanded.value
}

const refreshSensor = async () => {
  isRefreshing.value = true
  emit('refresh')
  // Имитация задержки обновления
  setTimeout(() => {
    isRefreshing.value = false
  }, 500)
}

const getSensorIcon = (sensorId) => {
  const icons = {
    'Humanidity': 'droplets',
    'Temperature': 'thermometer',
    'Pressure': 'gauge',
    'Motion': 'motion-sensor',
    'Light': 'sun'
  }
  return icons[sensorId] || 'activity'
}

const getSensorName = (sensorId) => {
  const names = {
    'Humanidity': 'Влажность',
    'Temperature': 'Температура',
    'Pressure': 'Давление',
    'Motion': 'Движение',
    'Light': 'Освещенность'
  }
  return names[sensorId] || sensorId
}

const getUnit = (sensorId) => {
  const units = {
    'Humanidity': '%',
    'Temperature': '°C',
    'Pressure': 'hPa',
    'Motion': 'движ/мин',
    'Light': 'люкс'
  }
  return units[sensorId] || ''
}

const getStatusClass = () => {
  if (!props.sensor.latest) return 'status-offline'
  const value = props.sensor.latest.value
  
  switch (props.sensor.sensorId) {
    case 'Temperature':
      return value > 25 ? 'status-warning' : 'status-ok'
    case 'Humanidity':
      return value < 30 || value > 70 ? 'status-warning' : 'status-ok'
    case 'Pressure':
      return value < 980 || value > 1020 ? 'status-warning' : 'status-ok'
    default:
      return 'status-ok'
  }
}

const formatValue = (value) => {
  if (typeof value === 'number') {
    return value.toFixed(2)
  }
  return value
}

const formatTime = (timestamp) => {
  if (!timestamp) return '—'
  return new Date(timestamp).toLocaleTimeString('ru-RU')
}
</script>

<style scoped>
.sensor-card {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
  transition: all 0.3s ease;
}

.sensor-card:hover {
  border-color: #cbd5e0;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.sensor-header {
  padding: 16px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  background: #f7fafc;
  border-bottom: 1px solid #e2e8f0;
}

.sensor-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sensor-icon {
  width: 40px;
  height: 40px;
  color: #4c51bf;
}

.sensor-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 4px;
}

.sensor-id {
  font-size: 0.85rem;
  color: #718096;
}

.sensor-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.status-ok {
  background: #38a169;
  box-shadow: 0 0 8px rgba(56, 161, 105, 0.3);
}

.status-warning {
  background: #ed8936;
  box-shadow: 0 0 8px rgba(237, 137, 54, 0.3);
  animation: pulse 2s infinite;
}

.status-offline {
  background: #a0aec0;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

.toggle-btn {
  background: none;
  border: none;
  color: #718096;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
}

.toggle-btn:hover {
  background: #edf2f7;
  color: #4a5568;
}

.sensor-details {
  padding: 20px;
  background: white;
}

.sensor-readings {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.reading {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #f1f5f9;
}

.reading:last-child {
  border-bottom: none;
}

.reading-label {
  color: #718096;
  font-size: 0.9rem;
}

.reading-value {
  font-size: 1.2rem;
  font-weight: 600;
  color: #2d3748;
}

.reading-time {
  color: #4a5568;
  font-size: 0.9rem;
}

.reading-unit {
  color: #4c51bf;
  font-weight: 500;
}

.no-data {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  color: #a0aec0;
  background: #f7fafc;
  border-radius: 8px;
  margin-bottom: 20px;
}

.sensor-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 10px;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.refresh-btn {
  background: #edf2f7;
  color: #4a5568;
}

.refresh-btn:hover:not(:disabled) {
  background: #e2e8f0;
  transform: translateY(-2px);
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.details-btn {
  background: linear-gradient(90deg, #4c51bf 0%, #667eea 100%);
  color: white;
}

.details-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(76, 81, 191, 0.3);
}
</style>