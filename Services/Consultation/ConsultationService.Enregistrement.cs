using System;
using Npgsql;
using Patients.Models;

namespace Patients.Services
{
    public partial class ConsultationService
    {
        public ResultatEnregistrementConsultation EnregistrerConsultation(Consultation consultation, Ordonnance? ordonnance)
        {
            // Vérifier si une consultation existe déjà pour ce rendez-vous
            var consultationExistante = ObtenirParNumeroRendezVous(consultation.NumeroRdv);
            if (consultationExistante != null)
            {
                return new ResultatEnregistrementConsultation
                {
                    Succes = false,
                    MessageErreur = $"Une consultation existe déjà pour ce rendez-vous (N° {consultationExistante.NumeroConsultation}). Veuillez sélectionner un autre rendez-vous."
                };
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                InsererConsultation(consultation, conn, transaction);

                if (ordonnance != null)
                {
                    InsererOrdonnance(consultation, ordonnance, conn, transaction);
                }

                if (!string.IsNullOrWhiteSpace(consultation.NumeroDossier))
                {
                    MettreAJourDossierMedical(consultation, conn, transaction);
                }

                // Le rendez-vous a bien eu lieu : il ne doit plus rester
                // "Planifie" (sinon il continuerait a apparaitre comme
                // disponible pour une nouvelle consultation ou un acompte).
                const string sqlTerminer = "UPDATE RENDEZ_VOUS SET STATUT = 'TERMINE' WHERE NUMERORDV = @numRdv;";
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
                Console.WriteLine($"[Erreur ADO.NET] : {ex.Message}");
                return new ResultatEnregistrementConsultation
                {
                    Succes = false,
                    MessageErreur = ex.Message
                };
            }

            // La consultation est bien enregistree a ce stade. La creation
            // de la facture est une etape distincte : si elle echoue (ex:
            // probleme reseau passager), la consultation reste valide -
            // on le signale juste clairement plutot que de faire echouer
            // tout le reste retroactivement.
            try
            {
                decimal montant = _paiementService.CalculerMontantSuggere(consultation.NumeroConsultation);
                var resultatFacture = _paiementService.CreerPaiementDu(consultation.NumeroConsultation, montant);

                return new ResultatEnregistrementConsultation
                {
                    Succes = true,
                    FactureCreee = resultatFacture.FactureCreee,
                    MontantFacture = resultatFacture.Montant,
                    MessageFacture = resultatFacture.Message
                };
            }
            catch (Exception ex)
            {
                return new ResultatEnregistrementConsultation
                {
                    Succes = true,
                    FactureCreee = false,
                    MessageFacture = $"La facture n'a pas pu être créée automatiquement : {ex.Message}"
                };
            }
        }

        private static void InsererConsultation(Consultation consultation, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            const string sqlConsultation = @"
                INSERT INTO CONSULTATION (NUMEROCONSULTATION, NUMERORDV, DIAGNOSTIQUE, NOTESMEDICALES)
                VALUES (@numCons, @numRdv, @diag, @notes);";

            using var cmd = new NpgsqlCommand(sqlConsultation, conn, transaction);
            cmd.Parameters.AddWithValue("@numCons", consultation.NumeroConsultation);
            cmd.Parameters.AddWithValue("@numRdv", consultation.NumeroRdv);
            cmd.Parameters.AddWithValue("@diag", consultation.Diagnostique);
            cmd.Parameters.AddWithValue("@notes", consultation.NotesMedicales);
            cmd.ExecuteNonQuery();
        }

        private static void InsererOrdonnance(Consultation consultation, Ordonnance ordonnance, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            // NUMEROPRESCRITPTION (avec le "T" en trop) : c'est la faute
            // de frappe historique du schema d'origine, pas une erreur -
            // c'est le nom reel de la colonne en base.
            const string sqlOrdonnance = @"
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
    }
}
