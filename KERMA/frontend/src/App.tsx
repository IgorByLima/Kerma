import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/AuthContext';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import CaixaPage from './pages/CaixaPage';
import EstoquePage from './pages/EstoquePage';
import AdminPage from './pages/AdminPage';
import RetiradaPage from './pages/RetiradaPage';
import CozinhaPage from './pages/CozinhaPage';
import DevolucoesPage from './pages/DevolucoesPage';
import AnalyticsPage from './pages/AnalyticsPage';

const RotaProtegida = ({ children }: { children: React.ReactNode }) => {
  const { isLogado, carregando } = useAuth();

  if (carregando) return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh', background: '#0f0520' }}>
      <p style={{ color: '#6b4fa0' }}>Carregando...</p>
    </div>
  );

  return isLogado ? <>{children}</> : <Navigate to="/login" replace />;
};

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={
          <RotaProtegida>
            <DashboardPage />
          </RotaProtegida>
        } />
        <Route path="/analytics" element={
  <RotaProtegida>
    <AnalyticsPage />
  </RotaProtegida>
} />
        <Route path="/caixa" element={
          <RotaProtegida>
            <CaixaPage />
          </RotaProtegida>
        } />
        <Route path="/devolucoes" element={
  <RotaProtegida>
    <DevolucoesPage />
  </RotaProtegida>
} />
        <Route path="/admin" element={
          <RotaProtegida>
            <AdminPage />
          </RotaProtegida>
        } />
        <Route path="/estoque" element={
          <RotaProtegida>
            <EstoquePage />
          </RotaProtegida>
        } />
        
        <Route path="/retirada" element={
          <RotaProtegida>
            <RetiradaPage />
          </RotaProtegida>
        } />
        <Route path="/cozinha" element={
  <RotaProtegida>
    <CozinhaPage />
  </RotaProtegida>
} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;