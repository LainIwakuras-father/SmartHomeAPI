<template>
  <section class="card bg-gradient-to-r from-blue-50 to-indigo-50">
    <div class="card-header">
      <Icon name="system-status" class="w-6 h-6 text-indigo-600" />
      <h2 class="card-title">Статус системы</h2>
    </div>
    
    <div v-if="isLoading" class="loading-spinner">Загрузка...</div>
    
    <div v-else-if="error" class="error-alert">
      {{ error }}
    </div>
    
    <div v-else-if="status" class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <MetricItem 
        label="Статус" 
        :value="status.status" 
        :variant="status.status === 'Operational' ? 'success' : 'warning'"
      />
      <MetricItem label="Версия" :value="status.version" />
      <MetricItem 
        label="Аптайм" 
        :value="formatUptime(status.uptime)"
        icon="clock"
      />
      <MetricItem 
        label="Время сервера" 
        :value="formatDateTime(status.timestamp)"
        icon="calendar"
      />
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import { onMounted } from 'vue'
import { useSystemStore } from '@/stores/useSystemStore'
import MetricItem from '@/components/ui/MetricItem.vue'

const store = useSystemStore()
const { status, isLoading, error } = store

// Загружаем статус при монтировании
onMounted(() => {
  store.fetchStatus()
})

const formatUptime = (seconds) => {
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours}ч ${minutes}м`
}

const formatDateTime = (isoString) => {
  return new Date(isoString).toLocaleString('ru-RU')
}
</script>