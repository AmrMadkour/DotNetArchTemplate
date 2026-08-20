using Application.Results;
using Domain.Models;

namespace Application.Services;

public interface IQuoteService
{
   Task<Result<Quote>> PrepareQuote(Order order);
}
