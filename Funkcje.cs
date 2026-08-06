static class Funkcje
{
    public static void WyswietlListe(List<Item> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            {
                Console.WriteLine($"{i + 1}. {lista[i].Name} | {lista[i].Quantity} szt. | {lista[i].Price} zł");
            }
        }
    }
    public static decimal WartoscMagazynu(List<Item> lista)
    {
        decimal wartosc = 0;
        foreach (Item przedmiot in lista)
        {
            wartosc += przedmiot.Quantity * przedmiot.Price;
        }
        return wartosc;
    }
}