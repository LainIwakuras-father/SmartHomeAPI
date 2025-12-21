<template>
  <div class="home-container">
    <!-- Статус системы -->
    <SystemStatusCard />
    
    <!-- Датчики -->
    <SensorsDashboard />
    
    <!-- История телеметрии -->
    <TelemetryHistory />
  </div>
</template>

<script setup>
import SystemStatusCard from '@/components/home/SystemStatusCard.vue'
import SensorsDashboard from '@/components/home/SensorsDashboard.vue'
import TelemetryHistory from '@/components/home/TelemetryHistory.vue'
</script>

<style scoped>
.home-container {
  @apply p-6 max-w-7xl mx-auto space-y-8;
}
</style>