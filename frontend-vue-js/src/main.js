// import { createApp } from 'vue'
// import App from './App.vue'
// import { createRouter, createWebHistory } from 'vue-router';
// import { createPinia } from 'pinia';
// import Home from './views/Home.vue';
// import Login from './views/Login.vue';
// import Register from './views/Register.vue';

// const router = createRouter({
//     history: createWebHistory(),
//     routes: [
//         { path: '/', component: Home, meta: { requiresAuth: true } },
//         { path: '/login', component: Login },
//         { path: '/register', component: Register }
//     ]
// });
// router.beforeEach(async (to, from) => {
//     if (to.meta.requiresAuth) {
//         const token = localStorage.getItem('token');
//         if (!token) {
//             return { path: '/login' };
//         }
//     }
// });

// const pinia = createPinia();

// const app = createApp(App);
// app.use(pinia);
// app.use(router);
// app.mount('#app');

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './style.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')