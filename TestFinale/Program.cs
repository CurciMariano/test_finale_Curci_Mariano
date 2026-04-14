using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaGestioneAule
{
    // --- MODELLI DATI ---
    public class Aula
    {
        public required string Nome { get; set; }
        public int CapienzaMassima { get; set; }
    }

    public class Prenotazione
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string MatricolaStudente { get; set; }
        public required string NomeAula { get; set; }
        public DateTime Giorno { get; set; }
        public required string FasciaOraria { get; set; }
        public int PostiPrenotati { get; set; }

        public override string ToString() =>
            $"[ID: {Id.ToString().Substring(0, 8)}] {NomeAula} | Data: {Giorno.ToShortDateString()} | Ore: {FasciaOraria} | Posti: {PostiPrenotati}";
    }

    public static class SistemaCentrale
    {
        public static List<Aula> Aule = new List<Aula>();
        public static List<Prenotazione> TutteLePrenotazioni = new List<Prenotazione>();
        public static List<Studente> DatabaseStudenti = new List<Studente>();

        public static int GetPostiDisponibili(string nomeAula, DateTime giorno, string fascia)
        {
            var aula = Aule.FirstOrDefault(a => a.Nome == nomeAula);
            if (aula == null) return 0;
            int occupati = TutteLePrenotazioni
                .Where(p => p.NomeAula == nomeAula && p.Giorno.Date == giorno.Date && p.FasciaOraria == fascia)
                .Sum(p => p.PostiPrenotati);
            return aula.CapienzaMassima - occupati;
        }
    }

    // --- ATTORI ---
    public abstract class Account
    {
        public required string Nome { get; set; }
        public required string Cognome { get; set; }
    }

    public class Studente : Account
    {
        public required string Matricola { get; set; }

        public void MenuStudente()
        {
            bool inSessione = true;
            while (inSessione)
            {
                Console.WriteLine($"\n--- MENU STUDENTE: {Nome} {Cognome} ({Matricola}) ---");
                Console.WriteLine("1. Prenota Aula\n2. Visualizza/Cancella mie prenotazioni\n0. Logout");
                Console.Write("Scelta: ");
                switch (Console.ReadLine())
                {
                    case "1": EseguiPrenotazione(); break;
                    case "2": GestisciPrenotazioni(); break;
                    case "0": inSessione = false; break;
                }
            }
        }

        private void EseguiPrenotazione()
        {
            Console.WriteLine("\nAule disponibili:");
            for (int i = 0; i < SistemaCentrale.Aule.Count; i++)
                Console.WriteLine($"{i + 1}. {SistemaCentrale.Aule[i].Nome} (Capienza: {SistemaCentrale.Aule[i].CapienzaMassima})");
            
            Console.Write("Scegli numero aula: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > SistemaCentrale.Aule.Count) return;

            string aula = SistemaCentrale.Aule[idx - 1].Nome;
            Console.Write("Inserisci fascia oraria (es. 10-12): ");
            string fascia = Console.ReadLine() ?? "";
            Console.Write("Posti necessari: ");
            int posti = int.Parse(Console.ReadLine() ?? "0");

            int disponibili = SistemaCentrale.GetPostiDisponibili(aula, DateTime.Now.AddDays(1), fascia);
            if (posti <= disponibili)
            {
                SistemaCentrale.TutteLePrenotazioni.Add(new Prenotazione {
                    MatricolaStudente = Matricola, NomeAula = aula, Giorno = DateTime.Now.AddDays(1),
                    FasciaOraria = fascia, PostiPrenotati = posti
                });
                Console.WriteLine("OK: Prenotazione salvata.");
            }
            else Console.WriteLine($"ERRORE: Disponibili solo {disponibili} posti.");
        }

        private void GestisciPrenotazioni()
        {
            var mie = SistemaCentrale.TutteLePrenotazioni.Where(p => p.MatricolaStudente == Matricola).ToList();
            if (!mie.Any()) { Console.WriteLine("Nessuna prenotazione trovata."); return; }
            
            for (int i = 0; i < mie.Count; i++) Console.WriteLine($"{i + 1}. {mie[i]}");
            Console.Write("\nDigita numero per cancellare o Invio per tornare: ");
            if (int.TryParse(Console.ReadLine(), out int delIdx) && delIdx > 0 && delIdx <= mie.Count)
            {
                SistemaCentrale.TutteLePrenotazioni.Remove(mie[delIdx - 1]);
                Console.WriteLine("Cancellata.");
            }
        }
    }

    public class Amministratore : Account
    {
        public void MenuAdmin()
        {
            bool inAdmin = true;
            while (inAdmin)
            {
                Console.WriteLine("\n--- PANNELLO AMMINISTRATORE ---");
                Console.WriteLine("1. Lista Completa Alunni\n2. Report Prenotazioni\n3. Aggiungi/Modifica Aule\n0. Logout");
                Console.Write("Scelta: ");
                switch (Console.ReadLine())
                {
                    case "1":
                        Console.WriteLine("\nELENCO ISCRITTI:");
                        foreach (var s in SistemaCentrale.DatabaseStudenti)
                            Console.WriteLine($"- {s.Nome} {s.Cognome} (Matr: {s.Matricola})");
                        break;
                    case "2":
                        foreach (var p in SistemaCentrale.TutteLePrenotazioni) Console.WriteLine(p);
                        break;
                    case "0": inAdmin = false; break;
                }
            }
        }
    }

    // --- PROGRAMMA ---
    class Program
    {
        static void Main()
        {
            SetupIniziale();
            Amministratore admin = new Amministratore { Nome = "Responsabile", Cognome = "Sistemi" };

            while (true)
            {
                Console.WriteLine("\n=== SISTEMA DI GESTIONE UNIVERSITARIA ===");
                Console.WriteLine("1. Accesso Alunni");
                Console.WriteLine("A. Amministratore");
                Console.WriteLine("0. Esci");
                Console.Write("Scelta: ");
                string scelta = Console.ReadLine()?.ToUpper() ?? "";

                if (scelta == "0") break;

                if (scelta == "1")
                {
                    MenuRicercaStudente();
                }
                else if (scelta == "A")
                {
                    admin.MenuAdmin();
                }
            }
        }

        static void MenuRicercaStudente()
        {
            Console.Write("\nCerca alunno (Nome, Cognome o Matricola): ");
            string query = Console.ReadLine()?.ToLower() ?? "";

            var risultati = SistemaCentrale.DatabaseStudenti.Where(s =>
                s.Nome.ToLower().Contains(query) ||
                s.Cognome.ToLower().Contains(query) ||
                s.Matricola.Contains(query)).ToList();

            if (risultati.Count == 0)
            {
                Console.WriteLine("Nessun alunno trovato.");
            }
            else if (risultati.Count == 1)
            {
                risultati[0].MenuStudente();
            }
            else
            {
                Console.WriteLine("Trovati più alunni. Sii più specifico:");
                foreach (var r in risultati) Console.WriteLine($"- {r.Nome} {r.Cognome} ({r.Matricola})");
            }
        }

        static void SetupIniziale()
        {
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Lettura", CapienzaMassima = 10 });
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Scienze", CapienzaMassima = 20 });
            
            SistemaCentrale.DatabaseStudenti.Add(new Studente { Nome = "Mario", Cognome = "Rossi", Matricola = "1001" });
            SistemaCentrale.DatabaseStudenti.Add(new Studente { Nome = "Anna", Cognome = "Verdi", Matricola = "1002" });
            SistemaCentrale.DatabaseStudenti.Add(new Studente { Nome = "Luca", Cognome = "Bianchi", Matricola = "1003" });
            SistemaCentrale.DatabaseStudenti.Add(new Studente { Nome = "Giulia", Cognome = "Neri", Matricola = "1004" });
            SistemaCentrale.DatabaseStudenti.Add(new Studente { Nome = "Paolo", Cognome = "Gialli", Matricola = "1005" });
        }
    }
}