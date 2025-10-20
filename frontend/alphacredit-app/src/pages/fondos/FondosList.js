import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import fondoService from '../../services/fondoService';
import Table from '../../components/common/Table';
import Button from '../../components/common/Button';
import MovimientoFondoModal from '../../components/common/MovimientoFondoModal';
import FondoMovimientosModal from '../../components/common/FondoMovimientosModal';
import { formatCurrency } from '../../utils/currency';
import './FondosList.css';

const FondosList = () => {
  const navigate = useNavigate();
  const [fondos, setFondos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
  });
  const [showMovimientoModal, setShowMovimientoModal] = useState(false);
  const [showVerMovimientosModal, setShowVerMovimientosModal] = useState(false);
  const [fondoSeleccionado, setFondoSeleccionado] = useState(null);

  useEffect(() => {
    loadFondos();
  }, [pagination.pageNumber, pagination.pageSize]);

  const loadFondos = async () => {
    try {
      setLoading(true);
      const response = await fondoService.getFondos(
        pagination.pageNumber,
        pagination.pageSize
      );
      setFondos(response.data);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.totalCount,
      }));
    } catch (error) {
      console.error('Error al cargar fondos:', error);
      alert('Error al cargar los fondos');
    } finally {
      setLoading(false);
    }
  };

  const handleNuevoFondo = () => {
    navigate('/fondos/nuevo');
  };

  const handleEditarFondo = (fondo) => {
    navigate(`/fondos/${fondo.fondoId}`);
  };

  const handleAgregarMovimiento = (fondo) => {
    setFondoSeleccionado(fondo);
    setShowMovimientoModal(true);
  };

  const handleMovimientoCreado = () => {
    setShowMovimientoModal(false);
    setFondoSeleccionado(null);
    loadFondos(); // Recargar para mostrar saldo actualizado
  };

  const handleVerMovimientos = (fondo) => {
    setFondoSeleccionado(fondo);
    setShowVerMovimientosModal(true);
  };

  const columns = [
    {
      header: 'ID',
      field: 'fondoId',
      width: '80px',
    },
    {
      header: 'Nombre',
      field: 'fondoNombre',
    },
    {
      header: 'Tipo',
      render: (row) => row.tipoFondo?.tipoFondoNombre || '-',
    },
    {
      header: 'Moneda',
      render: (row) => row.moneda?.monedaCodigo || '-',
      width: '100px',
    },
    {
      header: 'Saldo Inicial',
      render: (row) => formatCurrency(row.fondoSaldoInicial),
      width: '150px',
    },
    {
      header: 'Saldo Actual',
      render: (row) => (
        <span className={`saldo ${row.fondoSaldoActual < 0 ? 'negativo' : 'positivo'}`}>
          {formatCurrency(row.fondoSaldoActual)}
        </span>
      ),
      width: '150px',
    },
    {
      header: 'Estado',
      field: 'fondoEstaActivo',
      width: '100px',
      render: (row) => (
        <span className={`status-badge ${row.fondoEstaActivo ? 'active' : 'inactive'}`}>
          {row.fondoEstaActivo ? 'Activo' : 'Inactivo'}
        </span>
      ),
    },
    {
      header: 'Acciones',
      width: '350px',
      render: (row) => (
        <div className="action-buttons">
          <Button
            variant="info"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleVerMovimientos(row);
            }}
          >
            👁️ Ver
          </Button>
          <Button
            variant="primary"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleAgregarMovimiento(row);
            }}
          >
            💰 Movimiento
          </Button>
          <Button
            variant="secondary"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleEditarFondo(row);
            }}
          >
            Editar
          </Button>
        </div>
      ),
    },
  ];

  const handlePageChange = (newPage) => {
    setPagination((prev) => ({
      ...prev,
      pageNumber: newPage,
    }));
  };

  const handlePageSizeChange = (newSize) => {
    setPagination({
      pageNumber: 1,
      pageSize: newSize,
      totalCount: pagination.totalCount,
    });
  };

  if (loading && fondos.length === 0) {
    return <div className="loading">Cargando fondos...</div>;
  }

  return (
    <div className="fondos-list-page">
      <div className="page-header">
        <div>
          <h1>Gestión de Fondos</h1>
          <p>Administre los fondos disponibles para desembolsos</p>
        </div>
        <Button variant="primary" onClick={handleNuevoFondo}>
          ➕ Nuevo Fondo
        </Button>
      </div>

      <div className="fondos-stats">
        <div className="stat-card">
          <div className="stat-label">Total de Fondos</div>
          <div className="stat-value">{pagination.totalCount}</div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Fondos Activos</div>
          <div className="stat-value">
            {fondos.filter((f) => f.fondoEstaActivo).length}
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-label">Saldo Total</div>
          <div className="stat-value">
            {formatCurrency(fondos.reduce((sum, f) => sum + f.fondoSaldoActual, 0))}
          </div>
        </div>
      </div>

      <Table
        columns={columns}
        data={fondos}
        pagination={{
          currentPage: pagination.pageNumber,
          pageSize: pagination.pageSize,
          totalCount: pagination.totalCount,
          onPageChange: handlePageChange,
          onPageSizeChange: handlePageSizeChange,
        }}
        onRowClick={handleEditarFondo}
      />

      {showMovimientoModal && fondoSeleccionado && (
        <MovimientoFondoModal
          isOpen={showMovimientoModal}
          onClose={() => {
            setShowMovimientoModal(false);
            setFondoSeleccionado(null);
          }}
          fondo={fondoSeleccionado}
          onMovimientoCreado={handleMovimientoCreado}
        />
      )}

      {showVerMovimientosModal && fondoSeleccionado && (
        <FondoMovimientosModal
          fondo={fondoSeleccionado}
          onClose={() => {
            setShowVerMovimientosModal(false);
            setFondoSeleccionado(null);
          }}
        />
      )}
    </div>
  );
};

export default FondosList;
