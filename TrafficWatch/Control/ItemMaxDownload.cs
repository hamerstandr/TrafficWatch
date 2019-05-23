using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficWatch.Control
{
    public class ItemMaxDownload
    {
        public ItemMaxDownload(double Download, DateTime Date)
        {
            this.Download = Download;
            this.Date = Date;
        }
        public DateTime Date;
        public double Download;
    }
}
