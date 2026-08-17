using Domain.Models;

namespace Application;

public class QuoteService : IQuoteService
{
    public QuoteService()
    {

    }
    public async Task<Quote> PrepareQuote(Order order)
    {
        throw new Exception();
        //throw new BadHttpRequestException();
        throw new NotImplementedException();
    }
}
