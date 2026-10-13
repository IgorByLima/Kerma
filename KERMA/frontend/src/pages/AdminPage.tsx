import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '../hooks/AuthContext';
import api from '../api/client';
import type { Usuario, Evento } from '../types';
import Navbar from '../components/Navbar';

type Aba = 'usuarios' | 'eventos' | 'itens' | 'cozinhas';
type Perfil = 'Admin' | 'Operador' | 'Visualizador';

interface NovoUsuario { nome: string; email: string; perfil: Perfil; eventoId: number | null; }
interface NovoEvento { nome: string; descricao: string; dataInicio: string; dataFim: string; }
interface NovoItem {
  nome: string; descricao: string; custo: string; precoVenda: string;
  estoqueInicial: string; estoqueMinimo: string; cozinhaEventoId: string;
}
interface NovaCozinha { nome: string; }

export default function AdminPage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const queryClient = useQueryClient();
  const EVENTO_ID = usuario?.eventoId ?? 0;

  const [aba, setAba] = useState<Aba>('usuarios');
  const [erroForm, setErroForm] = useState('');
  const [sucessoForm, setSucessoForm] = useState('');
  const [novaSenha, setNovaSenha] = useState<{ usuarioId: number; senha: string } | null>(null);

  const [novoUsuario, setNovoUsuario] = useState<NovoUsuario>({ nome: '', email: '', perfil: 'Operador', eventoId: EVENTO_ID });
  const [novoEvento, setNovoEvento] = useState<NovoEvento>({ nome: '', descricao: '', dataInicio: '', dataFim: '' });
  const [novoItem, setNovoItem] = useState<NovoItem>({ nome: '', descricao: '', custo: '', precoVenda: '', estoqueInicial: '', estoqueMinimo: '5', cozinhaEventoId: '' });
  const [novaCozinha, setNovaCozinha] = useState<NovaCozinha>({ nome: '' });

  const { data: usuarios = [] } = useQuery<Usuario[]>({
    queryKey: ['usuarios'],
    queryFn: async () => (await api.get('/usuarios')).data,
  });

  const { data: eventos = [] } = useQuery<Evento[]>({
    queryKey: ['eventos'],
    queryFn: async () => (await api.get('/eventos')).data,
  });

  const { data: cozinhas = [] } = useQuery<{ id: number; nome: string; ativa: boolean }[]>({
    queryKey: ['cozinhas', EVENTO_ID],
    queryFn: async () => (await api.get(`/cozinhas-evento/evento/${EVENTO_ID}`)).data,
    enabled: EVENTO_ID > 0,
  });

  const mutCriarUsuario = useMutation({
    mutationFn: async () => (await api.post('/usuarios', novoUsuario)).data,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      setSucessoForm(`Usuário criado! Código: ${data.codigo} | Senha: ${data.senha}`);
      setErroForm('');
      setNovoUsuario({ nome: '', email: '', perfil: 'Operador', eventoId: EVENTO_ID });
    },
    onError: () => setErroForm('Erro ao criar usuário.'),
  });

  const mutDesativarUsuario = useMutation({
    mutationFn: async (id: number) => api.delete(`/usuarios/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['usuarios'] }),
  });

  const mutRedefinirSenha = useMutation({
    mutationFn: async (id: number) => (await api.patch(`/usuarios/${id}/redefinir-senha`)).data,
    onSuccess: (data, id) => {
      setNovaSenha({ usuarioId: id, senha: data.novaSenha });
      setErroForm('');
    },
    onError: () => setErroForm('Erro ao redefinir senha.'),
  });

  const mutCriarEvento = useMutation({
    mutationFn: async () => (await api.post('/eventos', novoEvento)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      setSucessoForm('Evento criado com sucesso!');
      setErroForm('');
      setNovoEvento({ nome: '', descricao: '', dataInicio: '', dataFim: '' });
    },
    onError: () => setErroForm('Erro ao criar evento.'),
  });

  const mutCriarItem = useMutation({
    mutationFn: async () => (await api.post('/itens', {
      eventoId: EVENTO_ID,
      nome: novoItem.nome,
      descricao: novoItem.descricao,
      custo: parseFloat(novoItem.custo),
      precoVenda: parseFloat(novoItem.precoVenda),
      estoqueInicial: parseInt(novoItem.estoqueInicial),
      estoqueMinimo: parseInt(novoItem.estoqueMinimo),
      cozinhaEventoId: novoItem.cozinhaEventoId ? parseInt(novoItem.cozinhaEventoId) : null,
    })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['itens', EVENTO_ID] });
      setSucessoForm('Item criado com sucesso!');
      setErroForm('');
      setNovoItem({ nome: '', descricao: '', custo: '', precoVenda: '', estoqueInicial: '', estoqueMinimo: '5', cozinhaEventoId: '' });
    },
    onError: () => setErroForm('Erro ao criar item.'),
  });

  const mutCriarCozinha = useMutation({
    mutationFn: async () => (await api.post('/cozinhas-evento', { eventoId: EVENTO_ID, nome: novaCozinha.nome })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cozinhas', EVENTO_ID] });
      setSucessoForm('Cozinha criada com sucesso!');
      setErroForm('');
      setNovaCozinha({ nome: '' });
    },
    onError: () => setErroForm('Erro ao criar cozinha.'),
  });

  const inputStyle = { width: '100%', background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 8, padding: '8px 12px', color: '#e8d5ff', fontSize: 13, outline: 'none' };
  const labelStyle = { fontSize: 11, color: '#9c7bc4', display: 'block', marginBottom: 5 } as const;

  const ABAS: { key: Aba; label: string }[] = [
    { key: 'usuarios', label: 'Usuários' },
    { key: 'eventos',  label: 'Eventos'  },
    { key: 'itens',    label: 'Itens'    },
    { key: 'cozinhas', label: 'Cozinhas' },
  ];

  const PERFIS: Perfil[] = ['Admin', 'Operador', 'Visualizador'];
  const STATUS_EVENTO: Record<string, { color: string; bg: string; border: string }> = {
    Rascunho:  { color: '#9c7bc4', bg: '#2d115533', border: '#4a1b8a'   },
    Ativo:     { color: '#4ade80', bg: '#0a2a1a',   border: '#166534'   },
    Encerrado: { color: '#f87171', bg: '#2a0a0a',   border: '#7f1d1d'   },
  };

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>
      <Navbar titulo="Administração" />

      <div style={{ padding: '18px 20px', maxWidth: 1100, margin: '0 auto' }}>

        {/* Abas */}
        <div style={{ display: 'flex', gap: 8, marginBottom: 20, borderBottom: '0.5px solid #2d1155', paddingBottom: 12 }}>
          {ABAS.map(a => (
            <div key={a.key} onClick={() => { setAba(a.key); setErroForm(''); setSucessoForm(''); setNovaSenha(null); }}
              style={{ padding: '8px 16px', borderRadius: 8, fontSize: 13, cursor: 'pointer', background: aba === a.key ? '#4a1b8a' : 'transparent', color: aba === a.key ? '#e8d5ff' : '#6b4fa0', border: aba === a.key ? '0.5px solid #b8860b55' : 'none' }}
            >
              {a.label}
            </div>
          ))}
        </div>

        {/* Feedback */}
        {erroForm && <div style={{ background: '#2a0a0a', border: '0.5px solid #7f1d1d', color: '#f87171', borderRadius: 8, padding: '10px 14px', fontSize: 13, marginBottom: 14 }}>{erroForm}</div>}
        {sucessoForm && <div style={{ background: '#0a2a1a', border: '0.5px solid #166534', color: '#4ade80', borderRadius: 8, padding: '10px 14px', fontSize: 13, marginBottom: 14 }}>{sucessoForm}</div>}

        {/* Nova senha gerada */}
        {novaSenha && (
          <div style={{ background: '#2a1f00', border: '0.5px solid #b8860b55', borderRadius: 8, padding: '12px 16px', marginBottom: 14 }}>
            <p style={{ fontSize: 12, color: '#b8860b', marginBottom: 6 }}>🔑 Nova senha gerada — copie e entregue ao usuário:</p>
            <p style={{ fontSize: 18, color: '#e8d5ff', fontFamily: 'monospace', letterSpacing: '0.15em', fontWeight: 600 }}>{novaSenha.senha}</p>
            <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 8, cursor: 'pointer' }} onClick={() => setNovaSenha(null)}>× Fechar</p>
          </div>
        )}

        {/* ── ABA USUÁRIOS ─────────────────────────────────────────────── */}
        {aba === 'usuarios' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: 16 }}>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Usuários cadastrados</p>
              <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr 80px', gap: 8, padding: '8px 12px', borderBottom: '0.5px solid #2d1155', marginBottom: 4 }}>
                {['Nome', 'Código', 'Perfil', 'Status', ''].map(h => <span key={h} style={{ fontSize: 11, color: '#6b4fa0' }}>{h}</span>)}
              </div>
              {usuarios.map((u, idx) => (
                <div key={u.id} style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr 80px', gap: 8, padding: '10px 12px', borderBottom: idx < usuarios.length - 1 ? '0.5px solid #2d115533' : 'none', alignItems: 'center' }}>
                  <div>
                    <p style={{ fontSize: 13, color: '#e8d5ff' }}>{u.nome}</p>
                    <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2 }}>{u.email}</p>
                  </div>
                  <span style={{ fontSize: 12, color: '#b8860b', fontFamily: 'monospace' }}>{u.codigo}</span>
                  <span style={{ fontSize: 11, color: '#c084fc', background: '#2d115533', border: '0.5px solid #4a1b8a', borderRadius: 20, padding: '2px 8px', width: 'fit-content' }}>{u.perfil}</span>
                  <span style={{ fontSize: 11, color: '#4ade80', background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 8px', width: 'fit-content' }}>{u.ativo ? 'Ativo' : 'Inativo'}</span>
                  <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
                    <span onClick={() => mutRedefinirSenha.mutate(u.id)} title="Redefinir senha" style={{ fontSize: 15, cursor: 'pointer' }}>🔑</span>
                    {u.id !== 1 && <i className="ti ti-trash" onClick={() => mutDesativarUsuario.mutate(u.id)} style={{ color: '#7f1d1d', fontSize: 15, cursor: 'pointer' }} />}
                  </div>
                </div>
              ))}
            </div>

            <div style={{ background: '#1a0a2e', border: '0.5px solid #4a1b8a', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Novo usuário</p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div><label style={labelStyle}>Nome</label><input style={inputStyle} value={novoUsuario.nome} onChange={e => setNovoUsuario(p => ({ ...p, nome: e.target.value }))} placeholder="Nome completo" /></div>
                <div><label style={labelStyle}>E-mail</label><input style={inputStyle} value={novoUsuario.email} onChange={e => setNovoUsuario(p => ({ ...p, email: e.target.value }))} placeholder="email@exemplo.com" /></div>
                <div>
                  <label style={labelStyle}>Perfil</label>
                  <div style={{ display: 'flex', gap: 6 }}>
                    {PERFIS.map(p => (
                      <div key={p} onClick={() => setNovoUsuario(prev => ({ ...prev, perfil: p }))}
                        style={{ flex: 1, background: novoUsuario.perfil === p ? '#4a1b8a' : '#0f0520', border: `0.5px solid ${novoUsuario.perfil === p ? '#b8860b55' : '#2d1155'}`, borderRadius: 6, padding: '6px 8px', textAlign: 'center', fontSize: 11, color: novoUsuario.perfil === p ? '#e8d5ff' : '#6b4fa0', cursor: 'pointer' }}
                      >{p}</div>
                    ))}
                  </div>
                </div>
                <div>
                  <label style={labelStyle}>Habilitar para evento</label>
                  <select value={novoUsuario.eventoId ?? ''} onChange={e => setNovoUsuario(p => ({ ...p, eventoId: e.target.value ? parseInt(e.target.value) : null }))} style={{ ...inputStyle, appearance: 'none' } as React.CSSProperties}>
                    <option value="">Nenhum evento</option>
                    {eventos.map(ev => <option key={ev.id} value={ev.id}>{ev.nome} ({ev.codigo})</option>)}
                  </select>
                </div>
                <div onClick={() => !mutCriarUsuario.isPending && mutCriarUsuario.mutate()}
                  style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 10, textAlign: 'center', color: '#e8d5ff', fontSize: 13, fontWeight: 500, cursor: 'pointer', marginTop: 4 }}>
                  {mutCriarUsuario.isPending ? 'Criando...' : 'Criar usuário'}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* ── ABA EVENTOS ──────────────────────────────────────────────── */}
        {aba === 'eventos' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: 16 }}>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Eventos</p>
              <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr', gap: 8, padding: '8px 12px', borderBottom: '0.5px solid #2d1155', marginBottom: 4 }}>
                {['Nome', 'Código', 'Status', 'Período'].map(h => <span key={h} style={{ fontSize: 11, color: '#6b4fa0' }}>{h}</span>)}
              </div>
              {eventos.map((ev, idx) => {
                const s = STATUS_EVENTO[ev.status] ?? STATUS_EVENTO['Rascunho'];
                return (
                  <div key={ev.id} style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr', gap: 8, padding: '10px 12px', borderBottom: idx < eventos.length - 1 ? '0.5px solid #2d115533' : 'none', alignItems: 'center' }}>
                    <p style={{ fontSize: 13, color: '#e8d5ff' }}>{ev.nome}</p>
                    <span style={{ fontSize: 12, color: '#b8860b', fontFamily: 'monospace' }}>{ev.codigo}</span>
                    <span style={{ fontSize: 11, color: s.color, background: s.bg, border: `0.5px solid ${s.border}`, borderRadius: 20, padding: '2px 8px', width: 'fit-content' }}>{ev.status}</span>
                    <span style={{ fontSize: 11, color: '#6b4fa0' }}>{ev.dataInicio} → {ev.dataFim}</span>
                  </div>
                );
              })}
            </div>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #4a1b8a', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Novo evento</p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div><label style={labelStyle}>Nome</label><input style={inputStyle} value={novoEvento.nome} onChange={e => setNovoEvento(p => ({ ...p, nome: e.target.value }))} placeholder="Nome do evento" /></div>
                <div><label style={labelStyle}>Descrição</label><input style={inputStyle} value={novoEvento.descricao} onChange={e => setNovoEvento(p => ({ ...p, descricao: e.target.value }))} placeholder="Descrição opcional" /></div>
                <div><label style={labelStyle}>Data início</label><input type="date" style={inputStyle} value={novoEvento.dataInicio} onChange={e => setNovoEvento(p => ({ ...p, dataInicio: e.target.value }))} /></div>
                <div><label style={labelStyle}>Data fim</label><input type="date" style={inputStyle} value={novoEvento.dataFim} onChange={e => setNovoEvento(p => ({ ...p, dataFim: e.target.value }))} /></div>
                <div onClick={() => !mutCriarEvento.isPending && mutCriarEvento.mutate()}
                  style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 10, textAlign: 'center', color: '#e8d5ff', fontSize: 13, fontWeight: 500, cursor: 'pointer', marginTop: 4 }}>
                  {mutCriarEvento.isPending ? 'Criando...' : 'Criar evento'}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* ── ABA ITENS ────────────────────────────────────────────────── */}
        {aba === 'itens' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: 16 }}>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Itens do evento atual</p>
              <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 14 }}>Veja a tela de Estoque para o semáforo completo.</p>
              <div onClick={() => navigate('/estoque')} style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 10, textAlign: 'center', color: '#e8d5ff', fontSize: 13, cursor: 'pointer', marginBottom: 16 }}>
                → Ir para Estoque
              </div>
            </div>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #4a1b8a', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Novo item</p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div><label style={labelStyle}>Nome</label><input style={inputStyle} value={novoItem.nome} onChange={e => setNovoItem(p => ({ ...p, nome: e.target.value }))} placeholder="Ex: Coxinha" /></div>
                <div><label style={labelStyle}>Descrição</label><input style={inputStyle} value={novoItem.descricao} onChange={e => setNovoItem(p => ({ ...p, descricao: e.target.value }))} placeholder="Descrição opcional" /></div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                  <div><label style={labelStyle}>Custo (R$)</label><input type="number" style={inputStyle} value={novoItem.custo} onChange={e => setNovoItem(p => ({ ...p, custo: e.target.value }))} placeholder="0,00" /></div>
                  <div><label style={labelStyle}>Preço venda (R$)</label><input type="number" style={inputStyle} value={novoItem.precoVenda} onChange={e => setNovoItem(p => ({ ...p, precoVenda: e.target.value }))} placeholder="0,00" /></div>
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                  <div><label style={labelStyle}>Estoque inicial</label><input type="number" style={inputStyle} value={novoItem.estoqueInicial} onChange={e => setNovoItem(p => ({ ...p, estoqueInicial: e.target.value }))} placeholder="0" /></div>
                  <div><label style={labelStyle}>Estoque mínimo</label><input type="number" style={inputStyle} value={novoItem.estoqueMinimo} onChange={e => setNovoItem(p => ({ ...p, estoqueMinimo: e.target.value }))} placeholder="5" /></div>
                </div>
                <div>
                  <label style={labelStyle}>Cozinha responsável</label>
                  <select value={novoItem.cozinhaEventoId} onChange={e => setNovoItem(p => ({ ...p, cozinhaEventoId: e.target.value }))} style={{ ...inputStyle, appearance: 'none' } as React.CSSProperties}>
                    <option value="">Sem cozinha</option>
                    {cozinhas.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
                  </select>
                </div>
                <div onClick={() => !mutCriarItem.isPending && mutCriarItem.mutate()}
                  style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 10, textAlign: 'center', color: '#e8d5ff', fontSize: 13, fontWeight: 500, cursor: 'pointer', marginTop: 4 }}>
                  {mutCriarItem.isPending ? 'Criando...' : 'Criar item'}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* ── ABA COZINHAS ─────────────────────────────────────────────── */}
        {aba === 'cozinhas' && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: 16 }}>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Cozinhas do evento</p>
              {cozinhas.length === 0 ? (
                <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhuma cozinha cadastrada.</p>
              ) : cozinhas.map((c, idx) => (
                <div key={c.id} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '10px 12px', borderBottom: idx < cozinhas.length - 1 ? '0.5px solid #2d115533' : 'none' }}>
                  <span style={{ fontSize: 13, color: '#e8d5ff' }}>{c.nome}</span>
                  <span style={{ fontSize: 11, color: '#4ade80', background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 8px' }}>Ativa</span>
                </div>
              ))}
            </div>
            <div style={{ background: '#1a0a2e', border: '0.5px solid #4a1b8a', borderRadius: 12, padding: 16 }}>
              <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500, marginBottom: 14 }}>Nova cozinha</p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                <div><label style={labelStyle}>Nome</label><input style={inputStyle} value={novaCozinha.nome} onChange={e => setNovaCozinha({ nome: e.target.value })} placeholder="Ex: Cozinha Quente, Bar" /></div>
                <div onClick={() => !mutCriarCozinha.isPending && mutCriarCozinha.mutate()}
                  style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 10, textAlign: 'center', color: '#e8d5ff', fontSize: 13, fontWeight: 500, cursor: 'pointer', marginTop: 4 }}>
                  {mutCriarCozinha.isPending ? 'Criando...' : 'Criar cozinha'}
                </div>
              </div>
            </div>
          </div>
        )}

      </div>
    </div>
  );
}