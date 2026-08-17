using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Services
{
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
}
