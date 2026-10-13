import api from './client';

export interface TicketImpressao {
  codigo: string;
  nomeItem: string;
  eventoNome: string;
  numeroTicket: number;
  totalTickets: number;
}

export async function imprimirTickets(tickets: TicketImpressao[]): Promise<void> {
  await api.post('/impressao/imprimir', { tickets });
}

// Mantemos essas funções para compatibilidade com CaixaPage
export function impressoraConectada(): boolean {
  return true; // sempre true — impressora gerenciada pelo backend
}

export function desconectarImpressora(): void {
  // não necessário — backend gerencia a conexão
}

export async function conectarImpressora(): Promise<string> {
  return 'HPRT MPT-II'; // backend gerencia a conexão
}