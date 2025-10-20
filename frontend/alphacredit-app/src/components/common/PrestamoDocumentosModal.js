import React, { useState, useEffect } from 'react';
import prestamoDocumentoService from '../../services/prestamoDocumentoService';
import Button from './Button';
import './PrestamoDocumentosModal.css';

const PrestamoDocumentosModal = ({ isOpen, onClose, prestamo }) => {
  const [loading, setLoading] = useState(false);
  const [estadisticas, setEstadisticas] = useState(null);
  const [historialVisible, setHistorialVisible] = useState(null);
  const [historialData, setHistorialData] = useState([]);

  useEffect(() => {
    if (isOpen && prestamo) {
      cargarEstadisticas();
    }
  }, [isOpen, prestamo]);

  const cargarEstadisticas = async () => {
    try {
      setLoading(true);
      const data = await prestamoDocumentoService.obtenerEstadisticas(prestamo.prestamoId);
      setEstadisticas(data);
    } catch (error) {
      console.error('Error al cargar estadísticas:', error);
    } finally {
      setLoading(false);
    }
  };

  const generarYMostrarDocumento = async (tipo) => {
    try {
      setLoading(true);
      let htmlContent;

      switch (tipo) {
        case 'CONTRATO':
          htmlContent = await prestamoDocumentoService.generarContrato(prestamo.prestamoId, 'user');
          break;
        case 'PAGARE':
          htmlContent = await prestamoDocumentoService.generarPagare(prestamo.prestamoId, 'user');
          break;
        case 'PLAN_PAGOS':
          htmlContent = await prestamoDocumentoService.generarPlanPagos(prestamo.prestamoId, 'user');
          break;
        default:
          throw new Error('Tipo de documento no válido');
      }

      prestamoDocumentoService.imprimirDocumento(htmlContent, tipo);

      // Recargar estadísticas
      await cargarEstadisticas();
    } catch (error) {
      console.error('Error al generar documento:', error);
      alert('Error al generar el documento: ' + (error.response?.data?.message || error.message));
    } finally {
      setLoading(false);
    }
  };

  const verHistorial = async (tipo) => {
    if (historialVisible === tipo) {
      setHistorialVisible(null);
      setHistorialData([]);
      return;
    }

    try {
      setLoading(true);
      const data = await prestamoDocumentoService.obtenerHistorialImpresiones(
        prestamo.prestamoId,
        tipo
      );
      setHistorialData(data.impresiones || []);
      setHistorialVisible(tipo);
    } catch (error) {
      console.error('Error al cargar historial:', error);
      alert('Error al cargar el historial');
    } finally {
      setLoading(false);
    }
  };

  const getDocumentoStats = (tipo) => {
    if (!estadisticas || !estadisticas.documentos) return null;
    return estadisticas.documentos.find((d) => d.tipo === tipo);
  };

  const formatearFecha = (fecha) => {
    if (!fecha) return '-';
    return new Date(fecha).toLocaleString('es-HN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (!isOpen) return null;

  const documentos = [
    {
      tipo: 'CONTRATO',
      nombre: 'Contrato de Préstamo',
      icono: '📄',
      descripcion: 'Contrato legal del préstamo con todas las cláusulas',
    },
    {
      tipo: 'PAGARE',
      nombre: 'Pagaré',
      icono: '📝',
      descripcion: 'Documento de compromiso de pago',
    },
    {
      tipo: 'PLAN_PAGOS',
      nombre: 'Plan de Pagos',
      icono: '📊',
      descripcion: 'Tabla de amortización del préstamo',
    },
  ];

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content documentos-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>📋 Documentos del Préstamo</h2>
          <button className="modal-close" onClick={onClose}>
            ×
          </button>
        </div>

        <div className="modal-body">
          <div className="prestamo-info-header">
            <div className="info-item">
              <strong>Préstamo:</strong> {prestamo.prestamoNumero}
            </div>
            <div className="info-item">
              <strong>Cliente:</strong> {prestamo.persona?.personaNombreCompleto}
            </div>
            <div className="info-item">
              <strong>Monto:</strong> L {prestamo.prestamoMonto?.toLocaleString('es-HN', { minimumFractionDigits: 2 })}
            </div>
          </div>

          {loading && !estadisticas ? (
            <div className="loading-container">
              <p>Cargando documentos...</p>
            </div>
          ) : (
            <div className="documentos-grid">
              {documentos.map((doc) => {
                const stats = getDocumentoStats(doc.tipo);
                return (
                  <div key={doc.tipo} className="documento-card">
                    <div className="documento-header">
                      <span className="documento-icono">{doc.icono}</span>
                      <div className="documento-info">
                        <h3>{doc.nombre}</h3>
                        <p className="documento-descripcion">{doc.descripcion}</p>
                      </div>
                    </div>

                    <div className="documento-stats">
                      {stats ? (
                        <>
                          <div className="stat-item">
                            <span className="stat-label">Veces impreso:</span>
                            <span className="stat-value">{stats.vecesImpreso || 0}</span>
                          </div>
                          {stats.primeraImpresion && (
                            <div className="stat-item">
                              <span className="stat-label">Primera impresión:</span>
                              <span className="stat-value">
                                {formatearFecha(stats.primeraImpresion)}
                              </span>
                            </div>
                          )}
                          {stats.ultimaImpresion && (
                            <div className="stat-item">
                              <span className="stat-label">Última impresión:</span>
                              <span className="stat-value">
                                {formatearFecha(stats.ultimaImpresion)}
                              </span>
                            </div>
                          )}
                        </>
                      ) : (
                        <div className="stat-item no-impresiones">
                          <span>Sin impresiones</span>
                        </div>
                      )}
                    </div>

                    <div className="documento-actions">
                      <Button
                        variant="primary"
                        size="small"
                        onClick={() => generarYMostrarDocumento(doc.tipo)}
                        disabled={loading}
                      >
                        🖨️ Generar e Imprimir
                      </Button>
                      {stats && stats.vecesImpreso > 0 && (
                        <Button
                          variant="outline"
                          size="small"
                          onClick={() => verHistorial(doc.tipo)}
                          disabled={loading}
                        >
                          📜 {historialVisible === doc.tipo ? 'Ocultar' : 'Ver'} Historial
                        </Button>
                      )}
                    </div>

                    {historialVisible === doc.tipo && (
                      <div className="historial-impresiones">
                        <h4>Historial de Impresiones</h4>
                        {historialData.length === 0 ? (
                          <p className="no-data">No hay impresiones registradas</p>
                        ) : (
                          <table className="historial-table">
                            <thead>
                              <tr>
                                <th>Fecha</th>
                                <th>Usuario</th>
                                <th>IP</th>
                              </tr>
                            </thead>
                            <tbody>
                              {historialData.map((imp, index) => (
                                <tr key={index}>
                                  <td>{formatearFecha(imp.fecha)}</td>
                                  <td>{imp.usuario || '-'}</td>
                                  <td>{imp.ip || '-'}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        )}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="modal-footer">
          <div className="footer-note">
            <small>
              ℹ️ Los documentos se registran automáticamente cada vez que se generan o imprimen
            </small>
          </div>
          <Button variant="secondary" onClick={onClose}>
            Cerrar
          </Button>
        </div>
      </div>
    </div>
  );
};

export default PrestamoDocumentosModal;
