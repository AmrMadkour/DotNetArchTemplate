using API.Models;

namespace API.Interfaces
{
    public interface IQuoteService
    {
        Task<Quote> PrepareQuote(Order order);
    }
}
