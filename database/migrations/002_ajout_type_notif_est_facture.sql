

BEGIN;

ALTER TABLE notification
    ADD COLUMN IF NOT EXISTS type_notif VARCHAR(20) NOT NULL DEFAULT 'RESERVATION';

UPDATE notification SET type_notif = 'PAIEMENT' WHERE textenotif ILIKE 'Relance de paiement%';

ALTER TABLE paiement
    ADD COLUMN IF NOT EXISTS est_facture BOOLEAN NOT NULL DEFAULT false;

COMMIT;

SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name IN ('notification', 'paiement')
  AND column_name IN ('type_notif', 'est_facture');
