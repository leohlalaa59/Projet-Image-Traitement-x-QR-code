using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using ReedSolomon;

namespace Problème_scientifique
{
    class QRCode : MyImage
    {
        string chain;
        string qrcodechain_global;
        string version;

        public QRCode(string chain, string version)
        {
            this.chain = chain;
            this.version = version;
            if (this.version == "1") { this.qrcodechain_global = QRCode_with_errorV1(QR_Code_Chain()); }
            if (this.version == "2") { this.qrcodechain_global = QRCode_with_errorV2(QR_Code_Chain()); }

        }

        #region Accès
        public string Chaîne
        {
            get { return this.chain; }
        }
        public string QRChainWithErr
        {
            get { return this.qrcodechain_global; }
        }
        #endregion


        #region Conversion

        /// <summary>
        /// Convertit un nombre entier en nombre binaire.
        /// </summary>
        /// <param name="octet">Nombre binaire à convertir</param>
        /// <param name="longueur">Longueur de la chaine de bits du nombre binaire</param>
        /// <returns></returns>
        string IntToBinary(int octet, int longueur)
        {
            int dividende = octet;
            int quotient = 0;
            int reste = 0;
            string resultat = "";
            do
            {
                quotient = dividende / 2;
                reste = dividende % 2;
                resultat = reste + resultat;
                dividende = quotient;
            } while (dividende > 1);
            resultat = dividende + resultat;

            int taille_bin = resultat.Length;
            int acombler = longueur - taille_bin;
            int i = 0;
            /// On rajoute des 0 au début de la chaine de bits afin que le nombre binaire ait la longueur désirée
            while (i < acombler)
            {
                resultat = "0" + resultat;
                i++;
            }
            return resultat;
        }

        /// <summary>
        /// Convertit un caractère en son entier alphanumérique correspondant.
        /// </summary>
        /// <param name="car">Caractère à convertir.</param>
        /// <returns></returns>
        private int ConvertLetterToQRInt(char car)
        {
            int letter_in_int = 0;
            int convert_char = Convert.ToInt32(car);
            if (convert_char >= 65 && convert_char <= 90)
            {
                letter_in_int = convert_char - 55;
            }
            if (convert_char >= 38 && convert_char <= 39)
            {
                letter_in_int = convert_char - 1;
            }
            if (car == '*')
            {
                letter_in_int = 42;
            }
            if (car == ' ') { letter_in_int = 36; }
            if (car == '+') { letter_in_int = 40; }
            if (car == '-') { letter_in_int = 41; }
            if (car == '.') { letter_in_int = 42; }
            if (car == '/') { letter_in_int = 43; }
            if (car == ':') { letter_in_int = 44; }

            return letter_in_int;
        }

        /// <summary>
        /// Encode une chaine de caractères en une chaine binaire.
        /// </summary>
        /// <param name="chain">Chaine de caractères à encoder.</param>
        /// <returns></returns>
        private string Convert_Chain_To_Bits()
        {
            int longueur_chaine = chain.Length;
            int maxdécoupage = 0;
            char carac_1 = ' ';
            char carac_2 = ' ';
            string chain_in_bits = "";
            bool a = longueur_chaine % 2 == 0;
            if (a == true)
            {
                maxdécoupage = longueur_chaine - 1;
            }
            else
            {
                maxdécoupage = longueur_chaine - 2;
            }
            for (var i = 0; i < maxdécoupage; i += 2)
            {
                carac_1 = chain[i];
                carac_2 = chain[i + 1];
                int code_car_1 = ConvertLetterToQRInt(carac_1);
                int code_car_2 = ConvertLetterToQRInt(carac_2);

                double paire = Math.Pow(45, 1) * code_car_1 + Math.Pow(45, 0) * code_car_2;
                string paire_bits = IntToBinary(Convert.ToInt32(paire), 11);
                chain_in_bits += paire_bits;
            }
            if (a == false)
            {
                carac_1 = chain[longueur_chaine - 1];
                int code_car1 = ConvertLetterToQRInt(carac_1);
                double solo = Math.Pow(45, 0) * code_car1;
                string solo_bits = IntToBinary(Convert.ToInt32(solo), 6);
                chain_in_bits += solo_bits;
            }

            return chain_in_bits;
        }

        /// <summary>
        /// Retourne la chaine binaire correspondant au mode, la taille de la chaine, les données encodées de la chaine, la terminaison, et les bits pour compléter l'octet.
        /// </summary>
        /// <returns></returns>
        private string Chain_QR_Intermediate()
        {
            int longueurchaine = chain.Length;
            string mode = "0010"; // Correspond aux 4 bits du mode alphanumeric
            string nb_carac_bits = "";
            string chain_bits = "";
            string total_bits = "";
            int qrSize = 0;
            /// Si le QR code est de version 1, sa taille est de 152 bits.
            if (version == "1") { qrSize = 152; }
            /// Si le QR code est de version 2, sa taille est de 272 bits.
            if (version == "2") { qrSize = 272; }
            /// Cette vatiable string correspond à la longueur de la chaine sur 9 bits.
            nb_carac_bits = IntToBinary(longueurchaine, 9);
            chain_bits = Convert_Chain_To_Bits();
            /// La chaine totale (avant terminaison) correspond à l'enchainements des bts du mode, du nombre de caractères, et de la chaine chaine convertie en bits.
            total_bits += mode;
            total_bits += nb_carac_bits;
            total_bits += chain_bits;
            int mediumsize = total_bits.Length;
            int difference = qrSize - mediumsize;
            if (difference >= 4)
            {
                total_bits += "0000";
            }
            else
            {
                for (var i = 0; i < difference; i++)
                    total_bits += "0";
            }
            int totalsize = total_bits.Length;
            bool modulo8 = totalsize % 8 == 0;
            do
            {
                total_bits += "0";
                totalsize = total_bits.Length;
                if (totalsize % 8 == 0) { modulo8 = true; }
            } while (modulo8 == false);

            return total_bits;
        }

        /// <summary>
        /// Retourne la chaine inégrale de bits de la taille max de bits de la version du code QR (avant l'ajout de l'erreur)
        /// </summary>
        /// <returns></returns>
        public string QR_Code_Chain()
        {
            int longueurchaine = chain.Length;
            string qr_intermediate = Chain_QR_Intermediate();
            string octet1 = "11101100";
            string octet2 = "00010001";
            int qrsize = 0;
            if (version == "1") { qrsize = 152; }
            if (version == "2") { qrsize = 272; }

            /// On détermine la différence entre la taille maximale de la chaine QR Code et la taille de la chaine
            int difference = qrsize - qr_intermediate.Length;
            /// On divise cette différence par 8 afin de connaitre le nombre d'octets de complétion à rajouter
            int missingbytes = difference / 8;
            Console.WriteLine(missingbytes);
            bool pair = missingbytes % 2 == 0;
            if (pair == true)
            {
                for (var i = 0; i < missingbytes / 2; i++)
                {
                    qr_intermediate += octet1;
                    qr_intermediate += octet2;
                }
            }
            else
            {
                var i = 0;
                while (i < missingbytes - 1)
                {
                    qr_intermediate += octet1;
                    i++;
                    qr_intermediate += octet2;
                    i++;
                }
                qr_intermediate += octet1;
            }
            return qr_intermediate;
        }

        /// <summary>
        /// Retourne sous forme de tableau d'octets la chaine de correction d'erreur (pour version 1).
        /// </summary>
        /// <param name="bytes">Chaine d'octets de 152 bits.</param>
        /// <returns></returns>
        public byte[] GetCorrectionErreurV1(byte[] bytes)
        {

            //byte[] result = ReedSolomonAlgorithm.Encode(bytes, 7);
            //Privilégiez l'écriture suivante car par défaut le type choisi est DataMatrix
            byte[] result = ReedSolomonAlgorithm.Encode(bytes, 7, ErrorCorrectionCodeType.QRCode);
            //Console.WriteLine("\nGET ReedSolomonAlgorithm:");
            //foreach (byte val in result) Console.Write(val + " ");
            Console.WriteLine();
            return result;
        }
        /// <summary>
        /// Retourne sous forme de tableau d'octets la chaine de correction d'erreur (pour version 2).
        /// </summary>
        /// <param name="bytes">Chaine d'octets de 272 bits.</param>
        /// <returns></returns>
        public byte[] GetCorrectionErreurV2(byte[] bytes)
        {
            //byte[] result = ReedSolomonAlgorithm.Encode(bytes, 7);
            //Privilégiez l'écriture suivante car par défaut le type choisi est DataMatrix 
            byte[] result = ReedSolomonAlgorithm.Encode(bytes, 10, ErrorCorrectionCodeType.QRCode);
            //Console.WriteLine("\nGET ReedSolomonAlgorithm:");
            //foreach (byte val in result) Console.Write(val + " ");
            //Console.WriteLine();
            return result;
        }

        /// <summary>
        /// Renvoie la entière du QR code à placer ensuite dans la matrice de pixels.
        /// </summary>
        /// <param name="qr_inter">Chaine de bits du QR code avant l'ajout de la chaine de correction d'erreurs.</param>
        /// <returns></returns>
        public string QRCode_with_errorV1(string qr_inter)
        {
            byte[] bytes = GetBytes(qr_inter);
            byte[] correctionerreur = GetCorrectionErreurV1(bytes);

            string correctionbits = "";
            foreach (byte b in correctionerreur)
            {
                string binaire = IntToBinary(Convert.ToInt32(b), 8);
                correctionbits += binaire;
            }
            return qr_inter + correctionbits;
        }
        public string QRCode_with_errorV2(string qr_inter)
        {
            byte[] bytes = GetBytes(qr_inter);
            byte[] correctionerreur = GetCorrectionErreurV2(bytes);
            string correctionbits = "";
            foreach (byte b in correctionerreur)
            {
                string binaire = IntToBinary(Convert.ToInt32(b), 8);
                correctionbits += binaire;
            }
            return qr_inter + correctionbits;
        }

        /// <summary>
        /// Convertit uen chaine de string correspondant à une chaine binaire en un tableau d'octets.
        /// </summary>
        /// <param name="bits">Chaine binaire à convertir en tableau d'octets.</param>
        /// <returns></returns>
        public byte[] GetBytes(string bits)
        {
            int elements = bits.Length / 8; //La taille du tableau d'octets est 8x inférieure à la chaine de bits
            string[] stringbytes = new string[elements];
            int start = 0;
            int length = 0;
            for (int i = 0; i < elements; i++)
            {
                string stringbyte = bits.Substring(start, 8);
                stringbytes[i] = stringbyte;
                length += stringbyte.Length;
                start += 8;
            }

            byte[] bytes = new byte[stringbytes.Length];

            
            //Console.WriteLine("BYTES:");
            for (int j = 0; j < stringbytes.Length; j++)
            {
                byte b = Convert.ToByte(stringbytes[j], 2);
                //Console.Write(stringbytes[j] + "(" + b.ToString() + ")  ");
                //Console.Write(stringbytes[j] + " ");
                bytes[j] = b;
            }
            //Console.WriteLine();
            
            return bytes;
            
        }
        #endregion

        #region Écriture de l'image
        public byte[,,] GenerateImage()
        {
            int version;
            if (chain.Length > 25) { version = 2; }
            else { version = 1; }
            byte[,,] qrpixels;
            if (version == 1) { qrpixels = new byte[21, 21, 3]; }
            else { qrpixels = new byte[25, 25, 3]; }

            /// Simple remplissage intégral de la matrice en vert, afin de pouvoir vérifer que la matrice se remplit correctement au fur et à mesure
            /// À supprimer une fois que la matrice se remplit et surtout s'affiche bien comme il faut.
            for (var i = 0; i < qrpixels.GetLength(0); i++)
            {
                for (var j = 0; j < qrpixels.GetLength(1); j++)
                {
                    qrpixels[i, j, 0] = 0;
                    qrpixels[i, j, 1] = 255;
                    qrpixels[i, j, 2] = 0;
                }
            }

            return qrpixels;
        }

        /// <summary>
        /// Ajoute les pixels de manière horizontale.
        /// </summary>
        /// <param name="matrix">Matrice du QR code.</param>
        /// <param name="pixelX">Coordonnée x (ou j) du module.</param>
        /// <param name="pixelY">Coordonnée y (ou i) du module.</param>
        /// <param name="length">Longueur horizontale sur laquelle ajouter les pixels.</param>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private byte[,,] AddHorizontalLine(byte[,,] matrix, int pixelX, int pixelY, int length, byte R, byte G, byte B)
        {
            for (int i = 0; i < length; i++)
            {
                int x = pixelX + i;
                matrix = SetModule(matrix, x, pixelY, R, G, B);
            }
            return matrix;
        }
        /// <summary>
        /// Ajoute les pixels de manière verticale.
        /// </summary>
        /// <param name="matrix">Matrice du QR code.</param>
        /// <param name="pixelX">Coordonnée x (ou j) du module.</param>
        /// <param name="pixelY">Coordonnée y (ou i) du module.</param>
        /// <param name="length">Longueur verticale sur laquelle ajouter les pixels.</param>
        /// <returns></returns>
        private byte[,,] AddVerticalLine(byte[,,] matrix, int pixelX, int pixelY, int length, byte R, byte G, byte B)
        {
            for (int i = 0; i < length; i++)
            {
                int y = pixelY + i;
                matrix = SetModule(matrix, pixelX, y, R, G, B);
            }
            return matrix;
        }
        /// <summary>
        /// Ajoute les pixels sous forme d'un carré, en ajoutant les pixels de manière horizontale, puis de manière verticale.
        /// </summary>
        /// <param name="matrix">Matrice du QR code.</param>
        /// <param name="x">Coordonnée x (ou j) du module.</param>
        /// <param name="y">Coordonnée y (ou i) du module.</param>
        /// <param name="length">Longueur sur laquelle ajouter les pixels.</param>
        /// <returns></returns>
        private byte[,,] AjouterCarre(byte[,,] matrix, int x, int y, int length, byte R, byte G, byte B)
        {
            matrix = AddHorizontalLine(matrix, x, y, length, R, G, B);
            matrix = AddHorizontalLine(matrix, x, y + length - 1, length, R, G, B);

            matrix = AddVerticalLine(matrix, x, y, length, R, G, B);
            matrix = AddVerticalLine(matrix, x + length - 1, y, length, R, G, B);

            return matrix;
        }

        /// <summary>
        /// Ajoute les motifs de recherche à leur emplacemenent dans la matrice du QR code.
        /// </summary>
        /// <param name="matrix">MAtrice du QR code.</param>
        /// <param name="imageWidth">Largeur du QR code.</param>
        /// <returns></returns>
        private byte[,,] AjouterMotifsRecherche(byte[,,] matrix, int imageWidth)
        {
            /// Carré noir exterieur de 7x7 - Haut Gauche
            /// On joute successivement la varré noir extérieur de 7x7, puis le carré blance de 5x5, puis le carré noir de 3x3, puis le carré noir de 1x1.
            matrix = AjouterCarre(matrix, 0, 0, 7, 0, 0, 0);
            matrix = AjouterCarre(matrix, 1, 1, 5, 255, 255, 255);
            matrix = AjouterCarre(matrix, 2, 2, 3, 0, 0, 0);
            matrix = AjouterCarre(matrix, 3, 3, 1, 0, 0, 0);

            /// Carré noir extérieur de 7x7 - Haut Droite
            /// Même principe de'ajout.
            matrix = AjouterCarre(matrix, imageWidth - 7, 0, 7, 0, 0, 0);
            matrix = AjouterCarre(matrix, imageWidth - 7 + 1, 1, 5, 255, 255, 255);
            matrix = AjouterCarre(matrix, imageWidth - 7 + 2, 2, 3, 0, 0, 0);
            matrix = AjouterCarre(matrix, imageWidth - 7 + 3, 3, 1, 0, 0, 0);

            /// Carré noir extérieur de 7x7 - bas gauche
            /// Même principe d'ajout.
            matrix = AjouterCarre(matrix, 0, imageWidth - 7 + 0, 7, 0, 0, 0);
            matrix = AjouterCarre(matrix, 1, imageWidth - 7 + 1, 5, 255, 255, 255);
            matrix = AjouterCarre(matrix, 2, imageWidth - 7 + 2, 3, 0, 0, 0);
            matrix = AjouterCarre(matrix, 3, imageWidth - 7 + 3, 1, 0, 0, 0);

            return matrix;
        }

        /// <summary>
        /// Ajoute le module sombre du QR code à son emplacement en fonction de la version.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="imageWidth">Largeur du QR code.</param>
        /// <param name="version">Version du code QR.</param>
        /// <returns></returns>
        private byte[,,] AjouterModuleSombre(byte[,,] source, int imageWidth, string version)
        {
            int v = Convert.ToInt32(version);
            source = SetModule(source, 8, (4 * v) + 9, 0, 0, 0);
            return source;
        }

        private byte[,,] AjouterMotifsSynchro(byte[,,] source, int imageWidth)
        {
            int pixelY = 6;
            int pixelX = 6;

            byte color = 0;
            for (int i = 0; i < 5; i++)
            {
                int x = 8 + i;

                source = SetModule(source, x, pixelY, color, color, color);

                if (color == 0)
                {
                    color = 255;
                }
                else
                {
                    color = 0;
                }
            }

            color = 0;
            for (int i = 0; i < 5; i++)
            {
                int y = 8 + i;
                source = SetModule(source, pixelX, y, color, color, color);
                if (color == 0)
                {
                    color = 255;
                }
                else
                {
                    color = 0;
                }
            }

            return source;
        }

        private byte[,,] AjouterSeperateur(byte[,,] source, int width)
        {
            // haut gauche
            source = AddHorizontalLine(source, 0, 7, 8, 255, 255, 255);
            source = AddVerticalLine(source, 7, 0, 8, 255, 255, 255);

            // haut droite
            source = AddHorizontalLine(source, width - 8, 7, 8, 255, 255, 255);
            source = AddVerticalLine(source, width - 8, 0, 8, 255, 255, 255);

            // bas gauche
            source = AddHorizontalLine(source, 0, width - 8, 8, 255, 255, 255);
            source = AddVerticalLine(source, 7, width - 8, 8, 255, 255, 255);

            return source;
        }

        private byte[,,] SetModule(byte[,,] source, char bit, int x, int y)
        {
            if (bit == '0')
            {
                source = SetModule(source, x, y, 255, 255, 255);
                //  Console.Write(" W");
            }
            else
            {
                source = SetModule(source, x, y, 0, 0, 0);
            }

            return source;
        }
        private byte[,,] SetModule(byte[,,] source, int x, int y, byte R, byte G, byte B)
        {
            source[y, x, 0] = R;
            source[y, x, 1] = G;
            source[y, x, 2] = B;
            return source;
        }


        private byte[,,] FillDataUp(byte[,,] source, string bits, int x, int y)
        {
            int count = 0;
            while (count < bits.Length)
            {
                char c1 = bits[count];
                char c2 = bits[count + 1];
                /// faire un bool if (y + x) mod 2 == 0, alors if c1 = 0 ==> c1 = 1, if c1 = 1 ==> c1 = c1
                c1 = ApplyMask(c1, x, y);
                c2 = ApplyMask(c2, x - 1, y);
                source = SetModule(source, c1, x, y);
                source = SetModule(source, c2, x - 1, y);
                y = y - 1;
                count = count + 2;
            }
            return source;
        }
        private byte[,,] FillDataDown(byte[,,] source, string bits, int x, int y)
        {
            int count = 0;
            while (count < bits.Length)
            {
                char c1 = bits[count];
                char c2 = bits[count + 1];
                c1 = ApplyMask(c1, x, y);
                c2 = ApplyMask(c2, x - 1, y);
                source = SetModule(source, c1, x, y);
                source = SetModule(source, c2, x - 1, y);
                y = y + 1;
                count = count + 2;
            }

            return source;
        }

        private byte[,,] AjouterBitsDonneeV1(byte[,,] source, string bits, int width)
        {
            string part = bits.Substring(0, 24); // 24 = 12x2
            source = FillDataUp(source, part, 20, 20);

            part = bits.Substring(24, 24);
            source = FillDataDown(source, part, 18, 9);

            part = bits.Substring(48, 24);
            source = FillDataUp(source, part, 16, 20);

            part = bits.Substring(72, 24);
            source = FillDataDown(source, part, 14, 9);

            part = bits.Substring(96, 28);
            source = FillDataUp(source, part, 12, 20);

            part = bits.Substring(124, 12);
            source = FillDataUp(source, part, 12, 5);

            part = bits.Substring(136, 12);
            source = FillDataDown(source, part, 10, 0);

            part = bits.Substring(148, 28);
            source = FillDataDown(source, part, 10, 7);

            part = bits.Substring(176, 8);
            source = FillDataUp(source, part, 8, 12);

            part = bits.Substring(184, 8);
            source = FillDataDown(source, part, 5, 9);

            part = bits.Substring(192, 8);
            source = FillDataUp(source, part, 3, 12);

            part = bits.Substring(200, 8);
            source = FillDataDown(source, part, 1, 9);

            return source;
        }
        private byte[,,] AjouterBitsDonneeV2(byte[,,] source, string bits, int width)
        {
            string part = bits.Substring(0, 32); // 32 = 16x2
            source = FillDataUp(source, part, 24, 24);

            part = bits.Substring(32, 32);
            source = FillDataDown(source, part, 22, 9);

            part = bits.Substring(64, 8);
            source = FillDataUp(source, part, 20, 24);
            /// Onsaut le motif d'alignement
            part = bits.Substring(72, 14);
            source = FillDataUp(source, part, 20, 15);

            part = bits.Substring(86, 14);
            source = FillDataDown(source, part, 18, 9);
            /// On resaute le motif d'alignement
            part = bits.Substring(100, 8);
            source = FillDataDown(source, part, 18, 21);

            part = bits.Substring(108, 8);
            source = FillDataUp(source, part, 16, 24);

            /// Seule 1 colonne est impactée par le mitof d'alignement, donc étant donné que la fonction FillDataDown() utilse 2 colonnes, on remplit manuellement les pixels.
            /// En s'assurant de ne pas oublier l'application du masque.
            char c = ApplyMask(bits[116], 15, 20);
            SetModule(source, c, 15, 20);
            c = ApplyMask(bits[117], 15, 19);
            SetModule(source, c, 15, 19);
            c = ApplyMask(bits[118], 15, 18);
            SetModule(source, c, 15, 18);
            c = ApplyMask(bits[119], 15, 17);
            SetModule(source, c, 15, 17);
            c = ApplyMask(bits[120], 15, 16);
            SetModule(source, c, 15, 16);

            part = bits.Substring(121, 18);
            source = FillDataUp(source, part, 16, 15);
            /// On saute les motifs de sychronisation
            part = bits.Substring(139, 12);
            source = FillDataUp(source, part, 16, 5);

            part = bits.Substring(151, 12);
            source = FillDataDown(source, part, 14, 0);
            /// On resaute le motif de synchronisation
            part = bits.Substring(163, 36);
            source = FillDataDown(source, part, 14, 7);

            part = bits.Substring(199, 36);
            source = FillDataUp(source, part, 12, 24);
            /// On saute le motif de sychronisation
            part = bits.Substring(235, 12);
            source = FillDataUp(source, part, 12, 5);

            part = bits.Substring(247, 12);
            source = FillDataDown(source, part, 10, 0);
            /// On resaute le motif de synchronisation
            part = bits.Substring(259, 36);
            source = FillDataDown(source, part, 10, 7);

            part = bits.Substring(295, 16);
            source = FillDataUp(source, part, 8, 16);

            part = bits.Substring(311, 16);
            source = FillDataDown(source, part, 5, 9);

            part = bits.Substring(327, 16);
            source = FillDataUp(source, part, 3, 16);

            part = bits.Substring(343, 8);
            source = FillDataDown(source, part, 1, 9);
            c = ApplyMask(bits[151], 1, 13);
            SetModule(source, c, 1, 13);
            c = ApplyMask('0', 0, 13);
            SetModule(source, c, 0, 13);
            part = "000000";
            source = FillDataDown(source, part, 1, 14);

            return source;
        }


        /// <summary>
        /// Ajoute les motifs d'alignement de la version 2.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        private byte[,,] AjouterMotifsAlignement(byte[,,] source, int imageWidth)
        {
            /// On ajoute le carré noir de 5x5.
            source = AjouterCarre(source, imageWidth - 9, imageWidth - 9, 5, 0, 0, 0);
            /// On ajoute ensuite le carré blanc de 3x3.
            source = AjouterCarre(source, imageWidth - 8, imageWidth - 8, 3, 255, 255, 255);
            /// On ajoute le module central noir du motif d'alignement
            source = SetModule(source, imageWidth - 7, imageWidth - 7, 0, 0, 0);
            return source;
        }


        public byte[,,] GenerateQRCodeImage(string bits)
        {
            byte[,,] matrix = null;

            if (version == "1")
            {
                matrix = new byte[21, 21, 3];
                matrix = AjouterMotifsRecherche(matrix, 21);
                matrix = AjouterSeperateur(matrix, 21);
                matrix = AjouterMotifsSynchro(matrix, 21);
                matrix = AjouterModuleSombre(matrix, 21, "1");
                matrix = AjouterMasqueV1(matrix);
                matrix = AjouterBitsDonneeV1(matrix, bits, 21);
            }
            if (version == "2")
            {
                matrix = new byte[25, 25, 3];
                matrix = AjouterMotifsRecherche(matrix, 25);
                matrix = AjouterSeperateur(matrix, 25);
                matrix = AjouterMotifsAlignement(matrix, 25);
                matrix = AjouterMotifsSynchro(matrix, 25);
                matrix = AjouterModuleSombre(matrix, 21, "2");
                matrix = AjouterMasqueV2(matrix);
                matrix = AjouterBitsDonneeV2(matrix, bits, 25);
            }
            
            /// Enregistrer l'image ???

            return matrix;
        }

        private char ApplyMask(char bit, int x, int y)
        {
            bool modulo2 = (x + y) % 2 == 0;
            if (modulo2 == true)
            {
                if (bit == '0') { bit = '1'; }
                else { bit = '0'; }
            }
            return bit;
        }

        private byte[,,] AjouterMasqueV1(byte[,,] matrix)
        {
            string masque = "1110111 11000100";
            matrix = SetModule(matrix, 0, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 1, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 2, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 3, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 4, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 5, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 7, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 8, 0, 0, 0); //1

            matrix = SetModule(matrix, 8, 7, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 5, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 4, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 3, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 2, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 1, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 0, 255, 255, 255); //0

            matrix = SetModule(matrix, 8, 20, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 19, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 18, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 17, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 16, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 15, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 14, 0, 0, 0); //1


            matrix = SetModule(matrix, 13, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 14, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 15, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 16, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 17, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 18, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 19, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 20, 8, 255, 255, 255); //0

            return matrix;
        }

        private byte[,,] AjouterMasqueV2(byte[,,] matrix)
        {
            string masque = "1110111 11000100";
            matrix = SetModule(matrix, 0, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 1, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 2, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 3, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 4, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 5, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 7, 8, 0, 0, 0); //1

            matrix = SetModule(matrix, 8, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 7, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 5, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 4, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 3, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 2, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 1, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 0, 255, 255, 255); //0

            matrix = SetModule(matrix, 8, 24, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 23, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 22, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 21, 255, 255, 255); //0
            matrix = SetModule(matrix, 8, 20, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 19, 0, 0, 0); //1
            matrix = SetModule(matrix, 8, 18, 0, 0, 0); //1


            matrix = SetModule(matrix, 17, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 18, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 19, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 20, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 21, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 22, 8, 0, 0, 0); //1
            matrix = SetModule(matrix, 23, 8, 255, 255, 255); //0
            matrix = SetModule(matrix, 24, 8, 255, 255, 255); //0

            return matrix;
        }

        #endregion
        /*
        private void Main(string[] args)
        {
            string chaine = "hello world".ToUpper();
            string bitsChaine = ConvertirChaine(chaine);


            string mode = "0010";
            string nombreCar = IntToBinary(chaine.Length, 9);
            string bits = mode + nombreCar + bitsChaine;
            bits = AjouterTerminaison(bits);
            bits = CompleterPourMulitple8(bits);
            bits = ToCapaciteMax(bits);

            byte[] bytes = GetBytes(bits);

            //Console.WriteLine();
            byte[] correctionErreur = GetErreur(bytes);

            string correctionBits = "";
            foreach (byte b in correctionErreur)
            {
                string binary = IntToBinary(Convert.ToInt32(b), 8);
                correctionBits += binary;
            }
            //  Console.WriteLine("correctionBits: " + correctionBits);

            string finalBits = bits + correctionBits;

            //Console.WriteLine(finalBits);

            GenerateQRCodeImage(finalBits);
            //Console.WriteLine(res.ToString());
        }
        */


    }
}