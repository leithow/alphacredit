import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import personaService from '../../services/personaService';
import Table from '../../components/common/Table';
import Button from '../../components/common/Button';
import './PersonasList.css';

const PersonasList = () => {
  const navigate = useNavigate();
  const [personas, setPersonas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
  });

  useEffect(() => {
    loadPersonas();
  }, [pagination.currentPage]);

  const loadPersonas = async () => {
    try {
      setLoading(true);
      const data = await personaService.getPersonas(pagination.currentPage, pagination.pageSize);
      setPersonas(data.data);
      setPagination((prev) => ({
        ...prev,
        totalCount: data.totalCount,
      }));
    } catch (error) {
      console.error('Error al cargar personas:', error);
      alert('Error al cargar la lista de personas');
    } finally {
      setLoading(false);
    }
  };

  const handleRowClick = (persona) => {
    navigate(`/personas/${persona.personaId}`);
  };

  const handleNewPersona = () => {
    navigate('/personas/nuevo');
  };

  const handlePageChange = (newPage) => {
    setPagination((prev) => ({ ...prev, currentPage: newPage }));
  };

  const handleInactivar = async (e, persona) => {
    e.stopPropagation(); // Evitar que se active el click de la fila

    if (!persona.personaEstaActiva) {
      alert('Esta persona ya está inactiva');
      return;
    }

    const confirmacion = window.confirm(
      `¿Está seguro que desea inactivar a ${persona.personaNombreCompleto}?\n\n` +
      'Solo se puede inactivar si todos los préstamos están cancelados.'
    );

    if (!confirmacion) return;

    try {
      const result = await personaService.inactivarPersona(persona.personaId);
      alert(result.message || 'Persona inactivada exitosamente');
      loadPersonas(); // Recargar la lista
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Error al inactivar la persona';
      alert(errorMessage);
    }
  };

  const columns = [
    {
      header: 'ID',
      field: 'personaId',
      width: '80px',
    },
    {
      header: 'Identificación',
      field: 'personaIdentificacion',
      width: '150px',
    },
    {
      header: 'Nombre Completo',
      field: 'personaNombreCompleto',
    },
    {
      header: 'Email',
      field: 'personaEmail',
      render: (row) => row.personaEmail || '-',
    },
    {
      header: 'Dirección',
      field: 'personaDireccion',
      render: (row) => row.personaDireccion || '-',
    },
    {
      header: 'Estado',
      field: 'personaEstaActiva',
      width: '100px',
      render: (row) => (
        <span className={`status-badge ${row.personaEstaActiva ? 'active' : 'inactive'}`}>
          {row.personaEstaActiva ? 'Activa' : 'Inactiva'}
        </span>
      ),
    },
    {
      header: 'Acciones',
      width: '120px',
      render: (row) => (
        <Button
          variant="danger"
          size="small"
          onClick={(e) => handleInactivar(e, row)}
          disabled={!row.personaEstaActiva}
        >
          Inactivar
        </Button>
      ),
    },
  ];

  const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);

  return (
    <div className="personas-list-page">
      <div className="page-header">
        <div>
          <h1>Personas</h1>
          <p>Gestión de clientes y personas en el sistema</p>
        </div>
        <Button variant="primary" onClick={handleNewPersona}>
          ➕ Nueva Persona
        </Button>
      </div>

      <div className="page-content">
        <Table columns={columns} data={personas} onRowClick={handleRowClick} loading={loading} />

        {!loading && totalPages > 1 && (
          <div className="pagination">
            <Button
              variant="secondary"
              size="small"
              disabled={pagination.currentPage === 1}
              onClick={() => handlePageChange(pagination.currentPage - 1)}
            >
              ← Anterior
            </Button>
            <span className="pagination-info">
              Página {pagination.currentPage} de {totalPages} ({pagination.totalCount} registros)
            </span>
            <Button
              variant="secondary"
              size="small"
              disabled={pagination.currentPage >= totalPages}
              onClick={() => handlePageChange(pagination.currentPage + 1)}
            >
              Siguiente →
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};

export default PersonasList;
