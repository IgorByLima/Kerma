import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/AuthContext';

interface NavbarProps {
  titulo: string;
  corTitulo?: string;
  bgTitulo?: string;
  borderTitulo?: string;
  mostrarUsuario?: boolean;
  acaoExtra?: React.ReactNode;
}

const Logo = ({ size = 30, onClick }: { size?: number; onClick?: () => void }) => (
  <div
    onClick={onClick}
    style={{ width: size, height: size, background: '#2d1155', borderRadius: size * 0.25, border: '1px solid #b8860b', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0, cursor: onClick ? 'pointer' : 'default' }}
  >
    <svg width={size * 0.6} height={size * 0.6} viewBox="0 0 32 32">
      <text x="3" y="24" fontSize="22" fontWeight="700" fill="#e8d5ff" fontFamily="sans-serif">K</text>
      <text x="17" y="14" fontSize="13" fontWeight="600" fill="#b8860b" fontFamily="sans-serif">$</text>
    </svg>
  </div>
);

export { Logo };

export default function Navbar({ titulo, corTitulo = '#b8860b', bgTitulo = 'transparent', borderTitulo = '#b8860b44', mostrarUsuario = false, acaoExtra }: NavbarProps) {
  const navigate = useNavigate();
  const { usuario } = useAuth();

  return (
    <div style={{ background: '#1a0a2e', borderBottom: '0.5px solid #2d1155', padding: '11px 20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        <Logo size={30} onClick={() => navigate('/')} />
        <span style={{ color: '#e8d5ff', fontSize: 15, fontWeight: 500 }}>Kerma</span>
        <span style={{ fontSize: 11, color: corTitulo, border: `0.5px solid ${borderTitulo}`, borderRadius: 20, padding: '2px 10px', background: bgTitulo }}>
          {titulo}
        </span>
        {acaoExtra}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        {mostrarUsuario && (
          <>
            <span style={{ fontSize: 12, color: '#6b4fa0' }}>{usuario?.nome}</span>
            <span style={{ fontSize: 11, color: '#c084fc', border: '0.5px solid #4a1b8a', borderRadius: 20, padding: '2px 8px' }}>{usuario?.perfil}</span>
          </>
        )}
        <span onClick={() => navigate('/')} style={{ fontSize: 11, color: '#6b4fa0', cursor: 'pointer' }}>← Home</span>
      </div>
    </div>
  );
}