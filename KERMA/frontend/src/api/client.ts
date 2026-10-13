import axios from 'axios';

const api = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL ?? 'https://localhost:7205'}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor — anexa o token JWT em toda requisição automaticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('kerma_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor — redireciona para login se o token expirar
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('kerma_token');
      localStorage.removeItem('kerma_usuario');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;