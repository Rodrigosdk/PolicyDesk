using System.Text;

namespace CommonTestUtilities.Requests;

public static class CpfGenerator
{
    public static string Generate(Random? random = null, bool formatted = false)
    {
        random ??= Random.Shared;

        int[] digits;
        do
        {
            digits = Enumerable.Range(0, 9).Select(_ => random.Next(0, 10)).ToArray();
        } while (digits.Distinct().Count() == 1);

        var all = digits.Append(CheckDigit(digits)).ToArray();
        all = all.Append(CheckDigit(all)).ToArray();

        var raw = new StringBuilder();
        foreach (var digit in all)
            raw.Append(digit);

        var cpf = raw.ToString();
        return formatted ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}" : cpf;
    }

    private static int CheckDigit(int[] digits)
    {
        var weight = digits.Length + 1;
        var sum = digits.Sum(digit => digit * weight--);
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
