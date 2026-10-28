using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdblokowanieZS
{
    public class ZS
    {
        
             public int GIDNumer { set; get; }
             public int GIDTyp { set; get; }
             public int GIDFirma { set; get; }
             public short GIDLp { set; get; }

        

        public ZS(int GidNumer, int GidFirma, short GidLp, int GidTyp)
        {
            this.GIDNumer = GidNumer;
            this.GIDFirma = GidFirma;
            this.GIDLp = GidLp;
            this.GIDTyp = GidTyp;
        }
    }
}
