import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import personaService from '../../services/personaService';
import catalogService from '../../services/catalogService';
import Button from '../../components/common/Button';
import MapModal from '../../components/common/MapModal';
import './PersonaForm.css';

const PersonaForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditMode = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [edad, setEdad] = useState(null);
  const [showMapModal, setShowMapModal] = useState(false);
  const [catalogs, setCatalogs] = useState({
    tiposIdentificacion: [],
    sexos: [],
    estadosCiviles: [],
  });

  const [formData, setFormData] = useState({
    tipoIdentificacionId: '',
    personaIdentificacion: '',
    personaPrimerNombre: '',
    personaSegundoNombre: '',
    personaPrimerApellido: '',
    personaSegundoApellido: '',
    personaNombreCompleto: '',
    sexoId: '',
    estadoCivilId: '',
    personaFechaNacimiento: '',
    personaDireccion: '',
    personaGeolocalizacion: '',
    personaEmail: '',
    personaEsNatural: true,
    personaEsCliente: true,
    personaEsEmpleado: false,
    personaEsProveedor: false,
    // Datos del cónyuge
    conyugeNombre: '',
    conyugeTelefono: '',
  });

  useEffect(() => {
    loadCatalogs();
    if (isEditMode) {
      loadPersona();
    }
  }, [id]);

  // Calcular edad cuando se carga una persona o cambia la fecha
  useEffect(() => {
    if (formData.personaFechaNacimiento) {
      calcularEdad(formData.personaFechaNacimiento);
    }
  }, [formData.personaFechaNacimiento]);

  const loadCatalogs = async () => {
    try {
      const [tiposId, sexos, estadosCiviles] = await Promise.all([
        catalogService.getTiposIdentificacion(),
        catalogService.getSexos(),
        catalogService.getEstadosCiviles(),
      ]);

      setCatalogs({
        tiposIdentificacion: tiposId,
        sexos: sexos,
        estadosCiviles: estadosCiviles,
      });
    } catch (error) {
      console.error('Error al cargar catálogos:', error);
      alert('Error al cargar los catálogos');
    }
  };

  const loadPersona = async () => {
    try {
      setLoading(true);
      const persona = await personaService.getPersonaById(id);
      setFormData({
        ...persona,
        personaFechaNacimiento: persona.personaFechaNacimiento
          ? persona.personaFechaNacimiento.split('T')[0]
          : '',
        conyugeNombre: persona.personaConyuge?.conyugeNombre || '',
        conyugeTelefono: persona.personaConyuge?.conyugeTelefono || '',
      });
    } catch (error) {
      console.error('Error al cargar persona:', error);
      alert('Error al cargar los datos de la persona');
    } finally {
      setLoading(false);
    }
  };

  // Calcular edad basada en fecha de nacimiento
  const calcularEdad = (fechaNacimiento) => {
    if (!fechaNacimiento) {
      setEdad(null);
      return;
    }

    const hoy = new Date();
    const nacimiento = new Date(fechaNacimiento);
    let edad = hoy.getFullYear() - nacimiento.getFullYear();
    const mes = hoy.getMonth() - nacimiento.getMonth();

    if (mes < 0 || (mes === 0 && hoy.getDate() < nacimiento.getDate())) {
      edad--;
    }

    setEdad(edad >= 0 ? edad : null);
  };

  // Aplicar máscara a cédula: ####-####-#####
  const aplicarMascaraCedula = (valor) => {
    const numeros = valor.replace(/\D/g, '');
    let formatted = numeros;
    if (numeros.length > 4) {
      formatted = numeros.slice(0, 4) + '-' + numeros.slice(4);
    }
    if (numeros.length > 8) {
      formatted = numeros.slice(0, 4) + '-' + numeros.slice(4, 8) + '-' + numeros.slice(8, 13);
    }
    return formatted;
  };

  // Abrir modal de mapa
  const abrirMapaUbicacion = () => {
    setShowMapModal(true);
  };

  // Manejar selección de ubicación desde el mapa
  const handleSelectLocation = (geoString) => {
    setFormData((prev) => ({
      ...prev,
      personaGeolocalizacion: geoString,
    }));
  };

  // Obtener tipo de identificación seleccionado
  const getTipoIdentificacion = () => {
    if (!formData.tipoIdentificacionId) return null;
    return catalogs.tiposIdentificacion.find(
      (tipo) => tipo.tipoIdentificacionId === parseInt(formData.tipoIdentificacionId)
    );
  };

  const isCedula = () => {
    const tipo = getTipoIdentificacion();
    return tipo && tipo.tipoIdentificacionNombre.toUpperCase().includes('CEDULA');
  };

  // Verificar si el estado civil requiere cónyuge
  const requiereConyuge = () => {
    if (!formData.estadoCivilId) return false;
    const estadoCivil = catalogs.estadosCiviles.find(
      (estado) => estado.estadoCivilId === parseInt(formData.estadoCivilId)
    );
    return estadoCivil?.estadoCivilRequiereConyuge || false;
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    let finalValue = type === 'checkbox' ? checked : value;

    // Aplicar máscara si es cédula
    if (name === 'personaIdentificacion' && isCedula()) {
      finalValue = aplicarMascaraCedula(value);
    }

    // Calcular edad si cambió la fecha de nacimiento
    if (name === 'personaFechaNacimiento') {
      calcularEdad(value);
    }

    setFormData((prev) => ({
      ...prev,
      [name]: finalValue,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Validaciones básicas
    if (!formData.tipoIdentificacionId || !formData.personaIdentificacion) {
      alert('Por favor complete los campos requeridos');
      return;
    }

    if (!formData.personaPrimerNombre || !formData.personaPrimerApellido) {
      alert('El primer nombre y primer apellido son requeridos');
      return;
    }

    if (!formData.personaDireccion) {
      alert('La dirección es requerida');
      return;
    }

    // Validar cónyuge si es requerido
    if (requiereConyuge() && !formData.conyugeNombre) {
      alert('Debe ingresar el nombre del cónyuge');
      return;
    }

    // Auto-generar nombre completo
    const nombreCompleto = [
      formData.personaPrimerNombre,
      formData.personaSegundoNombre,
      formData.personaPrimerApellido,
      formData.personaSegundoApellido,
    ]
      .filter(Boolean)
      .join(' ');

    // Preparar fecha de nacimiento
    let fechaNacimiento = formData.personaFechaNacimiento;
    if (!fechaNacimiento || fechaNacimiento === '') {
      fechaNacimiento = '1900-01-01';
    }
    if (!fechaNacimiento.includes('T')) {
      fechaNacimiento = fechaNacimiento + 'T00:00:00Z';
    }

    const dataToSend = {
      ...formData,
      personaNombreCompleto: nombreCompleto,
      personaFechaNacimiento: fechaNacimiento,
      tipoIdentificacionId: parseInt(formData.tipoIdentificacionId),
      sexoId: formData.sexoId ? parseInt(formData.sexoId) : null,
      estadoCivilId: formData.estadoCivilId ? parseInt(formData.estadoCivilId) : null,
    };

    // Si requiere cónyuge, agregar los datos
    if (requiereConyuge() && formData.conyugeNombre) {
      dataToSend.personaConyuge = {
        conyugeNombre: formData.conyugeNombre,
        conyugeTelefono: formData.conyugeTelefono || null,
      };
    }

    try {
      setLoading(true);
      if (isEditMode) {
        await personaService.updatePersona(id, dataToSend);
        alert('Persona actualizada exitosamente');
      } else {
        await personaService.createPersona(dataToSend);
        alert('Persona creada exitosamente');
      }
      navigate('/personas');
    } catch (error) {
      console.error('Error al guardar persona:', error);
      alert(
        error.response?.data?.message ||
          error.response?.data?.title ||
          'Error al guardar la persona'
      );
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditMode) {
    return <div className="form-loading">Cargando datos...</div>;
  }

  return (
    <div className="persona-form-page">
      <div className="form-header">
        <h1>{isEditMode ? 'Editar Persona' : 'Nueva Persona'}</h1>
        <p>Complete los datos de la persona</p>
      </div>

      <form onSubmit={handleSubmit} className="persona-form">
        {/* IDENTIFICACIÓN */}
        <div className="form-section">
          <h3>Identificación</h3>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="tipoIdentificacionId">
                Tipo de Identificación <span className="required">*</span>
              </label>
              <select
                id="tipoIdentificacionId"
                name="tipoIdentificacionId"
                value={formData.tipoIdentificacionId}
                onChange={handleChange}
                disabled={isEditMode}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.tiposIdentificacion.map((tipo) => (
                  <option key={tipo.tipoIdentificacionId} value={tipo.tipoIdentificacionId}>
                    {tipo.tipoIdentificacionNombre}
                  </option>
                ))}
              </select>
              {isEditMode && (
                <small className="helper-text">No se puede cambiar el tipo de identificación</small>
              )}
            </div>

            <div className="form-group">
              <label htmlFor="personaIdentificacion">
                Número de Identificación <span className="required">*</span>
              </label>
              <input
                type="text"
                id="personaIdentificacion"
                name="personaIdentificacion"
                value={formData.personaIdentificacion}
                onChange={handleChange}
                placeholder={isCedula() ? '0000-0000-00000' : 'Ingrese el número de identificación'}
                maxLength={isCedula() ? 15 : 20}
                readOnly={isEditMode}
                className={isEditMode ? 'readonly-field' : ''}
                required
              />
              {isCedula() && !isEditMode && (
                <small className="helper-text">Formato: ####-####-#####</small>
              )}
              {isEditMode && (
                <small className="helper-text">La identificación no puede ser modificada</small>
              )}
            </div>
          </div>
        </div>

        {/* DATOS PERSONALES */}
        <div className="form-section">
          <h3>Datos Personales</h3>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="personaPrimerNombre">
                Primer Nombre <span className="required">*</span>
              </label>
              <input
                type="text"
                id="personaPrimerNombre"
                name="personaPrimerNombre"
                value={formData.personaPrimerNombre}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="personaSegundoNombre">Segundo Nombre</label>
              <input
                type="text"
                id="personaSegundoNombre"
                name="personaSegundoNombre"
                value={formData.personaSegundoNombre}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="personaPrimerApellido">
                Primer Apellido <span className="required">*</span>
              </label>
              <input
                type="text"
                id="personaPrimerApellido"
                name="personaPrimerApellido"
                value={formData.personaPrimerApellido}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="personaSegundoApellido">Segundo Apellido</label>
              <input
                type="text"
                id="personaSegundoApellido"
                name="personaSegundoApellido"
                value={formData.personaSegundoApellido}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="sexoId">Sexo</label>
              <select id="sexoId" name="sexoId" value={formData.sexoId} onChange={handleChange}>
                <option value="">Seleccione...</option>
                {catalogs.sexos.map((sexo) => (
                  <option key={sexo.sexoId} value={sexo.sexoId}>
                    {sexo.sexoNombre}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="estadoCivilId">Estado Civil</label>
              <select
                id="estadoCivilId"
                name="estadoCivilId"
                value={formData.estadoCivilId}
                onChange={handleChange}
              >
                <option value="">Seleccione...</option>
                {catalogs.estadosCiviles.map((estado) => (
                  <option key={estado.estadoCivilId} value={estado.estadoCivilId}>
                    {estado.estadoCivilNombre}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="personaFechaNacimiento">Fecha de Nacimiento</label>
              <input
                type="date"
                id="personaFechaNacimiento"
                name="personaFechaNacimiento"
                value={formData.personaFechaNacimiento}
                onChange={handleChange}
              />
              {edad !== null && (
                <small className="edad-calculada">
                  Edad: <strong>{edad} años</strong>
                </small>
              )}
            </div>
          </div>
        </div>

        {/* DATOS DEL CÓNYUGE - Condicional */}
        {requiereConyuge() && (
          <div className="form-section conyuge-section">
            <h3>Datos del Cónyuge</h3>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="conyugeNombre">
                  Nombre del Cónyuge <span className="required">*</span>
                </label>
                <input
                  type="text"
                  id="conyugeNombre"
                  name="conyugeNombre"
                  value={formData.conyugeNombre}
                  onChange={handleChange}
                  placeholder="Nombre completo del cónyuge"
                  required={requiereConyuge()}
                />
              </div>

              <div className="form-group">
                <label htmlFor="conyugeTelefono">Teléfono del Cónyuge</label>
                <input
                  type="text"
                  id="conyugeTelefono"
                  name="conyugeTelefono"
                  value={formData.conyugeTelefono}
                  onChange={handleChange}
                  placeholder="Ej: 9999-9999"
                  maxLength="20"
                />
              </div>
            </div>
          </div>
        )}

        {/* CONTACTO */}
        <div className="form-section">
          <h3>Contacto y Ubicación</h3>
          <div className="form-row">
            <div className="form-group full-width">
              <label htmlFor="personaDireccion">
                Dirección <span className="required">*</span>
              </label>
              <input
                type="text"
                id="personaDireccion"
                name="personaDireccion"
                value={formData.personaDireccion}
                onChange={handleChange}
                placeholder="Dirección completa"
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="personaEmail">Email</label>
              <input
                type="email"
                id="personaEmail"
                name="personaEmail"
                value={formData.personaEmail}
                onChange={handleChange}
                placeholder="correo@ejemplo.com"
              />
            </div>

            <div className="form-group">
              <label htmlFor="personaGeolocalizacion">
                Geolocalización
                <Button
                  type="button"
                  variant="outline"
                  size="small"
                  onClick={abrirMapaUbicacion}
                  style={{ marginLeft: '10px' }}
                >
                  🗺️ Seleccionar en Mapa
                </Button>
              </label>
              <input
                type="text"
                id="personaGeolocalizacion"
                name="personaGeolocalizacion"
                value={formData.personaGeolocalizacion}
                onChange={handleChange}
                placeholder="Latitud,Longitud"
                readOnly
              />
              {formData.personaGeolocalizacion && (
                <small className="helper-text success">
                  ✓ Ubicación guardada: {formData.personaGeolocalizacion}
                </small>
              )}
            </div>
          </div>
        </div>

        {/* TIPO DE PERSONA */}
        <div className="form-section">
          <h3>Tipo de Persona</h3>
          <div className="form-row">
            <div className="form-group checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="personaEsNatural"
                  checked={formData.personaEsNatural}
                  onChange={handleChange}
                />
                Persona Natural
              </label>
            </div>

            <div className="form-group checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="personaEsCliente"
                  checked={formData.personaEsCliente}
                  onChange={handleChange}
                />
                Cliente
              </label>
            </div>

            <div className="form-group checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="personaEsEmpleado"
                  checked={formData.personaEsEmpleado}
                  onChange={handleChange}
                />
                Empleado
              </label>
            </div>

            <div className="form-group checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="personaEsProveedor"
                  checked={formData.personaEsProveedor}
                  onChange={handleChange}
                />
                Proveedor
              </label>
            </div>
          </div>
        </div>

        {/* ACCIONES */}
        <div className="form-actions">
          <Button type="button" variant="secondary" onClick={() => navigate('/personas')}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
          </Button>
        </div>
      </form>

      {/* MODAL DE MAPA */}
      <MapModal
        isOpen={showMapModal}
        onClose={() => setShowMapModal(false)}
        onSelectLocation={handleSelectLocation}
        initialLocation={formData.personaGeolocalizacion}
      />
    </div>
  );
};

export default PersonaForm;
