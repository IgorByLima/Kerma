import api from './client';
import type { Item } from '../types';

export const listarItensPorEvento = async (eventoId: number): Promise<Item[]> => {
  const { data } = await api.get<Item[]>(`/itens/evento/${eventoId}`);
  return data;
};

export const listarItensCriticos = async (eventoId: number): Promise<Item[]> => {
  const { data } = await api.get<Item[]>(`/itens/evento/${eventoId}/criticos`);
  return data;
};