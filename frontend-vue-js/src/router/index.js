import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    redirect: '/login',
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/Register.vue'),
  },
  {
    path: '/Home',
    name: 'Dashboard-HomeAssistant',
    component: () => import('@/views/Home.vue'),
    meta: { requiresAuth: true },
  },
  {
      path: '/audit-security',
      name: 'AuditSecurity',
      component: () => import('@/views/AuditSecurity.vue'),
      meta: { 
        requiresAuth: true,
        requiredRoles: ['Administrator', 'Auditor'] // Роли из вашего токена
      }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/login',
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Навигационные хуки
router.beforeEach((to, from, next) => {

  
  const isAuthenticated = !!localStorage.getItem('jwt_token')

  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
  } else if ((to.name === 'Login' || to.name === 'Register') && isAuthenticated) {
    next('/Home')
  } else {
    next()
  }
})

export default router