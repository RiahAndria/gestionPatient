-- Fusion des fonctions en double dans la table FONCTION.
-- A executer une seule fois sur gestion_patient_db (psql, pgAdmin, DBeaver...).
--
-- Constat (fourni par l'utilisateur) :
--   SELECT nom_fonction FROM fonction;
--     Chirurgien
--     Cardiologue
--     Dermatologue
--     Medecin generaliste
--     Chirurgie generale
--     Généraliste
--     Pédiatre
--
-- "Chirurgien" et "Chirurgie generale" designent la meme specialite,
-- de meme que "Medecin generaliste" et "Généraliste". On garde une
-- seule ligne par specialite (celle dont le nom est deja utilise
-- comme reference dans Services/ServiceMedicalLookupService.cs cote
-- code : "Chirurgie generale" et "Généraliste"), on reattribue les
-- medecins concernes, puis on supprime la ligne devenue inutile.

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
