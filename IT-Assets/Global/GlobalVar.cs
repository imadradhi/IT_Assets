using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT_Assets.Global
{
    public static class GlobalVar
    {
        public static string IdToken {  get; set; }
        public static string UserEmail { get; set; }
        public static string DatabaseUrl { get; set; } = "https://assetsmanagement-b8a6f-default-rtdb.firebaseio.com/";

    }
}
