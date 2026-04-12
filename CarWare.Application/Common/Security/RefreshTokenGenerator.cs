using CarWare.Application.Common.Security;
using CarWare.Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;

public class RefreshTokenGenerator
{
    private readonly JWT _jwt;

    public RefreshTokenGenerator(IOptions<JWT> jwt)
    {
        _jwt = jwt.Value;
    }

    public RefreshToken Generate()
    {
        var randomNumber = new byte[32];
        RandomNumberGenerator.Fill(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpiresOn = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDurationDays),
            CreatedOn = DateTime.UtcNow
        };
    }
}