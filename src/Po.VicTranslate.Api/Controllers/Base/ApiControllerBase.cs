using Microsoft.AspNetCore.Mvc;

namespace Po.VicTranslate.Api.Controllers.Base;

/// <summary>
/// Base controller with shared error handling logic
/// Implements Template Method Pattern for consistent exception handling
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ILogger _logger;

    protected ApiControllerBase(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes an action with standardized error handling
    /// </summary>
    /// <typeparam name="T">The return type of the action</typeparam>
    /// <param name="action">The action to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="errorMessage">User-friendly error message</param>
    /// <returns>ActionResult with appropriate status code</returns>
    protected async Task<ActionResult<T>> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> action,
        string operationName,
        string errorMessage)
    {
        try
        {
            var result = await action();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {OperationName}", operationName);
            return StatusCode(500, errorMessage);
        }
    }

    /// <summary>
    /// Executes an action with standardized error handling (no return value)
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="errorMessage">User-friendly error message</param>
    /// <param name="successMessage">Success message to return</param>
    /// <returns>ActionResult with appropriate status code</returns>
    protected async Task<ActionResult> ExecuteWithErrorHandlingAsync(
        Func<Task> action,
        string operationName,
        string errorMessage,
        string successMessage)
    {
        try
        {
            await action();
            return Ok(successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {OperationName}", operationName);
            return StatusCode(500, errorMessage);
        }
    }

    /// <summary>
    /// Executes an action that may return null with standardized error handling
    /// </summary>
    /// <typeparam name="T">The return type of the action</typeparam>
    /// <param name="action">The action to execute</param>
    /// <param name="operationName">Name of the operation for logging</param>
    /// <param name="errorMessage">User-friendly error message</param>
    /// <param name="notFoundMessage">Message to return when result is null</param>
    /// <returns>ActionResult with appropriate status code</returns>
    protected async Task<ActionResult<T>> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T?>> action,
        string operationName,
        string errorMessage,
        string notFoundMessage) where T : class
    {
        try
        {
            var result = await action();
            if (result == null)
            {
                return NotFound(notFoundMessage);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {OperationName}", operationName);
            return StatusCode(500, errorMessage);
        }
    }
}
