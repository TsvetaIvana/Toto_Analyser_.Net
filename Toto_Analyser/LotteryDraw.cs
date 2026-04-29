using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toto_Analyser
{
    public class LotteryDraw : IEnumerable
    {
        public int DrawNumber { get; set; }
        public int[] Numbers { get; set; }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
