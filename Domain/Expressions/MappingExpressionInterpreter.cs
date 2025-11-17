using System.Globalization;
using DynamicExpresso;

namespace Domain.Expressions;

 public static class MappingExpressionInterpreter
{
    public static string EvaluateToString(string expressionText, IReadOnlyDictionary<string, string> variableValues)
    {
        if (string.IsNullOrWhiteSpace(expressionText))
        {
            return string.Empty;
        }

        Interpreter interpreter = CreateInterpreter();

        interpreter.SetVariable("row", variableValues);

        foreach (KeyValuePair<string, string> variablePair in variableValues)
        {
            if (IsValidIdentifier(variablePair.Key))
            {
                interpreter.SetVariable(variablePair.Key, variablePair.Value);
            }
        }

        object evaluationResult = interpreter.Eval(expressionText);

        return ConvertResultToString(evaluationResult);
    }

    private static Interpreter CreateInterpreter()
    {
        Interpreter interpreter = new();
        interpreter.Reference(typeof(string));
        interpreter.Reference(typeof(Math));
        interpreter.Reference(typeof(DateTime));
        interpreter.Reference(typeof(Convert));
        return interpreter;
    }

    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        char firstCharacter = name[0];
        if (!char.IsLetter(firstCharacter) && firstCharacter != '_')
        {
            return false;
        }

        for (int index = 1; index < name.Length; index++)
        {
            char character = name[index];
            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static string ConvertResultToString(object? evaluationResult)
    {
        if (evaluationResult == null)
        {
            return string.Empty;
        }

        if (evaluationResult is string stringResult)
        {
            return stringResult;
        }

        string? convertedResult = Convert.ToString(evaluationResult, CultureInfo.InvariantCulture);
        return convertedResult ?? string.Empty;
    }
}