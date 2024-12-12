using System.Collections;

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
        
        ClearAndPrintEquation(variables);
        
        while (true)
        {
            try
            {
                InputEquationVariables(variables);

                ClearAndPrintEquation(variables);
                SolveEquation(variables);
                break;
            }
            catch (NoRootsException e)
            {
                FormatData(e.Message, Severity.Warning);
            }
            catch (Exception e)
            {
                FormatData(e.Message, Severity.Error, e.Data);
            }
        }
    }

    private static void ClearAndPrintEquation(Dictionary<char, int?> variables)
    {
        Console.Clear();
        Console.WriteLine($"{FormatVariable('a', variables['a'])} * x^2 {FormatVariable('b', variables['b'])} * x {FormatVariable('c', variables['c'])} = 0");
    }
    
    private static string FormatVariable(char variable, int? value)
    {
        if (value == null)
        {
            return variable == 'a' ? $"{variable}" : $"+ {variable}";
        }
        string sign;
        if (variable != 'a')
        {
            sign = value < 0  ? "- " : "+ ";
        }
        else
        {
            sign = value < 0  ? "- " : string.Empty;
        }
            
        return sign + Math.Abs(value.Value);
    }
    
    private static void InputEquationVariables(Dictionary<char, int?> variables)
    {
        var indexedKeys = variables.Keys.ToList();
        var currentValueIndex = 0;
        var inputValues = new List<string> { "", "", "" };

        do
        {
            ClearAndPrintEquation(variables);
            
            for (var i = 0; i < variables.Count; i++)
            {
                var pointer = i == currentValueIndex ? "> " : "  ";
                Console.WriteLine($"{pointer}{indexedKeys[i]}: {inputValues[i]}");
            }

            Console.SetCursorPosition(5 + inputValues[currentValueIndex].Length, currentValueIndex + 1);

            var key = Console.ReadKey();
            
            if (key.Key == ConsoleKey.Enter) break;
            
            NavigationKey(key.Key, ref currentValueIndex, variables.Count - 1);
            HandleInputKey(key, inputValues, currentValueIndex, variables, indexedKeys);
        } while (true);
    }
    
    private static void NavigationKey(ConsoleKey key, ref int currentValueIndex, int maxIndex)
    {
        if (key == ConsoleKey.DownArrow && currentValueIndex < maxIndex) currentValueIndex++;
        if (key == ConsoleKey.UpArrow && currentValueIndex > 0) currentValueIndex--;
    }

    private static void HandleInputKey(ConsoleKeyInfo key, List<string> inputValues, int currentValueIndex, Dictionary<char, int?> variables, List<char> indexedKeys)
    {
        if (char.IsLetterOrDigit(key.KeyChar) || (key.KeyChar == '-' && inputValues[currentValueIndex].Length == 0))
        {
            inputValues[currentValueIndex] += key.KeyChar;

            if (key.KeyChar == '-') return;
            
            if (!ValidateEquationInput(inputValues[currentValueIndex], indexedKeys[currentValueIndex], variables, out var result))
            {
                inputValues[currentValueIndex] = inputValues[currentValueIndex].Substring(0, inputValues[currentValueIndex].Length - 1);
            }
            else
            {
                variables[indexedKeys[currentValueIndex]] = result;
            }
        }
        else if (key.Key == ConsoleKey.Backspace && inputValues[currentValueIndex].Length > 0)
        {
            inputValues[currentValueIndex] = inputValues[currentValueIndex].Substring(0, inputValues[currentValueIndex].Length - 1);
        }
    }

    private static void SolveEquation(Dictionary<char, int?> variables)
    {
        try
        {
            var a = variables['a'].Value;
            var b = variables['b'].Value;
            var c = variables['c'].Value;

            var discriminant = Math.Pow(b, 2) - 4 * a * c;
            
            var x = (-b + SquareRoot(discriminant)) / (2 * a);
                
            if (discriminant == 0)
            {
                Console.WriteLine($"x = {x}");
            }
            else
            {
                var x2 = (-b - SquareRoot(discriminant)) / (2 * a);
                Console.WriteLine($"x1 = {x}, x2 = {x2}");
            }
        }
        catch (Exception e)
        {
            throw new NoRootsException("No real values found");
        }
    }
    
    private static double SquareRoot(double value)
    {
        if (value < 0) throw new Exception("Cannot square root a negative number");

        return Math.Sqrt(value);
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
        catch (OverflowException)
        {
            FormatData($"Number must be in range {int.MinValue} < \"{input}\" < {int.MaxValue}.", Severity.Notification);
            result = null;
            return false;
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

            FormatData($"Cannot parse the variable \"{input}\".", Severity.Error, e.Data);
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

    private static void FormatData(string message, Severity severity, IDictionary? data = null)
    {
        Console.SetCursorPosition(0, 5);
        
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
            case Severity.Notification:
                Console.BackgroundColor = ConsoleColor.DarkGreen;
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
    }
}