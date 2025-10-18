import React, { useState } from 'react';
import axiosInstance from '../../api/axiosConfig';
import Modal from './Modal';
import Button from './Button';
import './MovimientoFondoModal.css';

const MovimientoFondoModal = ({ isOpen, onClose, fondo, onMovimientoCreado }) => {
  const [formData, setFormData] = useState({
    fondoMovimientoTipo: 'INGRESO',
    fondoMovimientoMonto: '',
    fondoMovimientoConcepto: '',
    fondoMovimientoFecha: new Date().toISOString().split('T')[0],
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.fondoMovimientoMonto || parseFloat(formData.fondoMovimientoMonto) <= 0) {
      alert('El monto debe ser mayor a cero');
      return;
    }

    if (!formData.fondoMovimientoConcepto.trim()) {
      alert('El concepto es requerido');
      return;
    }

    try {
      setLoading(true);

      const dataToSend = {
        fondoId: fondo.fondoId,
        fondoMovimientoTipo: formData.fondoMovimientoTipo,
        fondoMovimientoMonto: parseFloat(formData.fondoMovimientoMonto),
        fondoMovimientoConcepto: formData.fondoMovimientoConcepto,
        fondoMovimientoFecha: formData.fondoMovimientoFecha + 'T00:00:00Z',
      };

      // Crear el movimiento
      await axiosInstance.post('/fondos/movimientos', dataToSend);

      alert('Movimiento creado exitosamente');
      onMovimientoCreado();
    } catch (error) {
      console.error('Error al crear movimiento:', error);
      alert(
        error.response?.data?.message ||
          error.response?.data?.title ||
          'Error al crear el movimiento'
      );
    } finally {
      setLoading(false);
    }
  };

  const calcularNuevoSaldo = () => {
    if (!formData.fondoMovimientoMonto) return fondo.fondoSaldoActual;

    const monto = parseFloat(formData.fondoMovimientoMonto);
    if (formData.fondoMovimientoTipo === 'INGRESO') {
      return fondo.fondoSaldoActual + monto;
    } else {
      return fondo.fondoSaldoActual - monto;
    }
  };

  const nuevoSaldo = calcularNuevoSaldo();

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Nuevo Movimiento de Fondo" size="medium">
      <div className="movimiento-fondo-modal">
        <div className="fondo-info-card">
          <h3>{fondo.fondoNombre}</h3>
          <div className="fondo-details">
            <div className="detail-item">
              <span className="label">Tipo:</span>
              <span className="value">{fondo.tipoFondo?.tipoFondoNombre || '-'}</span>
            </div>
            <div className="detail-item">
              <span className="label">Moneda:</span>
              <span className="value">{fondo.moneda?.monedaCodigo || '-'}</span>
            </div>
            <div className="detail-item">
              <span className="label">Saldo Actual:</span>
              <span className="value saldo-actual">
                ${fondo.fondoSaldoActual.toFixed(2)}
              </span>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="movimiento-form">
          <div className="form-group">
            <label htmlFor="fondoMovimientoTipo">
              Tipo de Movimiento <span className="required">*</span>
            </label>
            <select
              id="fondoMovimientoTipo"
              name="fondoMovimientoTipo"
              value={formData.fondoMovimientoTipo}
              onChange={handleChange}
              required
            >
              <option value="INGRESO">INGRESO (+)</option>
              <option value="EGRESO">EGRESO (-)</option>
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="fondoMovimientoMonto">
                Monto <span className="required">*</span>
              </label>
              <input
                type="number"
                id="fondoMovimientoMonto"
                name="fondoMovimientoMonto"
                value={formData.fondoMovimientoMonto}
                onChange={handleChange}
                step="0.01"
                min="0.01"
                placeholder="0.00"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="fondoMovimientoFecha">
                Fecha <span className="required">*</span>
              </label>
              <input
                type="date"
                id="fondoMovimientoFecha"
                name="fondoMovimientoFecha"
                value={formData.fondoMovimientoFecha}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="fondoMovimientoConcepto">
              Concepto <span className="required">*</span>
            </label>
            <textarea
              id="fondoMovimientoConcepto"
              name="fondoMovimientoConcepto"
              value={formData.fondoMovimientoConcepto}
              onChange={handleChange}
              rows="3"
              placeholder="Describa el motivo del movimiento..."
              required
            />
          </div>

          {formData.fondoMovimientoMonto && (
            <div className="saldo-preview">
              <div className="preview-item">
                <span className="label">Saldo Actual:</span>
                <span className="value">${fondo.fondoSaldoActual.toFixed(2)}</span>
              </div>
              <div className="preview-item">
                <span className="label">
                  {formData.fondoMovimientoTipo === 'INGRESO' ? '+' : '-'} Movimiento:
                </span>
                <span className={`value ${formData.fondoMovimientoTipo.toLowerCase()}`}>
                  ${parseFloat(formData.fondoMovimientoMonto).toFixed(2)}
                </span>
              </div>
              <div className="preview-item total">
                <span className="label">Nuevo Saldo:</span>
                <span className={`value ${nuevoSaldo < 0 ? 'negativo' : 'positivo'}`}>
                  ${nuevoSaldo.toFixed(2)}
                </span>
              </div>
            </div>
          )}

          <div className="modal-actions">
            <Button type="button" variant="secondary" onClick={onClose} disabled={loading}>
              Cancelar
            </Button>
            <Button type="submit" variant="primary" disabled={loading}>
              {loading ? 'Guardando...' : 'Guardar Movimiento'}
            </Button>
          </div>
        </form>
      </div>
    </Modal>
  );
};

export default MovimientoFondoModal;
