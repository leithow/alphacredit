/**
 * Currency formatting utility
 * Formats numbers as currency using the locale and currency code from .env
 */

const CURRENCY_LOCALE = process.env.REACT_APP_CURRENCY_LOCALE || 'es-HN';
const CURRENCY_CODE = process.env.REACT_APP_CURRENCY_CODE || 'HNL';

/**
 * Format a number as currency
 * @param {number} amount - The amount to format
 * @param {number} minimumFractionDigits - Minimum decimal places (default: 2)
 * @param {number} maximumFractionDigits - Maximum decimal places (default: 2)
 * @returns {string} Formatted currency string
 */
export const formatCurrency = (amount, minimumFractionDigits = 2, maximumFractionDigits = 2) => {
  if (amount === null || amount === undefined || isNaN(amount)) {
    return new Intl.NumberFormat(CURRENCY_LOCALE, {
      style: 'currency',
      currency: CURRENCY_CODE,
      minimumFractionDigits,
      maximumFractionDigits
    }).format(0);
  }

  return new Intl.NumberFormat(CURRENCY_LOCALE, {
    style: 'currency',
    currency: CURRENCY_CODE,
    minimumFractionDigits,
    maximumFractionDigits
  }).format(amount);
};

/**
 * Format a number as currency without decimals
 * @param {number} amount - The amount to format
 * @returns {string} Formatted currency string without decimals
 */
export const formatCurrencyNoDecimals = (amount) => {
  return formatCurrency(amount, 0, 0);
};

export default formatCurrency;
