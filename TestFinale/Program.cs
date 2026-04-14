using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaGestioneAule
{
    // --- MODELLI DATI (Invariati con required) ---
    public class Aula
    {
        public required string Nome { get; set; }
        public int CapienzaMassima { get; set; }
    }

    public class Prenotazione
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string NomeStudente { get; set; }
        public required string CognomeStudente { get; set; }
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

        public void Prenota()
        {
            Console.WriteLine("\n--- Nuova Prenotazione ---");
            for (int i = 0; i < SistemaCentrale.Aule.Count; i++)
                Console.WriteLine($"{i + 1}. {SistemaCentrale.Aule[i].Nome} (Max: {SistemaCentrale.Aule[i].CapienzaMassima})");
            
            Console.Write("Scegli il numero dell'aula: ");
            if (!int.TryParse(Console.ReadLine(), out int scelta) || scelta < 1 || scelta > SistemaCentrale.Aule.Count) return;
            
            string aulaScelta = SistemaCentrale.Aule[scelta - 1].Nome;
            Console.Write("Inserisci ore (es. 09:00-11:00): ");
            string fascia = Console.ReadLine() ?? "";
            Console.Write("Quanti posti? ");
            int posti = int.Parse(Console.ReadLine() ?? "0");

            int disp = SistemaCentrale.GetPostiDisponibili(aulaScelta, DateTime.Now.AddDays(1), fascia);
            if (posti <= disp)
            {
                SistemaCentrale.TutteLePrenotazioni.Add(new Prenotazione {
                    NomeStudente = Nome, CognomeStudente = Cognome, MatricolaStudente = Matricola,
                    NomeAula = aulaScelta, Giorno = DateTime.Now.AddDays(1), FasciaOraria = fascia, PostiPrenotati = posti
                });
                Console.WriteLine("OK: Prenotazione effettuata!");
            }
            else Console.WriteLine($"ERRORE: Solo {disp} posti disponibili.");
        }

        public void VisualizzaEModifica()
        {
            var mie = SistemaCentrale.TutteLePrenotazioni.Where(p => p.MatricolaStudente == Matricola).ToList();
            if (mie.Count == 0) { Console.WriteLine("Nessuna prenotazione."); return; }

            for (int i = 0; i < mie.Count; i++) Console.WriteLine($"{i + 1}. {mie[i]}");
            
            Console.Write("\nVuoi (C)ancellarne una o (T)ornare indietro? ");
            string opz = Console.ReadLine()?.ToUpper() ?? "";
            if (opz == "C")
            {
                Console.Write("Quale numero? ");
                int idx = int.Parse(Console.ReadLine() ?? "0") - 1;
                SistemaCentrale.TutteLePrenotazioni.Remove(mie[idx]);
                Console.WriteLine("Cancellata.");
            }
        }
    }

    public class Amministratore : Account
    {
        public void MenuAdmin()
        {
            Console.WriteLine("\n--- Pannello Admin ---");
            Console.WriteLine("1. Report Globale\n2. Aggiungi Aula\n3. Modifica Capienza\n0. Logout");
            string scelta = Console.ReadLine() ?? "";
            switch (scelta)
            {
                case "1":
                    foreach (var p in SistemaCentrale.TutteLePrenotazioni) Console.WriteLine(p);
                    break;
                case "2":
                    Console.Write("Nome Aula: "); string n = Console.ReadLine() ?? "";
                    Console.Write("Capienza: "); int c = int.Parse(Console.ReadLine() ?? "0");
                    SistemaCentrale.Aule.Add(new Aula { Nome = n, CapienzaMassima = c });
                    break;
            }
        }
    }

    // --- PROGRAMMA ---
    class Program
    {
        static void Main()
        {
            // Setup Iniziale
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Lettura", CapienzaMassima = 15 });
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Scienze", CapienzaMassima = 20 });
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Informatica", CapienzaMassima = 25 });
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Matematica", CapienzaMassima = 10 });
            SistemaCentrale.Aule.Add(new Aula { Nome = "Aula di Storia", CapienzaMassima = 30 });
            
            List<Studente> studenti = new List<Studente> {
                new Studente { Nome = "Mario", Cognome = "Rossi", Matricola = "101" },
                new Studente { Nome = "Anna", Cognome = "Verdi", Matricola = "102" },
                new Studente { Nome = "Luca", Cognome = "Bianchi", Matricola = "103" },
                new Studente { Nome = "Sara", Cognome = "Neri", Matricola = "104" },
                new Studente { Nome = "Giulia", Cognome = "Gialli", Matricola = "105" }
            };
            Amministratore admin = new Amministratore { Nome = "Admin", Cognome = "Sistemi" };

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA PRENOTAZIONI UNIVERSITARIE ===");
                Console.WriteLine("Login come:");
                for (int i = 0; i < studenti.Count; i++) Console.WriteLine($"{i + 1}. Studente: {studenti[i].Nome} {studenti[i].Cognome}");
                Console.WriteLine("A. Amministratore");
                Console.WriteLine("0. Esci");
                
                Console.Write("\nScelta: ");
                string login = Console.ReadLine()?.ToUpper() ?? "";

                if (login == "0") break;
                if (login == "A")
                {
                    admin.MenuAdmin();
                }
                else if (int.TryParse(login, out int sIdx) && sIdx <= studenti.Count)
                {
                    Studente attuale = studenti[sIdx - 1];
                    bool inSessione = true;
                    while (inSessione)
                    {
                        Console.WriteLine($"\nBenvenuto {attuale.Nome}. 1. Prenota | 2. Mie Prenotazioni | 0. Logout");
                        string azione = Console.ReadLine() ?? "";
                        if (azione == "1") attuale.Prenota();
                        else if (azione == "2") attuale.VisualizzaEModifica();
                        else inSessione = false;
                    }
                }
            }
        }
    }
}