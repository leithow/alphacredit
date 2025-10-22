import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import prestamoService from '../../services/prestamoService';
import Table from '../../components/common/Table';
import Button from '../../components/common/Button';
import { formatCurrency } from '../../utils/currency';
import './PrestamosList.css';

const PrestamosList = () => {
  const navigate = useNavigate();
  const [prestamos, setPrestamos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
  });
  const [filters, setFilters] = useState({
    search: '',
    estadoPrestamoId: '',
  });

  useEffect(() => {
    loadPrestamos();
  }, [pagination.pageNumber, pagination.pageSize]);

  const loadPrestamos = async () => {
    try {
      setLoading(true);
      const response = await prestamoService.getPrestamos(
        pagination.pageNumber,
        pagination.pageSize,
        filters
      );
      // Asegurar que siempre sea un array
      setPrestamos(Array.isArray(response.data) ? response.data : []);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.totalCount || 0,
      }));
    } catch (error) {
      console.error('Error al cargar préstamos:', error);
      setPrestamos([]); // Resetear a array vacío en caso de error
      alert('Error al cargar los préstamos');
    } finally {
      setLoading(false);
    }
  };

  const handleNuevoPrestamo = () => {
    navigate('/prestamos/nuevo');
  };

  const handleEditarPrestamo = (prestamo) => {
    navigate(`/prestamos/${prestamo.prestamoId}`);
  };

  const handleVerGarantias = (prestamo) => {
    navigate(`/prestamos/${prestamo.prestamoId}/garantias`);
  };

  const handleSearch = (e) => {
    e.preventDefault();
    loadPrestamos();
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    });
  };

  const columns = [
    {
      header: 'No. Préstamo',
      field: 'prestamoNumero',
      width: '120px',
    },
    {
      header: 'Cliente',
      render: (row) => row.persona?.personaNombreCompleto || '-',
    },
    {
      header: 'Monto',
      render: (row) => formatCurrency(row.prestamoMonto),
      width: '120px',
    },
    {
      header: 'Saldo Capital',
      render: (row) => (
        <span className={row.prestamosaldocapital > 0 ? 'saldo-pendiente' : 'saldo-pagado'}>
          {formatCurrency(row.prestamosaldocapital || 0)}
        </span>
      ),
      width: '120px',
    },
    {
      header: 'Tasa %',
      render: (row) => `${row.prestamoTasaInteres}%`,
      width: '80px',
    },
    {
      header: 'Plazo',
      render: (row) => `${row.prestamoPlazo} ${row.frecuenciaPago?.frecuenciaPagoNombre || ''}`,
      width: '120px',
    },
    {
      header: 'Fecha Desembolso',
      render: (row) => formatDate(row.prestamoFechaDesembolso),
      width: '130px',
    },
    {
      header: 'Estado',
      render: (row) => (
        <span className={`estado-badge estado-${row.estadoPrestamo?.estadoPrestamoNombre?.toLowerCase()}`}>
          {row.estadoPrestamo?.estadoPrestamoNombre || '-'}
        </span>
      ),
      width: '100px',
    },
    {
      header: 'Acciones',
      width: '280px',
      render: (row) => (
        <div className="action-buttons">
          <Button
            variant="info"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleVerGarantias(row);
            }}
          >
            🛡️ Garantías
          </Button>
          <Button
            variant="secondary"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleEditarPrestamo(row);
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
    setPagination((prev) => ({
      ...prev,
      pageSize: newSize,
      pageNumber: 1,
    }));
  };

  // Calcular estadísticas - asegurar que prestamos sea un array
  const prestamosArray = Array.isArray(prestamos) ? prestamos : [];
  const totalMontoPrestado = prestamosArray.reduce((sum, p) => sum + (p.prestamoMonto || 0), 0);
  const totalSaldoCapital = prestamosArray.reduce((sum, p) => sum + (p.prestamosaldocapital || 0), 0);
  const prestamosActivos = prestamosArray.filter(p => (p.prestamosaldocapital || 0) > 0).length;

  return (
    <div className="prestamos-list-container">
      <div className="page-header">
        <h1>Gestión de Préstamos</h1>
        <Button variant="primary" onClick={handleNuevoPrestamo}>
          + Nuevo Préstamo
        </Button>
      </div>

      <div className="stats-cards">
        <div className="stat-card stat-card-blue">
          <div className="stat-icon">📋</div>
          <div className="stat-content">
            <div className="stat-label">Total Préstamos</div>
            <div className="stat-value">{pagination.totalCount}</div>
          </div>
        </div>

        <div className="stat-card stat-card-green">
          <div className="stat-icon">💰</div>
          <div className="stat-content">
            <div className="stat-label">Monto Desembolsado</div>
            <div className="stat-value">{formatCurrency(totalMontoPrestado)}</div>
          </div>
        </div>

        <div className="stat-card stat-card-orange">
          <div className="stat-icon">📊</div>
          <div className="stat-content">
            <div className="stat-label">Saldo Total</div>
            <div className="stat-value">{formatCurrency(totalSaldoCapital)}</div>
          </div>
        </div>

        <div className="stat-card stat-card-purple">
          <div className="stat-icon">✅</div>
          <div className="stat-content">
            <div className="stat-label">Préstamos Activos</div>
            <div className="stat-value">{prestamosActivos}</div>
          </div>
        </div>
      </div>

      <div className="filters-section">
        <form onSubmit={handleSearch} className="search-form">
          <input
            type="text"
            placeholder="Buscar por número de préstamo o cliente..."
            value={filters.search}
            onChange={(e) => setFilters({ ...filters, search: e.target.value })}
            className="search-input"
          />
          <Button type="submit" variant="primary">
            Buscar
          </Button>
        </form>
      </div>

      <Table
        columns={columns}
        data={prestamos}
        loading={loading}
        pagination={{
          currentPage: pagination.pageNumber,
          pageSize: pagination.pageSize,
          totalCount: pagination.totalCount,
          onPageChange: handlePageChange,
          onPageSizeChange: handlePageSizeChange,
        }}
        onRowClick={handleEditarPrestamo}
      />
    </div>
  );
};

export default PrestamosList;
