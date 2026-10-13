import api from './client';
import type { LoginRequest, LoginResponse } from '../types';
export const login = async (dto: LoginRequest): Promise<LoginResponse> => {
  const { data } = await api.post<LoginResponse>('/usuarios/login', dto);
  return data;
};

export const logout = () => {
  localStorage.removeItem('kerma_token');
  localStorage.removeItem('kerma_usuario');
  window.location.href = '/login';
};