import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import fondoService from '../../services/fondoService';
import catalogService from '../../services/catalogService';
import Button from '../../components/common/Button';
import './FondoForm.css';

const FondoForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditMode = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [catalogs, setCatalogs] = useState({
    tiposFondo: [],
    monedas: [],
  });

  const [formData, setFormData] = useState({
    fondoNombre: '',
    fondoDescripcion: '',
    tipoFondoId: '',
    monedaId: '',
    fondoSaldoInicial: '',
    fondoEstaActivo: true,
  });

  useEffect(() => {
    loadCatalogs();
    if (isEditMode) {
      loadFondo();
    }
  }, [id]);

  const loadCatalogs = async () => {
    try {
      const [tiposFondoData, monedasData] = await Promise.all([
        catalogService.getTiposFondo(),
        catalogService.getMonedas(),
      ]);

      setCatalogs({
        tiposFondo: tiposFondoData,
        monedas: monedasData,
      });

      // Establecer valores por defecto
      if (monedasData.length > 0 && !isEditMode) {
        const monedaDOP = monedasData.find((m) =>
          m.monedaCodigo?.toUpperCase().includes('DOP')
        );
        setFormData((prev) => ({
          ...prev,
          monedaId: monedaDOP ? monedaDOP.monedaId : monedasData[0].monedaId,
        }));
      }
    } catch (error) {
      console.error('Error al cargar catálogos:', error);
      alert('Error al cargar los datos necesarios');
    }
  };

  const loadFondo = async () => {
    try {
      setLoading(true);
      const fondo = await fondoService.getFondoById(id);
      setFormData({
        fondoNombre: fondo.fondoNombre,
        fondoDescripcion: fondo.fondoDescripcion || '',
        tipoFondoId: fondo.tipoFondoId,
        monedaId: fondo.monedaId,
        fondoSaldoInicial: fondo.fondoSaldoInicial,
        fondoEstaActivo: fondo.fondoEstaActivo,
      });
    } catch (error) {
      console.error('Error al cargar fondo:', error);
      alert('Error al cargar los datos del fondo');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.fondoNombre.trim()) {
      alert('El nombre del fondo es requerido');
      return;
    }

    if (!formData.tipoFondoId || !formData.monedaId) {
      alert('Complete todos los campos requeridos');
      return;
    }

    if (!isEditMode && (!formData.fondoSaldoInicial || parseFloat(formData.fondoSaldoInicial) < 0)) {
      alert('El saldo inicial debe ser mayor o igual a cero');
      return;
    }

    const dataToSend = {
      fondoNombre: formData.fondoNombre.trim(),
      fondoDescripcion: formData.fondoDescripcion?.trim() || '',
      tipoFondoId: parseInt(formData.tipoFondoId),
      monedaId: parseInt(formData.monedaId),
      fondoEstaActivo: formData.fondoEstaActivo,
      fondoUserCrea: 'admin',
    };

    if (!isEditMode) {
      dataToSend.fondoSaldoInicial = parseFloat(formData.fondoSaldoInicial);
    }

    try {
      setLoading(true);
      if (isEditMode) {
        await fondoService.updateFondo(id, { ...dataToSend, fondoId: parseInt(id) });
        alert('Fondo actualizado exitosamente');
      } else {
        await fondoService.createFondo(dataToSend);
        alert('Fondo creado exitosamente');
      }
      navigate('/fondos');
    } catch (error) {
      console.error('Error al guardar fondo:', error);
      alert(
        error.response?.data?.message ||
          error.response?.data?.title ||
          'Error al guardar el fondo'
      );
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditMode) {
    return <div className="form-loading">Cargando datos del fondo...</div>;
  }

  return (
    <div className="fondo-form-page">
      <div className="form-header">
        <h1>{isEditMode ? 'Editar Fondo' : 'Nuevo Fondo'}</h1>
        <p>Complete los datos del fondo</p>
      </div>

      <form onSubmit={handleSubmit} className="fondo-form">
        {/* INFORMACIÓN BÁSICA */}
        <div className="form-section">
          <h3>Información Básica</h3>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="fondoNombre">
                Nombre del Fondo <span className="required">*</span>
              </label>
              <input
                type="text"
                id="fondoNombre"
                name="fondoNombre"
                value={formData.fondoNombre}
                onChange={handleChange}
                placeholder="Ej: Fondo Efectivo Principal"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="tipoFondoId">
                Tipo de Fondo <span className="required">*</span>
              </label>
              <select
                id="tipoFondoId"
                name="tipoFondoId"
                value={formData.tipoFondoId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.tiposFondo.map((tipo) => (
                  <option key={tipo.tipoFondoId} value={tipo.tipoFondoId}>
                    {tipo.tipoFondoNombre}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="monedaId">
                Moneda <span className="required">*</span>
              </label>
              <select
                id="monedaId"
                name="monedaId"
                value={formData.monedaId}
                onChange={handleChange}
                required
                disabled={isEditMode}
              >
                <option value="">Seleccione...</option>
                {catalogs.monedas.map((moneda) => (
                  <option key={moneda.monedaId} value={moneda.monedaId}>
                    {moneda.monedaNombre} ({moneda.monedaCodigo})
                  </option>
                ))}
              </select>
              {isEditMode && (
                <small className="helper-text">
                  No se puede modificar la moneda de un fondo existente
                </small>
              )}
            </div>

            {!isEditMode && (
              <div className="form-group">
                <label htmlFor="fondoSaldoInicial">
                  Saldo Inicial <span className="required">*</span>
                </label>
                <input
                  type="number"
                  id="fondoSaldoInicial"
                  name="fondoSaldoInicial"
                  value={formData.fondoSaldoInicial}
                  onChange={handleChange}
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  required
                />
              </div>
            )}
          </div>

          <div className="form-group full-width">
            <label htmlFor="fondoDescripcion">Descripción</label>
            <textarea
              id="fondoDescripcion"
              name="fondoDescripcion"
              value={formData.fondoDescripcion}
              onChange={handleChange}
              rows="3"
              placeholder="Describa el propósito o características del fondo..."
            />
          </div>

          <div className="form-group">
            <label className="checkbox-label">
              <input
                type="checkbox"
                name="fondoEstaActivo"
                checked={formData.fondoEstaActivo}
                onChange={handleChange}
              />
              <span>Fondo activo</span>
            </label>
            <small className="helper-text">
              Los fondos inactivos no aparecerán disponibles para nuevos préstamos
            </small>
          </div>
        </div>

        {/* ACCIONES */}
        <div className="form-actions">
          <Button
            type="button"
            variant="secondary"
            onClick={() => navigate('/fondos')}
            disabled={loading}
          >
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear Fondo'}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default FondoForm;
