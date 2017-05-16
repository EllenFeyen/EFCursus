using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.Entity.Infrastructure;

namespace EFCursus
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var entities = new OpleidingenEntities())
            {
                var query = from bestBetaaldeDocentPerCampus
                            in entities.BestBetaaldeDocentenPerCampus
                            orderby bestBetaaldeDocentPerCampus.CampusNr, 
                            bestBetaaldeDocentPerCampus.Voornaam, 
                            bestBetaaldeDocentPerCampus.Familienaam
                            select bestBetaaldeDocentPerCampus;
                var vorigCampusNr = 0;
                foreach (var bestbetaaldeDocentPerCampus in query)
                {
                    if (bestbetaaldeDocentPerCampus.CampusNr != vorigCampusNr)
                    {
                        Console.WriteLine("{0} Grootste wedde: {1}",
                            bestbetaaldeDocentPerCampus.Naam, bestbetaaldeDocentPerCampus.GrootsteWedde);
                        vorigCampusNr = bestbetaaldeDocentPerCampus.CampusNr;
                    }
                    Console.WriteLine("\t{0} {1}", bestbetaaldeDocentPerCampus.Voornaam,
                        bestbetaaldeDocentPerCampus.Familienaam);
                }
            }

            Console.WriteLine("Druk enter om af te sluiten");
            Console.Read();
        }

        static void VoorraadBijvulling(int artikelNr, int magazijnNr, int aantalStuks)
        {
            using (var entities = new OpleidingenEntities())
            {
                var voorraad = entities.Voorraden.Find(magazijnNr, artikelNr);
                if (voorraad != null)
                {
                    voorraad.AantalStuks += aantalStuks;
                    Console.WriteLine("Pas nu de voorraad aan met de Server Explorer, druk daarna op Enter");
                    Console.ReadLine();
                    try
                    {
                        entities.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        Console.WriteLine("Voorraad werd door andere applicatie aangepast.");
                    }
                }
                else
                {
                    Console.WriteLine("Voorraad niet gevonden");
                }
            }
        }

        static void VoorraadTransfer(int artikelNr, int vanMagazijnNr, int naarMagazijnNr, int aantalStuks)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.RepeatableRead
            };
            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
            {
                using (var entities = new OpleidingenEntities())
                {
                    var vanVoorraad = entities.Voorraden.Find(vanMagazijnNr, artikelNr);
                    if (vanVoorraad != null)
                    {
                        if (vanVoorraad.AantalStuks >= aantalStuks)
                        {
                            vanVoorraad.AantalStuks -= aantalStuks;
                            var naarVoorraad = entities.Voorraden.Find(naarMagazijnNr, artikelNr);
                            if (naarVoorraad != null)
                            {
                                naarVoorraad.AantalStuks += aantalStuks;
                            }
                            else
                            {
                                naarVoorraad = new Voorraad
                                { ArtikelNr = artikelNr, MagazijnNr = naarMagazijnNr, AantalStuks = aantalStuks };
                                entities.Voorraden.Add(naarVoorraad);
                            }
                            entities.SaveChanges();
                            transactionScope.Complete();
                        }
                        else
                        {
                            Console.WriteLine("Te weinig voorraad voor transfer");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Artikel niet gevonden in magazijn {0}", vanMagazijnNr);
                    }
                }
            }
        }

        static List<Campus> FindAllCampussen()
        {
            using (var entities = new OpleidingenEntities())
            {
                return (from campus in entities.Campussen
                        orderby campus.Naam
                        select campus).ToList();
            }
        }
    }
}
