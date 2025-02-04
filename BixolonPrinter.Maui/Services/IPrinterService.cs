using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BixolonPrinter.Maui.Services
{
    public interface IPrinterService
    {
        Task<bool> Connect(string macAddress);
        Task PrintText(string text);
        Task Disconnect();
    }
}
