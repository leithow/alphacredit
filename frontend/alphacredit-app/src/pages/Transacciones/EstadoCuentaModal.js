import React, { useState } from 'react';
import './EstadoCuentaModal.css';

const EstadoCuentaModal = ({ estadoCuenta, onCerrar }) => {
  const [filtroEstado, setFiltroEstado] = useState('TODAS'); // TODAS, PENDIENTES, PAGADAS

  // Defensive check - ensure estadoCuenta has the expected structure
  if (!estadoCuenta || !estadoCuenta.cuotas || !Array.isArray(estadoCuenta.cuotas)) {
    return (
      <div className="modal-overlay" onClick={onCerrar}>
        <div className="modal-contenido estado-cuenta-modal" onClick={(e) => e.stopPropagation()}>
          <div className="modal-header">
            <h2>Estado de Cuenta</h2>
            <button className="btn-cerrar-modal" onClick={onCerrar}>✕</button>
          </div>
          <div className="modal-body">
            <div className="sin-resultados-tabla">
              No se pudo cargar el estado de cuenta. Los datos no tienen el formato esperado.
            </div>
          </div>
          <div className="modal-footer">
            <button className="btn-cerrar" onClick={onCerrar}>Cerrar</button>
          </div>
        </div>
      </div>
    );
  }

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

  const determinarEstadoCuota = (cuota) => {
    // Backend returns flat structure, not componentes array
    if (!cuota) {
      return 'PENDIENTE';
    }

    // Use the estado field if available
    if (cuota.estado) {
      return cuota.estado.toUpperCase();
    }

    // Otherwise determine from saldos
    const capitalPagado = cuota.capitalSaldo === 0;
    const interesPagado = cuota.interesSaldo === 0;
    const tieneMora = cuota.moraCalculada > 0;

    if (capitalPagado && interesPagado) return 'PAGADA';
    if (tieneMora || cuota.estaVencida) return 'VENCIDA';
    return 'PENDIENTE';
  };

  const cuotasFiltradas = estadoCuenta.cuotas.filter(cuota => {
    if (filtroEstado === 'TODAS') return true;
    const estado = determinarEstadoCuota(cuota);
    if (filtroEstado === 'PENDIENTES') return estado === 'PENDIENTE' || estado === 'VENCIDA';
    if (filtroEstado === 'PAGADAS') return estado === 'PAGADA';
    return true;
  });

  const calcularTotalesCuota = (cuota) => {
    // Backend returns flat structure with direct properties
    if (!cuota) {
      return {
        capital: 0,
        interes: 0,
        mora: 0,
        saldoCapital: 0,
        saldoInteres: 0
      };
    }

    return {
      capital: cuota.capitalMonto || 0,
      interes: cuota.interesMonto || 0,
      mora: cuota.moraCalculada || 0,
      saldoCapital: cuota.capitalSaldo || 0,
      saldoInteres: cuota.interesSaldo || 0
    };
  };

  const calcularDiasVencidos = (fechaVencimiento) => {
    if (!fechaVencimiento) return 0;
    const hoy = new Date();
    const vencimiento = new Date(fechaVencimiento);
    const diferencia = Math.floor((hoy - vencimiento) / (1000 * 60 * 60 * 24));
    return diferencia > 0 ? diferencia : 0;
  };

  return (
    <div className="modal-overlay" onClick={onCerrar}>
      <div className="modal-contenido estado-cuenta-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Estado de Cuenta - Préstamo {estadoCuenta.prestamoNumero}</h2>
          <button className="btn-cerrar-modal" onClick={onCerrar}>
            ✕
          </button>
        </div>

        <div className="modal-body">
          {/* Resumen General */}
          <div className="resumen-general">
            <div className="resumen-card">
              <div className="resumen-item">
                <label>Cliente:</label>
                <span>{estadoCuenta.nombreCliente}</span>
              </div>
              <div className="resumen-item">
                <label>Monto Original:</label>
                <span className="valor-destacado">{formatearMoneda(estadoCuenta.montoOriginal)}</span>
              </div>
              <div className="resumen-item">
                <label>Fecha Desembolso:</label>
                <span>{formatearFecha(estadoCuenta.fechaDesembolso)}</span>
              </div>
              <div className="resumen-item">
                <label>Fecha Vencimiento:</label>
                <span>{formatearFecha(estadoCuenta.fechaVencimiento)}</span>
              </div>
            </div>

            <div className="resumen-saldos">
              <h3>Saldos Actuales</h3>
              <div className="saldos-grid">
                <div className="saldo-box capital">
                  <label>Capital</label>
                  <span className="saldo-monto">{formatearMoneda(estadoCuenta.saldoCapital)}</span>
                </div>
                <div className="saldo-box interes">
                  <label>Interés</label>
                  <span className="saldo-monto">{formatearMoneda(estadoCuenta.saldoInteres)}</span>
                </div>
                <div className="saldo-box mora">
                  <label>Mora</label>
                  <span className="saldo-monto">{formatearMoneda(estadoCuenta.saldoMora)}</span>
                </div>
                <div className="saldo-box total">
                  <label>Total</label>
                  <span className="saldo-monto-total">{formatearMoneda(estadoCuenta.saldoTotal)}</span>
                </div>
              </div>
            </div>

            <div className="resumen-cuotas">
              <div className="cuota-stat">
                <label>Cuotas Pagadas:</label>
                <span className="stat-valor pagadas">{estadoCuenta.resumen.cuotasPagadas}</span>
              </div>
              <div className="cuota-stat">
                <label>Cuotas Pendientes:</label>
                <span className="stat-valor pendientes">{estadoCuenta.resumen.cuotasPendientes}</span>
              </div>
              <div className="cuota-stat">
                <label>Total Cuotas:</label>
                <span className="stat-valor total">{estadoCuenta.resumen.totalCuotas}</span>
              </div>
            </div>
          </div>

          {/* Filtros */}
          <div className="cuotas-filtros">
            <button
              className={`filtro-btn ${filtroEstado === 'TODAS' ? 'activo' : ''}`}
              onClick={() => setFiltroEstado('TODAS')}
            >
              Todas ({estadoCuenta.cuotas.length})
            </button>
            <button
              className={`filtro-btn ${filtroEstado === 'PENDIENTES' ? 'activo' : ''}`}
              onClick={() => setFiltroEstado('PENDIENTES')}
            >
              Pendientes ({estadoCuenta.cuotas.filter(c => determinarEstadoCuota(c) !== 'PAGADA').length})
            </button>
            <button
              className={`filtro-btn ${filtroEstado === 'PAGADAS' ? 'activo' : ''}`}
              onClick={() => setFiltroEstado('PAGADAS')}
            >
              Pagadas ({estadoCuenta.cuotas.filter(c => determinarEstadoCuota(c) === 'PAGADA').length})
            </button>
          </div>

          {/* Tabla de Cuotas */}
          <div className="cuotas-tabla-container">
            <table className="cuotas-tabla">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Fecha Venc.</th>
                  <th>Capital</th>
                  <th>Interés</th>
                  <th>Mora</th>
                  <th>Total Cuota</th>
                  <th>Saldo</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {cuotasFiltradas.map((cuota) => {
                  const totales = calcularTotalesCuota(cuota);
                  const estado = determinarEstadoCuota(cuota);
                  const diasVencidos = estado === 'VENCIDA' ? calcularDiasVencidos(cuota.fechaVencimiento) : 0;
                  const totalCuota = totales.capital + totales.interes;
                  const saldoCuota = totales.saldoCapital + totales.saldoInteres + totales.mora;

                  return (
                    <tr key={cuota.numeroCuota} className={`cuota-row estado-${estado.toLowerCase()}`}>
                      <td className="cuota-numero">{cuota.numeroCuota}</td>
                      <td className="cuota-fecha">
                        {formatearFecha(cuota.fechaVencimiento)}
                        {diasVencidos > 0 && (
                          <span className="dias-vencidos">
                            ({diasVencidos} días)
                          </span>
                        )}
                      </td>
                      <td className="monto-componente">
                        {formatearMoneda(totales.capital)}
                        {totales.saldoCapital > 0 && totales.saldoCapital < totales.capital && (
                          <small className="saldo-parcial">
                            Saldo: {formatearMoneda(totales.saldoCapital)}
                          </small>
                        )}
                      </td>
                      <td className="monto-componente">
                        {formatearMoneda(totales.interes)}
                        {totales.saldoInteres > 0 && totales.saldoInteres < totales.interes && (
                          <small className="saldo-parcial">
                            Saldo: {formatearMoneda(totales.saldoInteres)}
                          </small>
                        )}
                      </td>
                      <td className="monto-mora">
                        {totales.mora > 0 ? formatearMoneda(totales.mora) : '-'}
                      </td>
                      <td className="monto-total-cuota">
                        {formatearMoneda(totalCuota)}
                      </td>
                      <td className="monto-saldo">
                        {formatearMoneda(saldoCuota)}
                      </td>
                      <td className={`cuota-estado estado-${estado.toLowerCase()}`}>
                        <span className="badge-estado">{estado}</span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>

            {cuotasFiltradas.length === 0 && (
              <div className="sin-resultados-tabla">
                No hay cuotas que mostrar con el filtro seleccionado.
              </div>
            )}
          </div>

          {/* Detalles de Cuotas con Mora */}
          {estadoCuenta.resumen.moraTotal > 0 && (
            <div className="detalles-mora-section">
              <h3>Detalle de Cuotas con Mora</h3>
              <div className="mora-tabla-container">
                <table className="mora-tabla">
                  <thead>
                    <tr>
                      <th>Cuota</th>
                      <th>Fecha Vencimiento</th>
                      <th>Días Vencidos</th>
                      <th>Mora Calculada</th>
                      <th>Mora Pagada</th>
                      <th>Saldo Mora</th>
                    </tr>
                  </thead>
                  <tbody>
                    {estadoCuenta.cuotas
                      .filter(cuota => cuota.moraCalculada > 0)
                      .map(cuota => {
                        const saldoMora = cuota.moraCalculada - (cuota.moraPagada || 0);
                        return (
                          <tr key={cuota.numeroCuota}>
                            <td>{cuota.numeroCuota}</td>
                            <td>{formatearFecha(cuota.fechaVencimiento)}</td>
                            <td>{cuota.diasVencidos || calcularDiasVencidos(cuota.fechaVencimiento)}</td>
                            <td>{formatearMoneda(cuota.moraCalculada)}</td>
                            <td>{formatearMoneda(cuota.moraPagada || 0)}</td>
                            <td>{formatearMoneda(saldoMora)}</td>
                          </tr>
                        );
                      })}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button className="btn-cerrar" onClick={onCerrar}>
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};

export default EstadoCuentaModal;
