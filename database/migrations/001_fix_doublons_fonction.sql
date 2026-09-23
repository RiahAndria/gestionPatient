
BEGIN;

-- 1) Chirurgien -> Chirurgie generale
UPDATE medecin
SET code_fonction = (SELECT code_fonction FROM fonction WHERE nom_fonction = 'Chirurgie generale')
WHERE code_fonction = (SELECT code_fonction FROM fonction WHERE nom_fonction = 'Chirurgien');

DELETE FROM fonction WHERE nom_fonction = 'Chirurgien';

-- 2) Medecin generaliste -> Généraliste
UPDATE medecin
SET code_fonction = (SELECT code_fonction FROM fonction WHERE nom_fonction = 'Généraliste')
WHERE code_fonction = (SELECT code_fonction FROM fonction WHERE nom_fonction = 'Medecin generaliste');

DELETE FROM fonction WHERE nom_fonction = 'Medecin generaliste';

COMMIT;

-- Verification : ne doit plus lister que 5 fonctions distinctes.
SELECT code_fonction, nom_fonction FROM fonction ORDER BY nom_fonction;
