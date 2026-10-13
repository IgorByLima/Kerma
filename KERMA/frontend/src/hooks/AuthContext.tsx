import { createContext, useContext, useState, useEffect } from 'react';
import type { LoginResponse } from '../types';

interface UsuarioLogado {
  nome: string;
  perfil: string;
  token: string;
  eventoId: number;
  eventoCodigo: string;
  eventoNome: string;
}

interface AuthContextType {
  usuario: UsuarioLogado | null;
  carregando: boolean;
  entrar: (response: LoginResponse) => void;
  sair: () => void;
  isAdmin: boolean;
  isLogado: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('kerma_token');
    const dadosSalvos = localStorage.getItem('kerma_usuario');
    if (token && dadosSalvos) {
      setUsuario(JSON.parse(dadosSalvos));
    }
    setCarregando(false);
  }, []);

  const entrar = (response: LoginResponse) => {
    const dadosUsuario: UsuarioLogado = {
      nome:         response.nome,
      perfil:       response.perfil,
      token:        response.token,
      eventoId:     response.eventoId,
      eventoCodigo: response.eventoCodigo,
      eventoNome:   response.eventoNome,
    };
    localStorage.setItem('kerma_token', response.token);
    localStorage.setItem('kerma_usuario', JSON.stringify(dadosUsuario));
    setUsuario(dadosUsuario);
  };

  const sair = () => {
    localStorage.removeItem('kerma_token');
    localStorage.removeItem('kerma_usuario');
    setUsuario(null);
  };

  return (
    <AuthContext.Provider value={{
      usuario,
      carregando,
      entrar,
      sair,
      isAdmin:  usuario?.perfil === 'Admin',
      isLogado: !!usuario,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return context;
}