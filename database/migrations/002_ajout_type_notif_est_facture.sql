-- Migration necessaire pour les nouvelles fonctionnalites : alertes
-- RDV/paiement typees, facturation.
-- A executer une seule fois sur gestion_patient_db.

BEGIN;

-- Distingue les notifications "rendez-vous" (rappels) des notifications
-- "paiement" (relances), pour les 2 onglets de la page Notifications.
ALTER TABLE notification
    ADD COLUMN IF NOT EXISTS type_notif VARCHAR(20) NOT NULL DEFAULT 'RESERVATION';

-- Les lignes deja en base sont soit des relances de paiement (texte
-- commencant par "Relance de paiement"), soit des rappels/confirmations
-- de RDV. On retype les relances existantes pour que l'historique reste
-- coherent avec les nouveaux onglets :
UPDATE notification SET type_notif = 'PAIEMENT' WHERE textenotif ILIKE 'Relance de paiement%';

-- Sait si un paiement regle a deja ete transforme en facture affichee
-- (page Paiements, colonne "Facturation").
ALTER TABLE paiement
    ADD COLUMN IF NOT EXISTS est_facture BOOLEAN NOT NULL DEFAULT false;

COMMIT;

-- Verification
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name IN ('notification', 'paiement')
  AND column_name IN ('type_notif', 'est_facture');
