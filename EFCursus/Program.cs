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
                foreach (var campus in entities.CampussenVanTotPostCode("8000","8999"))
                {
                    Console.WriteLine("{0}: {1}", campus.Naam, campus.PostCode);
                }
            }

            using (var entities = new OpleidingenEntities())
            {
                foreach (var voornaamAantal in entities.AantalDocentenPerVoornaam())
                {
                    Console.WriteLine("{0} {1}", voornaamAantal.Voornaam, voornaamAantal.Aantal);
                }
            }

            Console.Write("Opslagpercentage:");
            decimal percentage;
            if (decimal.TryParse(Console.ReadLine(), out percentage))
            {
                using (var entities = new OpleidingenEntities())
                {
                    var aantalDocentenAangepast = entities.WeddeVerhoging(percentage);
                    Console.WriteLine("{0} docenten aangepast", aantalDocentenAangepast);
                }
            }
            else
            {
                Console.WriteLine("Tik een getal");
            }

            Console.Write("Familienaam:");
            var familienaam = Console.ReadLine();
            using (var entities = new OpleidingenEntities())
            {
                var aantalDocenten = entities.AantalDocentenMetFamilienaam(familienaam);
                Console.WriteLine("{0} docent(en)", aantalDocenten.First());
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
