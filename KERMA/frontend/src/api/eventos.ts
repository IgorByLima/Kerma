import api from './client';
import type { Evento } from '../types';
export const listarEventos = async (): Promise<Evento[]> => {
  const { data } = await api.get<Evento[]>('/eventos');
  return data;
};

export const obterEvento = async (id: number): Promise<Evento> => {
  const { data } = await api.get<Evento>(`/eventos/${id}`);
  return data;
};