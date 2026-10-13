import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import { useAuth } from '../hooks/AuthContext';
import api from '../api/client';
import type { TicketResponse } from '../types';


export default function CozinhaPage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const EVENTO_ID = usuario?.eventoId ?? 0;

  const [conexao, setConexao] = useState<HubConnection | null>(null);
  const [conectado, setConectado] = useState(false);
  const [cozinhaId, setCozinhaId] = useState<number | null>(null);

  // Cozinhas disponíveis
  const { data: cozinhas = [] } = useQuery<{ id: number; nome: string }[]>({
    queryKey: ['cozinhas', EVENTO_ID],
    queryFn: async () => (await api.get(`/cozinhas-evento/evento/${EVENTO_ID}`)).data,
    enabled: EVENTO_ID > 0,
  });

  // Fila da cozinha
  const { data: fila = [], refetch } = useQuery<TicketResponse[]>({
    queryKey: ['fila-cozinha', EVENTO_ID, cozinhaId],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (cozinhaId) params.append('cozinhaEventoId', String(cozinhaId));
      return (await api.get(`/tickets/fila/${EVENTO_ID}?${params}`)).data;
    },
    refetchInterval: 5000,
    enabled: EVENTO_ID > 0,
  });

  const naFila    = fila.filter(t => t.status === 'EmFila');
  const emPreparo = fila.filter(t => t.status === 'EmPreparo');
  const prontos   = fila.filter(t => t.status === 'Pronto');

  // SignalR
  useEffect(() => {
    const token = localStorage.getItem('kerma_token');
    const conn = new HubConnectionBuilder()
      .withUrl('https://localhost:7205/hubs/cozinha', { accessTokenFactory: () => token ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    conn.on('FilaAtualizada', () => refetch());
    conn.on('TicketAtualizado', () => refetch());

    conn.start()
      .then(() => { setConectado(true); conn.invoke('EntrarNoEvento', EVENTO_ID); })
      .catch(() => setConectado(false));

    setConexao(conn);
    return () => { conn.stop(); };
  }, [EVENTO_ID]);

  const mutIniciarPreparo = useMutation({
    mutationFn: (id: number) => api.patch(`/tickets/${id}/iniciar-preparo`),
    onSuccess: () => refetch(),
  });

  const mutMarcarPronto = useMutation({
    mutationFn: (id: number) => api.patch(`/tickets/${id}/pronto`),
    onSuccess: () => refetch(),
  });

  const cardStyle = {
    background: '#0f0520', border: '0.5px solid #2d1155',
    borderRadius: 10, padding: 14, marginBottom: 8,
  };

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      {/* Navbar */}
      <div style={{ background: '#1a0a2e', borderBottom: '0.5px solid #2d1155', padding: '11px 20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div onClick={() => navigate('/')} style={{ width: 30, height: 30, background: '#2d1155', borderRadius: 7, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer' }}>
            <svg width="18" height="18" viewBox="0 0 32 32">
              <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
              <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
            </svg>
          </div>
          <span style={{ color: '#e8d5ff', fontSize: 15, fontWeight: 500 }}>Kerma</span>
          <span style={{ fontSize: 11, color: '#b8860b', border: '0.5px solid #b8860b44', borderRadius: 20, padding: '2px 10px' }}>🍳 Cozinha</span>
          <span style={{ fontSize: 11, color: conectado ? '#4ade80' : '#f87171', border: `0.5px solid ${conectado ? '#166534' : '#7f1d1d'}`, borderRadius: 20, padding: '2px 8px', background: conectado ? '#0a2a1a' : '#2a0a0a' }}>
            {conectado ? '● Ao vivo' : '○ Desconectado'}
          </span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 11, color: '#6b4fa0' }}>{usuario?.eventoNome}</span>
          <span onClick={() => navigate('/')} style={{ fontSize: 11, color: '#6b4fa0', cursor: 'pointer' }}>← Home</span>
        </div>
      </div>

      {/* Filtro de cozinha */}
      <div style={{ background: '#1a0a2e', borderBottom: '0.5px solid #2d1155', padding: '10px 20px', display: 'flex', alignItems: 'center', gap: 8 }}>
        <span style={{ fontSize: 11, color: '#6b4fa0' }}>Cozinha:</span>
        <span
          onClick={() => setCozinhaId(null)}
          style={{ padding: '4px 12px', borderRadius: 20, fontSize: 11, cursor: 'pointer', border: `0.5px solid ${cozinhaId === null ? '#b8860b55' : '#2d1155'}`, color: cozinhaId === null ? '#e8d5ff' : '#6b4fa0', background: cozinhaId === null ? '#4a1b8a' : '#0f0520' }}
        >
          Todas
        </span>
        {cozinhas.map(c => (
          <span
            key={c.id}
            onClick={() => setCozinhaId(c.id)}
            style={{ padding: '4px 12px', borderRadius: 20, fontSize: 11, cursor: 'pointer', border: `0.5px solid ${cozinhaId === c.id ? '#b8860b55' : '#2d1155'}`, color: cozinhaId === c.id ? '#e8d5ff' : '#6b4fa0', background: cozinhaId === c.id ? '#4a1b8a' : '#0f0520' }}
          >
            {c.nome}
          </span>
        ))}
      </div>

      {/* Painel estilo BK */}
      <div style={{ padding: '16px 20px', display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 16 }}>

        {/* NA FILA */}
        <div style={{ background: '#2a1f00', border: '0.5px solid #b8860b55', borderRadius: 12, padding: 16, minHeight: 400 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ fontSize: 20 }}>⏳</span>
              <p style={{ fontSize: 14, color: '#b8860b', fontWeight: 600 }}>NA FILA</p>
            </div>
            <span style={{ fontSize: 13, color: '#b8860b', fontWeight: 700 }}>{naFila.length}</span>
          </div>
          {naFila.length === 0 ? (
            <p style={{ fontSize: 12, color: '#b8860b88', textAlign: 'center', marginTop: 40 }}>Fila vazia</p>
          ) : naFila.map(ticket => (
            <div key={ticket.id} style={cardStyle}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ fontSize: 11, color: '#b8860b', fontFamily: 'monospace' }}>{ticket.codigo}</span>
                <span style={{ fontSize: 11, color: '#6b4fa0' }}>
                  {ticket.enviadoFilaEm ? new Date(ticket.enviadoFilaEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) : ''}
                </span>
              </div>
              <p style={{ fontSize: 15, color: '#e8d5ff', fontWeight: 500, marginBottom: 4 }}>{ticket.nomeItem}</p>
              <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 10 }}>👤 {ticket.nomeCliente}</p>
              <div onClick={() => mutIniciarPreparo.mutate(ticket.id)} style={{ background: '#b8860b', borderRadius: 6, padding: '7px 0', textAlign: 'center', cursor: 'pointer', fontSize: 12, color: '#0f0520', fontWeight: 600 }}>
                🔥 Iniciar preparo
              </div>
            </div>
          ))}
        </div>

        {/* EM PREPARO */}
        <div style={{ background: '#0a1a2a', border: '0.5px solid #1e3a5f', borderRadius: 12, padding: 16, minHeight: 400 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ fontSize: 20 }}>🔥</span>
              <p style={{ fontSize: 14, color: '#60a5fa', fontWeight: 600 }}>EM PREPARO</p>
            </div>
            <span style={{ fontSize: 13, color: '#60a5fa', fontWeight: 700 }}>{emPreparo.length}</span>
          </div>
          {emPreparo.length === 0 ? (
            <p style={{ fontSize: 12, color: '#60a5fa88', textAlign: 'center', marginTop: 40 }}>Nenhum em preparo</p>
          ) : emPreparo.map(ticket => (
            <div key={ticket.id} style={cardStyle}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ fontSize: 11, color: '#60a5fa', fontFamily: 'monospace' }}>{ticket.codigo}</span>
                <span style={{ fontSize: 11, color: '#6b4fa0' }}>
                  {ticket.enviadoFilaEm ? new Date(ticket.enviadoFilaEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) : ''}
                </span>
              </div>
              <p style={{ fontSize: 15, color: '#e8d5ff', fontWeight: 500, marginBottom: 4 }}>{ticket.nomeItem}</p>
              <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 10 }}>👤 {ticket.nomeCliente}</p>
              <div onClick={() => mutMarcarPronto.mutate(ticket.id)} style={{ background: '#4ade80', borderRadius: 6, padding: '7px 0', textAlign: 'center', cursor: 'pointer', fontSize: 12, color: '#0a2a1a', fontWeight: 600 }}>
                ✅ Marcar como pronto
              </div>
            </div>
          ))}
        </div>

        {/* PRONTOS */}
        <div style={{ background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 12, padding: 16, minHeight: 400 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ fontSize: 20 }}>✅</span>
              <p style={{ fontSize: 14, color: '#4ade80', fontWeight: 600 }}>PRONTOS</p>
            </div>
            <span style={{ fontSize: 13, color: '#4ade80', fontWeight: 700 }}>{prontos.length}</span>
          </div>
          {prontos.length === 0 ? (
            <p style={{ fontSize: 12, color: '#4ade8088', textAlign: 'center', marginTop: 40 }}>Nenhum pronto</p>
          ) : prontos.map(ticket => (
            <div key={ticket.id} style={{ ...cardStyle, border: '0.5px solid #166534' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ fontSize: 11, color: '#4ade80', fontFamily: 'monospace' }}>{ticket.codigo}</span>
                <span style={{ fontSize: 11, color: '#6b4fa0' }}>
                  {ticket.prontoEm ? new Date(ticket.prontoEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) : ''}
                </span>
              </div>
              <p style={{ fontSize: 15, color: '#e8d5ff', fontWeight: 500, marginBottom: 4 }}>{ticket.nomeItem}</p>
              <p style={{ fontSize: 12, color: '#4ade80', marginBottom: 4 }}>👤 {ticket.nomeCliente}</p>
              <p style={{ fontSize: 11, color: '#6b4fa0' }}>Aguardando retirada...</p>
            </div>
          ))}
        </div>

      </div>
    </div>
  );
}