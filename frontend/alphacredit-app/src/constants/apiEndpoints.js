// API Endpoints Constants
export const API_ENDPOINTS = {
  // Personas
  PERSONAS: '/personas',
  PERSONAS_BY_ID: (id) => `/personas/${id}`,
  PERSONAS_CLIENTES: '/personas/clientes',

  // Préstamos
  PRESTAMOS: '/prestamos',
  PRESTAMOS_BY_ID: (id) => `/prestamos/${id}`,
  PRESTAMOS_AMORTIZACION: (id) => `/prestamos/${id}/amortizacion`,

  // Garantías
  GARANTIAS: '/garantias',
  GARANTIAS_BY_ID: (id) => `/garantias/${id}`,

  // Fondos
  FONDOS: '/fondos',
  FONDOS_BY_ID: (id) => `/fondos/${id}`,

  // Empresas
  EMPRESAS: '/empresas',
  EMPRESAS_BY_ID: (id) => `/empresas/${id}`,

  // Sucursales
  SUCURSALES: '/sucursales',
  SUCURSALES_BY_ID: (id) => `/sucursales/${id}`,

  // Catálogos
  CATALOGS: {
    TIPO_IDENTIFICACION: '/catalogs/tipoidentificacion',
    SEXO: '/catalogs/sexo',
    ESTADO_CIVIL: '/catalogs/estadocivil',
    ESTADO_PRESTAMO: '/catalogs/estadoprestamo',
    FRECUENCIA_PAGO: '/catalogs/frecuenciapago',
    MONEDA: '/catalogs/moneda',
    TIPO_GARANTIA: '/catalogs/tipogarantia',
    ESTADO_GARANTIA: '/catalogs/estadogarantia',
    DESTINO_CREDITO: '/catalogs/destinocredito',
    COMPONENTE_PRESTAMO: '/catalogs/componenteprestamo',
    ESTADO_COMPONENTE: '/catalogs/estadocomponente',
    TIPO_FONDO: '/catalogs/tipofondo',
  },

  // Health
  HEALTH: '/health',
};

export default API_ENDPOINTS;
