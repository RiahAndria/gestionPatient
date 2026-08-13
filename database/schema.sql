-- =====================================================================
-- gestion_patient_db — schéma complet (tables, séquences, contraintes)
-- =====================================================================
-- Extrait tel quel d'un export réel de la base (pg_dump --schema-only),
-- fourni par l'équipe le 10/08/2026. Ce fichier est la RÉFÉRENCE du
-- schéma pour toute l'équipe : en cas de doute sur une colonne, une
-- contrainte ou une clé étrangère, c'est ici qu'il faut regarder (pas
-- dans le code C#, qui peut se tromper ou être en retard sur la base réelle).
--
-- Pour recréer une base vide à partir de zéro :
--   createdb -U postgres gestion_patient_db
--   psql -U postgres -d gestion_patient_db -f schema.sql
-- =====================================================================

--
-- PostgreSQL database dump
--


-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: consultation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.consultation (
    numeroconsultation character varying(50) NOT NULL,
    diagnostique text NOT NULL,
    notesmedicales text NOT NULL,
    numerordv character varying(50) NOT NULL
);


ALTER TABLE public.consultation OWNER TO postgres;

--
-- Name: disponibilite; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.disponibilite (
    id_medecin character varying(50) NOT NULL,
    date_disponibilite date NOT NULL,
    numero_bloc integer NOT NULL
);


ALTER TABLE public.disponibilite OWNER TO postgres;

--
-- Name: dossier_medical; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.dossier_medical (
    numerodossier character varying(50) NOT NULL,
    poids numeric(5,2) NOT NULL,
    taille numeric(5,2) NOT NULL,
    groupesanguin character varying(5) NOT NULL,
    allergies text,
    antecedents text
);


ALTER TABLE public.dossier_medical OWNER TO postgres;

--
-- Name: fonction; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.fonction (
    code_fonction integer NOT NULL,
    nom_fonction character varying(100) NOT NULL
);


ALTER TABLE public.fonction OWNER TO postgres;

--
-- Name: fonction_code_fonction_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.fonction ALTER COLUMN code_fonction ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.fonction_code_fonction_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: medecin; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.medecin (
    id_medecin character varying(50) NOT NULL,
    numero_ordre character varying(50) NOT NULL,
    statut character varying(50) NOT NULL,
    code_fonction integer NOT NULL,
    taux_horaire numeric(8,2) NOT NULL
);


ALTER TABLE public.medecin OWNER TO postgres;

--
-- Name: notification; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notification (
    numeronotif character varying(50) NOT NULL,
    numerordv character varying(50) NOT NULL,
    textenotif text NOT NULL,
    datenotif timestamp without time zone DEFAULT now() NOT NULL,
    lu boolean DEFAULT false NOT NULL,
    type_notif character varying(20) DEFAULT 'RESERVATION'::character varying NOT NULL
);


ALTER TABLE public.notification OWNER TO postgres;

--
-- Name: ordonance; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ordonance (
    numeroprescritption character varying(50) NOT NULL,
    numeroconsultation character varying(50) NOT NULL,
    traitement text NOT NULL,
    duree character varying(50) NOT NULL,
    diagnostique text NOT NULL
);


ALTER TABLE public.ordonance OWNER TO postgres;

--
-- Name: paiement; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.paiement (
    numeropaiement character varying(50) NOT NULL,
    numeroconsultation character varying(50),
    datepaiement timestamp without time zone NOT NULL,
    montant numeric(10,2) NOT NULL,
    modepaiement character varying(50) NOT NULL,
    statut boolean DEFAULT false NOT NULL,
    numerordv character varying(50) NOT NULL,
    typepaiement character varying(20) NOT NULL,
    est_facture boolean DEFAULT false NOT NULL,
    CONSTRAINT chk_paiement_type_coherent CHECK (((((typepaiement)::text = 'NORMAL'::text) AND (numeroconsultation IS NOT NULL)) OR (((typepaiement)::text = 'ACOMPTE'::text) AND (numeroconsultation IS NULL))))
);


ALTER TABLE public.paiement OWNER TO postgres;

--
-- Name: patient; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.patient (
    id character varying(50) NOT NULL,
    numerodossier character varying(50) NOT NULL,
    id_her_2 character varying(50),
    numeroassurance character varying(50),
    nom character varying(100) NOT NULL,
    prenom character varying(100) NOT NULL,
    datedenaissance date NOT NULL,
    genre character varying(20) NOT NULL,
    adresse character varying(250) NOT NULL,
    telephone character varying(20) NOT NULL,
    mail character varying(150) NOT NULL
);


ALTER TABLE public.patient OWNER TO postgres;

--
-- Name: personne; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.personne (
    id character varying(50) NOT NULL,
    nom character varying(100) NOT NULL,
    prenom character varying(100) NOT NULL,
    datedenaissance date NOT NULL,
    genre character varying(20) NOT NULL,
    adresse character varying(250) NOT NULL,
    telephone character varying(20) NOT NULL,
    mail character varying(150) NOT NULL
);


ALTER TABLE public.personne OWNER TO postgres;

--
-- Name: rendez_vous; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rendez_vous (
    numerordv character varying(50) NOT NULL,
    id character varying(50) NOT NULL,
    id_her_2 character varying(50) NOT NULL,
    dateheurerdv timestamp without time zone NOT NULL,
    motifrdv text NOT NULL,
    statut character varying(20) DEFAULT 'PLANIFIE'::character varying NOT NULL,
    motifannulation text
);


ALTER TABLE public.rendez_vous OWNER TO postgres;

--
-- Name: temps; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.temps (
    id_temps integer NOT NULL,
    id_medecin character varying(50) NOT NULL,
    date_disponibilite date NOT NULL,
    numero_bloc integer NOT NULL,
    heure_debut timestamp without time zone NOT NULL,
    heure_fin timestamp without time zone NOT NULL,
    est_disponible boolean DEFAULT true NOT NULL,
    est_reserve boolean DEFAULT false NOT NULL
);


ALTER TABLE public.temps OWNER TO postgres;

--
-- Name: temps_id_temps_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.temps ALTER COLUMN id_temps ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.temps_id_temps_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

-- =====================================================================
-- Contraintes (clés primaires, uniques, clés étrangères)
-- =====================================================================



--
-- Name: fonction fonction_nom_fonction_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.fonction
    ADD CONSTRAINT fonction_nom_fonction_key UNIQUE (nom_fonction);


--
-- Name: fonction fonction_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.fonction
    ADD CONSTRAINT fonction_pkey PRIMARY KEY (code_fonction);


--
-- Name: medecin medecin_numero_ordre_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medecin
    ADD CONSTRAINT medecin_numero_ordre_key UNIQUE (numero_ordre);


--
-- Name: medecin medecin_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medecin
    ADD CONSTRAINT medecin_pkey PRIMARY KEY (id_medecin);


--
-- Name: consultation pk_consultation; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation
    ADD CONSTRAINT pk_consultation PRIMARY KEY (numeroconsultation);


--
-- Name: disponibilite pk_disponibilite; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.disponibilite
    ADD CONSTRAINT pk_disponibilite PRIMARY KEY (id_medecin, date_disponibilite, numero_bloc);


--
-- Name: dossier_medical pk_dossier_medical; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.dossier_medical
    ADD CONSTRAINT pk_dossier_medical PRIMARY KEY (numerodossier);


--
-- Name: notification pk_notification; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notification
    ADD CONSTRAINT pk_notification PRIMARY KEY (numeronotif);


--
-- Name: ordonance pk_ordonance; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ordonance
    ADD CONSTRAINT pk_ordonance PRIMARY KEY (numeroprescritption);


--
-- Name: paiement pk_paiement; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paiement
    ADD CONSTRAINT pk_paiement PRIMARY KEY (numeropaiement);


--
-- Name: patient pk_patient; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.patient
    ADD CONSTRAINT pk_patient PRIMARY KEY (id);


--
-- Name: personne pk_personne; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.personne
    ADD CONSTRAINT pk_personne PRIMARY KEY (id);


--
-- Name: rendez_vous pk_rendez_vous; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rendez_vous
    ADD CONSTRAINT pk_rendez_vous PRIMARY KEY (numerordv);


--
-- Name: temps temps_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.temps
    ADD CONSTRAINT temps_pkey PRIMARY KEY (id_temps);


--
-- Name: consultation uq_consultation_numerordv; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation
    ADD CONSTRAINT uq_consultation_numerordv UNIQUE (numerordv);


--
-- Name: consultation fk_consultation_rendez_vous; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.consultation
    ADD CONSTRAINT fk_consultation_rendez_vous FOREIGN KEY (numerordv) REFERENCES public.rendez_vous(numerordv);


--
-- Name: disponibilite fk_disponibilite_medecin; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.disponibilite
    ADD CONSTRAINT fk_disponibilite_medecin FOREIGN KEY (id_medecin) REFERENCES public.medecin(id_medecin) ON DELETE CASCADE;


--
-- Name: medecin fk_medecin_fonction; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medecin
    ADD CONSTRAINT fk_medecin_fonction FOREIGN KEY (code_fonction) REFERENCES public.fonction(code_fonction);


--
-- Name: medecin fk_medecin_personne; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.medecin
    ADD CONSTRAINT fk_medecin_personne FOREIGN KEY (id_medecin) REFERENCES public.personne(id) ON DELETE CASCADE;


--
-- Name: notification fk_notification_rendez_vous; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notification
    ADD CONSTRAINT fk_notification_rendez_vous FOREIGN KEY (numerordv) REFERENCES public.rendez_vous(numerordv);


--
-- Name: ordonance fk_ordonance_consultation; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ordonance
    ADD CONSTRAINT fk_ordonance_consultation FOREIGN KEY (numeroconsultation) REFERENCES public.consultation(numeroconsultation);


--
-- Name: paiement fk_paiement_consultation; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paiement
    ADD CONSTRAINT fk_paiement_consultation FOREIGN KEY (numeroconsultation) REFERENCES public.consultation(numeroconsultation);


--
-- Name: paiement fk_paiement_rendez_vous; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paiement
    ADD CONSTRAINT fk_paiement_rendez_vous FOREIGN KEY (numerordv) REFERENCES public.rendez_vous(numerordv);


--
-- Name: patient fk_patient_dossier_medical; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.patient
    ADD CONSTRAINT fk_patient_dossier_medical FOREIGN KEY (numerodossier) REFERENCES public.dossier_medical(numerodossier);


--
-- Name: patient fk_patient_medecin; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.patient
    ADD CONSTRAINT fk_patient_medecin FOREIGN KEY (id_her_2) REFERENCES public.medecin(id_medecin);


--
-- Name: patient fk_patient_personne; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.patient
    ADD CONSTRAINT fk_patient_personne FOREIGN KEY (id) REFERENCES public.personne(id);


--
-- Name: rendez_vous fk_rendez_vous_medecin; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rendez_vous
    ADD CONSTRAINT fk_rendez_vous_medecin FOREIGN KEY (id_her_2) REFERENCES public.medecin(id_medecin);


--
-- Name: rendez_vous fk_rendez_vous_patient; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rendez_vous
    ADD CONSTRAINT fk_rendez_vous_patient FOREIGN KEY (id) REFERENCES public.patient(id);


--
-- Name: temps fk_temps_disponibilite; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.temps
    ADD CONSTRAINT fk_temps_disponibilite FOREIGN KEY (id_medecin, date_disponibilite, numero_bloc) REFERENCES public.disponibilite(id_medecin, date_disponibilite, numero_bloc) ON DELETE CASCADE;

