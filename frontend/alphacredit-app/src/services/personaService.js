import axiosInstance from '../api/axiosConfig';
import { API_ENDPOINTS } from '../constants/apiEndpoints';

class PersonaService {
  /**
   * Obtiene la lista de personas con paginación
   * @param {number} pageNumber - Número de página (default: 1)
   * @param {number} pageSize - Tamaño de página (default: 10)
   * @param {boolean} soloActivos - Filtrar solo activos
   * @returns {Promise} Lista de personas
   */
  async getPersonas(pageNumber = 1, pageSize = 10, soloActivos = true) {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.PERSONAS, {
        params: { pageNumber, pageSize, soloActivos },
      });

      // Obtener el total de registros del header
      const totalCount = response.headers['x-total-count'];

      return {
        data: response.data,
        totalCount: totalCount ? parseInt(totalCount, 10) : 0,
        currentPage: pageNumber,
        pageSize,
      };
    } catch (error) {
      console.error('Error al obtener personas:', error);
      throw error;
    }
  }

  /**
   * Obtiene una persona por ID
   * @param {number} id - ID de la persona
   * @returns {Promise} Datos de la persona
   */
  async getPersonaById(id) {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.PERSONAS_BY_ID(id));
      return response.data;
    } catch (error) {
      console.error(`Error al obtener persona ${id}:`, error);
      throw error;
    }
  }

  /**
   * Crea una nueva persona
   * @param {Object} personaData - Datos de la persona
   * @returns {Promise} Persona creada
   */
  async createPersona(personaData) {
    try {
      const response = await axiosInstance.post(API_ENDPOINTS.PERSONAS, personaData);
      return response.data;
    } catch (error) {
      console.error('Error al crear persona:', error);
      throw error;
    }
  }

  /**
   * Actualiza una persona existente
   * @param {number} id - ID de la persona
   * @param {Object} personaData - Datos actualizados
   * @returns {Promise} Persona actualizada
   */
  async updatePersona(id, personaData) {
    try {
      const response = await axiosInstance.put(API_ENDPOINTS.PERSONAS_BY_ID(id), personaData);
      return response.data;
    } catch (error) {
      console.error(`Error al actualizar persona ${id}:`, error);
      throw error;
    }
  }

  /**
   * Elimina (desactiva) una persona
   * @param {number} id - ID de la persona
   * @returns {Promise}
   */
  async deletePersona(id) {
    try {
      const response = await axiosInstance.delete(API_ENDPOINTS.PERSONAS_BY_ID(id));
      return response.data;
    } catch (error) {
      console.error(`Error al eliminar persona ${id}:`, error);
      throw error;
    }
  }

  /**
   * Obtiene la lista de clientes activos
   * @returns {Promise} Lista de clientes
   */
  async getClientes() {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.PERSONAS_CLIENTES);
      return response.data;
    } catch (error) {
      console.error('Error al obtener clientes:', error);
      throw error;
    }
  }

  /**
   * Inactiva una persona (solo si todos los préstamos están cancelados)
   * @param {number} id - ID de la persona
   * @returns {Promise}
   */
  async inactivarPersona(id) {
    try {
      const response = await axiosInstance.put(`${API_ENDPOINTS.PERSONAS}/${id}/inactivar`);
      return response.data;
    } catch (error) {
      console.error(`Error al inactivar persona ${id}:`, error);
      throw error;
    }
  }
}

export default new PersonaService();
