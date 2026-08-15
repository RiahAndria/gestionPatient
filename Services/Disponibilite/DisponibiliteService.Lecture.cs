using Npgsql;
using Dapper;
using Patients.Models;

namespace Patients.Services;

public partial class DisponibiliteService
{
    public async Task<AgendaJournee> ObtenirAgendaJourneeAsync(string id_medecin, DateTime date)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT 
                    id_temps, 
                    id_medecin, 
                    date_disponibilite::timestamp AS date_disponibilite,
                    numero_bloc,
                    heure_debut,
                    heure_fin,
                    est_disponible,
                    est_reserve 
                    FROM temps
                    WHERE id_medecin =:id_medecin 
                    AND date_disponibilite =:date
                    ORDER BY numero_bloc, heure_debut;
                ";
        try
        {
            var resultat = (await connection.QueryAsync<Temps>(sql, new {id_medecin, date.Date }));
            return new AgendaJournee {
                Date = date,
                Id_medecin = id_medecin,
                Creneaux15Min = resultat.ToList()            
            };

        } 
        catch (NpgsqlException e)
        {
            message = "message lors de l'appel de ObtenirAgendaJourneeAsync : " + e.Message;
            return new AgendaJournee();
        }
    }

    public async Task<List<AgendaJournee>> ObtenirAgendaUneSemaine(string id_medecin, DateTime date)
    {
        var AgendaSemaine = new List<AgendaJournee>();

        DateTime dateDebut = date.Date;
        for (int i = 0; i < 6; i++)
        {
            DateTime DateCourant = dateDebut.AddDays(i);
            var Agenda_journee = await ObtenirAgendaJourneeAsync(id_medecin, DateCourant);
            AgendaSemaine.Add(Agenda_journee);
            
        }
        return AgendaSemaine;
    }
}
