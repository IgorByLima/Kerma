import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/AuthContext';
import { login } from '../api/auth';

const Logo = ({ size = 52 }: { size?: number }) => (
  <div style={{
    width: size, height: size,
    background: '#2d1155',
    borderRadius: size * 0.25,
    border: '1px solid #b8860b',
    display: 'flex', alignItems: 'center', justifyContent: 'center',
    flexShrink: 0,
  }}>
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

export default function LoginPage() {
  const [codigo, setCodigo] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  const { entrar } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      const response = await login({ codigo, senha });
      entrar(response);
      navigate('/');
    } catch {
      setErro('Código ou senha inválidos.');
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div style={{
      minHeight: '100vh',
      background: '#0f0520',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      padding: '1rem',
      position: 'relative', overflow: 'hidden',
    }}>
      <div style={{ position: 'absolute', top: -60, right: -60, width: 220, height: 220, borderRadius: '50%', background: '#2d1155', opacity: 0.4 }} />
      <div style={{ position: 'absolute', bottom: -60, left: -40, width: 180, height: 180, borderRadius: '50%', background: '#3d2000', opacity: 0.4 }} />

      <div style={{
        background: '#1a0a2e',
        border: '0.5px solid #2d1155',
        borderRadius: 16,
        padding: '2rem',
        width: '100%', maxWidth: 320,
        position: 'relative', zIndex: 1,
      }}>
        <div style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 12 }}>
            <Logo size={52} />
          </div>
          <p style={{ fontSize: 20, fontWeight: 500, color: '#e8d5ff', letterSpacing: '-0.02em' }}>Kerma</p>
          <p style={{ fontSize: 12, color: '#6b4fa0', marginTop: 3 }}>Sistema de Gestão de Quermesse</p>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div>
            <label style={{ fontSize: 12, color: '#9c7bc4', display: 'block', marginBottom: 6 }}>
              Código de acesso (Evento + Usuário)
            </label>
            <input
              type="text"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.toUpperCase())}
              placeholder="EV-XXXX-US-XXXX"
              required
              style={{
                width: '100%',
                background: '#0f0520',
                border: '0.5px solid #4a1b8a',
                borderRadius: 8,
                padding: '10px 14px',
                color: '#b8860b',
                fontSize: 14,
                letterSpacing: '0.1em',
                fontFamily: 'monospace',
                outline: 'none',
              }}
            />
          </div>

          <div>
            <label style={{ fontSize: 12, color: '#9c7bc4', display: 'block', marginBottom: 6 }}>
              Senha
            </label>
            <input
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              placeholder="••••••••"
              required
              style={{
                width: '100%',
                background: '#0f0520',
                border: '0.5px solid #4a1b8a',
                borderRadius: 8,
                padding: '10px 14px',
                color: '#e8d5ff',
                fontSize: 14,
                outline: 'none',
              }}
            />
          </div>

          {erro && (
            <div style={{
              background: '#2a0a0a',
              border: '0.5px solid #7f1d1d',
              color: '#f87171',
              borderRadius: 8,
              padding: '10px 14px',
              fontSize: 13,
            }}>
              {erro}
            </div>
          )}

          <button
            type="submit"
            disabled={carregando}
            style={{
              background: carregando ? '#2d1155' : '#4a1b8a',
              border: '0.5px solid #b8860b55',
              borderRadius: 8,
              padding: '11px',
              color: '#e8d5ff',
              fontSize: 14,
              fontWeight: 500,
              cursor: carregando ? 'not-allowed' : 'pointer',
              marginTop: 4,
            }}
          >
            {carregando ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>
    </div>
  );
}