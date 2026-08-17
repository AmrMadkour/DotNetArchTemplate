using Domain.Models;

namespace Application;

public interface IQuoteService
{
    Task<Quote> PrepareQuote(Order order);
}
