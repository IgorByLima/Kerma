// ─── Auth ─────────────────────────────────────────────────────────────────────
export interface LoginRequest {
  codigo: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  nome: string;
  perfil: string;
  eventoId: number;
  eventoCodigo: string;
  eventoNome: string;
  expiracao: string;
}

// ─── Usuario ──────────────────────────────────────────────────────────────────
export interface Usuario {
  id: number;
  nome: string;
  email: string;
  codigo: string;
  perfil: string;
  ativo: boolean;
  criadoEm: string;
}

// ─── Evento ───────────────────────────────────────────────────────────────────
export interface Evento {
  id: number;
  nome: string;
  codigo: string;
  descricao?: string;
  dataInicio: string;
  dataFim: string;
  status: string;
  criadoPor: number;
  criadoEm: string;
}

// ─── Item ─────────────────────────────────────────────────────────────────────
export interface Item {
  id: number;
  eventoId: number;
  nome: string;
  descricao?: string;
  custo: number;
  precoVenda: number;
  margemAbsoluta: number;
  margemPercentual: number;
  estoqueInicial: number;
  estoqueAtual: number;
  estoqueMinimo: number;
  estoqueCritico: boolean;
  statusEstoque: string;
  ativo: boolean;
  criadoEm: string;
}

// ─── Caixa ────────────────────────────────────────────────────────────────────
export interface Caixa {
  id: number;
  eventoId: number;
  operadorId: number;
  saldoInicial: number;
  saldoFinal?: number;
  abertoEm: string;
  fechadoEm?: string;
  status: string;
  observacao?: string;
}

// ─── Venda ────────────────────────────────────────────────────────────────────
export interface ItemVenda {
  id: number;
  itemId: number;
  nomeItem: string;
  quantidade: number;
  precoUnitario: number;
  custoUnitario: number;
  subtotal: number;
}

export interface Venda {
  id: number;
  eventoId: number;
  caixaId: number;
  operadorId: number;
  total: number;
  formaPagamento: string;
  status: string;
  origem: string;
  criadoEm: string;
  pagoEm?: string;
  itens: ItemVenda[];
}

// ─── Ticket ───────────────────────────────────────────────────────────────────
export interface Ticket {
  id: number;
  itemVendaId: number;
  codigo: string;
  barcode: string;
  status: string;
}

export interface PagamentoResponse {
  venda: Venda;
  tickets: Ticket[];
}
export interface TicketResponse {
  id: number;
  codigo: string;
  status: string;
  nomeCliente: string | null;
  nomeItem: string;
  eventoNome: string;
  retiradoPor: string | null;
  retiradoEm: string | null;
  enviadoFilaEm: string | null;
  prontoEm: string | null;
  criadoEm: string;
}