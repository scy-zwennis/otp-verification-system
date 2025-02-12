using System.Security.Cryptography;

public static class OneTimePinUtils
{
    public static string GenerateCode()
    {
        return GenerateRandomCode();
    }

    public static string GenerateCode(IEnumerable<string> existingCodes, int maxAttempts = 100)
    {
        var existingSet = existingCodes.ToHashSet();
        
        for (var i = 0; i < maxAttempts; i++)
        {
            var code = GenerateRandomCode();

            if (!existingSet.Contains(code))
            {
                return code;
            }
        }
        
        throw new TimeoutException("Failed to generate unique OTP after maximum attempts");
    }

    private static string GenerateRandomCode()
    {
        var randomBytes = new byte[4];
        RandomNumberGenerator.Fill(randomBytes);

        var randomInt = Math.Abs(BitConverter.ToInt32(randomBytes)) % 1000000;
        return randomInt.ToString("D6");
    }
}