<!-- <template>
  <div class="telemetry-history">
    <h3>Telemetry History</h3>
    <p>History component placeholder</p>
  </div>
</template>

<script>
export default {
  name: 'TelemetryHistory'
}
</script>

<style scoped>
.telemetry-history {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 16px;
  margin: 10px 0;
  background: #f9f9f9;
}
</style> -->

<template>
  <div class="telemetry-history">
    <div class="history-filters">
      <div class="filter-group">
        <label class="filter-label">Период:</label>
        <select v-model="selectedPeriod" class="filter-select">
          <option value="1">Последний час</option>
          <option value="24">Последние 24 часа</option>
          <option value="168">Последняя неделя</option>
          <option value="720">Последний месяц</option>
        </select>
      </div>
      
      <div class="filter-group">
        <label class="filter-label">Датчик:</label>
        <select v-model="selectedSensor" class="filter-select">
          <option value="all">Все датчики</option>
          <option value="Humanidity">Влажность</option>
          <option value="Temperature">Температура</option>
          <option value="Pressure">Давление</option>
          <option value="Motion">Движение</option>
          <option value="Light">Освещенность</option>
        </select>
      </div>
    </div>
    
    <div class="history-chart">
      <div class="chart-placeholder">
        <Icon name="bar-chart" class="chart-icon" />
        <p class="chart-text">График телеметрии</p>
        <p class="chart-subtext">Здесь будет отображаться график данных за выбранный период</p>
      </div>
    </div>
    
    <div class="history-stats">
      
      
      
      
      
      
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const selectedPeriod = ref('24')
const selectedSensor = ref('all')
</script>

<style scoped>
.telemetry-history {
  padding: 20px;
}

.history-filters {
  display: flex;
  gap: 20px;
  margin-bottom: 25px;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 10px;
}

.filter-label {
  color: #4a5568;
  font-weight: 500;
  white-space: nowrap;
}

.filter-select {
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: white;
  color: #2d3748;
  min-width: 180px;
  transition: all 0.2s;
}

.filter-select:focus {
  outline: none;
  border-color: #4c51bf;
  box-shadow: 0 0 0 3px rgba(76, 81, 191, 0.1);
}

.history-chart {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-radius: 12px;
  padding: 40px;
  margin-bottom: 25px;
  border: 1px solid #e2e8f0;
}

.chart-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
}

.chart-icon {
  width: 64px;
  height: 64px;
  margin-bottom: 16px;
}

.chart-text {
  font-size: 1.2rem;
  font-weight: 600;
  margin-bottom: 8px;
}

.chart-subtext {
  font-size: 0.9rem;
  text-align: center;
  max-width: 400px;
}

.history-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
  border-color: #cbd5e0;
}

.stat-icon {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-label {
  color: #718096;
  font-size: 0.85rem;
  margin-bottom: 4px;
}

.stat-value {
  font-size: 1.3rem;
  font-weight: 600;
  color: #2d3748;
}

@media (max-width: 768px) {
  .history-filters {
    flex-direction: column;
    gap: 12px;
  }
  
  .filter-group {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .filter-select {
    width: 100%;
  }
  
  .history-stats {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 480px) {
  .history-stats {
    grid-template-columns: 1fr;
  }
  
  .history-chart {
    padding: 20px;
  }
}
</style>