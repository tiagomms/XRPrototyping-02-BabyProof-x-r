using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GroqApiLibrary;

/// <summary>
/// Pure service class for AI assistant reasoning. Contains only core, reusable, and testable logic.
/// </summary>
public class GroqReasoningService
{
    /// <summary>
    /// The model name to use for reasoning.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// The current user intent.
    /// </summary>
    public string UserIntent { get; set; }

    /// <summary>
    /// The current system message.
    /// </summary>
    public string CurrentSystemMessage { get; set; }

    /// <summary>
    /// The list of tools available for reasoning.
    /// </summary>
    public List<ExtendedTool> Tools { get; set; } = new List<ExtendedTool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GroqReasoningService"/> class.
    /// </summary>
    /// <param name="model">The model name.</param>
    /// <param name="currentSystemMessage">The current system message.</param>
    /// <param name="tools">The list of tools (optional).</param>
    public GroqReasoningService(
        string model,
        string currentSystemMessage,
        List<ExtendedTool> tools = null)
    {
        Model = model;
        CurrentSystemMessage = currentSystemMessage;
        if (tools != null)
            Tools = tools;
    }

    /// <summary>
    /// Runs the reasoning process using the provided API delegate.
    /// </summary>
    /// <param name="userIntent">The user intent.</param>
    /// <param name="runConversationWithToolsAsync">The delegate to call the API.</param>
    /// <returns>The API response as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the delegate is null.</exception>
    public virtual async Task<string> RunReasoningAsync(
        string userIntent,
        Func<string, List<ExtendedTool>, string, string, Task<string>> runConversationWithToolsAsync)
    {
        if (runConversationWithToolsAsync == null)
            throw new ArgumentNullException(nameof(runConversationWithToolsAsync));

        return await runConversationWithToolsAsync(
            userIntent,
            Tools,
            Model,
            CurrentSystemMessage
        );
    }

    /// <summary>
    /// Adds a tool to the list.
    /// </summary>
    public void AddTool(ExtendedTool tool)
    {
        if (tool != null)
            Tools.Add(tool);
    }

    /// <summary>
    /// Removes a tool from the list.
    /// </summary>
    public void RemoveTool(ExtendedTool tool)
    {
        if (tool != null)
            Tools.Remove(tool);
    }
} 