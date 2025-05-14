using System;
using System.Threading.Tasks;

namespace Ebertin.Services;

public static class Messages
{
    public static async Task ShowSuccessMessage(string message)
    {
        // Implement your success message display logic
        // For now, just console output
        Console.WriteLine($"Success: {message}");
    }
    
    public static async Task ShowErrorMessage(string message)
    {
        // Implement your error message display logic
        // For now, just console output
        Console.WriteLine($"Error: {message}");
    }
    
    
}