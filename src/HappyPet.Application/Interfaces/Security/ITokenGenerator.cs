using HappyPet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Application.Interfaces.Security
{
    public interface ITokenGenerator
    {
         string GenerateToken(User user);
    }
}
