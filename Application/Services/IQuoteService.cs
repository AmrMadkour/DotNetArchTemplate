using Application.Results;
using Domain.Models;

namespace Application.Services;

public interface IQuoteService
{
   Task<Result<Quote, Error>> PrepareQuote(Order order);
}
