import { useState } from 'react';
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

interface DevolucaoResponse {
  id: number;
  tipo: string;
  motivo: string;
  autorizadoPor: string;
  codigoTicket: string;
  nomeItem: string;
  novoCodigoTicket: string | null;
  criadoEm: string;
}

interface ItemResponse {
  id: number;
  nome: string;
  estoqueAtual: number;
}

type TipoDevolucao = 'Cancelamento' | 'Troca';

export default function DevolucoesPage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const queryClient = useQueryClient();
  const EVENTO_ID = usuario?.eventoId ?? 0;

  const [codigoInput, setCodigoInput] = useState('');
  const [ticket, setTicket] = useState<TicketResponse | null>(null);
  const [tipo, setTipo] = useState<TipoDevolucao>('Cancelamento');
  const [motivo, setMotivo] = useState('');
  const [senhaAdmin, setSenhaAdmin] = useState('');
  const [novoItemId, setNovoItemId] = useState<number | null>(null);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [buscando, setBuscando] = useState(false);

  // Histórico de devoluções
  const { data: historico = [], refetch: refetchHistorico } = useQuery<DevolucaoResponse[]>({
    queryKey: ['devolucoes', EVENTO_ID],
    queryFn: async () => (await api.get(`/devolucoes/evento/${EVENTO_ID}`)).data,
    enabled: EVENTO_ID > 0,
  });

  // Lista de itens para troca
  const { data: itens = [] } = useQuery<ItemResponse[]>({
    queryKey: ['itens', EVENTO_ID],
    queryFn: async () => (await api.get(`/itens/evento/${EVENTO_ID}`)).data,
    enabled: EVENTO_ID > 0 && tipo === 'Troca',
  });

  const buscarTicket = async () => {
    const codigo = codigoInput.trim().toUpperCase();
    if (!codigo) return;
    setBuscando(true);
    setErro('');
    setTicket(null);
    try {
      const res = await api.get(`/tickets/codigo/${codigo}`);
      const t: TicketResponse = res.data;
      if (t.status === 'Retirado') {
        setErro('Ticket já foi retirado e não pode ser devolvido.');
      } else if (t.status === 'Cancelado') {
        setErro('Ticket já foi cancelado.');
      } else {
        setTicket(t);
      }
    } catch {
      setErro(`Ticket ${codigo} não encontrado.`);
    } finally {
      setBuscando(false);
    }
  };

  const mutValidarEConfirmar = useMutation({
    mutationFn: async () => {
      // Valida senha do admin
      const validacao = await api.post('/usuarios/validar-admin', { senha: senhaAdmin });
      if (!validacao.data.valido) throw new Error('Senha de administrador inválida.');

      // Executa a devolução
      if (tipo === 'Cancelamento') {
        return api.post('/devolucoes/cancelar', {
          codigoTicket: ticket!.codigo,
          motivo,
          autorizadoPor: usuario?.nome ?? 'Admin',
        });
      } else {
        if (!novoItemId) throw new Error('Selecione o novo item para a troca.');
        return api.post('/devolucoes/trocar', {
          codigoTicket: ticket!.codigo,
          novoItemId,
          motivo,
          autorizadoPor: usuario?.nome ?? 'Admin',
        });
      }
    },
    onSuccess: (res) => {
      const data = res.data;
      if (tipo === 'Cancelamento') {
        setSucesso(`Ticket ${data.codigoTicket} cancelado com sucesso.`);
      } else {
        setSucesso(`Troca realizada! Novo ticket: ${data.novoCodigoTicket}`);
      }
      setTicket(null);
      setCodigoInput('');
      setMotivo('');
      setSenhaAdmin('');
      setNovoItemId(null);
      setErro('');
      refetchHistorico();
      queryClient.invalidateQueries({ queryKey: ['itens', EVENTO_ID] });
      setTimeout(() => setSucesso(''), 5000);
    },
    onError: (err: Error) => {
      setErro(err.message || 'Erro ao processar devolução.');
    },
  });

  const STATUS_MAP: Record<string, { color: string; bg: string; border: string; label: string }> = {
    Pendente:  { color: '#4ade80', bg: '#0a2a1a', border: '#166534',   label: 'Pendente'   },
    EmFila:    { color: '#b8860b', bg: '#2a1f00', border: '#b8860b55', label: 'Na fila'    },
    EmPreparo: { color: '#60a5fa', bg: '#0a1a2a', border: '#1e3a5f',   label: 'Em preparo' },
    Pronto:    { color: '#4ade80', bg: '#0a2a1a', border: '#166534',   label: 'Pronto'     },
    Cancelado: { color: '#f87171', bg: '#2a0a0a', border: '#7f1d1d',   label: 'Cancelado'  },
  };

  const inputStyle = {
    width: '100%', background: '#0f0520', border: '0.5px solid #4a1b8a',
    borderRadius: 8, padding: '8px 12px', color: '#e8d5ff', fontSize: 13, outline: 'none',
  };

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      <Navbar titulo="Estoque" />

      <div style={{ padding: '16px 20px', display: 'grid', gridTemplateColumns: '1fr 320px', gap: 16 }}>

        {/* Coluna esquerda */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>

          {/* Feedback */}
          {erro && (
            <div style={{ background: '#2a0a0a', border: '0.5px solid #7f1d1d', color: '#f87171', borderRadius: 8, padding: '10px 14px', fontSize: 12 }}>
              {erro}
            </div>
          )}
          {sucesso && (
            <div style={{ background: '#0a2a1a', border: '0.5px solid #166534', color: '#4ade80', borderRadius: 8, padding: '10px 14px', fontSize: 12 }}>
              {sucesso}
            </div>
          )}

          {/* Busca */}
          <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
            <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 12 }}>Buscar ticket</p>
            <div style={{ display: 'flex', gap: 8, marginBottom: 6 }}>
              <input
                type="text"
                value={codigoInput}
                onChange={e => setCodigoInput(e.target.value.toUpperCase())}
                onKeyDown={e => e.key === 'Enter' && buscarTicket()}
                placeholder="TK-XXXX"
                style={{ flex: 1, background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 8, padding: '9px 12px', color: '#b8860b', fontSize: 14, fontFamily: 'monospace', letterSpacing: '0.1em', outline: 'none' }}
              />
              <div
                onClick={buscarTicket}
                style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: '9px 16px', color: '#e8d5ff', fontSize: 12, fontWeight: 500, cursor: 'pointer', whiteSpace: 'nowrap' }}
              >
                {buscando ? 'Buscando...' : 'Buscar'}
              </div>
            </div>
            <p style={{ fontSize: 11, color: '#6b4fa0' }}>Digite ou escaneie o código do ticket</p>
          </div>

          {/* Ticket encontrado */}
          {ticket && (
            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
                <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500 }}>Ticket encontrado</p>
                <span style={{ fontSize: 10, color: STATUS_MAP[ticket.status]?.color, background: STATUS_MAP[ticket.status]?.bg, border: `0.5px solid ${STATUS_MAP[ticket.status]?.border}`, borderRadius: 20, padding: '2px 8px' }}>
                  {STATUS_MAP[ticket.status]?.label ?? ticket.status}
                </span>
              </div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: 8, marginBottom: 16, background: '#0f0520', borderRadius: 8, padding: '10px 12px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span style={{ fontSize: 12, color: '#6b4fa0' }}>Código</span>
                  <span style={{ fontSize: 13, color: '#b8860b', fontFamily: 'monospace' }}>{ticket.codigo}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span style={{ fontSize: 12, color: '#6b4fa0' }}>Item</span>
                  <span style={{ fontSize: 13, color: '#e8d5ff' }}>{ticket.nomeItem}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span style={{ fontSize: 12, color: '#6b4fa0' }}>Emitido em</span>
                  <span style={{ fontSize: 13, color: '#e8d5ff' }}>{new Date(ticket.criadoEm).toLocaleString('pt-BR')}</span>
                </div>
              </div>

              {/* Tipo de devolução */}
              <p style={{ fontSize: 11, color: '#9c7bc4', marginBottom: 8 }}>Tipo de devolução</p>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8, marginBottom: 14 }}>
                <div
                  onClick={() => setTipo('Cancelamento')}
                  style={{ background: tipo === 'Cancelamento' ? '#2a0a0a' : '#0f0520', border: `0.5px solid ${tipo === 'Cancelamento' ? '#7f1d1d' : '#2d1155'}`, borderRadius: 8, padding: 10, textAlign: 'center', cursor: 'pointer' }}
                >
                  <p style={{ fontSize: 13, color: '#f87171', fontWeight: 500, pointerEvents: 'none' }}>❌ Cancelar</p>
                  <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2, pointerEvents: 'none' }}>Estorno do ticket</p>
                </div>
                <div
                  onClick={() => setTipo('Troca')}
                  style={{ background: tipo === 'Troca' ? '#4a1b8a' : '#0f0520', border: `0.5px solid ${tipo === 'Troca' ? '#b8860b55' : '#2d1155'}`, borderRadius: 8, padding: 10, textAlign: 'center', cursor: 'pointer' }}
                >
                  <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, pointerEvents: 'none' }}>🔄 Trocar</p>
                  <p style={{ fontSize: 11, color: '#9c7bc4', marginTop: 2, pointerEvents: 'none' }}>Trocar por outro item</p>
                </div>
              </div>

              {/* Seleção de novo item (só para Troca) */}
              {tipo === 'Troca' && (
                <div style={{ marginBottom: 14 }}>
                  <label style={{ fontSize: 11, color: '#9c7bc4', display: 'block', marginBottom: 5 }}>Novo item</label>
                  <select
                    value={novoItemId ?? ''}
                    onChange={e => setNovoItemId(parseInt(e.target.value))}
                    style={{ ...inputStyle, appearance: 'none' } as React.CSSProperties}
                  >
                    <option value="">Selecione o novo item</option>
                    {itens.filter(i => i.estoqueAtual > 0 && i.id !== ticket.id).map(i => (
                      <option key={i.id} value={i.id}>{i.nome} ({i.estoqueAtual} un.)</option>
                    ))}
                  </select>
                </div>
              )}

              {/* Motivo */}
              <label style={{ fontSize: 11, color: '#9c7bc4', display: 'block', marginBottom: 5 }}>Motivo</label>
              <input
                type="text"
                value={motivo}
                onChange={e => setMotivo(e.target.value)}
                placeholder="Ex: Cliente desistiu"
                style={inputStyle}
              />
            </div>
          )}
        </div>

        {/* Coluna direita */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>

          {/* Autorização Admin */}
          <div style={{ background: '#1a0a2e', border: '0.5px solid #7f1d1d', borderRadius: 12, padding: 16 }}>
            <p style={{ fontSize: 13, color: '#f87171', fontWeight: 500, marginBottom: 4 }}>🔐 Autorização Admin</p>
            <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 12 }}>Necessária para confirmar a devolução</p>

            <label style={{ fontSize: 11, color: '#9c7bc4', display: 'block', marginBottom: 5 }}>Senha do administrador</label>
            <input
              type="password"
              value={senhaAdmin}
              onChange={e => setSenhaAdmin(e.target.value)}
              placeholder="••••••••"
              style={{ ...inputStyle, border: '0.5px solid #7f1d1d', marginBottom: 14 }}
            />

            <div
              onClick={() => ticket && motivo && senhaAdmin && !mutValidarEConfirmar.isPending && mutValidarEConfirmar.mutate()}
              style={{
                background: (!ticket || !motivo || !senhaAdmin) ? '#2d1155' : tipo === 'Cancelamento' ? '#2a0a0a' : '#4a1b8a',
                border: `0.5px solid ${tipo === 'Cancelamento' ? '#7f1d1d' : '#b8860b55'}`,
                borderRadius: 8, padding: 11, textAlign: 'center',
                cursor: (!ticket || !motivo || !senhaAdmin) ? 'not-allowed' : 'pointer',
                opacity: (!ticket || !motivo || !senhaAdmin) ? 0.5 : 1
              }}
            >
              <p style={{ color: tipo === 'Cancelamento' ? '#f87171' : '#e8d5ff', fontSize: 14, fontWeight: 500, pointerEvents: 'none' }}>
                {mutValidarEConfirmar.isPending ? '⏳ Processando...' : tipo === 'Cancelamento' ? '❌ Confirmar cancelamento' : '🔄 Confirmar troca'}
              </p>
            </div>
          </div>

          {/* Histórico */}
          <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16, flex: 1 }}>
            <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 12 }}>Últimas devoluções</p>

            {historico.length === 0 ? (
              <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhuma devolução ainda.</p>
            ) : historico.map((d, i) => (
              <div key={d.id} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', paddingBottom: 8, marginBottom: 8, borderBottom: i < historico.length - 1 ? '0.5px solid #2d115533' : 'none' }}>
                <div>
                  <p style={{ fontSize: 12, color: '#e8d5ff' }}>
                    {d.codigoTicket} · {d.nomeItem}
                    {d.novoCodigoTicket && ` → ${d.novoCodigoTicket}`}
                  </p>
                  <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2 }}>
                    {d.tipo} · {new Date(d.criadoEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
                  </p>
                </div>
                <span style={{ fontSize: 10, color: d.tipo === 'Cancelamento' ? '#f87171' : '#b8860b', background: d.tipo === 'Cancelamento' ? '#2a0a0a' : '#2a1f00', border: `0.5px solid ${d.tipo === 'Cancelamento' ? '#7f1d1d' : '#b8860b55'}`, borderRadius: 20, padding: '2px 8px' }}>
                  {d.tipo === 'Cancelamento' ? 'Cancelado' : 'Troca'}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}