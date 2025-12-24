
import { defineStore } from 'pinia'
import api from '@/services/api'
// import router from '@/router'
export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('jwt_token'),
    isLoading: false,
    error: null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
  },

  actions: {
    async login(credentials) {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await api.post('/Auth/login', credentials)
        const { token } = response.data
        
        this.token = token
        localStorage.setItem('jwt_token', token)
        
        // Получаем информацию о пользователе
        await this.fetchUser()
        
        return { success: true }
      } catch (error) {
        this.error = error.response?.data?.error || 'Login failed'
        return { success: false, error: this.error }
      } finally {
        this.isLoading = false
      }
    },

    async register(userData) {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await api.post('/Auth/register', userData)
        
        // После регистрации автоматически логиним
        if (response.status === 200) {
          return await this.login({
            username: userData.username,
            password: userData.password,
          })
        }
        
        return { success: true }
      } catch (error) {
        this.error = error.response?.data?.error || 'Registration failed'
        return { success: false, error: this.error }
      } finally {
        this.isLoading = false
      }
    },

    async fetchUser() {
      try {
        // Если у вас есть endpoint для получения информации о пользователе
        // const response = await api.get('/auth/me')
        // this.user = response.data
        
        // Или парсим из токена
        if (this.token) {
          const payload = JSON.parse(atob(this.token.split('.')[1]))
          this.user = {
            username: payload.unique_name,
            role: payload.role
          }
        }
      } catch (error) {
        // console.error('Failed to fetch user:', error)
        this.error = error.response?.data?.message || 'Login failed'
        return { success: false, error: this.error }
      } finally {
        this.isLoading = false
      }
    },

    logout() {
      this.user = null
      this.token = null
      this.error = null
      // eslint-disable-next-line no-undef
      localStorage.removeItem('jwt_token')
      // eslint-disable-next-line no-undef
      window.location.href = '/login'
    },
  },
})