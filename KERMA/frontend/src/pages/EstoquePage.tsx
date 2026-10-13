import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../hooks/AuthContext';
import { listarItensPorEvento } from '../api/itens';
import type { Item } from '../types';
import Navbar from '../components/Navbar';

const Logo = ({ size = 30 }: { size?: number }) => (
  <div style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

const FLAG_MAP: Record<string, { color: string; shadow: string }> = {
  Verde:    { color: '#4ade80', shadow: '#4ade8088' },
  Amarelo:  { color: '#b8860b', shadow: '#b8860b88' },
  Vermelho: { color: '#f87171', shadow: '#f8717188' },
};

const Flag = ({ status, size = 12 }: { status: string; size?: number }) => {
  const s = FLAG_MAP[status] ?? FLAG_MAP['Verde'];
  return (
    <span style={{ width: size, height: size, borderRadius: '50%', background: s.color, boxShadow: `0 0 6px ${s.shadow}`, display: 'inline-block', flexShrink: 0 }} />
  );
};

type Filtro = 'Todos' | 'Verde' | 'Amarelo' | 'Vermelho';

export default function EstoquePage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const EVENTO_ID = usuario?.eventoId ?? 0;
  const [filtro, setFiltro] = useState<Filtro>('Todos');

  const { data: itens = [], isLoading } = useQuery({
    queryKey: ['itens', EVENTO_ID],
    queryFn: () => listarItensPorEvento(EVENTO_ID),
    refetchInterval: 15000,
    enabled: EVENTO_ID > 0,
  });

  const itensFiltrados = filtro === 'Todos' ? itens : itens.filter(i => i.statusEstoque === filtro);

  const contagem = {
    Verde:    itens.filter(i => i.statusEstoque === 'Verde').length,
    Amarelo:  itens.filter(i => i.statusEstoque === 'Amarelo').length,
    Vermelho: itens.filter(i => i.statusEstoque === 'Vermelho').length,
  };

  const FILTROS: { key: Filtro; color: string; bg: string; border: string }[] = [
    { key: 'Todos',    color: '#e8d5ff', bg: '#4a1b8a',  border: '#b8860b55' },
    { key: 'Verde',    color: '#4ade80', bg: '#0a2a1a',  border: '#166534'   },
    { key: 'Amarelo',  color: '#b8860b', bg: '#2a1f00',  border: '#b8860b55' },
    { key: 'Vermelho', color: '#f87171', bg: '#2a0a0a',  border: '#7f1d1d'   },
  ];

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      <Navbar titulo="Estoque" />

      <div style={{ padding: '18px 20px', maxWidth: 1100, margin: '0 auto' }}>

        {/* Legenda */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 14 }}>
          <p style={{ fontSize: 11, color: '#6b4fa0' }}>Status:</p>
          {(['Verde', 'Amarelo', 'Vermelho'] as const).map(s => (
            <div key={s} style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <Flag status={s} size={10} />
              <span style={{ fontSize: 11, color: FLAG_MAP[s].color }}>
                {s === 'Verde' ? 'Verde — estoque ok' : s === 'Amarelo' ? 'Amarelo — atenção' : 'Vermelho — sem estoque'}
              </span>
            </div>
          ))}
        </div>

        {/* Cards de resumo */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 10, marginBottom: 16 }}>
          {(['Verde', 'Amarelo', 'Vermelho'] as const).map(s => (
            <div key={s} onClick={() => setFiltro(s)} style={{ background: '#1a0a2e', border: `0.5px solid ${FLAG_MAP[s].shadow}`, borderRadius: 12, padding: '14px 16px', display: 'flex', alignItems: 'center', gap: 12, cursor: 'pointer' }}>
              <Flag status={s} size={16} />
              <div>
                <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 4 }}>Itens em {s}</p>
                <p style={{ fontSize: 22, fontWeight: 500, color: FLAG_MAP[s].color }}>{contagem[s]}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Tabela */}
        <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
            <p style={{ fontSize: 13, color: '#e8d5ff', fontWeight: 500 }}>Todos os itens</p>
            <div style={{ display: 'flex', gap: 6 }}>
              {FILTROS.map(f => (
                <span key={f.key} onClick={() => setFiltro(f.key)} style={{ fontSize: 11, color: f.color, background: filtro === f.key ? f.bg : '#0f0520', border: `0.5px solid ${filtro === f.key ? f.border : '#2d1155'}`, borderRadius: 6, padding: '4px 10px', cursor: 'pointer' }}>
                  {f.key}
                </span>
              ))}
            </div>
          </div>

          {/* Header */}
          <div style={{ display: 'grid', gridTemplateColumns: '2fr 80px 1fr 1fr 1fr 1fr', gap: 8, padding: '8px 12px', borderBottom: '0.5px solid #2d1155', marginBottom: 4 }}>
            {['Item', 'Status', 'Inicial', 'Atual', 'Mínimo', 'Vendidos'].map(h => (
              <span key={h} style={{ fontSize: 11, color: '#6b4fa0', textAlign: h === 'Item' ? 'left' : 'center' }}>{h}</span>
            ))}
          </div>

          {/* Linhas */}
          {isLoading ? (
            <p style={{ fontSize: 12, color: '#6b4fa0', padding: '12px' }}>Carregando...</p>
          ) : itensFiltrados.length === 0 ? (
            <p style={{ fontSize: 12, color: '#6b4fa0', padding: '12px' }}>Nenhum item encontrado.</p>
          ) : itensFiltrados.map((item: Item, idx) => (
            <div key={item.id} style={{ display: 'grid', gridTemplateColumns: '2fr 80px 1fr 1fr 1fr 1fr', gap: 8, padding: '10px 12px', borderBottom: idx < itensFiltrados.length - 1 ? '0.5px solid #2d115533' : 'none', alignItems: 'center', opacity: item.statusEstoque === 'Vermelho' ? 0.6 : 1 }}>
              <div>
                <p style={{ fontSize: 13, color: '#e8d5ff' }}>{item.nome}</p>
                <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 2 }}>R$ {item.precoVenda.toFixed(2)}</p>
              </div>
              <div style={{ display: 'flex', justifyContent: 'center' }}>
                <Flag status={item.statusEstoque} />
              </div>
              <span style={{ fontSize: 13, color: '#9c7bc4', textAlign: 'center' }}>{item.estoqueInicial}</span>
              <span style={{ fontSize: 13, fontWeight: 500, textAlign: 'center', color: item.statusEstoque === 'Vermelho' ? '#f87171' : item.statusEstoque === 'Amarelo' ? '#b8860b' : '#e8d5ff' }}>
                {item.estoqueAtual}
              </span>
              <span style={{ fontSize: 13, color: '#9c7bc4', textAlign: 'center' }}>{item.estoqueMinimo}</span>
              <span style={{ fontSize: 13, fontWeight: 500, textAlign: 'center', color: '#b8860b' }}>
                {item.estoqueInicial - item.estoqueAtual}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}