import React from 'react';
import './Table.css';

const Table = ({ columns, data, onRowClick, loading, emptyMessage = 'No hay datos disponibles' }) => {
  if (loading) {
    return <div className="table-loading">Cargando...</div>;
  }

  if (!data || data.length === 0) {
    return <div className="table-empty">{emptyMessage}</div>;
  }

  return (
    <div className="table-container">
      <table className="table">
        <thead>
          <tr>
            {columns.map((column, index) => (
              <th key={index} style={{ width: column.width }}>
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, rowIndex) => (
            <tr
              key={rowIndex}
              onClick={() => onRowClick && onRowClick(row)}
              className={onRowClick ? 'clickable' : ''}
            >
              {columns.map((column, colIndex) => (
                <td key={colIndex}>
                  {column.render ? column.render(row) : row[column.field]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default Table;
