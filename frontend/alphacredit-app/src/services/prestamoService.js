import axiosInstance from '../api/axiosConfig';
import { API_ENDPOINTS } from '../constants/apiEndpoints';

class PrestamoService {
  /**
   * Obtiene la lista de préstamos con paginación
   * @param {number} pageNumber - Número de página
   * @param {number} pageSize - Tamaño de página
   * @param {Object|number} filtersOrPersonaId - Objeto de filtros {search, estadoPrestamoId} o personaId para compatibilidad
   * @param {number} estadoId - Filtrar por estado (opcional, para compatibilidad)
   * @returns {Promise} Lista de préstamos
   */
  async getPrestamos(pageNumber = 1, pageSize = 10, filtersOrPersonaId = null, estadoId = null) {
    try {
      const params = { pageNumber, pageSize };

      // Si se pasa un objeto de filtros
      if (filtersOrPersonaId && typeof filtersOrPersonaId === 'object') {
        if (filtersOrPersonaId.search) params.search = filtersOrPersonaId.search;
        if (filtersOrPersonaId.estadoPrestamoId) params.estadoId = filtersOrPersonaId.estadoPrestamoId;
      }
      // Si se pasa personaId directamente (compatibilidad con versión anterior)
      else if (filtersOrPersonaId) {
        params.personaId = filtersOrPersonaId;
      }

      if (estadoId) params.estadoId = estadoId;

      const response = await axiosInstance.get(API_ENDPOINTS.PRESTAMOS, { params });

      const totalCount = response.headers['x-total-count'];

      return {
        data: response.data,
        totalCount: totalCount ? parseInt(totalCount, 10) : 0,
        currentPage: pageNumber,
        pageSize,
      };
    } catch (error) {
      console.error('Error al obtener préstamos:', error);
      throw error;
    }
  }

  /**
   * Obtiene un préstamo por ID
   * @param {number} id - ID del préstamo
   * @returns {Promise} Datos del préstamo
   */
  async getPrestamoById(id) {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.PRESTAMOS_BY_ID(id));
      return response.data;
    } catch (error) {
      console.error(`Error al obtener préstamo ${id}:`, error);
      throw error;
    }
  }

  /**
   * Obtiene la tabla de amortización de un préstamo existente
   * @param {number} id - ID del préstamo
   * @returns {Promise} Tabla de amortización
   */
  async getAmortizacion(id) {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.PRESTAMOS_AMORTIZACION(id));
      return response.data;
    } catch (error) {
      console.error(`Error al obtener amortización del préstamo ${id}:`, error);
      throw error;
    }
  }

  /**
   * Calcula la amortización de un préstamo (sin guardarlo)
   * @param {Object} params - Parámetros: monto, tasaInteres, plazo, frecuenciaPagoDias
   * @returns {Promise} Tabla de amortización calculada
   */
  async calcularAmortizacion(params) {
    try {
      const response = await axiosInstance.post('/prestamos/calcular-amortizacion', params);
      return response.data;
    } catch (error) {
      console.error('Error al calcular amortización:', error);
      throw error;
    }
  }

  /**
   * Crea un nuevo préstamo
   * @param {Object} prestamoData - Datos del préstamo
   * @returns {Promise} Préstamo creado
   */
  async createPrestamo(prestamoData) {
    try {
      const response = await axiosInstance.post(API_ENDPOINTS.PRESTAMOS, prestamoData);
      return response.data;
    } catch (error) {
      console.error('Error al crear préstamo:', error);
      throw error;
    }
  }

  /**
   * Actualiza un préstamo existente
   * @param {number} id - ID del préstamo
   * @param {Object} prestamoData - Datos actualizados
   * @returns {Promise} Préstamo actualizado
   */
  async updatePrestamo(id, prestamoData) {
    try {
      const response = await axiosInstance.put(API_ENDPOINTS.PRESTAMOS_BY_ID(id), prestamoData);
      return response.data;
    } catch (error) {
      console.error(`Error al actualizar préstamo ${id}:`, error);
      throw error;
    }
  }

  /**
   * Elimina un préstamo
   * @param {number} id - ID del préstamo
   * @returns {Promise}
   */
  async deletePrestamo(id) {
    try {
      const response = await axiosInstance.delete(API_ENDPOINTS.PRESTAMOS_BY_ID(id));
      return response.data;
    } catch (error) {
      console.error(`Error al eliminar préstamo ${id}:`, error);
      throw error;
    }
  }
}

export default new PrestamoService();
