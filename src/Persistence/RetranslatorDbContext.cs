using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class RetranslatorDbContext : DbContext, IUnitOfWork
{

}
