import api from './client';
import type { Venda, PagamentoResponse } from '../types';

export const criarVenda = async (dto: {
  eventoId: number;
  caixaId: number;
  formaPagamento: string;
}): Promise<Venda> => {
  const { data } = await api.post<Venda>('/vendas', dto);
  return data;
};

export const adicionarItem = async (
  vendaId: number,
  dto: { itemId: number; quantidade: number }
): Promise<Venda> => {
  const { data } = await api.post<Venda>(`/vendas/${vendaId}/itens`, dto);
  return data;
};

export const confirmarPagamento = async (vendaId: number): Promise<PagamentoResponse> => {
  const { data } = await api.patch<PagamentoResponse>(`/vendas/${vendaId}/pagar`);
  return data;
};

export const cancelarVenda = async (vendaId: number): Promise<Venda> => {
  const { data } = await api.patch<Venda>(`/vendas/${vendaId}/cancelar`);
  return data;
};

export const listarVendasPorCaixa = async (caixaId: number): Promise<Venda[]> => {
  const { data } = await api.get<Venda[]>(`/vendas/caixa/${caixaId}`);
  return data;
};