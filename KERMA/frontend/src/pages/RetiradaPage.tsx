import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '../hooks/AuthContext';
import api from '../api/client';
import type { TicketResponse } from '../types';
import Navbar from '../components/Navbar';

const Logo = ({ size = 30 }: { size?: number }) => (
  <div style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

const STATUS_MAP: Record<string, { color: string; bg: string; border: string; label: string }> = {
  Pendente:  { color: '#4ade80', bg: '#0a2a1a', border: '#166534',   label: 'Pendente'   },
  EmFila:    { color: '#b8860b', bg: '#2a1f00', border: '#b8860b55', label: 'Na fila'    },
  EmPreparo: { color: '#60a5fa', bg: '#0a1a2a', border: '#1e3a5f',   label: 'Em preparo' },
  Pronto:    { color: '#4ade80', bg: '#0a2a1a', border: '#166534',   label: 'Pronto ✅'  },
  Retirado:  { color: '#6b4fa0', bg: '#2d115533', border: '#4a1b8a', label: 'Retirado'   },
  Cancelado: { color: '#f87171', bg: '#2a0a0a', border: '#7f1d1d',   label: 'Cancelado'  },
};

type Aba = 'enviar' | 'retirar';

export default function RetiradaPage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const queryClient = useQueryClient();
  const EVENTO_ID = usuario?.eventoId ?? 0;
  const inputRef = useRef<HTMLInputElement>(null);

  const [aba, setAba] = useState<Aba>('enviar');
  const [codigoInput, setCodigoInput] = useState('');
  const [nomeCliente, setNomeCliente] = useState('');
  const [fila, setFila] = useState<TicketResponse[]>([]);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [ticketsSelecionados, setTicketsSelecionados] = useState<string[]>([]);

  useEffect(() => {
    inputRef.current?.focus();
  }, [aba]);

  // Tickets prontos para retirar
  const { data: prontos = [], refetch: refetchProntos } = useQuery<TicketResponse[]>({
    queryKey: ['tickets-prontos', EVENTO_ID],
    queryFn: async () => (await api.get(`/tickets/prontos/${EVENTO_ID}`)).data,
    refetchInterval: 5000,
    enabled: EVENTO_ID > 0,
  });

  // Histórico de retiradas recentes
  const { data: retirados = [] } = useQuery<TicketResponse[]>({
    queryKey: ['tickets-retirados', EVENTO_ID],
    queryFn: async () => {
      const res = await api.get(`/tickets/evento/${EVENTO_ID}`);
      return res.data
        .filter((t: TicketResponse) => t.status === 'Retirado')
        .slice(-5)
        .reverse();
    },
    refetchInterval: 10000,
    enabled: EVENTO_ID > 0,
  });

  const buscarTicket = async () => {
    const codigo = codigoInput.trim().toUpperCase();
    if (!codigo) return;

    if (fila.find(t => t.codigo === codigo)) {
      setErro('Ticket já adicionado.');
      setCodigoInput('');
      return;
    }

    try {
      const res = await api.get(`/tickets/codigo/${codigo}`);
      const ticket: TicketResponse = res.data;

      if (ticket.status !== 'Pendente') {
        setErro(`Ticket ${codigo} não está pendente (status: ${ticket.status}).`);
        setCodigoInput('');
        return;
      }

      setFila(prev => [...prev, ticket]);
      setCodigoInput('');
      setErro('');
    } catch {
      setErro(`Ticket ${codigo} não encontrado.`);
      setCodigoInput('');
    }
    inputRef.current?.focus();
  };

  const removerDaFila = (codigo: string) => {
    setFila(prev => prev.filter(t => t.codigo !== codigo));
  };

  const toggleSelecionado = (codigo: string) => {
    setTicketsSelecionados(prev =>
      prev.includes(codigo) ? prev.filter(c => c !== codigo) : [...prev, codigo]
    );
  };

  const mutEnviarFila = useMutation({
    mutationFn: async () => {
      await api.post('/tickets/fila', {
        codigos: fila.map(t => t.codigo),
        nomeCliente: nomeCliente.trim() || 'Cliente',
      });
    },
    onSuccess: () => {
      setSucesso(`${fila.length} ticket(s) enviado(s) para a cozinha!`);
      setFila([]);
      setNomeCliente('');
      setErro('');
      setTimeout(() => setSucesso(''), 3000);
      inputRef.current?.focus();
    },
    onError: () => setErro('Erro ao enviar para a cozinha.'),
  });

  const mutConfirmarRetirada = useMutation({
    mutationFn: async () => {
      await api.post('/tickets/retirar', {
        codigos: ticketsSelecionados,
        operadorNome: usuario?.nome ?? 'Operador',
      });
    },
    onSuccess: () => {
      setSucesso(`${ticketsSelecionados.length} ticket(s) confirmado(s) como retirado(s)!`);
      setTicketsSelecionados([]);
      setErro('');
      refetchProntos();
      queryClient.invalidateQueries({ queryKey: ['tickets-retirados', EVENTO_ID] });
      setTimeout(() => setSucesso(''), 3000);
    },
    onError: () => setErro('Erro ao confirmar retirada.'),
  });

  const totalItens = fila.map(t => t.nomeItem).join(', ');

  // Agrupar prontos por cliente
  const prontosPorCliente = prontos.reduce((acc, t) => {
    const key = t.nomeCliente ?? 'Cliente';
    if (!acc[key]) acc[key] = [];
    acc[key].push(t);
    return acc;
  }, {} as Record<string, TicketResponse[]>);

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      <Navbar titulo="Estoque" />

      {/* Abas */}
      <div style={{ background: '#1a0a2e', borderBottom: '0.5px solid #2d1155', padding: '0 20px', display: 'flex', gap: 8 }}>
        <div
          onClick={() => { setAba('enviar'); setErro(''); setSucesso(''); }}
          style={{ padding: '10px 16px', fontSize: 13, cursor: 'pointer', color: aba === 'enviar' ? '#e8d5ff' : '#6b4fa0', borderBottom: aba === 'enviar' ? '2px solid #b8860b' : '2px solid transparent' }}
        >
          🍳 Enviar para cozinha
        </div>
        <div
          onClick={() => { setAba('retirar'); setErro(''); setSucesso(''); }}
          style={{ padding: '10px 16px', fontSize: 13, cursor: 'pointer', color: aba === 'retirar' ? '#e8d5ff' : '#6b4fa0', borderBottom: aba === 'retirar' ? '2px solid #4ade80' : '2px solid transparent', display: 'flex', alignItems: 'center', gap: 6 }}
        >
          ✅ Confirmar retirada
          {prontos.length > 0 && (
            <span style={{ background: '#166534', color: '#4ade80', fontSize: 10, borderRadius: 20, padding: '1px 6px' }}>
              {prontos.length}
            </span>
          )}
        </div>
      </div>

      <div style={{ padding: '16px 20px' }}>

        {/* Feedback */}
        {erro && (
          <div style={{ background: '#2a0a0a', border: '0.5px solid #7f1d1d', color: '#f87171', borderRadius: 8, padding: '10px 14px', fontSize: 12, marginBottom: 12 }}>
            {erro}
          </div>
        )}
        {sucesso && (
          <div style={{ background: '#0a2a1a', border: '0.5px solid #166534', color: '#4ade80', borderRadius: 8, padding: '10px 14px', fontSize: 12, marginBottom: 12 }}>
            {sucesso}
          </div>
        )}

        {/* ── ABA: ENVIAR PARA COZINHA ─────────────────────────────── */}
        {aba === 'enviar' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 320px', gap: 16 }}>

            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>

              {/* Scanner */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 12 }}>Adicionar ticket</p>
                <div style={{ display: 'flex', gap: 8, marginBottom: 6 }}>
                  <input
                    ref={inputRef}
                    type="text"
                    value={codigoInput}
                    onChange={e => setCodigoInput(e.target.value.toUpperCase())}
                    onKeyDown={e => e.key === 'Enter' && buscarTicket()}
                    placeholder="TK-XXXX"
                    style={{ flex: 1, background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 8, padding: '9px 12px', color: '#b8860b', fontSize: 14, fontFamily: 'monospace', letterSpacing: '0.1em', outline: 'none' }}
                  />
                  <div onClick={buscarTicket} style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: '9px 16px', color: '#e8d5ff', fontSize: 12, fontWeight: 500, cursor: 'pointer', whiteSpace: 'nowrap' }}>
                    + Adicionar
                  </div>
                </div>
                <p style={{ fontSize: 11, color: '#6b4fa0' }}>Escaneie o código de barras ou digite — pressione Enter para adicionar</p>
              </div>

              {/* Lista acumulativa */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16, flex: 1 }}>
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
                  <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500 }}>Tickets na fila</p>
                  <span style={{ fontSize: 11, color: '#6b4fa0' }}>{fila.length} ticket{fila.length !== 1 ? 's' : ''}</span>
                </div>

                {fila.length === 0 ? (
                  <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhum ticket adicionado ainda.</p>
                ) : fila.map(ticket => {
                  const s = STATUS_MAP[ticket.status] ?? STATUS_MAP['Pendente'];
                  return (
                    <div key={ticket.codigo} style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 12px', marginBottom: 6, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <div>
                        <p style={{ color: '#b8860b', fontSize: 13, fontFamily: 'monospace', letterSpacing: '0.05em' }}>{ticket.codigo}</p>
                        <p style={{ color: '#6b4fa0', fontSize: 11, marginTop: 2 }}>{ticket.nomeItem}</p>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <span style={{ fontSize: 10, color: s.color, background: s.bg, border: `0.5px solid ${s.border}`, borderRadius: 20, padding: '2px 8px' }}>{s.label}</span>
                        <span onClick={() => removerDaFila(ticket.codigo)} style={{ color: '#f87171', fontSize: 18, cursor: 'pointer', lineHeight: 1 }}>×</span>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Coluna direita */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              <div style={{ background: '#1a0a2e', border: '0.5px solid #4a1b8a', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Dados da retirada</p>

                <label style={{ fontSize: 11, color: '#9c7bc4', display: 'block', marginBottom: 5 }}>Nome do cliente</label>
                <input
                  type="text"
                  value={nomeCliente}
                  onChange={e => setNomeCliente(e.target.value)}
                  placeholder="Ex: João da Silva"
                  style={{ width: '100%', background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 8, padding: '8px 12px', color: '#e8d5ff', fontSize: 13, outline: 'none', marginBottom: 14 }}
                />

                <div style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 12px', marginBottom: 14 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                    <span style={{ fontSize: 12, color: '#6b4fa0' }}>Total de tickets</span>
                    <span style={{ fontSize: 12, color: '#e8d5ff', fontWeight: 500 }}>{fila.length}</span>
                  </div>
                  {totalItens && (
                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                      <span style={{ fontSize: 12, color: '#6b4fa0' }}>Itens</span>
                      <span style={{ fontSize: 11, color: '#6b4fa0', maxWidth: 150, textAlign: 'right' }}>{totalItens}</span>
                    </div>
                  )}
                </div>

                <div
                onClick={() => {
  console.log('fila:', fila.length, 'isPending:', mutEnviarFila.isPending);
  if (fila.length > 0 && !mutEnviarFila.isPending) {
    mutEnviarFila.mutate();
  }
}}
                  style={{ background: fila.length === 0 ? '#2d1155' : 'linear-gradient(135deg, #4a1b8a, #3d2000)', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 11, textAlign: 'center', cursor: fila.length === 0 ? 'not-allowed' : 'pointer', opacity: fila.length === 0 ? 0.5 : 1, marginBottom: 8 }}
                >
                  <p style={{ color: '#e8d5ff', fontSize: 14, fontWeight: 500, pointerEvents: 'none' }}>
                    {mutEnviarFila.isPending ? '⏳ Enviando...' : '🍳 Enviar para cozinha'}
                  </p>
                  {fila.length > 0 && (
                    <p style={{ color: '#9c7bc4', fontSize: 11, marginTop: 2, pointerEvents: 'none' }}>
                      Envia {fila.length} ticket{fila.length !== 1 ? 's' : ''} para preparo
                    </p>
                  )}
                </div>

                <div onClick={() => { setFila([]); setNomeCliente(''); }} style={{ textAlign: 'center', color: '#6b4fa0', fontSize: 12, cursor: 'pointer' }}>
                  Limpar fila
                </div>
              </div>

              {/* Histórico */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16, flex: 1 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 12 }}>Últimas retiradas</p>
                {retirados.length === 0 ? (
                  <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhuma retirada ainda.</p>
                ) : retirados.map((t, i) => (
                  <div key={t.codigo} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingBottom: 8, marginBottom: 8, borderBottom: i < retirados.length - 1 ? '0.5px solid #2d115533' : 'none' }}>
                    <div>
                      <p style={{ fontSize: 12, color: '#e8d5ff' }}>{t.codigo} · {t.nomeItem}</p>
                      <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2 }}>{t.nomeCliente} · {t.retiradoEm ? new Date(t.retiradoEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) : ''}</p>
                    </div>
                    <span style={{ fontSize: 10, color: '#6b4fa0', background: '#2d115533', border: '0.5px solid #4a1b8a', borderRadius: 20, padding: '2px 8px' }}>Retirado</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* ── ABA: CONFIRMAR RETIRADA ──────────────────────────────── */}
        {aba === 'retirar' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 320px', gap: 16 }}>

            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Pedidos prontos para retirar</p>

              {prontos.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '2rem' }}>
                  <p style={{ fontSize: 32, marginBottom: 8 }}>🍽️</p>
                  <p style={{ fontSize: 13, color: '#6b4fa0' }}>Nenhum pedido pronto ainda.</p>
                  <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 4 }}>Aguardando a cozinha liberar...</p>
                </div>
              ) : Object.entries(prontosPorCliente).map(([cliente, tickets]) => (
                <div key={cliente} style={{ background: '#0f0520', border: '0.5px solid #166534', borderRadius: 10, padding: 14, marginBottom: 10 }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 10 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <span style={{ fontSize: 16 }}>👤</span>
                      <span style={{ fontSize: 14, color: '#e8d5ff', fontWeight: 500 }}>{cliente}</span>
                    </div>
                    <span style={{ fontSize: 11, color: '#4ade80', background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 8px' }}>
                      {tickets.length} item{tickets.length !== 1 ? 's' : ''}
                    </span>
                  </div>

                  {tickets.map(ticket => (
                    <div
                      key={ticket.codigo}
                      onClick={() => toggleSelecionado(ticket.codigo)}
                      style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '8px 10px', borderRadius: 6, marginBottom: 4, background: ticketsSelecionados.includes(ticket.codigo) ? '#0a2a1a' : '#1a0a2e', border: `0.5px solid ${ticketsSelecionados.includes(ticket.codigo) ? '#166534' : '#2d1155'}`, cursor: 'pointer' }}
                    >
                      <div style={{ width: 16, height: 16, borderRadius: 4, border: `1.5px solid ${ticketsSelecionados.includes(ticket.codigo) ? '#4ade80' : '#4a1b8a'}`, background: ticketsSelecionados.includes(ticket.codigo) ? '#4ade80' : 'transparent', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                        {ticketsSelecionados.includes(ticket.codigo) && <span style={{ fontSize: 10, color: '#0a2a1a', fontWeight: 700 }}>✓</span>}
                      </div>
                      <div style={{ flex: 1 }}>
                        <p style={{ fontSize: 13, color: '#e8d5ff' }}>{ticket.nomeItem}</p>
                        <p style={{ fontSize: 11, color: '#6b4fa0', fontFamily: 'monospace' }}>{ticket.codigo}</p>
                      </div>
                      <span style={{ fontSize: 10, color: '#4ade80', background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 8px' }}>Pronto</span>
                    </div>
                  ))}

                  {/* Selecionar todos do cliente */}
                  <div
                    onClick={() => {
                      const codigos = tickets.map(t => t.codigo);
                      const todosSelecionados = codigos.every(c => ticketsSelecionados.includes(c));
                      if (todosSelecionados) {
                        setTicketsSelecionados(prev => prev.filter(c => !codigos.includes(c)));
                      } else {
                        setTicketsSelecionados(prev => [...new Set([...prev, ...codigos])]);
                      }
                    }}
                    style={{ fontSize: 11, color: '#b8860b', cursor: 'pointer', marginTop: 6, textAlign: 'right' }}
                  >
                    Selecionar todos de {cliente}
                  </div>
                </div>
              ))}
            </div>

            {/* Coluna direita — confirmar */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              <div style={{ background: '#1a0a2e', border: '0.5px solid #166534', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Confirmar entrega</p>

                <div style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 12px', marginBottom: 14 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                    <span style={{ fontSize: 12, color: '#6b4fa0' }}>Selecionados</span>
                    <span style={{ fontSize: 12, color: '#e8d5ff', fontWeight: 500 }}>{ticketsSelecionados.length}</span>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <span style={{ fontSize: 12, color: '#6b4fa0' }}>Operador</span>
                    <span style={{ fontSize: 12, color: '#6b4fa0' }}>{usuario?.nome}</span>
                  </div>
                </div>

                <div
                  onClick={() => ticketsSelecionados.length > 0 && !mutConfirmarRetirada.isPending && mutConfirmarRetirada.mutate()}
                  style={{ background: ticketsSelecionados.length === 0 ? '#2d1155' : 'linear-gradient(135deg, #0a2a1a, #166534)', border: '0.5px solid #166534', borderRadius: 8, padding: 11, textAlign: 'center', cursor: ticketsSelecionados.length === 0 ? 'not-allowed' : 'pointer', opacity: ticketsSelecionados.length === 0 ? 0.5 : 1 }}
                >
                  <p style={{ color: '#4ade80', fontSize: 14, fontWeight: 500, pointerEvents: 'none' }}>
                    {mutConfirmarRetirada.isPending ? '⏳ Confirmando...' : '✅ Confirmar retirada'}
                  </p>
                  {ticketsSelecionados.length > 0 && (
                    <p style={{ color: '#4ade8099', fontSize: 11, marginTop: 2, pointerEvents: 'none' }}>
                      {ticketsSelecionados.length} ticket{ticketsSelecionados.length !== 1 ? 's' : ''} entregues
                    </p>
                  )}
                </div>
              </div>

              {/* Histórico */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16, flex: 1 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 12 }}>Últimas retiradas</p>
                {retirados.length === 0 ? (
                  <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhuma retirada ainda.</p>
                ) : retirados.map((t, i) => (
                  <div key={t.codigo} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingBottom: 8, marginBottom: 8, borderBottom: i < retirados.length - 1 ? '0.5px solid #2d115533' : 'none' }}>
                    <div>
                      <p style={{ fontSize: 12, color: '#e8d5ff' }}>{t.codigo} · {t.nomeItem}</p>
                      <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2 }}>{t.nomeCliente} · {t.retiradoEm ? new Date(t.retiradoEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) : ''}</p>
                    </div>
                    <span style={{ fontSize: 10, color: '#6b4fa0', background: '#2d115533', border: '0.5px solid #4a1b8a', borderRadius: 20, padding: '2px 8px' }}>Retirado</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}