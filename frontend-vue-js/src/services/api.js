import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5033/api'

const apiClient = axios.create({
  // baseURL: '/api',
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Добавляем токен к каждому запросу (если используется авторизация)
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('jwt_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Статус системы
export const statusService = {
  getStatus() {
    return apiClient.get('/Status')
  }
}

// Датчики
export const sensorsService = {
  getAll() {
    return apiClient.get('/Sensors')
  },
  // getById(id) {
  //   return apiClient.get(`/sensors/${id}`)
  // }
}

// Телеметрия
export const telemetryService = {
  getLatest(sensorId) {
    return apiClient.get(`/Telemetry/${sensorId}/latest`)
  },
  getHistory(params) {
    return apiClient.get('/Telemetry/history', { params })
  },
  streamData(sensorId) {
    return new EventSource(`${API_BASE_URL}/Telemetry/${sensorId}/data`)
  }
}

export default apiClient