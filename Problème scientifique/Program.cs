using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace Problème_scientifique
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Appuyez sur n'importe quelle touche pour commencer le programme...");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("==================================  BONJOUR !  ==================================");
            Console.WriteLine("\nBienvenue dans le projet de traitement d'images de Léo OTSHUDI\n");
            bool a = true;
            string image = " ";
            while(a)
            {
                Console.WriteLine("Que voulez-vous faire ?");
                Console.WriteLine("\t 1. Nuances de gris" +
                                "\n\t 2. Noir et blanc" +
                                "\n\t 3. Miroir" +
                                "\n\t 4. Rotation" +
                                "\n\t 5. Agrandir / Rétrécir (non opérationnel)" +
                                "\n\t 6. Appliquer un filtre" +
                                "\n\t 7. Fractale (non opérationnel)" +
                                "\n\t 8. Afficher l'histogramme d'une image" +
                                "\n\t 9. Cacher une image dans une autre" +
                                "\n\t10. Génération d'un QR Code à partir d'une chaîne de caractères" +
                                "\n\t11. Test de lecture puis ouverture d'image");
                string choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        image = ChoixImage();
                        Gris(image);
                        a = false;
                        break;

                    case "2":
                        image = ChoixImage();
                        Noir_Blanc(image);
                        a = false;
                        break;

                    case "3":
                        image = ChoixImage();
                        Miroir(image);
                        a = false;
                        break;

                    case "4":
                        image = ChoixImage();
                        Rotation(image);
                        a = false;
                        break;

                    case "5":
                        Console.Clear();
                        Console.WriteLine("Non-opérationnel.");
                        break;

                    case "6":
                        image = ChoixImage();
                        Filtres(image);
                        a = false;
                        break;

                    case "7":
                        Console.Clear();
                        Console.WriteLine("Non-opérationnel.");
                        //Fractale();
                        //a = false;
                        break;

                    case "8":
                        image = ChoixImage();
                        Histogramme(image);
                        a = false;
                        break;

                    case "9":
                        Console.Clear();
                        Console.WriteLine("Non-opérationnel. (problème de dimension à résoudre)");
                        //image = ChoixImage();
                        //Cacher(image);
                        //a = false;
                        break;

                    case "11":
                        image = ChoixImage();
                        Test(image);
                        a = false;
                        break;

                    case "10":
                        QRCode();
                        a = false;
                        break;
                }
            }
            Console.ReadKey();
        }


        static string ChoixImage()
        {
            bool a = true;
            string image = " ";
            while (a)
            {
                Console.WriteLine("Quelle image voulez-vous utiliser ?");
                Console.WriteLine("\t - Image de test ======> appuyez sur A");
                Console.WriteLine("\t - Coco le perroquet ==> appuyez sur B");
                Console.WriteLine("\t - Lena ===============> appuyez sur C");
                Console.WriteLine("\t - Lac en montagne ====> appuyez sur D");
                string choix = Console.ReadLine().ToUpper();
                switch (choix)
                {
                    case "A":
                        image = "Test.bmp";
                        a = false;
                        break;

                    case "B":
                        image = "coco.bmp";
                        a = false;
                        break;

                    case "C":
                        image = "lena.bmp";
                        a = false;
                        break;

                    case "D":
                        image = "lac.bmp";
                        a = false;
                        break;

                    default:
                        Console.WriteLine("saisie invalide. Veuillez recommencer.");
                        break;
                }
            }
            return image;
        }

        #region Fonctions Menu
        static void Test(string choiximage)
        {
            MyImage monimagetest = new MyImage();
            monimagetest.FromFiletoMatrix(choiximage);
            monimagetest.WriteNewImage(choiximage);
        }

        static void Gris(string choiximage) //OK
        {
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            monimageRGB = monimage.Gris();
            monimage.WriteNewImage(choiximage);
        }
        static void Noir_Blanc(string choiximage) //OK
        {
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            monimageRGB = monimage.NoirBlanc();
            monimage.WriteNewImage(choiximage);
        }

        static void Rotation(string choiximage) //OK
        {
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            bool a = true;
            string angle = "";
            while (a)
            {
                Console.WriteLine("\n\nQuelle rotation voulez-vous obtenir ?");
                Console.WriteLine("\t- 180° --> 1\n" +
                                  "\t- 90° ---> 2\n" +
                                  "\t- 270° --> 3\n");
                angle = Console.ReadLine();
                if (angle == "1")
                {
                    monimageRGB = monimage.Rotation180();
                    monimage.WriteNewImage(choiximage);
                    a = false;
                }
                if (angle == "2")
                {
                    monimageRGB = monimage.Rotation90();
                    monimage.WriteNewImage90270(choiximage);
                    a = false;
                }
                if (angle == "3")
                {
                    monimageRGB = monimage.Rotation270();
                    monimage.WriteNewImage90270(choiximage);
                    a = false;
                }
                else { Console.WriteLine("Erreur de saisie..."); }
            }
        }

        static void Miroir(string choiximage) //OK
        {
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            monimageRGB = monimage.Miroir();
            monimage.WriteNewImage(choiximage);
        }

        static void Filtres(string choiximage) //OK
        {
            Console.Clear();
            Console.WriteLine("==================================  CONVOLUTION  ==================================");
            Console.WriteLine("\nVous avez choisi de travailler avec les filtres et matrices de convolution.");
            bool b = true;
            string chx = " ";
            while (b)
            {
                Console.WriteLine("\n\nVeuillez choisir quel effet de filtre appliquer :");
                Console.WriteLine("\t1. Flou" +
                                "\n\t2. Augmentation du contraste" +
                                "\n\t3. Filtre de repoussage" +
                                "\n\t4. Détection des bords" +
                                "\n\t5. Renforcement des bords");
                chx = Console.ReadLine();
                if (chx == "1" || chx == "2" || chx == "3" || chx == "4" || chx == "5") { b = false; }
            }
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            monimageRGB = monimage.Convolution(chx);
            monimage.WriteNewImage(choiximage);
        }

        static void Histogramme(string choiximage) //OK
        {
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            MyImage monhistogramme = new MyImage();
            byte[,,] monhistoRGB = monhistogramme.FromFiletoMatrix("blanc.bmp");
            monhistoRGB = monimage.Histogramme();
            monhistogramme.WriteImageHisto(monhistoRGB);
            monhistogramme.WriteNewImage(choiximage);
        }

        static void Cacher(string choiximage)
        {
            Console.Clear();
            Console.WriteLine("================================  STENOGRAPHIE  ================================");
            Console.WriteLine("\nVous avez choisi de travailler avec les filtres et matrices de convolution.");
            Console.WriteLine("\n\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
            bool a = true;
            string chx = " ";
            while (a)
            {
                Console.WriteLine("\nQuelle image voulez-vous cacher ? (attention, l'image à cacher doit être plus petite que l'image principale (Lac > lena > coco > test)");
                Console.WriteLine("\t - Image de test ======> appuyez sur A");
                Console.WriteLine("\t - Coco le perroquet ==> appuyez sur B");
                Console.WriteLine("\t - Lena ===============> appuyez sur C");
                Console.WriteLine("\t - Lac en montagne ====> appuyez sur D");
                string choix = Console.ReadLine().ToUpper();
                switch (choix)
                {
                    case "A":
                        chx = "Test.bmp";
                        a = false;
                        break;

                    case "B":
                        chx = "coco.bmp";
                        a = false;
                        break;

                    case "C":
                        chx = "lena.bmp";
                        a = false;
                        break;

                    case "D":
                        chx = "lac.bmp";
                        a = false;
                        break;

                    default:
                        Console.WriteLine("saisie invalide. Veuillez recommencer.");
                        break;
                }
            }
            MyImage monimage = new MyImage();
            byte[,,] monimageRGB = monimage.FromFiletoMatrix(choiximage);
            MyImage image_àcacher = new MyImage();
            byte[,,] à_cacher = image_àcacher.FromFiletoMatrix(chx);
            monimageRGB = monimage.Cacher(monimageRGB, à_cacher);
            monimage.WriteNewImage(choiximage);
        }

        /*
        static void Fractale()
        {
            MyImage monimage = new MyImage();
            byte[,,] monimagecarrée = monimage.FromFiletoMatrix("./lena.bmp"); // On prend lena car il s'agit d'une image carrée
            
        }
        */


        static void QRCode()
        {
            Console.Clear();
            Console.WriteLine("==================================  QR Code  ==================================");
            Console.WriteLine("\nVous avez choisi de générer un QR code à partir d'une chaîne de caractères");
            Console.WriteLine("\n\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
            Console.Clear();
            string version = "";
            QRCode monQRCode;
            string chaine = "";
            bool a = true;
            while(a)
            {
                Console.WriteLine("==================================  QR Code  ==================================");
                Console.WriteLine("\nVeuillez entrer la chaine de caractères :");
                chaine = Console.ReadLine().ToUpper();
                if (chaine.Length <= 47) { a = false; }
                else
                {
                    Console.WriteLine("\n\nLa chaine de caractères est trop longue ; veuillez saisir une chaine de 47 caractères ou moins.");
                    Console.WriteLine("\n\nAppuyez sur une touche pour une nouvelle saisie...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            if (chaine.Length > 25 && chaine.Length <= 47)
            {
                Console.WriteLine("\n\nUn QR code de version 2 sera utilisé.");
                Console.WriteLine("\n\nAppuyez sur une touche pour afficher le QR code...");
                Console.ReadKey();
                version = "2";
                monQRCode = new QRCode(chaine, version);
            }
            else
            {
                bool saisie = true;
                while (saisie)
                {
                    Console.WriteLine("\n\nQuelle version de code QR souhaitez vous utiliser ? (1 ou 2)");
                    version = Console.ReadLine();
                    if (version == "1" || version == "2") { saisie = false; }
                    else { Console.WriteLine("Saisie invalide. Veuillez rentrer le n° de la version"); }
                }
                Console.WriteLine("\n\nUn QR code de version " + version + " sera donc utilisé.");
                Console.WriteLine("\n\nAppuyez sur une touche pour continuer...");
                Console.ReadKey();
                monQRCode = new QRCode(chaine, version);
            }

            /// Simple affichage de la chaine de bits intégrale du code QR avec l'erreur
            Console.WriteLine("\n\n\n" + monQRCode.QRChainWithErr);
            Console.WriteLine("\nQR code bits chain length :\n" + monQRCode.QRChainWithErr.Length);

            MyImage monimage = new MyImage();
            byte[,,] imgcarrée = monimage.FromFiletoMatrix("lena.bmp"); /// On prend n'importe quelle image, afin d'en récupérer le header et d'évite de devoir réécrire l'intégralité de 54 bits
            
            byte[,,] qrcodematrix = monQRCode.GenerateQRCodeImage(monQRCode.QRChainWithErr);

            monimage.QRCodeImage(qrcodematrix);
        }
        #endregion
    }




}
