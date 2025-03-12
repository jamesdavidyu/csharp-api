using System;

namespace csharp_api.Models.Api;

public class LoginResponseModel
{
    public required string UserName { get; set; }
    public required string AccessToken { get; set; }
    public required int ExpiresIn { get; set; }
}
