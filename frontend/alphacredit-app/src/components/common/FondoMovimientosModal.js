import React, { useState, useEffect } from 'react';
import axiosInstance from '../../api/axiosConfig';
import '../../styles/Modal.css';

const FondoMovimientosModal = ({ fondo, onClose }) => {
  const [movimientos, setMovimientos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchMovimientos();
  }, [fondo.fondoId]);

  const fetchMovimientos = async () => {
    try {
      setLoading(true);
      const response = await axiosInstance.get(`/fondos/${fondo.fondoId}`);
      setMovimientos(response.data.fondoMovimientos || []);
      setError(null);
    } catch (err) {
      console.error('Error al cargar movimientos:', err);
      setError('Error al cargar los movimientos del fondo');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-NI', {
      style: 'currency',
      currency: 'NIO',
      minimumFractionDigits: 2
    }).format(amount);
  };

  const getTipoMovimientoClass = (tipo) => {
    return tipo === 'INGRESO' ? 'tipo-ingreso' : 'tipo-egreso';
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Movimientos del Fondo: {fondo.fondoNombre}</h2>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>

        <div className="modal-body">
          <div className="fondo-info-card">
            <div className="info-row">
              <div className="info-item">
                <label>Tipo:</label>
                <span>{fondo.tipoFondo?.tipoFondoNombre || '-'}</span>
              </div>
              <div className="info-item">
                <label>Moneda:</label>
                <span>{fondo.moneda?.monedaNombre || '-'} ({fondo.moneda?.monedaCodigo || '-'})</span>
              </div>
            </div>
            <div className="info-row">
              <div className="info-item">
                <label>Saldo Inicial:</label>
                <span className="amount">{formatCurrency(fondo.fondoSaldoInicial)}</span>
              </div>
              <div className="info-item">
                <label>Saldo Actual:</label>
                <span className={`amount ${fondo.fondoSaldoActual < 0 ? 'negative' : 'positive'}`}>
                  {formatCurrency(fondo.fondoSaldoActual)}
                </span>
              </div>
            </div>
          </div>

          {loading ? (
            <div className="loading-container">
              <p>Cargando movimientos...</p>
            </div>
          ) : error ? (
            <div className="error-container">
              <p className="error-message">{error}</p>
            </div>
          ) : movimientos.length === 0 ? (
            <div className="empty-state">
              <p>No hay movimientos registrados para este fondo</p>
            </div>
          ) : (
            <div className="movimientos-list">
              <h3>Historial de Movimientos ({movimientos.length})</h3>
              <div className="table-container">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th style={{ width: '80px' }}>ID</th>
                      <th style={{ width: '120px' }}>Tipo</th>
                      <th style={{ width: '150px' }}>Monto</th>
                      <th>Concepto</th>
                      <th style={{ width: '180px' }}>Fecha</th>
                      <th style={{ width: '120px' }}>Usuario</th>
                    </tr>
                  </thead>
                  <tbody>
                    {movimientos
                      .sort((a, b) => new Date(b.fondoMovimientoFecha) - new Date(a.fondoMovimientoFecha))
                      .map((mov) => (
                        <tr key={mov.fondoMovimientoId}>
                          <td>{mov.fondoMovimientoId}</td>
                          <td>
                            <span className={`tipo-badge ${getTipoMovimientoClass(mov.fondoMovimientoTipo)}`}>
                              {mov.fondoMovimientoTipo}
                            </span>
                          </td>
                          <td className={`amount ${mov.fondoMovimientoTipo === 'INGRESO' ? 'positive' : 'negative'}`}>
                            {mov.fondoMovimientoTipo === 'INGRESO' ? '+' : '-'}
                            {formatCurrency(Math.abs(mov.fondoMovimientoMonto))}
                          </td>
                          <td className="concepto-cell">
                            {mov.fondoMovimientoConcepto || '-'}
                          </td>
                          <td>{formatDate(mov.fondoMovimientoFecha)}</td>
                          <td>{mov.fondoMovimientoUserCrea || '-'}</td>
                        </tr>
                      ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button className="btn-secondary" onClick={onClose}>
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};

export default FondoMovimientosModal;
