<template>
  <div class="home-container">
    <!-- Заголовок -->
    <header class="page-header">
      <h1 class="page-title">Панель управления умным домом</h1>
      <div class="user-info">
        <span class="user-role">Пользователь: {{ currentUser }}</span>
         <button 
          v-if="userRole === 'Administrator' || userRole === 'Auditor'"
          class="btn-audit" 
          @click="$router.push('/audit-security')"
        >
          🔒 Аудит безопасности
        </button> 
        <button class="logout-btn" @click="logout">Выйти</button>
      </div>
    </header>

    <!-- Основное содержимое -->
    <main class="main-content">
      <!-- Левая колонка -->
      <div class="left-column">
        <!-- Статус системы -->
        <section class="compact-card status-card">
          <div class="card-header">
            <h2 class="card-title">Статус системы</h2>
            <button 
              :disabled="systemStore.isLoading" 
              class="icon-btn"
              @click="systemStore.fetchStatus"
            >
              ↻
            </button>
          </div>
          
          <div v-if="systemStore.isLoading" class="loading">
            <div class="spinner"></div>
          </div>
          
          <div v-else class="status-grid">
            <div class="status-item">
              <span class="status-label">Статус</span>
              <span 
                class="status-value" 
                :class="getStatusClass(systemStore.status?.status)"
              >
                {{ systemStore.status?.status || 'Неизвестно' }}
              </span>
              <span class="status-desc">Все системы работают нормально</span>
            </div>
            
            <div class="status-item">
              <span class="status-label">Версия ПО</span>
              <span class="status-value">{{ systemStore.status?.version || '1.0.0' }}</span>
              <span class="status-desc">Текущая версия системы</span>
            </div>
            
            <div class="status-item">
              <span class="status-label">Время работы</span>
              <span class="status-value">{{ formatUptime(systemStore.status?.uptime) }}</span>
              <span class="status-desc">С момента последнего запуска</span>
            </div>
            
            <div class="status-item">
              <span class="status-label">Время сервера</span>
              <span class="status-value">{{ formatDateTime(systemStore.status?.timestamp) }}</span>
              <span class="status-desc">Текущее системное время</span>
            </div>
          </div>
        </section>

        <!-- История телеметрии -->
        <section class="compact-card telemetry-card">
          <div class="card-header">
            <h2 class="card-title">История телеметрии</h2>
            <div class="filters">
              <select v-model="selectedPeriod" class="filter-select">
                <option value="24">Последние 24 часа</option>
                <option value="1">Последний час</option>
                <option value="168">Последняя неделя</option>
              </select>
              <select v-model="selectedSensor" class="filter-select">
                <option value="">Все датчики</option>
                <option value="Humanidity">Влажность</option>
                <option value="Temperature">Температура</option>
                <option value="Pressure">Давление</option>
                <option value="Motion">Движение</option>
                <option value="Light">Освещенность</option>
              </select>
            </div>
          </div>
          
          <div class="telemetry-placeholder">
            <div class="placeholder-icon">📈</div>
            <p class="placeholder-text">График телеметрии</p>
            <p class="placeholder-subtext">Здесь будет отображаться график данных за выбранный период</p>
          </div>
        </section>
      </div>

      <!-- Правая колонка -->
      <div class="right-column">
        <!-- Датчики -->
        <section class="compact-card sensors-card">
          <div class="card-header">
            <h2 class="card-title">Датчики</h2>
            <button 
              :disabled="sensorsStore.isLoading" 
              class="icon-btn"
              @click="refreshAllSensors"
            >
              ↻
            </button>
          </div>
          
          <div v-if="sensorsStore.isLoading" class="loading">
            <div class="spinner"></div>
          </div>
          
          <div v-else class="sensors-list">
            <div 
              v-for="sensor in sensorsStore.sensorsWithReadings" 
              :key="sensor.sensorId"
              class="sensor-item"
              @click="toggleSensor(sensor)"
            >
              <div class="sensor-info">
                <span class="sensor-name">{{ getSensorName(sensor.sensorId) }}</span>
                <span class="sensor-id">ID:{{ sensor.id }}</span>
              </div>
              <div class="sensor-status">
                <span 
                  class="status-dot"
                  :class="getSensorStatusClass(sensor)"
                ></span>
                <span class="toggle-icon">{{ expandedSensor === sensor.sensorId ? '▲' : '▼' }}</span>
              </div>
              
              <div v-if="expandedSensor === sensor.sensorId" class="sensor-details">
                <div v-if="sensor.latest" class="sensor-reading">
                  <span>Текущее значение:</span>
                  <span class="reading-value">
                    {{ formatSensorValue(sensor.latest.value, sensor.sensorId) }}
                  </span>
                </div>
                <div v-else class="no-data">Нет данных</div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useSystemStore } from '@/stores/useSystemStore'
import { useSensorsStore } from '@/stores/useSensorsStore'

const authStore = useAuthStore()
const systemStore = useSystemStore()
const sensorsStore = useSensorsStore()

const selectedPeriod = ref('24')
const selectedSensor = ref('')
const expandedSensor = ref(null)

const currentUser = computed(() => {
 
  return authStore.user.username
})

const userRole = computed(() => {
 
  return authStore.user.role
})

const logout = () => {
  authStore.logout()
}

const getStatusClass = (status) => {
  return status === 'Operational' ? 'status-ok' : 'status-warning'
}

const formatUptime = (seconds) => {
  if (!seconds) return '—'
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}ч ${minutes}м`
}

const formatDateTime = (isoString) => {
  if (!isoString) return '—'
  const date = new Date(isoString)
  return date.toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).replace(',', '')
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

const getSensorStatusClass = (sensor) => {
  if (!sensor.latest) return 'status-offline'
  
  const value = sensor.latest.value
  switch(sensor.sensorId) {
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

const formatSensorValue = (value, sensorId) => {
  if (value === undefined || value === null) return '—'
  
  const units = {
    'Humanidity': '%',
    'Temperature': '°C',
    'Pressure': 'hPa',
    'Motion': 'движ/мин',
    'Light': 'люкс'
  }
  
  const unit = units[sensorId] || ''
  const formattedValue = typeof value === 'number' ? value.toFixed(1) : value
  return `${formattedValue} ${unit}`
}

const toggleSensor = (sensor) => {
  expandedSensor.value = expandedSensor.value === sensor.sensorId ? null : sensor.sensorId
}

const refreshAllSensors = async () => {
  await sensorsStore.fetchAllLatestReadings()
}

onMounted(async () => {
  await systemStore.fetchStatus()
  await sensorsStore.fetchSensors()
  await sensorsStore.fetchAllLatestReadings()
})
</script>

<style scoped>
.home-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid #e2e8f0;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #2d3748;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-role {
  font-size: 0.9rem;
  color: #4a5568;
  background: #edf2f7;
  padding: 6px 12px;
  border-radius: 6px;
}

.logout-btn {
  background: #e53e3e;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.logout-btn:hover {
  background: #c53030;
}

.btn-audit{
  background: #4a5568;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}
.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.compact-card {
  background: white;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #f7fafc;
  border-bottom: 1px solid #e2e8f0;
}

.card-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2d3748;
}

.icon-btn {
  background: none;
  border: none;
  color: #718096;
  cursor: pointer;
  font-size: 1.1rem;
  padding: 4px 8px;
  border-radius: 4px;
  transition: all 0.2s;
}

.icon-btn:hover:not(:disabled) {
  background: #edf2f7;
  color: #4a5568;
}

.icon-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading {
  display: flex;
  justify-content: center;
  padding: 20px;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid #e2e8f0;
  border-top-color: #4299e1;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Статус системы */
.status-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  padding: 16px;
}

.status-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-label {
  font-size: 0.8rem;
  color: #718096;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-value {
  font-size: 1rem;
  font-weight: 600;
  color: #2d3748;
}

.status-ok {
  color: #38a169;
}

.status-warning {
  color: #d69e2e;
}

.status-desc {
  font-size: 0.75rem;
  color: #a0aec0;
}

/* История телеметрии */
.filters {
  display: flex;
  gap: 8px;
}

.filter-select {
  padding: 6px 10px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  font-size: 0.8rem;
  color: #4a5568;
  background: white;
  min-width: 120px;
}

.telemetry-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #94a3b8;
}

.placeholder-icon {
  font-size: 2rem;
  margin-bottom: 12px;
}

.placeholder-text {
  font-size: 0.9rem;
  font-weight: 500;
  margin-bottom: 4px;
}

.placeholder-subtext {
  font-size: 0.8rem;
  text-align: center;
  max-width: 300px;
  line-height: 1.4;
}

/* Датчики */
.sensors-list {
  padding: 0;
}

.sensor-item {
  padding: 12px 16px;
  border-bottom: 1px solid #f1f5f9;
  cursor: pointer;
  transition: background-color 0.2s;
}

.sensor-item:hover {
  background: #f8fafc;
}

.sensor-item:last-child {
  border-bottom: none;
}

.sensor-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.sensor-name {
  font-weight: 500;
  color: #2d3748;
}

.sensor-id {
  font-size: 0.8rem;
  color: #718096;
  background: #edf2f7;
  padding: 2px 6px;
  border-radius: 4px;
}

.sensor-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-ok {
  background: #38a169;
  box-shadow: 0 0 0 2px rgba(56, 161, 105, 0.2);
}

.status-warning {
  background: #d69e2e;
  box-shadow: 0 0 0 2px rgba(214, 158, 46, 0.2);
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

.toggle-icon {
  font-size: 0.8rem;
  color: #718096;
}

.sensor-details {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed #e2e8f0;
}

.sensor-reading {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: #4a5568;
}

.reading-value {
  font-weight: 600;
  color: #2d3748;
}

.no-data {
  font-size: 0.85rem;
  color: #a0aec0;
  text-align: center;
  padding: 8px;
}

/* Адаптивность */
@media (max-width: 768px) {
  .home-container {
    padding: 12px;
  }
  
  .page-header {
    flex-direction: column;
    gap: 12px;
    text-align: center;
  }
  
  .main-content {
    grid-template-columns: 1fr;
  }
  
  .status-grid {
    grid-template-columns: 1fr;
  }
  
  .filters {
    flex-direction: column;
  }
  
  .filter-select {
    min-width: 100%;
  }
}
</style>