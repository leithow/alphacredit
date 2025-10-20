import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/layout/Navbar';
import Dashboard from './pages/Dashboard';
import PersonasList from './pages/personas/PersonasList';
import PersonaForm from './pages/personas/PersonaForm';
import PrestamosList from './pages/prestamos/PrestamosList';
import PrestamoForm from './pages/prestamos/PrestamoForm';
import GarantiasList from './pages/garantias/GarantiasList';
import GarantiaForm from './pages/garantias/GarantiaForm';
import FondosList from './pages/fondos/FondosList';
import FondoForm from './pages/fondos/FondoForm';
import './App.css';

function App() {
  return (
    <Router>
      <div className="App">
        <Navbar />
        <main className="main-content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/personas" element={<PersonasList />} />
            <Route path="/personas/nuevo" element={<PersonaForm />} />
            <Route path="/personas/:id" element={<PersonaForm />} />

            {/* Préstamos */}
            <Route path="/prestamos" element={<PrestamosList />} />
            <Route path="/prestamos/nuevo" element={<PrestamoForm />} />
            <Route path="/prestamos/:id" element={<PrestamoForm />} />

            {/* Garantías - Independientes */}
            <Route path="/garantias" element={<GarantiasList />} />
            <Route path="/garantias/nuevo" element={<GarantiaForm />} />
            <Route path="/garantias/:garantiaId" element={<GarantiaForm />} />

            {/* Garantías - Asociadas a préstamos */}
            <Route path="/prestamos/:prestamoId/garantias" element={<GarantiasList />} />
            <Route path="/prestamos/:prestamoId/garantias/nueva" element={<GarantiaForm />} />
            <Route path="/prestamos/:prestamoId/garantias/:garantiaId" element={<GarantiaForm />} />

            {/* Fondos */}
            <Route path="/fondos" element={<FondosList />} />
            <Route path="/fondos/nuevo" element={<FondoForm />} />
            <Route path="/fondos/:id" element={<FondoForm />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
