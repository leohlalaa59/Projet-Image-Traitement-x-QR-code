using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Problème_scientifique
{
    class Complex
    {
        public double a;
        public double b;

        public Complex(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public Complex AuCarré()
        {
            double re_carré = (a * a) - (b * b);
            double im_carré = 2 * a * b;
            return new Complex(re_carré, im_carré);
        }

        public double Magnitude()
        {
            return Math.Sqrt((a * a) + (b * b));
        }

        public Complex Add(Complex c)
        {
            Complex somme = new Complex(c.a, c.b);
            return somme;
        }
    }
}
