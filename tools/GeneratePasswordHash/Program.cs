using Microsoft.AspNetCore.Identity;

namespace GeneratePasswordHash;

internal static class Program
{
    private static int Main(string[] args)
    {
        var password = args.Length > 0 ? args[0] : Console.In.ReadLine();

        if (string.IsNullOrEmpty(password))
        {
            Console.Error.WriteLine("Usage: GeneratePasswordHash <password>  (or pipe the password via stdin)");
            return 1;
        }

        var hash = new PasswordHasher<object>().HashPassword(null!, password);
        Console.WriteLine(hash);
        return 0;
    }
}