import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import personaService from '../services/personaService';
import prestamoService from '../services/prestamoService';
import './Dashboard.css';

const Dashboard = () => {
  const [stats, setStats] = useState({
    totalPersonas: 0,
    totalPrestamos: 0,
    loading: true,
  });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [personasData, prestamosData] = await Promise.all([
        personaService.getPersonas(1, 1),
        prestamoService.getPrestamos(1, 1),
      ]);

      setStats({
        totalPersonas: personasData.totalCount,
        totalPrestamos: prestamosData.totalCount,
        loading: false,
      });
    } catch (error) {
      console.error('Error al cargar datos del dashboard:', error);
      setStats((prev) => ({ ...prev, loading: false }));
    }
  };

  if (stats.loading) {
    return (
      <div className="dashboard">
        <div className="dashboard-loading">Cargando dashboard...</div>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Dashboard</h1>
        <p>Bienvenido al sistema AlphaCredit</p>
      </div>

      <div className="dashboard-stats">
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-content">
            <h3>Personas</h3>
            <p className="stat-number">{stats.totalPersonas}</p>
            <Link to="/personas" className="stat-link">
              Ver todas →
            </Link>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">💰</div>
          <div className="stat-content">
            <h3>Préstamos</h3>
            <p className="stat-number">{stats.totalPrestamos}</p>
            <Link to="/prestamos" className="stat-link">
              Ver todos →
            </Link>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🔒</div>
          <div className="stat-content">
            <h3>Garantías</h3>
            <p className="stat-number">-</p>
            <Link to="/garantias" className="stat-link">
              Ver todas →
            </Link>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">💼</div>
          <div className="stat-content">
            <h3>Fondos</h3>
            <p className="stat-number">-</p>
            <Link to="/fondos" className="stat-link">
              Ver todos →
            </Link>
          </div>
        </div>
      </div>

      <div className="dashboard-quick-actions">
        <h2>Acciones Rápidas</h2>
        <div className="quick-actions-grid">
          <Link to="/personas/nuevo" className="quick-action-card">
            <span className="action-icon">➕</span>
            <span>Nueva Persona</span>
          </Link>
          <Link to="/prestamos/nuevo" className="quick-action-card">
            <span className="action-icon">➕</span>
            <span>Nuevo Préstamo</span>
          </Link>
          <Link to="/garantias/nuevo" className="quick-action-card">
            <span className="action-icon">➕</span>
            <span>Nueva Garantía</span>
          </Link>
          <Link to="/fondos/nuevo" className="quick-action-card">
            <span className="action-icon">➕</span>
            <span>Nuevo Fondo</span>
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
