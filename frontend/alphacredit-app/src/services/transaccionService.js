import axiosInstance from '../api/axiosConfig';

/**
 * Servicio para gestión de transacciones (abonos a préstamos, etc.)
 */
const transaccionService = {
  /**
   * Aplica un abono a un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @param {object} abonoData - Datos del abono
   * @returns {Promise<object>} - Respuesta del servidor con detalles del abono aplicado
   */
  aplicarAbonoPrestamo: async (prestamoId, abonoData) => {
    try {
      const response = await axiosInstance.post(`/prestamos/${prestamoId}/abonos`, abonoData);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene el estado de cuenta detallado de un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<object>} - Estado de cuenta con todas las cuotas y saldos
   */
  obtenerEstadoCuenta: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}/estado-cuenta`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene el historial de pagos de un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<Array>} - Lista de pagos realizados
   */
  obtenerHistorialPagos: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}/historial-pagos`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Calcula la mora actual de un préstamo sin guardarla
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<object>} - Mora calculada
   */
  calcularMoraPrestamo: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}/calcular-mora`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene la mora total acumulada de un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<object>} - Mora total del préstamo
   */
  obtenerMoraTotal: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}/mora-total`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene todas las formas de pago activas con sus fondos asociados
   * @returns {Promise<Array>} - Lista de formas de pago
   */
  obtenerFormasPago: async () => {
    try {
      const response = await axiosInstance.get('/catalogs/formapago', {
        params: { soloActivos: true }
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Busca préstamos activos por número o cliente
   * @param {string} termino - Término de búsqueda
   * @returns {Promise<Array>} - Lista de préstamos que coinciden
   */
  buscarPrestamos: async (termino) => {
    try {
      const response = await axiosInstance.get('/prestamos/buscar', {
        params: { termino }
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene los detalles completos de un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<object>} - Detalles del préstamo
   */
  obtenerDetallePrestamo: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  },

  /**
   * Obtiene el resumen de un préstamo (saldos, cuotas pagadas, etc.)
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<object>} - Resumen del préstamo
   */
  obtenerResumenPrestamo: async (prestamoId) => {
    try {
      const response = await axiosInstance.get(`/prestamos/${prestamoId}/resumen`);
      return response.data;
    } catch (error) {
      throw error.response?.data || error.message;
    }
  }
};

export default transaccionService;
