import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/layout/Navbar';
import Dashboard from './pages/Dashboard';
import PersonasList from './pages/personas/PersonasList';
import PersonaForm from './pages/personas/PersonaForm';
import PrestamoForm from './pages/prestamos/PrestamoForm';
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
            <Route path="/prestamos" element={<div style={{padding: '20px'}}>Lista de Préstamos (En construcción)</div>} />
            <Route path="/prestamos/nuevo" element={<PrestamoForm />} />
            <Route path="/prestamos/:id" element={<PrestamoForm />} />

            {/* Fondos */}
            <Route path="/fondos" element={<FondosList />} />
            <Route path="/fondos/nuevo" element={<FondoForm />} />
            <Route path="/fondos/:id" element={<FondoForm />} />

            {/* Otros módulos */}
            <Route path="/garantias" element={<div style={{padding: '20px'}}>Módulo de Garantías (En construcción)</div>} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
