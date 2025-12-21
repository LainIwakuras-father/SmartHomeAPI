

<template>
  <div class="auth-page">
    <div class="auth-container">
      <h1 class="auth-title">Войти</h1>
      <p class="auth-subtitle">SmartHomeAPI</p>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <div class="form-group">
          <label for="username" class="form-label">Логин</label>
          <input
            id="username"
            v-model="form.username"
            type="text"
            required
            class="form-input"
            placeholder="Введите свой Логин"
          />
        </div>

        <div class="form-group">
          <label for="password" class="form-label">Пароль</label>
          <input
            id="password"
            v-model="form.password"
            type="password"
            required
            class="form-input"
            placeholder="Введите свой пароль"
          />
        </div>

        <button
          type="submit"
          class="auth-button"
          :disabled="authStore.isLoading"
        >
          {{ authStore.isLoading ? 'входим...' : 'Войти' }}
        </button>

        <div v-if="authStore.error" class="error-message">
          {{ authStore.error }}
        </div>

        <div class="auth-footer">
          <p>
            У вас нет аккаунта?
            <router-link to="/register" class="auth-link">
              Зарегистрироватся
            </router-link>
          </p>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  username: '',
  password: '',
})

const handleSubmit = async () => {
  const result = await authStore.login(form)
  
  if (result.success) {
    router.push('/Home')
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.auth-container {
  width: 100%;
  max-width: 400px;
  background: white;
  border-radius: 12px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.auth-title {
  font-size: 28px;
  font-weight: 700;
  color: #333;
  margin-bottom: 8px;
  text-align: center;
}

.auth-subtitle {
  color: #666;
  text-align: center;
  margin-bottom: 32px;
}

.auth-form {
  margin-top: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-weight: 500;
  color: #555;
  margin-bottom: 8px;
  font-size: 14px;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e1e5e9;
  border-radius: 8px;
  font-size: 16px;
  transition: border-color 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #667eea;
}

.auth-button {
  width: 100%;
  padding: 14px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
  margin-top: 8px;
}

.auth-button:hover:not(:disabled) {
  opacity: 0.95;
}

.auth-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.error-message {
  margin-top: 16px;
  padding: 12px;
  background-color: #fee;
  color: #c33;
  border-radius: 8px;
  font-size: 14px;
  text-align: center;
}

.auth-footer {
  margin-top: 24px;
  text-align: center;
  color: #666;
  font-size: 14px;
}

.auth-link {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.auth-link:hover {
  text-decoration: underline;
}
</style>