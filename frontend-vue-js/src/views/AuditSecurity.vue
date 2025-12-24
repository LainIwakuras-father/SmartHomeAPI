<template>
  <div class="audit-security-container">
    <!-- Заголовок страницы -->
    <div class="page-header">
      <h1 class="page-title">
        Аудит безопасности системы
      </h1>
      <div class="user-info">
        <span class="user-role">Роль: {{ userRole }}</span>
        <button @click="refreshAll" class="btn-refresh" :disabled="auditStore.isLoading">
          {{ auditStore.isLoading ? 'Обновление...' : '🔄 Обновить все' }}
        </button>
        <button @click="$router.push('/home')" class="btn-home">
          На главную
        </button>
      </div>
    </div>

    <!-- Статистика в карточках -->
    <section class="stats-section">
      <div class="section-header">
        <h2>📊 Общая статистика аудита</h2>
      </div>
      
      <div v-if="auditStore.isLoading && !auditStore.stats" class="loading-state">
        <div class="spinner"></div>
        <span>Загрузка статистики...</span>
      </div>
      
      <div v-else-if="auditStore.error" class="error-state">
        <span>⚠️ Ошибка: {{ auditStore.error }}</span>
        <button @click="refreshAll" class="btn-retry">Повторить</button>
      </div>
      
      <div v-else class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon">📈</div>
          <div class="stat-content">
            <span class="stat-value">{{ auditStore.stats?.totalEvents || 0 }}</span>
            <span class="stat-label">Всего событий</span>
          </div>
        </div>
        
        <div class="stat-card success">
          <div class="stat-icon">✅</div>
          <div class="stat-content">
            <span class="stat-value">{{ auditStore.stats?.successfulEvents || 0 }}</span>
            <span class="stat-label">Успешных</span>
            <span class="stat-percent" v-if="auditStore.stats?.totalEvents">
              {{ calculatePercentage(auditStore.stats.successfulEvents, auditStore.stats.totalEvents) }}%
            </span>
          </div>
        </div>
        
        <div class="stat-card warning">
          <div class="stat-icon">⚠️</div>
          <div class="stat-content">
            <span class="stat-value">{{ auditStore.stats?.failedEvents || 0 }}</span>
            <span class="stat-label">Неудачных</span>
          </div>
        </div>
        
        <div class="stat-card danger">
          <div class="stat-icon">🔒</div>
          <div class="stat-content">
            <span class="stat-value">{{ auditStore.stats?.recentSecurityBreaches || 0 }}</span>
            <span class="stat-label">Нарушений (7 дней)</span>
          </div>
        </div>
        
        <div class="stat-card info">
          <div class="stat-icon">📅</div>
          <div class="stat-content">
            <span class="stat-value">{{ auditStore.stats?.todayEvents || 0 }}</span>
            <span class="stat-label">Событий сегодня</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Основное содержимое в двух колонках -->
    <div class="audit-content">
      <!-- Левая колонка: Фильтры и события -->
      <div class="left-column">
        <!-- Фильтры событий -->
        <section class="filters-section">
          <div class="section-header">
            <h2>🔍 Фильтры событий</h2>
            <button @click="applyFilters" class="btn-apply" :disabled="auditStore.isLoading">
              Применить
            </button>
            <button @click="resetFilters" class="btn-reset">
              Сбросить
            </button>
          </div>
          
          <div class="filters-grid">
            <div class="filter-group">
              <label>Период с:</label>
              <input 
                type="datetime-local" 
                v-model="localFilters.from"
                @change="updateLocalFilters"
              />
            </div>
            
            <div class="filter-group">
              <label>Период по:</label>
              <input 
                type="datetime-local" 
                v-model="localFilters.to"
                @change="updateLocalFilters"
              />
            </div>
            
            <div class="filter-group">
              <label>Пользователь:</label>
              <input 
                type="text" 
                v-model="localFilters.user"
                @input="updateLocalFilters"
                placeholder="Введите имя пользователя"
              />
            </div>
            
            <div class="filter-group">
              <label>Действие:</label>
              <select v-model="localFilters.action" @change="updateLocalFilters">
                <option value="">Все действия</option>
                <option value="LOGIN">Вход в систему</option>
                <option value="LOGOUT">Выход из системы</option>
                <option value="DATA_ACCESS">Доступ к данным</option>
                <option value="SECURITY_BREACH">Нарушение безопасности</option>
                <option value="USER_CREATED">Создание пользователя</option>
                <option value="USER_MODIFIED">Изменение пользователя</option>
              </select>
            </div>
            
            <div class="filter-group">
              <label>Статус:</label>
              <select v-model="localFilters.isSuccessful" @change="updateLocalFilters">
                <option :value="null">Все статусы</option>
                <option :value="true">Только успешные</option>
                <option :value="false">Только неудачные</option>
              </select>
            </div>
          </div>
        </section>

        <!-- Таблица событий -->
        <section class="events-section">
          <div class="section-header">
            <h2>📋 События аудита</h2>
            <span class="events-count">
              Всего: {{ auditStore.events.length }}
            </span>
          </div>
          
          <div v-if="auditStore.isLoading" class="loading-state">
            <div class="spinner small"></div>
            <span>Загрузка событий...</span>
          </div>
          
          <div v-else class="events-table">
            <table>
              <thead>
                <tr>
                  <th @click="sortEvents('timestamp')">
                    Время {{ sortIcon('timestamp') }}
                  </th>
                  <th @click="sortEvents('userName')">
                    Пользователь {{ sortIcon('userName') }}
                  </th>
                  <th>Действие</th>
                  <th>Статус</th>
                  <th>Ресурс</th>
                </tr>
              </thead>
              <tbody>
                <tr 
                  v-for="event in sortedEvents" 
                  :key="event.id"
                  @click="selectEvent(event)"
                  :class="{
                    'success-row': event.isSuccessful,
                    'error-row': !event.isSuccessful,
                    'selected': selectedEventId === event.id
                  }"
                >
                  <td>{{ formatDateTime(event.timestamp) }}</td>
                  <td>{{ event.userName || 'System' }}</td>
                  <td>{{ translateAction(event.action) }}</td>
                  <td>
                    <span :class="['status-badge', event.isSuccessful ? 'success' : 'error']">
                      {{ event.isSuccessful ? '✅ Успешно' : '❌ Ошибка' }}
                    </span>
                  </td>
                  <td>{{ event.resource || '—' }}</td>
                </tr>
              </tbody>
            </table>
            
            <div v-if="auditStore.events.length === 0" class="empty-state">
              Нет событий для отображения
            </div>
          </div>
        </section>
      </div>

      <!-- Правая колонка: Детали и активные пользователи -->
      <div class="right-column">
        <!-- Детали выбранного события -->
        <section class="details-section" v-if="selectedEvent">
          <div class="section-header">
            <h2>📄 Детали события</h2>
            <button @click="selectedEvent = null" class="btn-close">
              ✕
            </button>
          </div>
          
          <div class="event-details">
            <div class="detail-row">
              <span class="detail-label">ID:</span>
              <span class="detail-value">{{ selectedEvent.id }}</span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">Время:</span>
              <span class="detail-value">{{ formatDateTime(selectedEvent.timestamp) }}</span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">Пользователь:</span>
              <span class="detail-value">{{ selectedEvent.userName || 'System' }}</span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">Действие:</span>
              <span class="detail-value">{{ translateAction(selectedEvent.action) }}</span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">Статус:</span>
              <span :class="['detail-value', selectedEvent.isSuccessful ? 'success' : 'error']">
                {{ selectedEvent.isSuccessful ? '✅ Успешно' : '❌ Ошибка' }}
              </span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">Ресурс:</span>
              <span class="detail-value">{{ selectedEvent.resource || '—' }}</span>
            </div>
            
            <div class="detail-row">
              <span class="detail-label">IP-адрес:</span>
              <span class="detail-value">{{ selectedEvent.ipAddress || '—' }}</span>
            </div>
            
            <div class="detail-row" v-if="selectedEvent.details">
              <span class="detail-label">Детали:</span>
              <span class="detail-value details-text">{{ selectedEvent.details }}</span>
            </div>
          </div>
        </section>

        <!-- Самые активные пользователи -->
        <section class="active-users-section">
          <div class="section-header">
            <h2>👥 Самые активные пользователи</h2>
          </div>
          
          <div class="users-list">
            <div 
              v-for="user in auditStore.stats?.mostActiveUsers || []" 
              :key="user.userName"
              class="user-item"
            >
              <div class="user-avatar">{{ getUserInitial(user.userName) }}</div>
              <div class="user-info">
                <span class="user-name">{{ user.userName }}</span>
                <span class="user-stats">
                  {{ user.eventCount }} событий • 
                  
                </span>
              </div>
              <div class="user-activity">
                <span class="last-activity">
                  {{ formatTimeAgo(user.lastActivity) }}
                </span>
              </div>
            </div>
          </div>
          
          <div v-if="!auditStore.stats?.mostActiveUsers?.length" class="empty-state">
            Нет данных об активных пользователях
          </div>
        </section>

        <!-- Последние нарушения безопасности -->
        <section class="breaches-section">
          <div class="section-header">
            <h2>🚨 Последние нарушения</h2>
            <button @click="fetchRecentBreaches" class="btn-refresh-small">
              🔄
            </button>
          </div>
          
          <div class="breaches-list">
            <div 
              v-for="breach in auditStore.recentBreaches" 
              :key="breach.timestamp"
              class="breach-item"
            >
              <div class="breach-icon">⚠️</div>
              <div class="breach-content">
                <span class="breach-title">{{ breach.action }}</span>
                <span class="breach-time">{{ formatDateTime(breach.timestamp) }}</span>
                <span class="breach-details" v-if="breach.details">
                  {{ truncateText(breach.details, 60) }}
                </span>
              </div>
            </div>
          </div>
          
          <div v-if="auditStore.recentBreaches.length === 0" class="empty-state">
            Нарушений не обнаружено
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useAuditStore } from '@/stores/useAuditStore'

const router = useRouter()
const authStore = useAuthStore()
const auditStore = useAuditStore()

// Реактивные данные
const selectedEvent = ref(null)
const selectedEventId = ref(null)
const sortField = ref('timestamp')
const sortDirection = ref('desc')
const localFilters = ref({ ...auditStore.filters })

// Вычисляемые свойства
const userRole = computed(() => {
  return authStore.user?.role || 'User'
})

const sortedEvents = computed(() => {
  const events = [...auditStore.events]
  
  return events.sort((a, b) => {
    let aVal = a[sortField.value]
    let bVal = b[sortField.value]
    
    // Для строк делаем сравнение без учета регистра
    if (typeof aVal === 'string') {
      aVal = aVal.toLowerCase()
      bVal = bVal.toLowerCase()
    }
    
    if (aVal < bVal) return sortDirection.value === 'asc' ? -1 : 1
    if (aVal > bVal) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
})

// Методы
const calculatePercentage = (part, total) => {
  if (!total) return 0
  return Math.round((part / total) * 100)
}

const formatDateTime = (isoString) => {
  if (!isoString) return '—'
  const date = new Date(isoString)
  return date.toLocaleString('ru-RU')
}

const formatTimeAgo = (isoString) => {
  if (!isoString) return 'давно'
  const date = new Date(isoString)
  const now = new Date()
  const diffMs = now - date
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)
  
  if (diffMins < 60) return `${diffMins} мин назад`
  if (diffHours < 24) return `${diffHours} ч назад`
  return `${diffDays} дн назад`
}

const translateAction = (action) => {
  const translations = {
    'LOGIN': 'Вход в систему',
    'LOGOUT': 'Выход из системы',
    'DATA_ACCESS': 'Доступ к данным',
    'SECURITY_BREACH': 'Нарушение безопасности',
    'USER_CREATED': 'Создание пользователя',
    'USER_MODIFIED': 'Изменение пользователя',
    'USER_DELETED': 'Удаление пользователя',
    'OPC_READ': 'Чтение OPC',
    'OPC_WRITE': 'Запись OPC'
  }
  return translations[action] || action
}

const getUserInitial = (userName) => {
  if (!userName) return '?'
  return userName.charAt(0).toUpperCase()
}

const truncateText = (text, maxLength) => {
  if (!text) return ''
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

const sortEvents = (field) => {
  if (sortField.value === field) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortField.value = field
    sortDirection.value = 'asc'
  }
}

const sortIcon = (field) => {
  if (sortField.value !== field) return ''
  return sortDirection.value === 'asc' ? '↑' : '↓'
}

const selectEvent = (event) => {
  selectedEvent.value = event
  selectedEventId.value = event.id
}

const updateLocalFilters = () => {
  auditStore.updateFilters(localFilters.value)
}

const applyFilters = () => {
  auditStore.fetchEvents()
}

const resetFilters = () => {
  auditStore.resetFilters()
  localFilters.value = { ...auditStore.filters }
  auditStore.fetchEvents()
}

const refreshAll = async () => {
  await auditStore.refreshAll()
}

const fetchRecentBreaches = () => {
  auditStore.fetchRecentBreaches(10)
}

// Проверка роли при входе на страницу
onMounted(() => {
  const allowedRoles = ['Administrator', 'Auditor']
  const userRole = authStore.user?.role
  
  if (!userRole || !allowedRoles.includes(userRole)) {
    router.push('/home')
    return
  }
  
  // Загружаем данные
  auditStore.refreshAll()
})

// Следим за изменениями фильтров в store
watch(() => auditStore.filters, (newFilters) => {
  localFilters.value = { ...newFilters }
}, { deep: true })
</script>

<style scoped>
/* Основные стили страницы аудита */
.audit-security-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f8fafc 0%, #e0f2fe 100%);
  padding: 16px;
}

@media (min-width: 768px) {
  .audit-security-container {
    padding: 24px;
  }
}

/* Заголовок страницы */
.page-header {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 20px;
  margin-bottom: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

@media (min-width: 768px) {
  .page-header {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}

.page-title {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.user-info {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}

.user-role {
  background: #e0e7ff;
  color: #3730a3;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
}

/* Кнопки */
.btn-refresh, .btn-home, .btn-apply, .btn-reset, .btn-close, .btn-retry, .btn-refresh-small {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
}

.btn-refresh {
  background: #3b82f6;
  color: white;
}

.btn-refresh:hover {
  background: #2563eb;
}

.btn-refresh:disabled {
  background: #93c5fd;
  cursor: not-allowed;
  opacity: 0.7;
}

.btn-home {
  background: #e2e8f0;
  color: #475569;
}

.btn-home:hover {
  background: #cbd5e1;
}

.btn-apply {
  background: #10b981;
  color: white;
}

.btn-apply:hover {
  background: #059669;
}

.btn-reset {
  background: #e2e8f0;
  color: #475569;
}

.btn-reset:hover {
  background: #cbd5e1;
}

.btn-close {
  background: #fee2e2;
  color: #dc2626;
}

.btn-close:hover {
  background: #fecaca;
}

.btn-retry {
  background: #ef4444;
  color: white;
  margin-top: 8px;
}

.btn-retry:hover {
  background: #dc2626;
}

.btn-refresh-small {
  padding: 4px 8px;
  background: none;
  color: #64748b;
}

.btn-refresh-small:hover {
  background: #f1f5f9;
}

/* Секции */
.stats-section,
.filters-section,
.events-section,
.details-section,
.active-users-section,
.breaches-section {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #e2e8f0;
  overflow: hidden;
  margin-bottom: 24px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.section-header h2 {
  font-size: 18px;
  font-weight: 600;
  color: #334155;
  margin: 0;
}

/* Статистика */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(1, 1fr);
  gap: 16px;
  padding: 16px;
}

@media (min-width: 640px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .stats-grid {
    grid-template-columns: repeat(5, 1fr);
  }
}

.stat-card {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 16px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.stat-card.success {
  border-color: #a7f3d0;
  background: #f0fdf4;
}

.stat-card.warning {
  border-color: #fde68a;
  background: #fefce8;
}

.stat-card.danger {
  border-color: #fecaca;
  background: #fef2f2;
}

.stat-card.info {
  border-color: #bae6fd;
  background: #f0f9ff;
}

.stat-icon {
  font-size: 24px;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
}

.stat-label {
  font-size: 14px;
  color: #64748b;
  margin-top: 4px;
}

.stat-percent {
  font-size: 12px;
  color: #059669;
  font-weight: 500;
  margin-top: 4px;
}

/* Фильтры */
.filters-grid {
  display: grid;
  grid-template-columns: repeat(1, 1fr);
  gap: 16px;
  padding: 16px;
}

@media (min-width: 768px) {
  .filters-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .filters-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

.filter-group {
  display: flex;
  flex-direction: column;
}

.filter-group label {
  font-size: 14px;
  font-weight: 500;
  color: #475569;
  margin-bottom: 8px;
}

.filter-group input,
.filter-group select {
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 14px;
  color: #334155;
  background: white;
  transition: all 0.2s ease;
}

.filter-group input:focus,
.filter-group select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.filter-group input::placeholder {
  color: #94a3b8;
}

/* Основной контент */
.audit-content {
  display: grid;
  grid-template-columns: 1fr;
  gap: 24px;
}

@media (min-width: 1024px) {
  .audit-content {
    grid-template-columns: 2fr 1fr;
  }
}

.left-column {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.right-column {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Таблица событий */
.events-table {
  overflow-x: auto;
}

.events-table table {
  width: 100%;
  border-collapse: collapse;
}

.events-table thead {
  background: #f8fafc;
}

.events-table th {
  padding: 12px 16px;
  text-align: left;
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 2px solid #e2e8f0;
  cursor: pointer;
  user-select: none;
}

.events-table th:hover {
  background: #f1f5f9;
}

.events-table td {
  padding: 12px 16px;
  white-space: nowrap;
  font-size: 14px;
  color: #475569;
  border-bottom: 1px solid #f1f5f9;
}

.events-table tbody tr {
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.events-table tbody tr:hover {
  background: #f8fafc;
}

.success-row {
  background: #f0fdf4;
}

.success-row:hover {
  background: #dcfce7;
}

.error-row {
  background: #fef2f2;
}

.error-row:hover {
  background: #fee2e2;
}

.selected {
  background: #eff6ff !important;
  box-shadow: inset 0 0 0 2px #3b82f6;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 9999px;
  font-size: 12px;
  font-weight: 500;
}

.status-badge.success {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.error {
  background: #fee2e2;
  color: #991b1b;
}

.events-count {
  font-size: 14px;
  color: #64748b;
}

/* Детали события */
.event-details {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  align-items: flex-start;
}

.detail-label {
  width: 33.333%;
  font-size: 14px;
  font-weight: 500;
  color: #64748b;
}

.detail-value {
  width: 66.667%;
  font-size: 14px;
  color: #334155;
}

.detail-value.success {
  color: #059669;
  font-weight: 500;
}

.detail-value.error {
  color: #dc2626;
  font-weight: 500;
}

.details-text {
  background: #f8fafc;
  padding: 8px;
  border-radius: 6px;
  font-size: 13px;
  color: #475569;
}

/* Активные пользователи */
.users-list {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  transition: background-color 0.2s ease;
}

.user-item:hover {
  background: #f8fafc;
}

.user-avatar {
  width: 40px;
  height: 40px;
  background: #e0e7ff;
  color: #3730a3;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
  flex-shrink: 0;
}

.user-info {
  flex: 1;
}

.user-name {
  display: block;
  font-weight: 500;
  color: #334155;
  margin-bottom: 2px;
}

.user-stats {
  font-size: 12px;
  color: #64748b;
}

.success-rate {
  color: #059669;
  font-weight: 500;
}

.user-activity {
  font-size: 12px;
  color: #94a3b8;
}

/* Нарушения безопасности */
.breaches-list {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.breach-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  background: #fef2f2;
  border: 1px solid #fecaca;
}

.breach-icon {
  font-size: 20px;
  flex-shrink: 0;
}

.breach-content {
  flex: 1;
}

.breach-title {
  display: block;
  font-weight: 500;
  color: #991b1b;
  font-size: 14px;
  margin-bottom: 2px;
}

.breach-time {
  display: block;
  font-size: 12px;
  color: #dc2626;
  margin-bottom: 4px;
}

.breach-details {
  display: block;
  font-size: 12px;
  color: #6b7280;
}

/* Состояния загрузки и ошибок */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px 16px;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 4px solid #dbeafe;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 12px;
}

.spinner.small {
  width: 24px;
  height: 24px;
  border-width: 3px;
  margin-bottom: 8px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px 16px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  text-align: center;
}

.empty-state {
  text-align: center;
  padding: 32px 16px;
  color: #94a3b8;
  font-size: 14px;
}

/* Адаптивность */
@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .filters-grid {
    grid-template-columns: 1fr;
  }
  
  .audit-content {
    grid-template-columns: 1fr;
  }
  
  .page-header {
    padding: 16px;
  }
  
  .section-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .section-header h2 {
    text-align: center;
  }
  
  .detail-row {
    flex-direction: column;
    gap: 4px;
  }
  
  .detail-label,
  .detail-value {
    width: 100%;
  }
}
</style>