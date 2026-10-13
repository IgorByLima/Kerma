import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useAuth } from '../hooks/AuthContext';
import { listarItensPorEvento } from '../api/itens';
import { criarVenda, adicionarItem, confirmarPagamento } from '../api/vendas';
import { abrirCaixa, obterCaixaAberto } from '../api/caixas';
import { conectarImpressora, imprimirTickets, impressoraConectada } from '../api/impressora';
import type { Item, Caixa, Ticket } from '../types';

const Logo = ({ size = 30 }: { size?: number }) => (
  <div style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

interface ItemCarrinho {
  item: Item;
  quantidade: number;
}

interface TicketComItem extends Ticket {
  nomeItem: string;
}

const FORMAS = ['Dinheiro', 'Pix', 'CartaoDebito', 'CartaoCredito'];
const FORMAS_LABEL: Record<string, string> = {
  Dinheiro: 'Dinheiro', Pix: 'Pix', CartaoDebito: 'Débito', CartaoCredito: 'Crédito'
};

const StatusBadge = ({ status, qtd }: { status: string; qtd: number }) => {
  const map: Record<string, { bg: string; color: string; border: string }> = {
    Verde:    { bg: '#0a2a1a', color: '#4ade80', border: '#166534' },
    Amarelo:  { bg: '#2a1f00', color: '#b8860b', border: '#b8860b55' },
    Vermelho: { bg: '#2a0a0a', color: '#f87171', border: '#7f1d1d' },
  };
  const s = map[status] ?? map['Verde'];
  return (
    <span style={{ background: s.bg, color: s.color, border: `0.5px solid ${s.border}`, borderRadius: 20, padding: '1px 6px', fontSize: 10 }}>
      {qtd} un.
    </span>
  );
};

export default function CaixaPage() {
  const { usuario } = useAuth();
  const navigate = useNavigate();
  const EVENTO_ID = usuario?.eventoId ?? 0;

  const [carrinho, setCarrinho] = useState<ItemCarrinho[]>([]);
  const [formaPagamento, setFormaPagamento] = useState('Dinheiro');
  const [tickets, setTickets] = useState<TicketComItem[]>([]);
  const [etapa, setEtapa] = useState<'caixa' | 'venda' | 'sucesso'>('caixa');
  const [saldoInicial, setSaldoInicial] = useState('');
  const [caixa, setCaixa] = useState<Caixa | null>(null);
  const [erro, setErro] = useState('');
  const [imprimindo, setImprimindo] = useState(false);
  const [erroImpressora, setErroImpressora] = useState('');
  const [impressoraOk, setImpressoraOk] = useState(false);

  const { data: itens = [], refetch: refetchItens } = useQuery({
    queryKey: ['itens', EVENTO_ID],
    queryFn: () => listarItensPorEvento(EVENTO_ID),
    enabled: EVENTO_ID > 0,
  });

  const { data: caixaExistente } = useQuery({
    queryKey: ['caixa-aberto', EVENTO_ID],
    queryFn: () => obterCaixaAberto(EVENTO_ID),
    enabled: EVENTO_ID > 0,
  });

  useEffect(() => {
    if (caixaExistente) {
      setCaixa(caixaExistente);
      setEtapa('venda');
    }
  }, [caixaExistente]);

  const mutAbrirCaixa = useMutation({
    mutationFn: () => abrirCaixa({
      eventoId: EVENTO_ID,
      saldoInicial: parseFloat(saldoInicial) || 0,
    }),
    onSuccess: (data) => {
      setCaixa(data);
      setEtapa('venda');
      setErro('');
    },
    onError: () => setErro('Erro ao abrir caixa. Tente novamente.'),
  });

  const mutConfirmar = useMutation({
    mutationFn: async () => {
      const venda = await criarVenda({
        eventoId: EVENTO_ID,
        caixaId: caixa!.id,
        formaPagamento,
      });
      for (const ic of carrinho) {
        await adicionarItem(venda.id, {
          itemId: ic.item.id,
          quantidade: ic.quantidade,
        });
      }
      return { pagamento: await confirmarPagamento(venda.id), carrinho };
    },
    onSuccess: ({ pagamento, carrinho: carrinhoAtual }) => {
      // Enriquecer tickets com nome do item
      const ticketsEnriquecidos: TicketComItem[] = pagamento.tickets.map(t => {
        const itemVenda = pagamento.venda.itens.find(iv => iv.id === t.itemVendaId);
        const itemCarrinho = carrinhoAtual.find(ic => ic.item.id === itemVenda?.itemId);
        return {
          ...t,
          nomeItem: itemCarrinho?.item.nome ?? itemVenda?.nomeItem ?? 'Item',
        };
      });
      setTickets(ticketsEnriquecidos);
      setEtapa('sucesso');
      setCarrinho([]);
      refetchItens();
    },
    onError: () => setErro('Erro ao confirmar pagamento. Tente novamente.'),
  });

const handleImprimir = async () => {
  console.log('Tickets:', tickets);
  setErroImpressora('');
  setImprimindo(true);
  try {
    if (!impressoraConectada()) {
      await conectarImpressora();
      setImpressoraOk(true);
    }
    await imprimirTickets(tickets.map((t, i) => ({
      codigo:       t.codigo,
      nomeItem:     t.nomeItem,
      eventoNome:   usuario?.eventoNome ?? 'Kerma',
      numeroTicket: i + 1,
      totalTickets: tickets.length,
    })));
    setImpressoraOk(true);
  } catch (err: unknown) {
    setErroImpressora(err instanceof Error ? err.message : 'Erro ao imprimir.');
  } finally {
    setImprimindo(false);
  }
};
  const adicionarAoCarrinho = (item: Item) => {
    if (item.estoqueAtual === 0) return;
    setCarrinho(prev => {
      const existente = prev.find(i => i.item.id === item.id);
      if (existente) {
        if (existente.quantidade >= item.estoqueAtual) return prev;
        return prev.map(i => i.item.id === item.id ? { ...i, quantidade: i.quantidade + 1 } : i);
      }
      return [...prev, { item, quantidade: 1 }];
    });
  };

  const alterarQuantidade = (itemId: number, delta: number) => {
    setCarrinho(prev => prev
      .map(i => i.item.id === itemId ? { ...i, quantidade: i.quantidade + delta } : i)
      .filter(i => i.quantidade > 0)
    );
  };

  const definirQuantidade = (itemId: number, valor: number, estoqueMax: number) => {
    if (isNaN(valor) || valor < 1) return;
    const quantidade = Math.min(valor, estoqueMax);
    setCarrinho(prev => prev.map(i => i.item.id === itemId ? { ...i, quantidade } : i));
  };

  const total = carrinho.reduce((acc, i) => acc + i.item.precoVenda * i.quantidade, 0);
  const totalTickets = carrinho.reduce((acc, i) => acc + i.quantidade, 0);

  // ── Tela: abrir caixa ────────────────────────────────────────────────────
  if (etapa === 'caixa') return (
    <div style={{ minHeight: '100vh', background: '#0f0520', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 16, padding: '2rem', width: 320 }}>
        <div style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 12 }}>
            <Logo size={48} />
          </div>
          <p style={{ color: '#e8d5ff', fontSize: 16, fontWeight: 500 }}>Abrir Caixa</p>
          <p style={{ color: '#6b4fa0', fontSize: 12, marginTop: 4 }}>Informe o saldo inicial para começar</p>
          <p style={{ color: '#b8860b', fontSize: 11, marginTop: 4, fontFamily: 'monospace' }}>{usuario?.eventoNome}</p>
        </div>

        <label style={{ fontSize: 12, color: '#9c7bc4', display: 'block', marginBottom: 6 }}>Saldo inicial (R$)</label>
        <input
          type="number"
          value={saldoInicial}
          onChange={e => setSaldoInicial(e.target.value)}
          placeholder="0,00"
          style={{ width: '100%', background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 8, padding: '10px 14px', color: '#e8d5ff', fontSize: 14, outline: 'none', marginBottom: 12 }}
        />

        {erro && <p style={{ color: '#f87171', fontSize: 12, marginBottom: 10 }}>{erro}</p>}

        <div
          onClick={() => !mutAbrirCaixa.isPending && mutAbrirCaixa.mutate()}
          style={{ background: '#4a1b8a', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 11, textAlign: 'center', color: '#e8d5ff', fontSize: 14, fontWeight: 500, cursor: 'pointer', marginBottom: 8 }}
        >
          {mutAbrirCaixa.isPending ? 'Abrindo...' : 'Abrir Caixa'}
        </div>

        <div onClick={() => navigate('/')} style={{ textAlign: 'center', color: '#6b4fa0', fontSize: 12, cursor: 'pointer', marginTop: 8 }}>
          ← Voltar ao dashboard
        </div>
      </div>
    </div>
  );

  // ── Tela: revisão de tickets ─────────────────────────────────────────────
  if (etapa === 'sucesso') return (
    <div style={{ minHeight: '100vh', background: '#0f0520', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div style={{ background: '#1a0a2e', border: '0.5px solid #2d1155', borderRadius: 16, padding: '2rem', width: 420 }}>

        <div style={{ textAlign: 'center', marginBottom: 20 }}>
          <div style={{ fontSize: 40, marginBottom: 8 }}>🎟️</div>
          <p style={{ color: '#e8d5ff', fontSize: 16, fontWeight: 500 }}>Pagamento confirmado!</p>
          <p style={{ color: '#6b4fa0', fontSize: 12, marginTop: 4 }}>
            Confira os tickets abaixo antes de imprimir
          </p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginBottom: 20, maxHeight: 280, overflowY: 'auto' }}>
          {tickets.map((t, i) => (
            <div key={t.codigo} style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 14px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <span style={{ fontSize: 11, color: '#6b4fa0', minWidth: 20 }}>#{i + 1}</span>
                <div>
                  <p style={{ color: '#b8860b', fontSize: 13, fontFamily: 'monospace', letterSpacing: '0.1em' }}>{t.codigo}</p>
                  <p style={{ color: '#6b4fa0', fontSize: 11, marginTop: 2 }}>{t.nomeItem}</p>
                </div>
              </div>
              <span style={{ fontSize: 10, color: '#4ade80', background: '#0a2a1a', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 8px' }}>
                Pendente
              </span>
            </div>
          ))}
        </div>

        <div style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 14px', marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
          <span style={{ fontSize: 12, color: '#6b4fa0' }}>Total de tickets</span>
          <span style={{ fontSize: 12, color: '#e8d5ff', fontWeight: 500 }}>
            {tickets.length} ticket{tickets.length > 1 ? 's' : ''}
          </span>
        </div>

        {erroImpressora && (
          <div style={{ background: '#2a0a0a', border: '0.5px solid #7f1d1d', color: '#f87171', borderRadius: 8, padding: '10px 14px', fontSize: 12, marginBottom: 12 }}>
            {erroImpressora}
          </div>
        )}

        {impressoraOk && !erroImpressora && (
          <div style={{ background: '#0a2a1a', border: '0.5px solid #166534', color: '#4ade80', borderRadius: 8, padding: '10px 14px', fontSize: 12, marginBottom: 12 }}>
            ✅ Impressora conectada — MPT-II
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
<div
  onClick={handleImprimir}
  style={{ background: imprimindo ? '#2d1155' : 'linear-gradient(135deg, #4a1b8a, #3d2000)', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 11, textAlign: 'center', cursor: imprimindo ? 'not-allowed' : 'pointer' }}
>
  <p style={{ color: '#e8d5ff', fontSize: 14, fontWeight: 500, pointerEvents: 'none' }}>
    {imprimindo ? '🖨️ Imprimindo...' : impressoraConectada() ? '🖨️ Imprimir tickets' : '🔵 Conectar e imprimir'}
  </p>
  <p style={{ color: '#9c7bc4', fontSize: 11, marginTop: 2, pointerEvents: 'none' }}>
    {impressoraConectada() ? 'Impressora conectada ✅' : 'Impressora Bluetooth MPT-II'}
  </p>
</div>

          <div
  onClick={() => { setEtapa('venda'); setErro(''); setErroImpressora(''); refetchItens(); }}
  style={{ background: '#4a1b8a', border: '0.5px solid #2d1155', borderRadius: 8, padding: 11, textAlign: 'center', color: '#e8d5ff', fontSize: 14, fontWeight: 500, cursor: 'pointer' }}
>
  Nova venda
</div>

<div
  onClick={() => { setEtapa('venda'); setErro(''); setErroImpressora(''); refetchItens(); }}
  style={{ textAlign: 'center', color: '#6b4fa0', fontSize: 12, cursor: 'pointer', marginTop: 4 }}
>
  ← Voltar à venda
</div>
        </div>
      </div>
    </div>
  );

  // ── Tela principal: frente de caixa ──────────────────────────────────────
  return (
    <div style={{ minHeight: '100vh', background: '#0f0520', display: 'flex', flexDirection: 'column' }}>

      <div style={{ background: '#1a0a2e', borderBottom: '0.5px solid #2d1155', padding: '10px 20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <Logo size={30} />
          <span style={{ color: '#e8d5ff', fontSize: 15, fontWeight: 500 }}>Kerma</span>
          <span style={{ fontSize: 11, color: '#4ade80', border: '0.5px solid #166534', borderRadius: 20, padding: '2px 10px', background: '#0a2a1a' }}>
            Caixa #{caixa?.id} aberto
          </span>
          <span style={{ fontSize: 11, color: '#b8860b', border: '0.5px solid #b8860b44', borderRadius: 20, padding: '2px 10px' }}>
            {usuario?.eventoNome}
          </span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <span style={{ fontSize: 12, color: '#6b4fa0' }}>{usuario?.nome}</span>
          <span style={{ fontSize: 11, color: '#c084fc', border: '0.5px solid #4a1b8a', borderRadius: 20, padding: '2px 8px' }}>{usuario?.perfil}</span>
          <span onClick={() => navigate('/')} style={{ fontSize: 11, color: '#f87171', border: '0.5px solid #7f1d1d', borderRadius: 20, padding: '2px 8px', cursor: 'pointer', background: '#2a0a0a' }}>
            ← Dashboard
          </span>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', flex: 1, overflow: 'hidden' }}>

        <div style={{ padding: 16, borderRight: '0.5px solid #2d1155', overflowY: 'auto' }}>
          <p style={{ color: '#e8d5ff', fontSize: 14, fontWeight: 500, marginBottom: 14 }}>Catálogo de itens</p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 8 }}>
            {itens.map(item => {
              const semEstoque = item.estoqueAtual === 0;
              return (
                <div
                  key={item.id}
                  onClick={() => adicionarAoCarrinho(item)}
                  style={{ background: '#1a0a2e', border: `0.5px solid ${item.estoqueCritico && !semEstoque ? '#b8860b55' : '#2d1155'}`, borderRadius: 10, padding: 12, cursor: semEstoque ? 'not-allowed' : 'pointer', opacity: semEstoque ? 0.5 : 1 }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
                    <span style={{ fontSize: 15, fontWeight: 500, color: '#e8d5ff' }}>{item.nome}</span>
                    <StatusBadge status={item.statusEstoque} qtd={item.estoqueAtual} />
                  </div>
                  <p style={{ fontSize: 20, fontWeight: 600, color: '#b8860b', marginBottom: 8 }}>
                    R$ {item.precoVenda.toFixed(2)}
                  </p>
                  <div style={{ background: semEstoque ? '#2d1155' : '#4a1b8a', borderRadius: 6, padding: 6, textAlign: 'center', fontSize: 12, color: semEstoque ? '#6b4fa0' : '#e8d5ff' }}>
                    {semEstoque ? 'Sem estoque' : '+ Adicionar'}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 12, overflowY: 'auto' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <p style={{ color: '#e8d5ff', fontSize: 14, fontWeight: 500 }}>Carrinho</p>
            {carrinho.length > 0 && (
              <span onClick={() => setCarrinho([])} style={{ fontSize: 11, color: '#6b4fa0', cursor: 'pointer' }}>Limpar</span>
            )}
          </div>

          {carrinho.length === 0 ? (
            <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <p style={{ color: '#6b4fa0', fontSize: 13 }}>Clique nos itens para adicionar</p>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8, flex: 1 }}>
              {carrinho.map(ic => (
                <div key={ic.item.id} style={{ background: '#0f0520', border: '0.5px solid #2d1155', borderRadius: 8, padding: '10px 12px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 6 }}>
                    <span style={{ fontSize: 13, color: '#e8d5ff' }}>{ic.item.nome}</span>
                    <span style={{ fontSize: 13, color: '#b8860b', fontWeight: 500 }}>
                      R$ {(ic.item.precoVenda * ic.quantidade).toFixed(2)}
                    </span>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <span style={{ fontSize: 11, color: '#6b4fa0' }}>
                      R$ {ic.item.precoVenda.toFixed(2)} × {ic.quantidade}
                    </span>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <div onClick={() => alterarQuantidade(ic.item.id, -1)} style={{ width: 22, height: 22, background: '#2d1155', borderRadius: 4, display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', color: '#e8d5ff', fontSize: 14 }}>−</div>
                      <input
                        type="number"
                        value={ic.quantidade}
                        min={1}
                        max={ic.item.estoqueAtual}
                        onChange={e => definirQuantidade(ic.item.id, parseInt(e.target.value), ic.item.estoqueAtual)}
                        onClick={e => (e.target as HTMLInputElement).select()}
                        style={{ width: 44, textAlign: 'center', background: '#0f0520', border: '0.5px solid #4a1b8a', borderRadius: 4, color: '#e8d5ff', fontSize: 13, padding: '2px 4px', outline: 'none' }}
                      />
                      <div onClick={() => alterarQuantidade(ic.item.id, 1)} style={{ width: 22, height: 22, background: '#4a1b8a', borderRadius: 4, display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', color: '#e8d5ff', fontSize: 14 }}>+</div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          <div style={{ borderTop: '0.5px solid #2d1155', paddingTop: 12 }}>
            <p style={{ fontSize: 11, color: '#6b4fa0', marginBottom: 8 }}>Forma de pagamento</p>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 6, marginBottom: 12 }}>
              {FORMAS.map(f => (
                <div
                  key={f}
                  onClick={() => setFormaPagamento(f)}
                  style={{ background: formaPagamento === f ? '#4a1b8a' : '#0f0520', border: `0.5px solid ${formaPagamento === f ? '#b8860b55' : '#2d1155'}`, borderRadius: 6, padding: 7, textAlign: 'center', fontSize: 11, color: formaPagamento === f ? '#e8d5ff' : '#6b4fa0', cursor: 'pointer' }}
                >
                  {FORMAS_LABEL[f]}
                </div>
              ))}
            </div>

            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
              <span style={{ fontSize: 13, color: '#6b4fa0' }}>Total</span>
              <span style={{ fontSize: 22, fontWeight: 500, color: '#e8d5ff', letterSpacing: '-0.02em' }}>
                R$ {total.toFixed(2)}
              </span>
            </div>

            {erro && <p style={{ color: '#f87171', fontSize: 12, marginBottom: 10 }}>{erro}</p>}

            <div
              onClick={() => carrinho.length > 0 && !mutConfirmar.isPending && mutConfirmar.mutate()}
              style={{ background: carrinho.length === 0 ? '#2d1155' : 'linear-gradient(135deg, #4a1b8a, #3d2000)', border: '0.5px solid #b8860b55', borderRadius: 8, padding: 12, textAlign: 'center', cursor: carrinho.length === 0 ? 'not-allowed' : 'pointer', opacity: carrinho.length === 0 ? 0.5 : 1 }}
            >
              <p style={{ color: '#e8d5ff', fontSize: 14, fontWeight: 500 }}>
                {mutConfirmar.isPending ? 'Processando...' : 'Confirmar pagamento'}
              </p>
              {carrinho.length > 0 && (
                <p style={{ color: '#9c7bc4', fontSize: 11, marginTop: 2 }}>
                  Gera {totalTickets} ticket{totalTickets > 1 ? 's' : ''} automaticamente
                </p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}