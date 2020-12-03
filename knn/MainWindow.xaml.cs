using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace knn
{
    public delegate double Metryka(double[] x, double[] y, double p);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double p=0;
        public int iloscKolumn = 0;
        public int iloscWierszy = 0;
        public double[,] probka;
        public double[,] probkaSave;
        public double[] nowy;
        public int pozostale;
        public double k;
        public bool pobrane=false;
        public bool dane=false;
        public bool normal = false;
        public double[] max;
        public double[] min;
        public int kminV;
        bool danenormal=false;
        bool pexist = true;
        public MainWindow()
        {
            InitializeComponent();
            test.Text = "JAK ZACZAC?\n 1. Wybierz plik (przed wybraniem pliku ustal separator dla danych w pliku) \n 2.Wybierz metryke i podaj dane (od momentu dodania pliku mozesz rowniez go znormalizowac) \n 3. Mozesz przeprowadzic walidacje (czysci tekst tutaj) \n 4. Mozesz oszacowac wynik dla swoich danych \n 5.Twoje wyniki nie znikna dopoki nie klikniesz waliduj! \n 6. Baw sie dobrze!";
            cb1.Items.Add(new KeyValuePair<string, object>("Tabulator", "\t"));
            cb1.Items.Add(new KeyValuePair<string, object>("Spacja", " "));
            cb2.Items.Add(new KeyValuePair<string, int>("Euklidesowa", 1));
            cb2.Items.Add(new KeyValuePair<string, int>("Manhattan", 2));
            cb2.Items.Add(new KeyValuePair<string, int>("Czebyszewa", 3));
            cb2.Items.Add(new KeyValuePair<string, int>("Z logarytmem", 4));
            cb2.Items.Add(new KeyValuePair<string, int>("Minkowskiego", 5));
            cb1.SelectedIndex = 0;
            cb2.SelectedIndex = 0;
        }

        public void read(OpenFileDialog plik, string separator="\t")
        {
            if (pobrane == true)
            {
                Array.Clear(nowy, 0, nowy.Length);
                pobrane = false;
                normal = false;
            }
            string zab = "5.5";
            double wynik;
            bool przecinek = double.TryParse(zab, out wynik);           
            var linie = File.ReadAllLines(plik.FileName);
            var linia2 = linie[0].Trim();
            var liczby2 = linia2.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            iloscWierszy = linie.Length;
            iloscKolumn = liczby2.Length;
            probka = new double[iloscWierszy, iloscKolumn];
            for (int i = 0; i < linie.Length; i++)
            {
                var linia = linie[i].Trim();
                linia = przecinek ? linia.Replace(",", ".") : linia.Replace(".", ",");
                var liczby = linia.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < liczby.Length; j++)
                {
                    probka[i, j] = double.Parse(liczby[j].Trim());
                }
            }
            dane = true;
        }
        Dictionary<double, List<double>> oblicz(int iloscWierszy, int iloscKolumn, double[,] probka, double[] nowy, Dictionary<double, List<double>> odleglosc, double ? at = null)
        {
            double[] nowy2 = new double[iloscKolumn];
                for (int i = 0; i < iloscWierszy; i++)
            {
                if (at == i)
                    continue;
               
                if (!odleglosc.ContainsKey(probka[i,iloscKolumn - 1]))
                    odleglosc.Add(probka[i,iloscKolumn-1], new List<double>());
                int b = 0;
                for (int j = 0; j < iloscKolumn; j++)
                {
                    nowy2[b++] = probka[i, j];
                }
                Metryka m;
                var metr = cb2.SelectedItem as KeyValuePair<string, int>?;
                int wyb= metr.Value.Value;
                m = Metryki.Euklidesowa;
                if (wyb == 1)
                    m = Metryki.Euklidesowa;
                else if (wyb == 2)
                    m = Metryki.Manhatan;
                else if (wyb == 3)
                    m = Metryki.Czebyszewa;
                else if (wyb == 4)
                    m = Metryki.zLogarytmem;
                else if (wyb == 5)
                {
                    if (p == 0)
                    {
                        MessageBox.Show("Nie wybrales p, nie mozesz skorzystac z tej metryki!");
                        return null;
                    }
                    m = Metryki.Minkowskiego;
                }
                double wynik = m(nowy, nowy2, p);              
                odleglosc[nowy2[nowy2.Length - 1]].Add(wynik);
            }
            return odleglosc;

        }
        public int kLicz(int iloscWierszy, int iloscKolumn, double[,] probka )
        {
            Dictionary<double, int> kIlosc = new Dictionary<double, int>();
            for (int i = 0; i < iloscWierszy; i++)
            {
                if (!kIlosc.ContainsKey(probka[i, iloscKolumn - 1]))
                    kIlosc.Add(probka[i, iloscKolumn - 1], 0);            
                kIlosc[probka[i,iloscKolumn-1]]+=1;
            }
            int kmin = Int32.MaxValue;
            foreach (KeyValuePair<double, int> k in kIlosc)
                if (k.Value < kmin)
                    kmin = k.Value;

            return kmin;

        }
        private double? porownanie()
        {
            Dictionary<double, List<double>> odleglosc = new Dictionary<double, List<double>>();            
            odleglosc=oblicz(iloscWierszy, iloscKolumn, probka, nowy, odleglosc);
            if (odleglosc == null)
            {
                pexist = false;
                return null;
            }
            Dictionary<double, double> wartosc = new Dictionary<double, double>();
            foreach (KeyValuePair<double, List<double>> el in odleglosc)
            {
                double suma = 0;                
                for (int i = 0; i < k; i++)
                    suma += el.Value.OrderBy(z => z).Skip(i).First();

                wartosc.Add(el.Key, suma);
            }
            double? odp = null;
            double najnizsza = Double.MaxValue;
            foreach (KeyValuePair<double, double> el in wartosc)            
                if (najnizsza > el.Value)
                    najnizsza = el.Value;

            foreach (KeyValuePair<double, double> el in wartosc)
            {
                if (najnizsza == el.Value && odp != null)
                    return null;
                if (najnizsza == el.Value)                
                    odp = el.Key;                               
            }
            return odp;
        }
        private void walidacja(object sender, RoutedEventArgs e)
        {
            if (!dane)
            {
                MessageBox.Show("Najpierw dodaj plik!");
                return;
            }
            if (!pobrane)
            {
                MessageBox.Show("Wybierz k i metryke!");
                return;
            }
            double[] nowy2 = new double[iloscKolumn - 1];
            double dobre = 0;
            for (int i = 0; i < iloscWierszy; i++)
            {
                int b = 0;
                for (int j = 0; j < iloscKolumn - 1; j++)
                {
                    nowy2[b++] = probka[i, j];
                }
                Dictionary<double, List<double>> odleglosc = new Dictionary<double, List<double>>();
                odleglosc = oblicz(iloscWierszy, iloscKolumn, probka, nowy2, odleglosc, i);
                if (odleglosc == null)
                    return;
                Dictionary<double, double> wartosc = new Dictionary<double, double>();
                foreach (KeyValuePair<double, List<double>> el in odleglosc)
                {
                    double suma = 0;
                    for (int a = 0; a < k; a++)
                        suma += el.Value.OrderBy(z => z).Skip(a).First();

                    wartosc.Add(el.Key, suma);
                }
                double najnizsza = Double.MaxValue;
                foreach (KeyValuePair<double, double> el in wartosc)
                {
                    if (najnizsza == el.Value)
                    {
                        najnizsza = Double.MaxValue;
                        break;
                    }
                    if (najnizsza > el.Value)
                        najnizsza = el.Value;
                }

                foreach (KeyValuePair<double, double> el in wartosc)
                    if (najnizsza == el.Value && el.Key == probka[i, iloscKolumn - 1])
                        dobre += 1;
            }
            dobre = dobre / iloscWierszy;
            test.Text = "Poprawność próbki wg 1 kontra reszta to " + dobre;
        }

        private void wartosciClick(object sender, RoutedEventArgs e)
        {          
            if (dane == false) { 
                MessageBox.Show("Najpierw wybierz plik z danymi!");
            return; }
            if (pobrane == true)
            {
                MessageBoxResult result = MessageBox.Show("Dane do porównania zostały już pobrane, chcesz je zmienić?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Array.Clear(nowy, 0, nowy.Length);
                        pobrane = false;
                        break;                  
                    case MessageBoxResult.No:
                        break;
                }
            }
            if (pobrane == false)
            {
                InputBox.Visibility = System.Windows.Visibility.Visible;
                nowy = new double[iloscKolumn - 1];
                pozostale = iloscKolumn;
                napis.Text = "Podaj jeszcze " + (pozostale - 1) + " liczb: ";
                kminV = kLicz(iloscWierszy, iloscKolumn, probka);
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (pozostale == -1)
            {
                String tmp2 = InputTextBox.Text;
                int num;
                if (!int.TryParse(tmp2, out num)) { }
                Regex rgx3 = new Regex(@"^\d+$");
                if (rgx3.IsMatch(InputTextBox.Text) != true ||  num <= 0)
                {
                    MessageBox.Show("Podawaj tylko liczbe wieksza od 0!");
                    InputTextBox.Text = String.Empty;
                    return;
                }
                p = num;
                InputBox.Visibility = System.Windows.Visibility.Collapsed;

            }
            if (pozostale == 1)
            {                
                String tmp2 = InputTextBox.Text;
                int num;
                if (!int.TryParse(tmp2, out num)){}
                Regex rgx2 = new Regex(@"^\d+$");
                if (rgx2.IsMatch(InputTextBox.Text)!=true || num >= kminV || num <=0)
                {
                    MessageBox.Show("Podawaj tylko liczbe mniejsza od " + (kminV-1) + " i wieksza od 0!");
                    InputTextBox.Text = String.Empty;
                    return;
                }
                k = num;
                pobrane = true;
                var metr = cb2.SelectedItem as KeyValuePair<string, int>?;
                int wyb = metr.Value.Value;
                if (wyb == 5)
                {
                    pozostale = -1;
                    napis.Text = "Podaj p (parametr \ndla metryki Minkowskiego >0):";
                }
                else                
                    InputBox.Visibility = System.Windows.Visibility.Collapsed;
                
            }
            Regex rgx = new Regex(@"^[+-]?([0-9]*[.])?[0-9]+$");
            if (!rgx.IsMatch(InputTextBox.Text))
            {
                MessageBox.Show("Podawaj tylko liczby typu double");
                InputTextBox.Text = String.Empty;
                return;
            }
            if (InputTextBox.Text != "" && pozostale>1)
                {
                    String tmp2 = InputTextBox.Text;
                    double num;
                    if (!double.TryParse(tmp2, out num))
                    { napis.Text = "fail"; }
                if (normal)
                {
                    num = (num - min[iloscKolumn - pozostale]) / (max[iloscKolumn - pozostale] - min[iloscKolumn - pozostale]);
                    danenormal = true;
                }
                        nowy[iloscKolumn-pozostale] = num;
                pozostale--;
                napis.Text = "Podaj jeszcze " + (pozostale - 1) + " liczb: ";            
            }
            
            InputTextBox.Text = String.Empty;
            if (pozostale == 1)
                napis.Text = "Podaj k (max "+(kminV-1)+"):";
        }
        

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
            Array.Clear(nowy, 0, nowy.Length);
            InputTextBox.Text = String.Empty;
        }

        private void otworz_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog plik = new OpenFileDialog();
            plik.Filter = "PlikTekstowy|*.txt";
            plik.DefaultExt=".txt";
            Nullable<bool> plikOK = plik.ShowDialog();
            var sep = cb1.SelectedItem as KeyValuePair<string, object>?;
            if (plikOK == true)            
                read(plik, sep.Value.Value.ToString());
        }
        private void liczClick(object sender, RoutedEventArgs e)
        {
            if (pobrane == false)
            {
                MessageBox.Show("Najpierw podaj dane do porownania!");
                return;
            }           
                var metr = cb2.SelectedItem as KeyValuePair<string, int>?;
                string wyb = metr.Value.Key;
                double? end=porownanie();                        
            if (end == null) {
                if (!pexist)
                {
                    pexist = true;
                    return;
                }
                if (metr.Value.Value == 4 && normal && end == null)
                    MessageBox.Show("Dla normalizacji i metryki z logarytmem wyniki moga byc nieprawidlowe (log0=infinity)");

                test.Text += "\nDla wartosci: ";
                foreach (double el in nowy)
                    test.Text += el + ", ";
                test.Text += "\nwiecej niz jeden atrybut pasuje do tych danych! Według metryki " + wyb + ".";
                    return; }
            test.Text += "\nDla wartosci: ";
            foreach (double el in nowy)
                test.Text += el + ", ";
            test.Text += " \npodany element to " + end + " według metryki "+ wyb+ ".";                       
        }      
        private void normalizujClick(object sender, RoutedEventArgs e)
        {
            if(!dane)
            {
                MessageBox.Show("Najpierw dodaj plik!");
                return;
            }
           
            if (normal)
            {
                for (int i = 0; i < iloscWierszy; i++)
                    for (int j = 0; j < iloscKolumn; j++)
                        probka[i, j] = probkaSave[i, j];
                normal = false;
                if (danenormal)
                    for (int i = 0; i < iloscKolumn - 1; i++)
                    {
                        nowy[i] = nowy[i] * (max[i] - min[i]) + min[i];
                        danenormal = false;
                    }
            
                normalizuj.Content = "Normalizuj OFF";
                return;
            }
            max = new double[iloscKolumn - 1];
            min = new double[iloscKolumn - 1];
            probkaSave = new double[iloscWierszy, iloscKolumn];
            for (int i = 0; i < iloscWierszy; i++)
                for (int j = 0; j < iloscKolumn; j++)
                    probkaSave[i, j] = probka[i, j];
            for (int j=0; j<iloscKolumn-1; j++)
            {
                max[j] = probka[0,j];
                min[j] = probka[0,j];
                for (int i=0;i<iloscWierszy; i++)
                {
                    max[j]=Math.Max(max[j], probka[i, j]);
                    min[j] = Math.Min(min[j], probka[i, j]);
                }
                for (int i = 0; i < iloscWierszy; i++)
                    probka[i, j] = (probka[i, j] - min[j]) / (max[j] - min[j]);

            }
            if (pobrane)
            {
                for(int i=0; i<iloscKolumn-1;i++)
                {
                    nowy[i]= (nowy[i] - min[i]) / (max[i] - min[i]);
                    danenormal = true;
                }
            }
            normal = true;
            normalizuj.Content = "Normalizuj ON";
            
        }
    }
}
