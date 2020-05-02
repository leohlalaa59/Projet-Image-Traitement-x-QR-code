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
    class MyImage /// Contient toutes les méthodes relaites au traitement de l'image (conversion, édition...)
    {
        #region Attributs
        byte[] header; /// Tableau d'octets pour le header
        byte[] headerinfo; /// Tableau d'octets pour le Header info sur l'image
        byte[] pixels_octets; /// Tableau d'octets pour la partie utile de l'image
        byte[,,] pixelsRGB; /// Matrice 3 dimensions pour la partie utile de l'image, la 3e dimension représentant les pixels bleu, green, red
                            /// L'utilisation de la classe Pixels aurait été possible, mais trop de problèmes de lecture surtout à la convolution

        byte[] image;
        
        int red;
        int green;
        int blue;

        int filesize;
        int height; /// Hauteur de l'image
        int width;  /// Largeur de l'image
        int taille_img; /// Taille de l'image
        #endregion


        #region Accès
        public byte[] Header
        {
            get { return this.header; }
            set { header = value; }
        }
        public byte[] HeaderInfo
        {
            get { return this.headerinfo; }
            set { headerinfo = value; }
        }

        #endregion
        #region Conversion

        public void From_Image_To_File(string file, byte[] image)
        {
            File.WriteAllBytes(file, image);
        }

        /*
        public int Convert_Endian_To_Int(byte[] tab) 
        {
            int i = BitConverter.ToInt32(tab, 0);
            return i;
        }
        public byte Convert_Int_To_Endian(int val)
        {
            byte octet = Convert.ToByte(val);
            return octet;
        }
        */

        /// <summary>
        /// Convertit un tableau d'octets au format Little Endian en un entier naturel de type int.
        /// </summary>
        /// <param name="tableau">Tableau d'octets à convertir.</param>
        /// <returns></returns>
        public int Convertir_Endian_To_Int(byte[] tableau)
        {
            int j = 0;
            int valeur = 0;
            for (int i = 0; i < 4; i++)
            {
                valeur += tableau[i] * (int)Math.Pow(256, j); /// 256 = 2^8
                j++;
            }
            return valeur;
        }
        /// <summary>
        /// Convertit un entier naturel de type int en un tableau d'octets au format little endian.
        /// </summary>
        /// <param name="valeur">Entier à convertir.</param>
        /// <returns></returns>
        public byte[] Convertir_Int_To_Endian(int valeur)
        {
            byte[] resultat = new byte[4];
            int j = 3;
            for (int i = 3; i >= 0; i--)
            {
                resultat[i] = (byte)(valeur / (int)(Math.Pow(256, j)));
                valeur = valeur - resultat[i] * (int)Math.Pow(256, j);
                j--;
            }
            return resultat;
        }
        
        /// <summary>
        /// Réécrit l'image à partir de son tableau de bytes.
        /// </summary>
        /// <param name="file"></param>
        public void WriteNewImage(string file) // Il faudra utiliser le code de la fonction QRCodeImage plutot que celle ci (à adapter ultérieurement (finir QR Code d'abord))
        {
            for (int i = 0; i < 14; i++)
            {
                image[i] = this.header[i];
            }
            for (int i = 14; i < 54; i++)
            {
                image[i] = this.headerinfo[i - 14];
            }
            int j = 54;
            for (int ligne = 0; ligne < this.height; ligne++)
            {
                for (int colonne = 0; colonne < this.width; colonne++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        image[j] = this.pixelsRGB[ligne, colonne, i];
                        j++;
                    }
                }
            }

            try
            {
                File.WriteAllBytes("./Sortie.bmp", image); ///permet de récrire l'image sous le nom de modification dans le debug 
                Process.Start("Sortie.bmp");
            }
            catch (IOException) { Console.WriteLine("erreur"); }
        }
        public void WriteNewImage90270(string file) /// Identique à WriteNewImage() mais utilisée uniquement pour la rotation à 90° et 270°
        {
            for (int i = 0; i < 14; i++)
            {
                image[i] = this.header[i];
            }
            for (int i = 14; i < 54; i++)
            {
                image[i] = this.headerinfo[i - 14];
            }
            byte a = image[18];
            byte b = image[19];
            byte c = image[20];
            byte d = image[21];
            for (int i = 18; i < 22; i++)
            {
                image[i] = image[i + 4];
            }
            image[22] = a;
            image[23] = b;
            image[24] = c;
            image[25] = d;
            int j = 54;
            for (int ligne = 0; ligne < this.width; ligne++)
            {
                for (int colonne = 0; colonne < this.height; colonne++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        image[j] = this.pixelsRGB[ligne, colonne, i];
                        j++;
                    }
                }
            }
            try
            {
                File.WriteAllBytes("./Sortie.bmp", image);
                Process.Start("Sortie.bmp");
            }
            catch (IOException) { Console.WriteLine("erreur"); }
        }


        /// <summary>
        /// Retourne la matrice 3 dimensions associée au fichier .bmp entré en paramètre.
        /// </summary>
        /// <param name="file">Nom du fichier .bmp</param>
        /// <returns></returns>
        public byte[,,] FromFiletoMatrix(string file)
        {
            /// On définit deux tableaux de taille 4 qui permettront d'extraire dans le headerinfo les données relatives au nombre de lignes et de colonnes.
            byte[] tablignes = new byte[4];
            byte[] tabcolonnes = new byte[4];
            this.header = new byte[14];
            this.headerinfo = new byte[40];

            /// On met l'image sous forme de tableau d'octets.
            this.image = File.ReadAllBytes(file);
            this.pixels_octets = new byte[this.image.Length - 54];
            for (int i = 0; i < 14; i++)
            {
                this.header[i] = this.image[i];
            }
            for (int i = 14; i < 54; i++)
            {
                this.headerinfo[i - 14] = this.image[i];
            }
            for (int i = 54; i < image.Length; i++)
            {
                this.pixels_octets[i - 54] = this.image[i];
            }

            /// On extrait dans le headerinfo les données relatives au nombre de colonnes.
            for (int i = 4; i < 8; i++)
            {
                tabcolonnes[i - 4] = this.headerinfo[i];

            }
            /// On extrait dans le headerinfo les données relatives au nombre de lignes.
            for (int i = 8; i < 12; i++)
            {
                tablignes[i - 8] = this.headerinfo[i];
            }

            this.width = Convertir_Endian_To_Int(tabcolonnes);
            this.height = Convertir_Endian_To_Int(tablignes);

            this.pixelsRGB = new byte[this.height, this.width, 3]; /// on crée une matrice 3 dimensions; 
            int j = 0;
            for (int ligne = 0; ligne < this.height; ligne++)
            {
                for (int colonne = 0; colonne < this.width; colonne++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.pixelsRGB[ligne, colonne, i] = this.pixels_octets[j];
                        j++;
                    }
                }
            }
            return this.pixelsRGB;
        }
        #endregion

        #region Edition
        /// <summary>
        /// Retourne la matrice d'octets avec les pixels au niveau de gris.
        /// </summary>
        /// <returns></returns>
        public byte[,,] Gris() //OK
        {
            for (int ligne = 0; ligne < this.height; ligne++)
            {
                for (int colonne = 0; colonne < this.width; colonne++)
                {
                    this.blue = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 0]);
                    this.green = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 1]);
                    this.red = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 2]);
                    int moyenne = (this.red + this.blue + this.green) / 3;
                    byte moyennebyte = Convert.ToByte(moyenne);
                    this.pixelsRGB[ligne, colonne, 0] = moyennebyte;
                    this.pixelsRGB[ligne, colonne, 1] = moyennebyte;
                    this.pixelsRGB[ligne, colonne, 2] = moyennebyte;
                }
            }
            return this.pixelsRGB;
        }
        /// <summary>
        /// Retourne la matrice d'octets avec les pixels au niveau de gris.
        /// </summary>
        /// <returns></returns>
        public byte[,,] NoirBlanc() //OK
        {
            for (int ligne = 0; ligne < this.height; ligne++)
            {
                for (int colonne = 0; colonne < this.width; colonne++)
                {
                    this.red = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 0]);
                    this.green = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 1]);
                    this.blue = Convert.ToInt32(this.pixelsRGB[ligne, colonne, 2]);
                    int moyenne = (red + green + blue) / 3;
                    byte moyennebyte = Convert.ToByte(moyenne);
                    for (int k = 0; k < 3; k++)
                    {
                        if (moyennebyte < 255 / 2)
                        {
                            this.pixelsRGB[ligne, colonne, k] = 0;

                        }
                        else
                        {
                            this.pixelsRGB[ligne, colonne, k] = 255;

                        }
                    }
                    
                }
            }
            return this.pixelsRGB;
        }

        public byte[,,] Rotation90() //OK
        {
            byte[,,] newmat = new byte[this.width, this.height, 3];
            for (var i = 0; i < this.width; i++)
            {
                for (var j = 0; j < this.height; j++)
                {
                    newmat[i, j, 0] = this.pixelsRGB[j, i, 0];
                    newmat[i, j, 1] = this.pixelsRGB[j, i, 1];
                    newmat[i, j, 2] = this.pixelsRGB[j, i, 2];
                }
            }
            this.pixelsRGB = new byte[this.width, this.height, 3];
            this.pixelsRGB = newmat;

            return this.pixelsRGB;
        }
        public byte[,,] Rotation270() //OK
        {
            byte[,,] rotation180 = new byte[this.height, this.width, 3];
            for (var i = 0; i < this.height; i++)
            {
                for (var j = 0; j < this.width; j++)
                {
                    rotation180[i, j, 0] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 0];
                    rotation180[i, j, 1] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 1];
                    rotation180[i, j, 2] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 2];
                }
            }

            byte[,,] newmat = new byte[this.width, this.height, 3];
            for (int ligne = 0; ligne < this.width; ligne++)
            {
                for (int colonne = 0; colonne < this.height; colonne++)
                {
                    newmat[ligne, colonne, 0] = rotation180[colonne, ligne, 0];
                    newmat[ligne, colonne, 1] = rotation180[colonne, ligne, 1];
                    newmat[ligne, colonne, 2] = rotation180[colonne, ligne, 2];
                }
            }
            this.pixelsRGB = new byte[this.width, this.height, 3];
            this.pixelsRGB = newmat;
            return this.pixelsRGB;
        }
        public byte[,,] Rotation180() //OK
        {
            byte[,,] newmat = new byte[this.height, this.width, 3];
            for (var i = 0; i < this.height; i++)
            {
                for (var j = 0; j < this.width; j++)
                {
                    newmat[i, j, 0] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 0];
                    newmat[i, j, 1] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 1];
                    newmat[i, j, 2] = this.pixelsRGB[this.pixelsRGB.GetLength(0) - 1 - i, j, 2];
                }
            }

            for (var i = 0; i < this.height; i++)
            {
                for (var j = 0; j < this.width; j++)
                {
                    this.pixelsRGB[i, j, 0] = newmat[i, j, 0];
                    this.pixelsRGB[i, j, 1] = newmat[i, j, 1];
                    this.pixelsRGB[i, j, 2] = newmat[i, j, 2];
                }
            }
            return this.pixelsRGB;
        }

        public byte[,,] Miroir() //OK
        {
            byte[,,] miroir = new byte[this.height, this.width, 3];

            for (int i = 0; i < this.height; i++)
            {
                int max = this.pixelsRGB.GetLength(1) - 1;
                for (int j = 0; j < this.width; j++)
                {
                    miroir[i, j, 0] = this.pixelsRGB[i, this.pixelsRGB.GetLength(1) - 1 - j, 0];
                    miroir[i, j, 1] = this.pixelsRGB[i, this.pixelsRGB.GetLength(1) - 1 - j, 1];
                    miroir[i, j, 2] = this.pixelsRGB[i, this.pixelsRGB.GetLength(1) - 1 - j, 2];
                }
            }
            this.pixelsRGB = miroir;
            return this.pixelsRGB;
        }
        
        /// <summary>
        /// Retourne en modifiant la nouvelle matrice de pixels de l'image après application d'un filtre.
        /// </summary>
        /// <param name="choixfiltre">Nom du filtre désiré.</param>
        /// <returns></returns>
        public byte[,,] Convolution(string choixfiltre) //OK
        {
            int[,] filtre = null;
            if (choixfiltre == "1") // Flou
            {
                filtre = new int[,] { { 1, 1, 1, }, { 1, 1, 1 }, { 1, 1, 1 } };
            }
            if (choixfiltre == "2")  // Augmentation du contraste
            {
                filtre = new int[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            }
            if (choixfiltre == "3") // Repoussage
            {
                filtre = new int[,] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
            }
            if (choixfiltre == "4") // Détection des bords
            {
                filtre = new int[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            }
            if (choixfiltre == "5") // Renforcement des bords
            {
                filtre = new int[,] { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };
            }

            byte[,,] newmat_convolution = new byte[this.height, this.width, 3];
            /// On effectue l'opération de convuliton pour chacun des pixels de la matrice (sauf les bords difficiles à gérer)
            for (var i = 1; i < this.height - 1; i++)
            {
                for (var j = 1; j < this.width - 1; j++)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        int a = pixelsRGB[i, j, k] * filtre[1, 1];
                        int b = pixelsRGB[i - 1, j - 1, k] * filtre[0, 0];
                        int c = pixelsRGB[i - 1, j, k] * filtre[0, 1];
                        int d = pixelsRGB[i - 1, j + 1, k] * filtre[0, 2];
                        int e = pixelsRGB[i, j - 1, k] * filtre[1, 0];
                        int f = pixelsRGB[i, j + 1, k] * filtre[1, 2];
                        int g = pixelsRGB[i + 1, j - 1, k] * filtre[2, 0];
                        int h = pixelsRGB[i + 1, j, k] * filtre[2, 1];
                        int l = pixelsRGB[i + 1, j + 1, k] * filtre[2, 2];

                        int y = 0;
                        if (choixfiltre == "1") { y = (a + b + c + d + e + f + g + h + l) / 9; }
                        else { y = (a + b + c + d + e + f + g + h + l); }


                        /// Si le résultat de l'opération de convulution est en dehors de borne des pixels RGB (0 - 255), on le ramène dans l'intervalle
                        if (y < 0) { y = 0; }
                        if (y > 255) { y = 255; }
                        newmat_convolution[i, j, k] = Convert.ToByte(y);
                    }
                }
            }
            pixelsRGB = newmat_convolution;
            return pixelsRGB;
        }

        /// <summary>
        /// Retourne la matrice 3 dimensions de pixels correspondant à l'histogramme de l'image.
        /// </summary>
        /// <returns></returns>
        public byte[,,] Histogramme() //OK
        {
            /// On crée une nouvelle matrice de pixels qui correspondra au fond de l'image.
            MyImage histo = new MyImage();
            byte[,,] histogramme = histo.FromFiletoMatrix("blanc.bmp");
            int[] rouge = new int[256]; /// Tableau de 256, correspondant à chaque degré de pixel rouge, de pixel vert et de pixel bleu
            int[] vert = new int[256];
            int[] bleu = new int[256];

            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    rouge[this.pixelsRGB[i, j, 0]]++; /// on parcours chaque case de la matrice et on incrémente de 1 la case correspondant à l'intensité  
                    vert[this.pixelsRGB[i, j, 1]]++;
                    bleu[this.pixelsRGB[i, j, 2]]++;
                }
            }

            int max = Math.Max(Math.Max(bleu.Max(), rouge.Max()), vert.Max()); /// Échelle permettant de ne pas dépasser la taille de l'image blanc si l'image a un trop fort présence de pixels d'une certaine couleur
            int echelle;
            int pic;

            if (max % histogramme.GetLength(0) == 0)
            {
                echelle = max / histogramme.GetLength(0);
            }
            else
            {
                echelle = max / histogramme.GetLength(0) + 1;
            }

            for (int j = 0; j < 768; j++) /// 768 = 3*256 ce qui permet de traiter d'abord les rouges, puis les verts, puis les bleus
            {
                if (j < 256)
                {
                    pic = rouge[j] / echelle;
                    for (int i = 0; i < pic; i++)
                    {
                        histogramme[i, j, 0] = 255;
                        histogramme[i, j, 1] = 0;
                        histogramme[i, j, 2] = 0;
                    }
                }
                else if (j >= 256 && j < 512)
                {
                    pic = vert[j - 256] / echelle;
                    for (int i = 0; i < pic; i++)
                    {
                        histogramme[i, j, 0] = 0;
                        histogramme[i, j, 1] = 255;
                        histogramme[i, j, 2] = 0;
                    }
                }
                else if (j >= 512)
                {
                    pic = vert[j - 512] / echelle;
                    for (int i = 0; i < pic; i++)
                    {
                        histogramme[i, j, 0] = 0;
                        histogramme[i, j, 1] = 0;
                        histogramme[i, j, 2] = 255;
                    }
                }
            }
            return histogramme;
        }
        public void WriteImageHisto(byte[,,] matrice)
        {
            this.pixelsRGB = matrice;
        }

        public byte[,,] Cacher(byte[,,] imageàcacher, byte[,,] grandeimage)  // NON OPERATIONNEL (problèmes dimensions à résoudre)
        {
            byte[,,] newmat = new byte[grandeimage.GetLength(0), grandeimage.GetLength(1), 3];

            /// On commence en remplissant le coin de l'image où il n'y a rien à cacher
            for (int i = imageàcacher.GetLength(0); i < grandeimage.GetLength(0); i++)
            {
                for (int j = 0; j < grandeimage.GetLength(1); j++)
                {
                    newmat[i, j, 0] = grandeimage[i, j, 0];
                    newmat[i, j, 1] = grandeimage[i, j, 1];
                    newmat[i, j, 2] = grandeimage[i, j, 2];
                }
            }
            for (int i = 0; i < grandeimage.GetLength(0); i++)
            {
                for (int j = imageàcacher.GetLength(1); j < grandeimage.GetLength(1); j++)
                {
                    newmat[i, j, 0] = grandeimage[i, j, 0];
                    newmat[i, j, 1] = grandeimage[i, j, 1];
                    newmat[i, j, 2] = grandeimage[i, j, 2];
                }
            }
            /// Puis on remplit la superposition es images, en changeant les 4 dernier bits de chaque R G B
            for (int i = 0; i < imageàcacher.GetLength(0); i++)
            {
                for (int j = 0; j < imageàcacher.GetLength(1); j++)
                {
                    for (int p = 0; p < 3; p++)
                    {
                        int h = imageàcacher[i, j, p]; /// On effectue la modifcation des 4 derniers bits 
                        int g = grandeimage[i, j, p];

                        int[] conversion1 = new int[8];
                        int[] conversion2 = new int[8];
                        int w = 0;
                        do
                        {
                            conversion1[w] = h % 2;
                            h = h / 2;
                            w++;

                        } while (h != 0); 
                        w = 0;
                        do
                        {

                            conversion2[w] = g % 2;
                            g = g / 2;
                            w++;


                        } while (g != 0);
                        int[] k = new int[8];
                        for (int s = 0; s < 8; s++)
                        {
                            if (s < 4)
                            {
                                k[s] = conversion1[s + 4];
                            }
                            else
                            {
                                k[s] = conversion2[s];
                            }
                        }
                        double m = 0;
                        for (int q = 0; q < 8; q++)
                        {
                            m += k[q] * (Math.Pow(2, q));
                        }

                        newmat[i, j, p] = Convert.ToByte(m); ///on créer la nouvel matrice 3*3 avec la modification des deux autres 
                    }
                }
            }

            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        this.pixelsRGB[i, j, k] = newmat[i, j, k];
                    }
                }
            }

            return this.pixelsRGB;
        }

        public byte[,,] Fractale(byte[,,] imgcarrée) // PAS ENCORE OPERATIONNEL (image blanche)
        {
            byte[] fractale = new byte[image.Length];
            for (int i = 0; i < 54; i++) /// le header et l'infoheader de la fractale reste les mêmes que ceux de l'image lena 
            {
                fractale[i] = image[i];
            }
            for (int x = 0; x < imgcarrée.GetLength(0); x++)
            {
                for (int y = 0; y < imgcarrée.GetLength(1); y++)
                {
                    /// on rempli la partie correspondant au pixel de notre image en utilisant les fonctions de la classe complex 
                    double a = (double)(x - (imgcarrée.GetLength(1) / 2)) / (double)(imgcarrée.GetLength(1) / 4);
                    double b = (double)(y - (imgcarrée.GetLength(0) / 2)) / (double)(imgcarrée.GetLength(0) / 4);
                    Complex c = new Complex(a, b);
                    Complex z = new Complex(0, 0);
                    int h = 0;
                    do
                    {
                        h++;
                        z.AuCarré();
                        z.Add(c);
                        if (z.Magnitude() > 2.0) break;

                    }
                    while (h < 100);

                    if (h < 100)
                    {
                        imgcarrée[x, y, 0] = 0;
                        imgcarrée[x, y, 1] = 0;
                        imgcarrée[x, y, 2] = 0;
                    }
                    else
                    {
                        imgcarrée[x, y, 0] = 255;
                        imgcarrée[x, y, 1] = 255;
                        imgcarrée[x, y, 2] = 255;
                    }

                }
            }
            //mat = test.Rotation90(); /// notre fractale étant vers le bas nous utilisons une rotation 90 pour faire la même que celle de Mandelbrot 
            int j = 54;
            for (int ligne = 0; ligne < imgcarrée.GetLength(0); ligne++)
            {
                for (int colonne = 0; colonne < imgcarrée.GetLength(1); colonne++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        fractale[j] = imgcarrée[ligne, colonne, i];
                        j++;
                    }
                }
            }
            return imgcarrée;

            try
            {
                File.WriteAllBytes("./fractale.bmp", fractale);
                Process.Start("fractale.bmp");
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("erreur");
            }
        }


        /// <summary>
        /// Retourne en la modifiant, la nouvelle matrice de pixels de l'image correspondant au QR Code.
        /// </summary>
        /// <param name="qrcodematrix">Il s'agit de la matrice de QR code (21x21 ou 25x25) que l'on utilise, générée dans la Classe QRCode.</param>
        /// <returns></returns>
        public void QRCodeImage(byte[,,] qrcodematrix) // OKKKKKKK  // Utiliser cette méthode ci plutot que WriteNewNytes en guise de fonction d'écriture de l'image (à faire ultérieurement)
        {
            pixelsRGB = qrcodematrix;
            int padding = 0;
            /// Le nombre de colonnes (largeur) d'une image Bitmap étant un multiple de 4, tant que la largeur de l'image Bitmap n'est pas égale à 4, on ajoute 1 octet de padding (de complétion) au bout de chaque ligne
            int hauteur_qrcode = qrcodematrix.GetLength(0);
            int largeur_qrcode = qrcodematrix.GetLength(1);
            int nb_octets_par_ligne = largeur_qrcode * 3 + padding;
            bool multiple4 = nb_octets_par_ligne % 4 == 0;
            do
            {
                padding++;
                nb_octets_par_ligne += padding;
                if (nb_octets_par_ligne % 4 == 0) { multiple4 = true; }
            } while (multiple4 == false);
            /// On crée une varibale temporaire correspondant à la taille de l'image QR code
            int tailleqrcode = nb_octets_par_ligne * hauteur_qrcode;
            /// La taille totale du fichier correspond au nombre total d'octets, donc la taille de l'image QR code ajoutés à la taille du Header et Header info
            int taille_fichierqr = tailleqrcode + 54;
            image = new byte[taille_fichierqr];

            // Il faut récupérer la taille de l'image ((largeur*3 + padding) x hautr). Pour cela :
            // On détermine le padding (= le nombre d'octets 0 qu'on ajoute en fin de ligne)
            // => 1 ligne contient multiple de 4 octets ==> largeur % 4 = 0
            // On pose une variable nombre_d'octets_par_ligne, et padding = 0, avec nombredoctetsparligne = largeur*3+padding
            // Bool nombre_d'octets_par_ligne  % 4 = 0, et tant que c'est faux, padding += 1

            // Pour la taille du fichier
            // On taille de l'image + 54

            for (var i = 0; i < 14; i++)
            {
                image[i] = this.header[i];
            }
            for (var i = 14; i < 54; i++)
            {
                image[i] = this.headerinfo[i - 14];
            }
            byte[] tab_fichier = new byte[4];
            tab_fichier = Convertir_Int_To_Endian(taille_fichierqr);
            byte[] tablignes = new byte[4];
            tablignes = Convertir_Int_To_Endian(hauteur_qrcode);
            byte[] tabcolonnes = new byte[4];
            tabcolonnes = Convertir_Int_To_Endian(largeur_qrcode);
            byte[] tab_tailleimage = new byte[4];
            tab_tailleimage = Convertir_Int_To_Endian(tailleqrcode);

            /// On procède à la modification du header, et du header_info afin que l'image puisse être lue
            /// On modifie l'offset correspondant respectivement la taille du fichier, la largeur de l'image, la hauteur de l'image, et la taille de l'image
            for (var i = 2; i < 6; i++) // Taille fichier
            {
                image[i] = tab_fichier[i - 2];
            }
            for (var i = 18; i < 22; i++) // Largeur image
            {
                image[i] = tabcolonnes[i - 18];
            }
            for (var i = 22; i < 26; i++) // Hauteur image
            {
                image[i] = tablignes[i - 22];
            }
            for (int i = 34; i < 38; i++) // Taille image
            {
                image[i] = tab_tailleimage[i - 34];
            }
            /// Une fois que le header et le headerinfo sont édités, on n'a qu'a faire idem que la classe WriteNew Image, mais en prenant en comte le padding à la fin de chaque ligne

            // On parcourt le tableau normalement et en bout de ligne on ajoute [padding] fois "0"

            /// Attention, l'écriture de l'image se fait en commençant par la dernière ligne.
            int indexoctet = 54;
            for (var i = qrcodematrix.GetLength(0) - 1; i >= 0; i--)
            {
                for (var j = 0; j < qrcodematrix.GetLength(1); j++)
                {
                    image[indexoctet] = qrcodematrix[i, j, 0];
                    indexoctet++;
                    image[indexoctet] = qrcodematrix[i, j, 1];
                    indexoctet++;
                    image[indexoctet] = qrcodematrix[i, j, 2];
                    indexoctet++;
                }
                for (var zeroajoutés = 0; zeroajoutés < padding; zeroajoutés++)
                {
                    image[indexoctet] = 0;
                    indexoctet++;
                }
            }

            try
            {
                File.WriteAllBytes("./QRcode.bmp", image); ///permet de récrire l'image sous le nom de modification dans le debug 
                Process.Start("QRCode.bmp");
            }
            catch (IOException) { Console.WriteLine("erreur"); }

            
        }
        #endregion

    }
}
