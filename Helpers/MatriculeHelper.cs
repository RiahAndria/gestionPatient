namespace Patients.Models;
public static class MatriculeHelper
{
    private static int _compteurNumerique = 0;
    private static char _lettreCourante = 'A';

    public static string GenererMatricule(string genre, bool estAssure)
    {
        string prefixe = "P";
        
        // Code Genre : 01 Homme, 02 Femme, 00 Autre
        string codeGenre = genre == "Homme" ? "01" : (genre == "Femme" ? "02" : "00");
        
        // Code Assurance : 10 Assuré, 00 Non assuré
        string codeAssurance = estAssure ? "10" : "00";

        // Gestion du compteur alphanumérique (000A à 999Z)
        //  je compte trouver un moyen pour passer de 999Z à 000AA mlus tard mais bon... 
        // Pour le moment on vas en rester là
        string codeUnique = $"{_compteurNumerique:D3}{_lettreCourante}";

        // Incrémentation pour le prochain patient
        _compteurNumerique++;
        if (_compteurNumerique > 999)
        {
            _compteurNumerique = 0;
            _lettreCourante++;
            if (_lettreCourante > 'Z') _lettreCourante = 'A'; // Sécurité réinitialisation
        }

        return $"{prefixe}-{codeGenre}-{codeAssurance}-{codeUnique}";
    }
}