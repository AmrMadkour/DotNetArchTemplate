using Application.Results;
using Domain.Models;

namespace Application.Services;

public class QuoteService : IQuoteService
{
    public QuoteService()
    {

    }
    public async Task<Result<Quote, Error>> PrepareQuote(Order order)
    {
        throw new NotImplementedException();
    }
}
