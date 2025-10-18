-- Update EstadoCivil to mark which statuses require spouse information
-- Typically "Casado" (Married) and "Unión Libre" (Common Law) require spouse info

UPDATE estadocivil
SET estadocivilrequiereconyuge = true
WHERE UPPER(estadocivilnombre) IN ('CASADO', 'CASADA', 'MARRIED', 'UNION LIBRE', 'COMMON LAW');

-- All others should not require spouse
UPDATE estadocivil
SET estadocivilrequiereconyuge = false
WHERE UPPER(estadocivilnombre) NOT IN ('CASADO', 'CASADA', 'MARRIED', 'UNION LIBRE', 'COMMON LAW');

-- Display results
SELECT estadocivilid, estadocivilnombre, estadocivilrequiereconyuge
FROM estadocivil
ORDER BY estadocivilnombre;
