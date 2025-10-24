import React from 'react';
import { Link } from 'react-router-dom';
import './Navbar.css';

const Navbar = () => {
  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          AlphaCredit
        </Link>

        <ul className="navbar-menu">
          <li className="navbar-item">
            <Link to="/" className="navbar-link">
              Dashboard
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/personas" className="navbar-link">
              Personas
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/prestamos" className="navbar-link">
              Préstamos
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/garantias" className="navbar-link">
              Garantías
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/fondos" className="navbar-link">
              Fondos
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/transacciones" className="navbar-link">
              Transacciones
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
