using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    // Resultat detaille de l'enregistrement d'une consultation : au-dela
    // du simple succes/echec, indique si la facture a bien ete generee,
    // pour que l'ecran puisse informer clairement l'utilisateur (plutot
    // qu'un simple booleen qui masquerait un probleme partiel).
    public class ResultatEnregistrementConsultation
    {
        public bool Succes { get; set; }
        public string? MessageErreur { get; set; }
        public bool FactureCreee { get; set; }
        public decimal MontantFacture { get; set; }
    }

    public class ConsultationService
    {
        private readonly string _connectionString;
        private readonly PaiementService _paiementService = new();

        public ConsultationService()
        {
            // Meme mecanisme que le reste de l'equipe (PatientService,
            // RendezVousService, PaiementService...) : lecture de la
            // chaine de connexion depuis appsettings.json, pas de valeur
            // codee en dur qui ne fonctionnerait que sur un seul poste.
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Enregistre une consultation (+ ordonnance facultative) rattachee
        // a un rendez-vous, puis :
        //   1. passe ce rendez-vous au statut TERMINE (il a bien eu lieu) ;
        //   2. genere automatiquement la facture correspondante, en
        //      deduisant les eventuels acomptes deja verses sur ce
        //      rendez-vous (voir PaiementService.CreerPaiementDu).
        public ResultatEnregistrementConsultation EnregistrerConsultation(Consultation consultation, Ordonnance? ordonnance)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlConsultation = @"
                    INSERT INTO CONSULTATION (NUMEROCONSULTATION, NUMERORDV, DIAGNOSTIQUE, NOTESMEDICALES)
                    VALUES (@numCons, @numRdv, @diag, @notes);";

                using (var cmd = new NpgsqlCommand(sqlConsultation, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
                    cmd.Parameters.AddWithValue("@numRdv", consultation.NumeroRdv);
                    cmd.Parameters.AddWithValue("@diag", consultation.Diagnostique);
                    cmd.Parameters.AddWithValue("@notes", consultation.NotesMedicales);
                    cmd.ExecuteNonQuery();
                }

                if (ordonnance != null)
                {
                    // NUMEROPRESCRITPTION (avec le "T" en trop) : c'est la
                    // faute de frappe historique du schema d'origine, pas
                    // une erreur ici - le nom de colonne reel en base.
                    string sqlOrdonnance = @"
                        INSERT INTO ORDONANCE (NUMEROPRESCRITPTION, NUMEROCONSULTATION, TRAITEMENT, DUREE, DIAGNOSTIQUE)
                        VALUES (@numPresc, @numCons, @traitement, @duree, @diag);";

                    using var cmdOrd = new NpgsqlCommand(sqlOrdonnance, conn, transaction);
                    cmdOrd.Parameters.AddWithValue("@numPresc", ordonnance.NumeroPrescritption);
                    cmdOrd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
                    cmdOrd.Parameters.AddWithValue("@traitement", ordonnance.Traitement);
                    cmdOrd.Parameters.AddWithValue("@duree", ordonnance.Duree);
                    cmdOrd.Parameters.AddWithValue("@diag", ordonnance.Diagnostique);
                    cmdOrd.ExecuteNonQuery();
                }

                // Le rendez-vous a bien eu lieu : il ne doit plus rester
                // "Planifie" (sinon il continuerait a apparaitre comme
                // disponible pour une nouvelle consultation ou un acompte).
                string sqlTerminer = "UPDATE RENDEZ_VOUS SET STATUT = 'TERMINE' WHERE NUMERORDV = @numRdv;";
                using (var cmdTerminer = new NpgsqlCommand(sqlTerminer, conn, transaction))
                {
                    cmdTerminer.Parameters.AddWithValue("@numRdv", consultation.NumeroRdv);
                    cmdTerminer.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ResultatEnregistrementConsultation
                {
                    Succes = false,
                    MessageErreur = ex.Message
                };
            }

            // La consultation est bien enregistree a ce stade. La creation
            // de la facture est une etape distincte, dans son propre essai/
            // erreur : si elle echoue (ex: probleme reseau passager), la
            // consultation reste valide - on le signale juste clairement
            // a l'utilisateur plutot que de faire echouer tout le reste.
            try
            {
                decimal montant = _paiementService.CalculerMontantSuggere(consultation.NumeroConsultation);
                _paiementService.CreerPaiementDu(consultation.NumeroConsultation, montant);

                return new ResultatEnregistrementConsultation
                {
                    Succes = true,
                    FactureCreee = true,
                    MontantFacture = montant
                };
            }
            catch (Exception ex)
            {
                return new ResultatEnregistrementConsultation
                {
                    Succes = true,
                    FactureCreee = false,
                    MessageErreur = $"Consultation enregistrée, mais la facture n'a pas pu être créée automatiquement : {ex.Message}"
                };
            }
        }

        public Consultation? ObtenirParNumero(string numeroConsultation)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string query = @"
                SELECT c.NUMEROCONSULTATION, c.NUMERORDV, c.DIAGNOSTIQUE, c.NOTESMEDICALES,
                       o.NUMEROPRESCRITPTION, o.TRAITEMENT, o.DUREE
                FROM CONSULTATION c
                LEFT JOIN ORDONANCE o ON c.NUMEROCONSULTATION = o.NUMEROCONSULTATION
                WHERE c.NUMEROCONSULTATION = @id;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", numeroConsultation);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var consultation = new Consultation
            {
                NumeroConsultation = reader.GetString(0),
                NumeroRdv = reader.GetString(1),
                Diagnostique = reader.GetString(2),
                NotesMedicales = reader.GetString(3)
            };

            if (!reader.IsDBNull(4))
            {
                consultation.OrdonnanceAssociee = new Ordonnance
                {
                    NumeroPrescritption = reader.GetString(4),
                    NumeroConsultation = consultation.NumeroConsultation,
                    Traitement = reader.GetString(5),
                    Duree = reader.GetString(6),
                    Diagnostique = consultation.Diagnostique
                };
            }

            return consultation;
        }
    }
}
