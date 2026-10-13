import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../hooks/AuthContext';
import api from '../api/client';
import Navbar from '../components/Navbar';

const Logo = ({ size = 30 }: { size?: number }) => (
  <div style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

interface VendasPorHora { hora: number; quantidade: number; total: number; }
interface ItemRanking { nomeItem: string; quantidade: number; total: number; }
interface FormaPagamento { forma: string; quantidade: number; total: number; percentual: number; }
interface Caixa { id: number; status: string; }

interface Analytics {
  totalVendido: number;
  custoEstimado: number;
  lucroEstimado: number;
  margemPercent: number;
  ticketMedio: number;
  totalVendas: number;
  totalTickets: number;
  ticketsRetirados: number;
  ticketsPendentes: number;
  totalDevolucoes: number;
  vendasPorHora: VendasPorHora[];
  rankingItens: ItemRanking[];
  formasPagamento: FormaPagamento[];
}

const FORMA_LABEL: Record<string, string> = {
  Dinheiro: 'Dinheiro', Pix: 'Pix', CartaoDebito: 'Débito', CartaoCredito: 'Crédito'
};

export default function AnalyticsPage() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const EVENTO_ID = usuario?.eventoId ?? 0;
  const [caixaId, setCaixaId] = useState<number | null>(null);

  const { data: caixas = [] } = useQuery<Caixa[]>({
    queryKey: ['caixas', EVENTO_ID],
    queryFn: async () => (await api.get(`/caixas/evento/${EVENTO_ID}`)).data,
    enabled: EVENTO_ID > 0,
  });

  const { data: analytics, isLoading } = useQuery<Analytics>({
    queryKey: ['analytics', EVENTO_ID, caixaId],
    queryFn: async () => {
      const params = new URLSearchParams({ eventoId: String(EVENTO_ID) });
      if (caixaId) params.append('caixaId', String(caixaId));
      return (await api.get(`/vendas/analytics?${params}`)).data;
    },
    enabled: EVENTO_ID > 0,
    refetchInterval: 30000,
  });

  const exportarPDF = () => {
    window.print();
  };

  const maxVendas = Math.max(...(analytics?.vendasPorHora.map(v => v.quantidade) ?? [1]));
  const maxItens = Math.max(...(analytics?.rankingItens.map(i => i.quantidade) ?? [1]));

  const MetricCard = ({ label, value, sub, color = '#e8d5ff' }: { label: string; value: string; sub: string; color?: string }) => (
    <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: '14px 16px' }}>
      <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 6 }}>{label}</p>
      <p style={{ fontSize: 22, fontWeight: 500, color, letterSpacing: '-0.02em' }}>{value}</p>
      <p style={{ fontSize: 11, color: '#6b4fa0', marginTop: 3 }}>{sub}</p>
    </div>
  );

  return (
    <div style={{ minHeight: '100vh', background: '#0f0520' }}>

      <Navbar titulo="Estoque" />

      <div style={{ padding: '16px 20px', maxWidth: 1100, margin: '0 auto' }}>

        {/* Filtros + Export */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <span style={{ fontSize: 11, color: '#6b4fa0' }}>Caixa:</span>
            <span
              onClick={() => setCaixaId(null)}
              style={{ padding: '5px 12px', borderRadius: 20, fontSize: 11, cursor: 'pointer', border: `0.5px solid ${caixaId === null ? '#b8860b55' : '#2d1155'}`, color: caixaId === null ? '#e8d5ff' : '#6b4fa0', background: caixaId === null ? '#4a1b8a' : '#0f0520' }}
            >
              Todos
            </span>
            {caixas.map(c => (
              <span
                key={c.id}
                onClick={() => setCaixaId(c.id)}
                style={{ padding: '5px 12px', borderRadius: 20, fontSize: 11, cursor: 'pointer', border: `0.5px solid ${caixaId === c.id ? '#b8860b55' : '#2d1155'}`, color: caixaId === c.id ? '#e8d5ff' : '#6b4fa0', background: caixaId === c.id ? '#4a1b8a' : '#0f0520' }}
              >
                Caixa #{c.id}
              </span>
            ))}
          </div>
          <div
            onClick={exportarPDF}
            style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 8, padding: '6px 14px', display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}
          >
        <span style={{ color: '#f87171', fontSize: 14 }}>📄</span>
            <span style={{ fontSize: 12, color: '#e8d5ff' }}>Exportar PDF</span>
          </div>
        </div>

        {isLoading ? (
          <p style={{ color: '#6b4fa0', fontSize: 13 }}>Carregando...</p>
        ) : !analytics ? null : (
          <>
            {/* Cards financeiros */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 10, marginBottom: 14 }}>
              <MetricCard label="Total vendido" value={`R$ ${analytics.totalVendido.toFixed(2)}`} sub={`${analytics.totalVendas} vendas pagas`} />
              <MetricCard label="Custo estimado" value={`R$ ${analytics.custoEstimado.toFixed(2)}`} sub="baseado no custo dos itens" />
              <MetricCard label="Lucro estimado" value={`R$ ${analytics.lucroEstimado.toFixed(2)}`} sub={`↑ margem ${analytics.margemPercent}%`} color="#4ade80" />
              <MetricCard label="Ticket médio" value={`R$ ${analytics.ticketMedio.toFixed(2)}`} sub="por venda paga" />
            </div>

            {/* Gráficos linha 1 */}
            <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 12, marginBottom: 12 }}>

              {/* Vendas por hora */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 12 }}>Vendas por hora</p>
                <div style={{ display: 'flex', alignItems: 'flex-end', gap: 6, height: 100 }}>
                  {analytics.vendasPorHora.map(v => (
                    <div key={v.hora} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4, flex: 1 }}>
                      <span style={{ fontSize: 9, color: '#6b4fa0' }}>{v.quantidade}</span>
                      <div
                        style={{ width: '100%', borderRadius: 3, background: v.quantidade === maxVendas ? '#b8860b' : '#4a1b8a', height: `${(v.quantidade / maxVendas) * 80}px` }}
                        title={`${v.hora}h — ${v.quantidade} vendas`}
                      />
                      <span style={{ fontSize: 9, color: '#6b4fa0' }}>{v.hora}h</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Forma de pagamento */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 12 }}>Forma de pagamento</p>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                  {analytics.formasPagamento.map((f, i) => (
                    <div key={f.forma}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                        <span style={{ fontSize: 11, color: '#e8d5ff' }}>{FORMA_LABEL[f.forma] ?? f.forma}</span>
                        <span style={{ fontSize: 11, color: i === 0 ? '#b8860b' : '#6b4fa0' }}>{f.percentual}%</span>
                      </div>
                      <div style={{ height: 4, background: '#2d1155', borderRadius: 2 }}>
                        <div style={{ width: `${f.percentual}%`, height: '100%', background: i === 0 ? '#b8860b' : '#4a1b8a', borderRadius: 2 }} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Gráficos linha 2 */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>

              {/* Ranking itens */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 12 }}>Itens mais vendidos</p>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                  {analytics.rankingItens.map((item, i) => (
                    <div key={item.nomeItem} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                      <span style={{ fontSize: 11, color: i === 0 ? '#b8860b' : '#6b4fa0', fontWeight: 500, width: 16 }}>{i + 1}</span>
                      <span style={{ fontSize: 13, color: '#e8d5ff', flex: 1 }}>{item.nomeItem}</span>
                      <div style={{ width: 80, height: 4, background: '#2d1155', borderRadius: 2 }}>
                        <div style={{ width: `${(item.quantidade / maxItens) * 100}%`, height: '100%', background: i === 0 ? '#b8860b' : '#4a1b8a', borderRadius: 2 }} />
                      </div>
                      <span style={{ fontSize: 12, color: '#9c7bc4', minWidth: 24, textAlign: 'right' }}>{item.quantidade}</span>
                      <span style={{ fontSize: 11, color: '#6b4fa0', minWidth: 60, textAlign: 'right' }}>R$ {item.total.toFixed(2)}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Resumo operacional */}
              <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 12, padding: 16 }}>
                <p style={{ fontSize: 12, color: '#6b4fa0', marginBottom: 12 }}>Resumo operacional</p>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                  {[
                    { label: 'Tickets gerados', value: analytics.totalTickets, color: '#e8d5ff' },
                    { label: 'Tickets retirados', value: analytics.ticketsRetirados, color: '#4ade80' },
                    { label: 'Aguardando retirada', value: analytics.ticketsPendentes, color: '#b8860b' },
                    { label: 'Devoluções', value: analytics.totalDevolucoes, color: '#f87171' },
                  ].map(item => (
                    <div key={item.label} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontSize: 12, color: '#6b4fa0' }}>{item.label}</span>
                      <span style={{ fontSize: 14, color: item.color, fontWeight: 500 }}>{item.value}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}