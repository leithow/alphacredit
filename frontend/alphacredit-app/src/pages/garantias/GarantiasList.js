import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import garantiaService from '../../services/garantiaService';
import prestamoService from '../../services/prestamoService';
import Table from '../../components/common/Table';
import Button from '../../components/common/Button';
import { formatCurrency } from '../../utils/currency';
import './GarantiasList.css';

const GarantiasList = () => {
  const { prestamoId } = useParams();
  const navigate = useNavigate();
  const [garantias, setGarantias] = useState([]);
  const [prestamo, setPrestamo] = useState(null);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
  });

  useEffect(() => {
    if (prestamoId) {
      loadPrestamo();
    }
    loadGarantias();
  }, [prestamoId, pagination.pageNumber, pagination.pageSize]);

  const loadPrestamo = async () => {
    try {
      const data = await prestamoService.getPrestamoById(prestamoId);
      setPrestamo(data);
    } catch (error) {
      console.error('Error al cargar préstamo:', error);
      alert('Error al cargar el préstamo');
    }
  };

  const loadGarantias = async () => {
    try {
      setLoading(true);
      let response;

      if (prestamoId) {
        // Cargar garantías de un préstamo específico
        response = await garantiaService.getGarantiasByPrestamo(
          prestamoId,
          pagination.pageNumber,
          pagination.pageSize
        );
      } else {
        // Cargar todas las garantías
        response = await garantiaService.getGarantias(
          pagination.pageNumber,
          pagination.pageSize
        );
      }

      setGarantias(response.data);
      setPagination((prev) => ({
        ...prev,
        totalCount: response.totalCount,
      }));
    } catch (error) {
      console.error('Error al cargar garantías:', error);
      alert('Error al cargar las garantías');
    } finally {
      setLoading(false);
    }
  };

  const handleNuevaGarantia = () => {
    if (prestamoId) {
      navigate(`/prestamos/${prestamoId}/garantias/nueva`);
    } else {
      navigate('/garantias/nuevo');
    }
  };

  const handleEditarGarantia = (garantia) => {
    if (prestamoId) {
      navigate(`/prestamos/${prestamoId}/garantias/${garantia.garantiaId}`);
    } else {
      navigate(`/garantias/${garantia.garantiaId}`);
    }
  };

  const handleEliminarGarantia = async (garantia) => {
    if (window.confirm(`¿Está seguro de eliminar la garantía: ${garantia.garantiaDescripcion}?`)) {
      try {
        await garantiaService.deleteGarantia(garantia.garantiaId);
        alert('Garantía eliminada exitosamente');
        loadGarantias();
      } catch (error) {
        console.error('Error al eliminar garantía:', error);
        alert(error.response?.data?.message || 'Error al eliminar la garantía');
      }
    }
  };

  const handleVolver = () => {
    if (prestamoId) {
      navigate('/prestamos');
    } else {
      navigate('/');
    }
  };

  const getTipoGarantiaLabel = (tipo) => {
    const tipos = {
      PRENDARIA: 'Prendaria',
      HIPOTECARIA: 'Hipotecaria',
      FIDUCIARIA: 'Fiduciaria',
    };
    return tipos[tipo] || tipo;
  };

  const getTipoGarantiaClass = (tipo) => {
    return `tipo-garantia-${tipo?.toLowerCase()}`;
  };

  const columns = [
    {
      header: 'ID',
      field: 'garantiaId',
      width: '80px',
    },
    {
      header: 'Tipo',
      render: (row) => (
        <span className={`tipo-badge ${getTipoGarantiaClass(row.tipoGarantia?.tipoGarantiaNombre)}`}>
          {getTipoGarantiaLabel(row.tipoGarantia?.tipoGarantiaNombre)}
        </span>
      ),
      width: '120px',
    },
    {
      header: 'Descripción',
      field: 'garantiaDescripcion',
    },
    {
      header: 'Valor Estimado',
      render: (row) => formatCurrency(row.garantiaValorEstimado || 0),
      width: '140px',
    },
    {
      header: 'Documento',
      field: 'garantiaDocumento',
      width: '150px',
    },
    {
      header: 'Estado',
      render: (row) => (
        <span className={`estado-badge ${row.garantiaEstaActiva ? 'estado-activo' : 'estado-inactivo'}`}>
          {row.garantiaEstaActiva ? 'Activa' : 'Inactiva'}
        </span>
      ),
      width: '100px',
    },
    {
      header: 'Acciones',
      width: '200px',
      render: (row) => (
        <div className="action-buttons">
          <Button
            variant="secondary"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleEditarGarantia(row);
            }}
          >
            Editar
          </Button>
          <Button
            variant="danger"
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              handleEliminarGarantia(row);
            }}
          >
            Eliminar
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

  const totalValorGarantias = (garantias || []).reduce((sum, g) => sum + (g.garantiaValorEstimado || 0), 0);
  const garantiasPrendarias = (garantias || []).filter(g => g.tipoGarantia?.tipoGarantiaNombre === 'PRENDARIA').length;
  const garantiasHipotecarias = (garantias || []).filter(g => g.tipoGarantia?.tipoGarantiaNombre === 'HIPOTECARIA').length;
  const garantiasFiduciarias = (garantias || []).filter(g => g.tipoGarantia?.tipoGarantiaNombre === 'FIDUCIARIA').length;

  return (
    <div className="garantias-list-container">
      <div className="page-header">
        <div>
          {prestamoId && (
            <Button variant="outline" onClick={handleVolver} className="back-button">
              ← Volver a Préstamos
            </Button>
          )}
          <h1>{prestamoId ? 'Garantías del Préstamo' : 'Gestión de Garantías'}</h1>
          {prestamo && (
            <div className="prestamo-info">
              <p><strong>No. Préstamo:</strong> {prestamo.prestamoNumero}</p>
              <p><strong>Cliente:</strong> {prestamo.persona?.personaNombreCompleto}</p>
              <p><strong>Monto:</strong> {formatCurrency(prestamo.prestamoMonto)}</p>
            </div>
          )}
        </div>
        <Button variant="primary" onClick={handleNuevaGarantia}>
          + Nueva Garantía
        </Button>
      </div>

      <div className="stats-cards">
        <div className="stat-card stat-card-blue">
          <div className="stat-icon">🛡️</div>
          <div className="stat-content">
            <div className="stat-label">Total Garantías</div>
            <div className="stat-value">{pagination.totalCount}</div>
          </div>
        </div>

        <div className="stat-card stat-card-green">
          <div className="stat-icon">💎</div>
          <div className="stat-content">
            <div className="stat-label">Valor Total</div>
            <div className="stat-value">{formatCurrency(totalValorGarantias)}</div>
          </div>
        </div>

        <div className="stat-card stat-card-orange">
          <div className="stat-icon">📦</div>
          <div className="stat-content">
            <div className="stat-label">Prendarias</div>
            <div className="stat-value">{garantiasPrendarias}</div>
          </div>
        </div>

        <div className="stat-card stat-card-purple">
          <div className="stat-icon">🏠</div>
          <div className="stat-content">
            <div className="stat-label">Hipotecarias</div>
            <div className="stat-value">{garantiasHipotecarias}</div>
          </div>
        </div>

        <div className="stat-card stat-card-teal">
          <div className="stat-icon">🤝</div>
          <div className="stat-content">
            <div className="stat-label">Fiduciarias</div>
            <div className="stat-value">{garantiasFiduciarias}</div>
          </div>
        </div>
      </div>

      <Table
        columns={columns}
        data={garantias}
        loading={loading}
        pagination={{
          currentPage: pagination.pageNumber,
          pageSize: pagination.pageSize,
          totalCount: pagination.totalCount,
          onPageChange: handlePageChange,
          onPageSizeChange: handlePageSizeChange,
        }}
        onRowClick={handleEditarGarantia}
      />
    </div>
  );
};

export default GarantiasList;
