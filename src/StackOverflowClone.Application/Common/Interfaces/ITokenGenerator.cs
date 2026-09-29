using StackOverflowClone.Application.Common.Models;
using StackOverflowClone.Domain.Entities;

namespace StackOverflowClone.Application.Common.Interfaces;

public interface ITokenGenerator
{
    AccessToken Generate(User user);
}
