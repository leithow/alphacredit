import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import transaccionService from '../../services/transaccionService';
import TransaccionAbonoForm from './TransaccionAbonoForm';
import './TransaccionesPage.css';

const TransaccionesPage = () => {
  const navigate = useNavigate();
  const [tipoTransaccion, setTipoTransaccion] = useState('ABONO_PRESTAMO');
  const [busquedaPrestamo, setBusquedaPrestamo] = useState('');
  const [prestamosEncontrados, setPrestamosEncontrados] = useState([]);
  const [prestamoSeleccionado, setPrestamoSeleccionado] = useState(null);
  const [cargandoBusqueda, setCargandoBusqueda] = useState(false);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);

  const tiposTransaccion = [
    { valor: 'ABONO_PRESTAMO', etiqueta: 'Abono a Préstamo', icono: '💵' },
    // Futuras transacciones:
    // { valor: 'RETIRO_FONDO', etiqueta: 'Retiro de Fondo', icono: '💸' },
    // { valor: 'TRANSFERENCIA_FONDO', etiqueta: 'Transferencia entre Fondos', icono: '🔄' },
  ];

  // Buscar préstamos cuando el usuario escribe
  useEffect(() => {
    const buscarPrestamosDebounced = setTimeout(async () => {
      if (busquedaPrestamo.trim().length >= 3) {
        setCargandoBusqueda(true);
        try {
          const prestamos = await transaccionService.buscarPrestamos(busquedaPrestamo);
          setPrestamosEncontrados(prestamos);
        } catch (error) {
          console.error('Error al buscar préstamos:', error);
          setPrestamosEncontrados([]);
        } finally {
          setCargandoBusqueda(false);
        }
      } else {
        setPrestamosEncontrados([]);
      }
    }, 500); // Esperar 500ms después de que el usuario deje de escribir

    return () => clearTimeout(buscarPrestamosDebounced);
  }, [busquedaPrestamo]);

  const handleSeleccionarPrestamo = (prestamo) => {
    setPrestamoSeleccionado(prestamo);
    setPrestamosEncontrados([]);
    setBusquedaPrestamo(`${prestamo.prestamoNumero} - ${prestamo.nombreCliente}`);
    setMostrarFormulario(true);
  };

  const handleCancelar = () => {
    setMostrarFormulario(false);
    setPrestamoSeleccionado(null);
    setBusquedaPrestamo('');
  };

  const handleTransaccionCompletada = () => {
    // Limpiar formulario después de completar transacción
    setMostrarFormulario(false);
    setPrestamoSeleccionado(null);
    setBusquedaPrestamo('');
    setPrestamosEncontrados([]);
  };

  const formatearMoneda = (monto) => {
    return new Intl.NumberFormat('es-HN', {
      style: 'currency',
      currency: 'HNL'
    }).format(monto);
  };

  const formatearFecha = (fecha) => {
    if (!fecha) return '-';
    return new Date(fecha).toLocaleDateString('es-HN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  };

  return (
    <div className="transacciones-page">
      <div className="transacciones-header">
        <h1>Transacciones</h1>
        <button className="btn-volver" onClick={() => navigate('/dashboard')}>
          Volver al Dashboard
        </button>
      </div>

      {!mostrarFormulario ? (
        <div className="transacciones-seleccion">
          {/* Selección de Tipo de Transacción */}
          <div className="tipo-transaccion-section">
            <h2>Seleccione el tipo de transacción</h2>
            <div className="tipos-transaccion-grid">
              {tiposTransaccion.map((tipo) => (
                <div
                  key={tipo.valor}
                  className={`tipo-transaccion-card ${
                    tipoTransaccion === tipo.valor ? 'activo' : ''
                  }`}
                  onClick={() => setTipoTransaccion(tipo.valor)}
                >
                  <div className="tipo-icono">{tipo.icono}</div>
                  <div className="tipo-etiqueta">{tipo.etiqueta}</div>
                </div>
              ))}
            </div>
          </div>

          {/* Búsqueda de Préstamo (solo para ABONO_PRESTAMO) */}
          {tipoTransaccion === 'ABONO_PRESTAMO' && (
            <div className="busqueda-prestamo-section">
              <h2>Buscar préstamo</h2>
              <div className="busqueda-input-container">
                <input
                  type="text"
                  className="busqueda-input"
                  placeholder="Buscar por número de préstamo o nombre de cliente..."
                  value={busquedaPrestamo}
                  onChange={(e) => setBusquedaPrestamo(e.target.value)}
                  autoFocus
                />
                {cargandoBusqueda && <div className="spinner-pequeno"></div>}
              </div>

              {/* Resultados de búsqueda */}
              {prestamosEncontrados.length > 0 && (
                <div className="resultados-busqueda">
                  <h3>Préstamos encontrados ({prestamosEncontrados.length})</h3>
                  <div className="prestamos-lista">
                    {prestamosEncontrados.map((prestamo) => (
                      <div
                        key={prestamo.prestamoId}
                        className="prestamo-item"
                        onClick={() => handleSeleccionarPrestamo(prestamo)}
                      >
                        <div className="prestamo-info-principal">
                          <div className="prestamo-numero">
                            {prestamo.prestamoNumero}
                          </div>
                          <div className="prestamo-cliente">
                            {prestamo.nombreCliente}
                          </div>
                          <div className={`prestamo-estado estado-${prestamo.estadoPrestamoId}`}>
                            {prestamo.estadoPrestamoNombre}
                          </div>
                        </div>
                        <div className="prestamo-info-secundaria">
                          <div className="info-item">
                            <span className="label">Monto:</span>
                            <span className="valor">{formatearMoneda(prestamo.prestamoMonto)}</span>
                          </div>
                          <div className="info-item">
                            <span className="label">Saldo:</span>
                            <span className="valor saldo-pendiente">
                              {formatearMoneda(
                                prestamo.prestamoSaldoCapital +
                                prestamo.prestamoSaldoInteres +
                                prestamo.prestamoSaldoMora
                              )}
                            </span>
                          </div>
                          <div className="info-item">
                            <span className="label">Desembolso:</span>
                            <span className="valor">{formatearFecha(prestamo.prestamoFechaDesembolso)}</span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {busquedaPrestamo.trim().length >= 3 &&
                !cargandoBusqueda &&
                prestamosEncontrados.length === 0 && (
                  <div className="sin-resultados">
                    No se encontraron préstamos que coincidan con la búsqueda.
                  </div>
                )}
            </div>
          )}
        </div>
      ) : (
        /* Formulario de Transacción */
        <div className="transacciones-formulario">
          {tipoTransaccion === 'ABONO_PRESTAMO' && (
            <TransaccionAbonoForm
              prestamo={prestamoSeleccionado}
              onCancelar={handleCancelar}
              onCompletado={handleTransaccionCompletada}
            />
          )}
        </div>
      )}
    </div>
  );
};

export default TransaccionesPage;
