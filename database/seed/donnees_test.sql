
SET client_encoding = 'UTF8';

BEGIN;

TRUNCATE TABLE
    consultation, disponibilite, dossier_medical, fonction, medecin,
    notification, ordonance, paiement, patient, personne, rendez_vous, temps
RESTART IDENTITY CASCADE;

INSERT INTO fonction (nom_fonction) VALUES
    ('Généraliste'),
    ('Pédiatre'),
    ('Cardiologue'),
    ('Dermatologue'),
    ('Chirurgie generale');

-- -------------------------------------------------------------------
INSERT INTO personne (id, nom, prenom, datedenaissance, genre, adresse, telephone, mail) VALUES

    ('2eed2c84-fd26-425b-8007-22bc3208bb48', 'Rakoto',          'Jean',       '1985-03-12', 'Homme', 'Lot II M 45 Antananarivo',  '0341234501', 'jean.rakoto@gmail.com'),
    ('09a03874-3b21-451f-af12-319446295e5b', 'Rasoa',           'Marie',      '1990-07-25', 'Femme', 'Lot IV B 12 Antananarivo',  '0341234502', 'marie.rasoa@gmail.com'),
    ('60f1f535-a4db-4a2e-9418-5b6622df031f', 'Andria',          'Paul',       '1978-11-02', 'Homme', 'Lot VI C 8 Antananarivo',   '0341234503', 'paul.andria@gmail.com'),
    ('73a3128f-3bec-4f4c-9281-c2762852a189', 'Ravao',           'Hanta',      '2001-01-30', 'Femme', 'Lot III A 21 Antananarivo', '0341234504', 'hanta.ravao@gmail.com'),
    ('99ca3843-c0b8-4b22-afe5-8e84ce69b838', 'Randria',         'Eric',       '1995-05-18', 'Homme', 'Lot I D 3 Antananarivo',    '0341234505', 'eric.randria@gmail.com'),
    ('0a571949-0671-4592-8d74-f7b2ca078eef', 'Rabe',            'Nandrasana', '2007-08-19', 'Femme', 'Lot V E 14 Antananarivo',   '0341234506', 'nandrasana.rabe@gmail.com'),
    ('d64685f7-cb31-48d2-8911-bc5686da140d', 'Razafy',          'Michel',     '1960-12-01', 'Homme', 'Lot II F 6 Antananarivo',   '0341234507', 'michel.razafy@gmail.com'),
    ('5b0bebe8-1d0a-4e54-a2b4-18c013058545', 'Ranaivo',         'Sitraka',    '1999-06-15', 'Homme', 'Lot VII G 2 Antananarivo',  '0341234508', 'sitraka.ranaivo@gmail.com'),

    ('M-01-1-000A', 'Randrianasolo',    'Solo',      '1975-02-14', 'Homme', 'Cabinet A, Analakely',        '0331234501', 'dr.solo@clinique.mg'),
    ('M-02-1-001A', 'Rakotondrabe',     'Voahangy',  '1982-06-01', 'Femme', 'Cabinet B, Analakely',        '0331234502', 'dr.voahangy@clinique.mg'),
    ('M-02-2-002A', 'Rabemananjara',    'Nirina',    '1980-09-09', 'Femme', 'Cabinet C, Ankorondrano',     '0331234503', 'dr.nirina@clinique.mg'),
    ('M-01-3-003A', 'Andriamampianina', 'Tojo',      '1970-01-20', 'Homme', 'Cabinet D, Ankorondrano',     '0331234504', 'dr.tojo@clinique.mg'),
    ('M-02-4-004A', 'Rasoanaivo',       'Voninkazo', '1985-10-10', 'Femme', 'Cabinet E, Ivandry',          '0331234505', 'dr.voninkazo@clinique.mg'),
    ('M-02-5-005A', 'Rasolofo',         'Fara',      '1988-04-22', 'Femme', 'Cabinet F, Ivandry',          '0331234506', 'dr.fara@clinique.mg'),
    ('M-01-5-006A', 'Rabearison',       'Tiana',     '1979-03-03', 'Homme', 'Cabinet G, Ambohipo',         '0331234507', 'dr.tiana@clinique.mg');

INSERT INTO medecin (id_medecin, numero_ordre, statut, code_fonction, taux_horaire) VALUES
    ('M-01-1-000A', '100000001', 'Actif', 1, 50000.00),
    ('M-02-1-001A', '100000002', 'Actif', 1, 45000.00),
    ('M-02-2-002A', '100000003', 'Actif', 2, 80000.00),
    ('M-01-3-003A', '100000004', 'Actif', 3, 150000.00),
    ('M-02-4-004A', '100000005', 'Actif', 4, 100000.00),
    ('M-02-5-005A', '100000006', 'Congé', 5, 120000.00),
    ('M-01-5-006A', '100000007', 'Actif', 5, 130000.00);

INSERT INTO dossier_medical (numerodossier, poids, taille, groupesanguin, allergies, antecedents) VALUES
    ('P-01-10-000A', 78.50,  1.75, 'O+',  'Pénicilline',        'Hypertension légère'),
    ('P-02-00-001A', 62.00,  1.65, 'A+',  NULL,                 'Aucun'),
    ('P-01-10-002A', 90.20,  1.80, 'B-',  'Arachides, pollen',  'Diabète type 2'),
    ('P-02-00-003A', 55.00,  1.60, 'AB+', NULL,                 NULL),
    ('P-01-00-004A', 0.00,   0.00, 'N/A', NULL,                 NULL),
    ('P-02-00-005A', 48.00,  1.58, 'O-',  NULL,                 NULL),
    ('P-01-10-006A', 82.00,  1.70, 'A-',  'Iode',               'Prothèse de hanche (2018)'),
    ('P-01-00-007A', 70.00,  1.72, 'B+',  NULL,                 NULL);

INSERT INTO patient (id, numerodossier, id_her_2, numeroassurance, nom, prenom, datedenaissance, genre, adresse, telephone, mail) VALUES
    ('2eed2c84-fd26-425b-8007-22bc3208bb48', 'P-01-10-000A', 'M-01-1-000A', 'ASSUR-001', 'Rakoto',  'Jean',       '1985-03-12', 'Homme', 'Lot II M 45 Antananarivo',  '0341234501', 'jean.rakoto@gmail.com'),
    ('09a03874-3b21-451f-af12-319446295e5b', 'P-02-00-001A', NULL,          NULL,        'Rasoa',   'Marie',      '1990-07-25', 'Femme', 'Lot IV B 12 Antananarivo',  '0341234502', 'marie.rasoa@gmail.com'),
    ('60f1f535-a4db-4a2e-9418-5b6622df031f', 'P-01-10-002A', 'M-02-2-002A', 'ASSUR-003', 'Andria',  'Paul',       '1978-11-02', 'Homme', 'Lot VI C 8 Antananarivo',   '0341234503', 'paul.andria@gmail.com'),
    ('73a3128f-3bec-4f4c-9281-c2762852a189', 'P-02-00-003A', NULL,          NULL,        'Ravao',   'Hanta',      '2001-01-30', 'Femme', 'Lot III A 21 Antananarivo', '0341234504', 'hanta.ravao@gmail.com'),
    ('99ca3843-c0b8-4b22-afe5-8e84ce69b838', 'P-01-00-004A', NULL,          NULL,        'Randria', 'Eric',       '1995-05-18', 'Homme', 'Lot I D 3 Antananarivo',    '0341234505', 'eric.randria@gmail.com'),
    ('0a571949-0671-4592-8d74-f7b2ca078eef', 'P-02-00-005A', NULL,          NULL,        'Rabe',    'Nandrasana', '2007-08-19', 'Femme', 'Lot V E 14 Antananarivo',   '0341234506', 'nandrasana.rabe@gmail.com'),
    ('d64685f7-cb31-48d2-8911-bc5686da140d', 'P-01-10-006A', 'M-02-2-002A', 'ASSUR-007', 'Razafy',  'Michel',     '1960-12-01', 'Homme', 'Lot II F 6 Antananarivo',   '0341234507', 'michel.razafy@gmail.com'),
    ('5b0bebe8-1d0a-4e54-a2b4-18c013058545', 'P-01-00-007A', NULL,          NULL,        'Ranaivo', 'Sitraka',    '1999-06-15', 'Homme', 'Lot VII G 2 Antananarivo',  '0341234508', 'sitraka.ranaivo@gmail.com');

INSERT INTO rendez_vous (numerordv, id, id_her_2, dateheurerdv, motifrdv, statut, motifannulation) VALUES
    ('RDV-000001', '2eed2c84-fd26-425b-8007-22bc3208bb48', 'M-01-1-000A', '2026-08-06 16:00:00', 'Consultation de routine',      'PLANIFIE', NULL),
    ('RDV-000002', '60f1f535-a4db-4a2e-9418-5b6622df031f', 'M-02-2-002A', '2026-08-07 08:00:00', 'Bilan annuel',                  'PLANIFIE', NULL),
    ('RDV-000003', '73a3128f-3bec-4f4c-9281-c2762852a189', 'M-01-3-003A', '2026-08-09 12:00:00', 'Douleurs thoraciques',          'PLANIFIE', NULL),
    ('RDV-000004', '0a571949-0671-4592-8d74-f7b2ca078eef', 'M-02-4-004A', '2026-08-13 08:00:00', 'Consultation dermatologique',   'PLANIFIE', NULL),
    ('RDV-000005', 'd64685f7-cb31-48d2-8911-bc5686da140d', 'M-01-1-000A', '2026-08-21 16:00:00', 'Suivi tension artérielle',      'PLANIFIE', NULL),
    ('RDV-000006', '60f1f535-a4db-4a2e-9418-5b6622df031f', 'M-02-4-004A', '2026-08-06 08:00:00', 'Contrôle de routine',           'PLANIFIE', NULL),
    ('RDV-000007', '09a03874-3b21-451f-af12-319446295e5b', 'M-02-2-002A', '2026-07-20 08:00:00', 'Suivi vaccination enfant',      'TERMINE',  NULL),
    ('RDV-000008', '2eed2c84-fd26-425b-8007-22bc3208bb48', 'M-01-3-003A', '2026-07-15 12:00:00', 'Douleurs abdominales',          'TERMINE',  NULL),
    ('RDV-000009', '5b0bebe8-1d0a-4e54-a2b4-18c013058545', 'M-01-5-006A', '2026-07-28 08:00:00', 'Consultation pré-opératoire',   'TERMINE',  NULL),
    ('RDV-000010', '99ca3843-c0b8-4b22-afe5-8e84ce69b838', 'M-01-1-000A', '2026-08-04 16:00:00', 'Consultation dermatologique',   'ANNULE',   'Patient indisponible, reprogrammation à venir');

INSERT INTO consultation (numeroconsultation, diagnostique, notesmedicales, numerordv) VALUES
    ('CONS-000001', 'Développement normal, vaccination à jour', 'RAS, prochain rappel dans 6 mois.',                              'RDV-000007'),
    ('CONS-000002', 'Gastrite légère',                          'Prescription de pansement gastrique, contrôle dans 2 semaines si persistance.', 'RDV-000008'),
    ('CONS-000003', 'Bilan pré-opératoire favorable',           'Patient apte à l''intervention, à programmer sous 1 mois.',       'RDV-000009');

INSERT INTO ordonance (numeroprescritption, numeroconsultation, traitement, duree, diagnostique) VALUES
    ('ORD-000001', 'CONS-000001', 'Vitamine D, 1 dose/semaine',       '4 semaines', 'Développement normal'),
    ('ORD-000002', 'CONS-000002', 'Oméprazole 20mg, 1 comprimé/jour', '14 jours',   'Gastrite légère');

-- -------------------------------------------------------
INSERT INTO paiement (numeropaiement, numeroconsultation, datepaiement, montant, modepaiement, statut, numerordv, typepaiement, est_facture) VALUES
    ('PAI-7BC2336A', NULL,           '2026-08-05 09:00:00', 90000.00,  'Mobile Money',   true,  'RDV-000003', 'ACOMPTE', false),
    ('PAI-9F21149D', NULL,           '2026-08-05 10:00:00', 100000.00, 'Espèces',        true,  'RDV-000004', 'ACOMPTE', false),
    ('PAI-20F1C8A9', 'CONS-000001',  '2026-07-20 09:30:00', 80000.00,  'Carte bancaire', true,  'RDV-000007', 'NORMAL',  true),
    ('PAI-CEDFC9AE', 'CONS-000002',  '2026-07-15 13:00:00', 150000.00, 'Espèces',        true,  'RDV-000008', 'NORMAL',  false),
    ('PAI-687CD121', 'CONS-000003',  '2026-07-28 09:00:00', 130000.00, 'Chèque',         false, 'RDV-000009', 'NORMAL',  false);

INSERT INTO notification (numeronotif, numerordv, textenotif, datenotif, lu, type_notif) VALUES
    ('NOTIF-B67B275C', 'RDV-000001', 'Rappel RDV : M./Mme Rakoto Jean le 06/08/2026 à 16:00 (J-0).',                                            '2026-08-06 07:00:00', false, 'RESERVATION'),
    ('NOTIF-4FC2CB2E', 'RDV-000003', 'Rappel RDV : M./Mme Ravao Hanta le 09/08/2026 à 12:00 (J-3).',                                             '2026-08-05 08:00:00', true,  'RESERVATION'),
    ('NOTIF-0A9E14E8', 'RDV-000005', 'Rappel RDV : M./Mme Razafy Michel le 21/08/2026 à 16:00 (J-15).',                                          '2026-08-04 08:00:00', false, 'RESERVATION'),
    ('NOTIF-5DD4E8FC', 'RDV-000004', 'Rendez-vous RDV-000004 confirmé et réglé intégralement (Espèces).',                                        '2026-08-05 10:00:05', true,  'RESERVATION'),
    ('NOTIF-B0629B55', 'RDV-000003', 'Rendez-vous RDV-000003 confirmé avec une avance de 90 000 Ar (Mobile Money). Reste dû : 60 000 Ar.',       '2026-08-05 09:00:05', true,  'RESERVATION'),
    ('NOTIF-571E4F8A', 'RDV-000003', 'Relance de paiement n°1 : merci de régulariser votre facture liée à ce rendez-vous.',                      '2026-08-05 12:00:00', true,  'PAIEMENT'),
    ('NOTIF-3026DB2A', 'RDV-000003', 'Relance de paiement n°2 : merci de régulariser votre facture liée à ce rendez-vous.',                      '2026-08-06 09:00:00', false, 'PAIEMENT'),
    ('NOTIF-7BBB9599', 'RDV-000009', 'M./Mme Ranaivo Sitraka : merci de compléter le règlement de votre paiement avant ou pendant votre rendez-vous du 28/07/2026 à 08:00.', '2026-07-29 08:00:00', false, 'PAIEMENT');


COMMIT;

SELECT 'fonction' AS table_name, COUNT(*) FROM fonction
UNION ALL SELECT 'personne', COUNT(*) FROM personne
UNION ALL SELECT 'patient', COUNT(*) FROM patient
UNION ALL SELECT 'medecin', COUNT(*) FROM medecin
UNION ALL SELECT 'dossier_medical', COUNT(*) FROM dossier_medical
UNION ALL SELECT 'rendez_vous', COUNT(*) FROM rendez_vous
UNION ALL SELECT 'consultation', COUNT(*) FROM consultation
UNION ALL SELECT 'ordonance', COUNT(*) FROM ordonance
UNION ALL SELECT 'paiement', COUNT(*) FROM paiement
UNION ALL SELECT 'notification', COUNT(*) FROM notification;
