<template>
  <section class="card bg-gradient-to-r from-blue-50 to-indigo-50">
    <div class="card-header">
      <div class="header-icon">📊</div>
      <h2 class="card-title">Статус системы</h2>
      <button 
        @click="store.fetchStatus" 
        class="btn-refresh"
        :disabled="store.isLoading"
      >
        <span :class="['refresh-icon', store.isLoading ? 'animate-spin' : '']">🔄</span>
      </button>
    </div>
    
    <div v-if="store.isLoading" class="loading-state">
      <div class="spinner"></div>
      <span>Загрузка статуса...</span>
    </div>
    
    <div v-else-if="store.error" class="error-state">
      <div class="error-icon">⚠️</div>
      <div>
        <p class="error-title">Ошибка загрузки</p>
        <p class="error-message">{{ store.error }}</p>
      </div>
      <button @click="store.fetchStatus" class="btn-retry">
        Повторить
      </button>
    </div>
    
    <div v-else class="metrics-grid">
      <MetricItem 
        label="Статус системы" 
        :value="store.status?.status || 'Неизвестно'"
        :variant="getStatusVariant(store.status?.status)"
        :description="getStatusDescription(store.status?.status)"
      />
      
      <MetricItem 
        label="Версия ПО" 
        :value="store.status?.version || '1.0.0'"
        variant="info"
        description="Текущая версия системы"
      />
      
      <MetricItem 
        label="Время работы" 
        :value="formatUptime(store.status?.uptime)"
        :variant="getUptimeVariant(store.status?.uptime)"
        description="С момента последнего запуска"
      />
      
      <MetricItem 
        label="Время сервера" 
        :value="formatDateTime(store.status?.timestamp)"
        variant="default"
        description="Текущее системное время"
      />
    </div>
  </section>
</template>

<script setup>
import { onMounted } from 'vue'
import MetricItem from '@/components/ui/MetricItem.vue'
import { useSystemStore } from '@/stores/useSystemStore'

const store = useSystemStore()

const getStatusVariant = (statusText) => {
  const statusMap = {
    'Operational': 'success',
    'Degraded': 'warning',
    'Down': 'danger',
    'Maintenance': 'info'
  }
  return statusMap[statusText] || 'warning'
}

const getStatusDescription = (statusText) => {
  const descriptions = {
    'Operational': 'Все системы работают нормально',
    'Degraded': 'Частичные проблемы в работе',
    'Down': 'Система не работает',
    'Maintenance': 'Проводятся технические работы'
  }
  return descriptions[statusText] || 'Неизвестный статус'
}

const getUptimeVariant = (uptimeSeconds) => {
  if (!uptimeSeconds) return 'default'
  const hours = uptimeSeconds / 3600
  if (hours > 24) return 'success'
  if (hours > 12) return 'info'
  if (hours > 1) return 'warning'
  return 'danger'
}

const formatUptime = (seconds) => {
  if (!seconds) return '—'
  
  const days = Math.floor(seconds / (3600 * 24))
  const hours = Math.floor((seconds % (3600 * 24)) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  
  if (days > 0) {
    return `${days}д ${hours}ч ${minutes}м`
  } else if (hours > 0) {
    return `${hours}ч ${minutes}м`
  } else {
    return `${minutes}м`
  }
}

const formatDateTime = (isoString) => {
  if (!isoString) return '—'
  const date = new Date(isoString)
  return date.toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

// Загружаем статус при монтировании
onMounted(() => {
  store.fetchStatus()
})
</script>

<style scoped>
.card {
  @apply rounded-xl shadow-sm border border-gray-200 overflow-hidden;
}

.card-header {
  @apply flex items-center gap-3 px-6 py-4 bg-white border-b border-gray-200;
}

.header-icon {
  @apply text-xl;
}

.card-title {
  @apply text-lg font-semibold text-gray-800 flex-1;
}

.btn-refresh {
  @apply p-2 rounded-lg hover:bg-gray-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed;
}

.refresh-icon {
  @apply text-lg;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.loading-state {
  @apply flex flex-col items-center justify-center py-12;
}

.spinner {
  @apply w-8 h-8 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin mb-3;
}

.error-state {
  @apply flex flex-col items-center justify-center py-8 px-4 text-center;
}

.error-icon {
  @apply text-2xl mb-2;
}

.error-title {
  @apply font-semibold text-red-600 mt-3 mb-1;
}

.error-message {
  @apply text-sm text-gray-600 mb-4;
}

.btn-retry {
  @apply px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm;
}

.metrics-grid {
  @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 p-6;
}

/* Адаптивность */
@media (max-width: 768px) {
  .metrics-grid {
    @apply grid-cols-1 gap-3 p-4;
  }
  
  .card-header {
    @apply px-4 py-3;
  }
}
</style>