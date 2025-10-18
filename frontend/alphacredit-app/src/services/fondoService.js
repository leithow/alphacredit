import axiosInstance from '../api/axiosConfig';
import { API_ENDPOINTS } from '../constants/apiEndpoints';

const fondoService = {
  /**
   * Obtener todos los fondos con paginación y filtros
   */
  async getFondos(pageNumber = 1, pageSize = 10, filters = {}) {
    try {
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
        ...filters,
      });

      const response = await axiosInstance.get(`${API_ENDPOINTS.FONDOS}?${params}`);

      return {
        data: response.data,
        totalCount: parseInt(response.headers['x-total-count'] || '0'),
        pageNumber: parseInt(response.headers['x-page-number'] || '1'),
        pageSize: parseInt(response.headers['x-page-size'] || '10'),
      };
    } catch (error) {
      console.error('Error al obtener fondos:', error);
      throw error;
    }
  },

  /**
   * Obtener un fondo por ID
   */
  async getFondoById(id) {
    try {
      const response = await axiosInstance.get(`${API_ENDPOINTS.FONDOS}/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error al obtener fondo ${id}:`, error);
      throw error;
    }
  },

  /**
   * Crear un nuevo fondo
   */
  async createFondo(fondoData) {
    try {
      const response = await axiosInstance.post(API_ENDPOINTS.FONDOS, fondoData);
      return response.data;
    } catch (error) {
      console.error('Error al crear fondo:', error);
      throw error;
    }
  },

  /**
   * Actualizar un fondo
   */
  async updateFondo(id, fondoData) {
    try {
      const response = await axiosInstance.put(`${API_ENDPOINTS.FONDOS}/${id}`, fondoData);
      return response.data;
    } catch (error) {
      console.error(`Error al actualizar fondo ${id}:`, error);
      throw error;
    }
  },

  /**
   * Eliminar un fondo
   */
  async deleteFondo(id) {
    try {
      const response = await axiosInstance.delete(`${API_ENDPOINTS.FONDOS}/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error al eliminar fondo ${id}:`, error);
      throw error;
    }
  },

  /**
   * Obtener fondos activos
   */
  async getFondosActivos() {
    try {
      const response = await axiosInstance.get(`${API_ENDPOINTS.FONDOS}/activos`);
      return response.data;
    } catch (error) {
      console.error('Error al obtener fondos activos:', error);
      throw error;
    }
  },

  /**
   * Obtener fondos disponibles con saldo suficiente
   */
  async getFondosDisponibles(montoMinimo = null) {
    try {
      const params = montoMinimo ? `?montoMinimo=${montoMinimo}` : '';
      const response = await axiosInstance.get(`${API_ENDPOINTS.FONDOS}/disponibles${params}`);
      return response.data;
    } catch (error) {
      console.error('Error al obtener fondos disponibles:', error);
      throw error;
    }
  },

  /**
   * Obtener saldo de un fondo
   */
  async getSaldoFondo(id) {
    try {
      const response = await axiosInstance.get(`${API_ENDPOINTS.FONDOS}/${id}/saldo`);
      return response.data;
    } catch (error) {
      console.error(`Error al obtener saldo del fondo ${id}:`, error);
      throw error;
    }
  },
};

export default fondoService;
