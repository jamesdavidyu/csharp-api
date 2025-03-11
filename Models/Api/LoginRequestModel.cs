using System;

namespace csharp_api.Models.Api;

public class LoginRequestModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
