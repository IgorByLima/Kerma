import { useAuth } from '../hooks/AuthContext';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { listarVendasPorCaixa } from '../api/vendas';
import { listarItensPorEvento } from '../api/itens';
import Navbar from '../components/Navbar';

const Logo = ({ size = 30 }: { size?: number }) => (
  <div style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

const MetricCard = ({ label, value, sub }: { label: string; value: string; sub: string }) => (
  <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: '14px 16px' }}>
    <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 6 }}>{label}</p>
    <p style={{ fontSize: 22, fontWeight: 500, color: '#e8d5ff', letterSpacing: '-0.02em' }}>{value}</p>
    <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 3 }}>{sub}</p>
  </div>
);

const NavCard = ({ icon, label, gold, onClick }: { icon: string; label: string; gold?: boolean; onClick?: () => void }) => (
  <div onClick={onClick} style={{ background: '#1a0a2e', border: `0.5px solid ${gold ? '#b8860b44' : '#2d1155'}`, borderRadius: 12, padding: 14, cursor: 'pointer' }}>
    <div style={{ width: 32, height: 32, borderRadius: 8, background: gold ? '#3d200022' : '#2d115533', display: 'flex', alignItems: 'center', justifyContent: 'center', marginBottom: 8 }}>
      <i className={`ti ti-${icon}`} style={{ color: gold ? '#b8860b' : '#c084fc', fontSize: 16 }} aria-hidden="true" />
    </div>
    <p style={{ color: '#e8d5ff', fontSize: 12, fontWeight: 500 }}>{label}</p>
  </div>
);

const StatusBadge = ({ status }: { status: string }) => {
  const map: Record<string, { bg: string; color: string; border: string }> = {
    Verde:    { bg: '#0a2a1a', color: '#4ade80', border: '#166534' },
    Amarelo:  { bg: '#2a1f00', color: '#b8860b', border: '#b8860b55' },
    Vermelho: { bg: '#2a0a0a', color: '#f87171', border: '#7f1d1d' },
  };
  const s = map[status] ?? map['Verde'];
  return (
    <span style={{ background: s.bg, color: s.color, border: `0.5px solid ${s.border}`, borderRadius: 20, padding: '2px 8px', fontSize: 10 }}>
      {status}
    </span>
  );
};

const CAIXA_ID = 2;

export default function DashboardPage() {
  const { usuario, sair, isAdmin } = useAuth();
  const navigate = useNavigate();
  const EVENTO_ID = usuario?.eventoId ?? 0;

  const { data: vendas = [] } = useQuery({
    queryKey: ['vendas', CAIXA_ID],
    queryFn: () => listarVendasPorCaixa(CAIXA_ID),
    refetchInterval: 30000,
    enabled: EVENTO_ID > 0,
  });

  const { data: itens = [] } = useQuery({
    queryKey: ['itens', EVENTO_ID],
    queryFn: () => listarItensPorEvento(EVENTO_ID),
    refetchInterval: 30000,
    enabled: EVENTO_ID > 0,
  });

  const vendasPagas    = vendas.filter(v => v.status === 'Paga');
  const totalVendido   = vendasPagas.reduce((acc, v) => acc + v.total, 0);
  const ticketsGerados = vendasPagas.reduce((acc, v) => acc + v.itens.reduce((a, i) => a + i.quantidade, 0), 0);
  const ticketMedio    = vendasPagas.length > 0 ? totalVendido / vendasPagas.length : 0;

  const rankingMap: Record<string, { nome: string; qtd: number }> = {};
  vendasPagas.forEach(v => v.itens.forEach(i => {
    if (!rankingMap[i.itemId]) rankingMap[i.itemId] = { nome: i.nomeItem, qtd: 0 };
    rankingMap[i.itemId].qtd += i.quantidade;
  }));
  const ranking = Object.values(rankingMap).sort((a, b) => b.qtd - a.qtd).slice(0, 3);
  const maxQtd  = ranking[0]?.qtd || 1;

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      <Navbar titulo="Estoque" />

      <div style={{ padding: '18px 20px', maxWidth: 1100, margin: '0 auto' }}>

        {/* Header */}
        <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 16 }}>
          <div>
            <p style={{ color: '#e8d5ff', fontSize: 15, fontWeight: 500 }}>Bem-vindo, {usuario?.nome}</p>
            <p style={{ color: '#6b4fa0', fontSize: 12, marginTop: 2 }}>Hoje · Evento ativo · {usuario?.eventoCodigo}</p>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <div style={{ width: 7, height: 7, borderRadius: '50%', background: '#4ade80' }} />
            <span style={{ fontSize: 11, color: '#4ade80' }}>Ao vivo</span>
          </div>
        </div>

        {/* Métricas */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 10, marginBottom: 14 }}>
          <MetricCard label="Total vendido" value={`R$ ${totalVendido.toFixed(2)}`} sub={`${vendasPagas.length} vendas pagas`} />
          <MetricCard label="Vendas hoje" value={String(vendas.length)} sub={`${vendasPagas.length} pagas · ${vendas.filter(v => v.status === 'Aberta').length} abertas`} />
          <MetricCard label="Tickets gerados" value={String(ticketsGerados)} sub="unidades vendidas" />
          <MetricCard label="Ticket médio" value={`R$ ${ticketMedio.toFixed(2)}`} sub="por venda paga" />
        </div>

        {/* Ranking + Estoque */}
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10, marginBottom: 14 }}>

          <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
              <p style={{ fontSize: 12, color: '#6b4fa0' }}>Itens mais vendidos</p>
              <i className="ti ti-trophy" style={{ color: '#b8860b', fontSize: 14 }} aria-hidden="true" />
            </div>
            {ranking.length === 0 ? (
              <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhuma venda ainda</p>
            ) : ranking.map((item, i) => (
              <div key={item.nome} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span style={{ fontSize: 11, color: i === 0 ? '#b8860b' : '#6b4fa0', fontWeight: 500, width: 16 }}>{i + 1}</span>
                  <span style={{ fontSize: 13, color: '#e8d5ff' }}>{item.nome}</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <div style={{ width: 60, height: 4, background: '#2d1155', borderRadius: 2, overflow: 'hidden' }}>
                    <div style={{ width: `${(item.qtd / maxQtd) * 100}%`, height: '100%', background: i === 0 ? '#b8860b' : '#6b21a8', borderRadius: 2 }} />
                  </div>
                  <span style={{ fontSize: 12, color: '#9c7bc4', minWidth: 24, textAlign: 'right' }}>{item.qtd}</span>
                </div>
              </div>
            ))}
          </div>

          <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
              <p style={{ fontSize: 12, color: '#6b4fa0' }}>Semáforo de estoque</p>
              <i className="ti ti-package" style={{ color: '#6b4fa0', fontSize: 14 }} aria-hidden="true" />
            </div>
            {itens.length === 0 ? (
              <p style={{ fontSize: 12, color: '#6b4fa0' }}>Nenhum item cadastrado</p>
            ) : itens.map(item => (
              <div key={item.id} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <StatusBadge status={item.statusEstoque} />
                  <span style={{ fontSize: 13, color: '#e8d5ff' }}>{item.nome}</span>
                </div>
                <span style={{ fontSize: 12, color: '#6b4fa0' }}>{item.estoqueAtual} un.</span>
              </div>
            ))}
          </div>

        </div>

        {/* Navegação */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(6, 1fr)', gap: 8 }}>
          <NavCard icon="building-store" label="Caixa" onClick={() => navigate('/caixa')} />
          <NavCard icon="package" label="Estoque" gold onClick={() => navigate('/estoque')} />
          {isAdmin && <NavCard icon="settings" label="Admin" gold onClick={() => navigate('/admin')} />}
          <NavCard icon="ticket" label="Retirada" onClick={() => navigate('/retirada')} />
          <NavCard icon="chef-hat" label="Cozinha" onClick={() => navigate('/cozinha')} />
          <NavCard icon="arrow-back-up" label="Devoluções" onClick={() => navigate('/devolucoes')} />
            <NavCard icon="chart-bar" label="Dashboard" onClick={() => navigate('/analytics')} />
        </div>

      </div>
    </div>
  );
}