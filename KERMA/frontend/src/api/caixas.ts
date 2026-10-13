import api from './client';
import type { Caixa } from '../types';

export const abrirCaixa = async (dto: {
  eventoId: number;
  saldoInicial: number;
}): Promise<Caixa> => {
  const { data } = await api.post<Caixa>('/caixas/abrir', dto);
  return data;
};

export const fecharCaixa = async (
  id: number,
  dto: { saldoFinal: number; observacao?: string }
): Promise<Caixa> => {
  const { data } = await api.patch<Caixa>(`/caixas/${id}/fechar`, dto);
  return data;
};

export const obterCaixaAberto = async (eventoId: number): Promise<Caixa | null> => {
  try {
    const { data } = await api.get<Caixa>(`/caixas/aberto/evento/${eventoId}`);
    return data;
  } catch {
    return null;
  }
};