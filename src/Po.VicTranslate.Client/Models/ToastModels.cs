namespace Po.VicTranslate.Client.Models;

/// <summary>
/// Types of toast notifications
/// </summary>
public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// Positions for toast notifications on screen
/// </summary>
public enum ToastPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}
