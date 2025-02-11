using BixolonPrinter.Maui.Models;

namespace BixolonPrinter.Maui.Services
{
    public interface IPrinterService
    {
        Task<bool> Connect(string macAddress);
        Task PrintText(string text);
        Task PrintTicket(List<TicketItem> items);

        Task PrintImage(string imageName);
       
        Task Disconnect();
    }
}
