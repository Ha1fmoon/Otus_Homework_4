using System.Collections;
using System.Text;

namespace ConsoleApp6;

class Program
{
    static void Main()
    {
        var variables = new Dictionary<char, int?>
        {
            {'a', null}, 
            {'b', null}, 
            {'c', null}
            
        };
        
        ClearAndPrintEquation();
        
        while (true)
        {
            try
            {
                InputEquationVariables(variables);
                
                ClearAndPrintEquation();
                SolveEquation(variables);
                break;
            }
            catch (NoRootsException e)
            {
                FormatData(e.Message, Severity.Warning, e.Data);
            }
            catch (Exception e)
            {
                FormatData(e.Message, Severity.Error, e.Data);
            }
        }
    }

    private static void ClearAndPrintEquation()
    {
        Console.Clear();
        Console.WriteLine("a * x^2 + b * x + c = 0");
    }
    
    private static void InputEquationVariables(Dictionary<char, int?> variables)
    {
        foreach (var variable in variables.Keys)
        {
            do
            {
                Console.Write($"Enter a number for \"{variable}\": ");
                
                var input = Console.ReadLine()?? string.Empty;
                if (!ValidateEquationInput(input, variable, variables, out var result)) continue;
                
                variables[variable] = result;

                ClearAndPrintEquation();
                
            } while(variables[variable] == null);
        }
    }

    private static void SolveEquation(Dictionary<char, int?> variables)
    {
        var a = variables['a'].Value;
        var b = variables['b'].Value;
        var c = variables['c'].Value;

        var discriminant = (int)Math.Pow(b, 2) - 4 * a * c;

        if (discriminant > 0)
        {
            var x1 = (-b + (int)Math.Sqrt(discriminant)) / (2 * a);
            var x2 = (-b - (int)Math.Sqrt(discriminant)) / (2 * a);
            Console.WriteLine($"x1 = {x1}, x2 = {x2}");
        }
        else if (discriminant == 0)
        {
            var x = -b / (2 * a);
            Console.WriteLine($"x = {x}");
        }
        else
        {
            throw new NoRootsException(discriminant);
        }
    }

    private static bool ValidateEquationInput(string input, char variable, Dictionary<char, int?> variables, out int? result)
    {
        if (!CustomIntTryParse(input, variable, variables, out result)) return false;
        if (!CheckAIsNotZero(variable, result)) return false;
        if (VariableIsNull(result)) return false;
        
        return true;
    }
    
    private static bool CustomIntTryParse(string input, char variable, Dictionary<char, int?> variables, out int? result)
    {
        try
        {
            result = int.Parse(input);
            return true;
        }
        catch (Exception e)
        {
            foreach (var pair in variables)
            {
                if (pair.Key == variable)
                {
                    e.Data[pair.Key] = $"{input} <- This value must be a number.";
                    continue;
                }

                if (pair.Value == null)
                {
                    e.Data[pair.Key] = "?";
                    continue;
                }
                
                e.Data[pair.Key] = pair.Value;
            }
            FormatData(e.Message, Severity.Error, e.Data);
            result = null;
            return false;
        }
    }

    private static bool CheckAIsNotZero(char variable, int? result)
    {
        if (variable == 'a' && result == 0)
        {
            FormatData("Variable \"a\" cannot be zero.", Severity.Warning);
            return false;
        }

        return true;

    }

    private static bool VariableIsNull(int? variable)
    {
        return variable == null;
    }

    static void FormatData(string message, Severity severity, IDictionary? data = null)
    {
        Console.Clear();
        
        switch (severity)
        {
            case Severity.Error:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case Severity.Warning:
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                break;
        }
        
        var separator = new string('-', message.Length);
        
        Console.WriteLine(separator);
        Console.WriteLine(message);
        Console.WriteLine(separator);

        if (data != null && data.Count > 0)
        {
            foreach (DictionaryEntry row in data)
            {
                Console.WriteLine($"{row.Key} = {row.Value}");
            }
        }
        
        Console.ResetColor();

        Console.WriteLine("Press any key to retry...");
        Console.ReadKey();
        
        ClearAndPrintEquation();
    }
}