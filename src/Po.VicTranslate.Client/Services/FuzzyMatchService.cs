using System;
using System.Collections.Generic;
using System.Linq;

namespace Po.VicTranslate.Client.Services;

/// <summary>
/// Service for fuzzy string matching using Levenshtein distance algorithm.
/// Used for smart search functionality to find songs with partial or misspelled queries.
/// </summary>
public class FuzzyMatchService
{
    /// <summary>
    /// Finds the best matches for a query from a list of candidates.
    /// Uses Levenshtein distance for similarity scoring.
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="candidates">List of candidates to search through</param>
    /// <param name="maxResults">Maximum number of results to return (default: 5)</param>
    /// <param name="threshold">Minimum similarity threshold 0-1 (default: 0.4)</param>
    /// <returns>List of matches with their similarity scores, sorted by best match first</returns>
    public List<FuzzyMatch> FindMatches(
        string query, 
        IEnumerable<string> candidates, 
        int maxResults = 5, 
        double threshold = 0.4)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<FuzzyMatch>();

        var queryLower = query.ToLowerInvariant().Trim();
        var matches = new List<FuzzyMatch>();

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var candidateLower = candidate.ToLowerInvariant();
            
            // Calculate similarity score
            var score = CalculateSimilarity(queryLower, candidateLower);
            
            // Boost score for exact prefix matches
            if (candidateLower.StartsWith(queryLower))
                score = Math.Min(1.0, score + 0.3);
            
            // Boost score for word boundary matches
            if (candidateLower.Contains(" " + queryLower) || 
                candidateLower.Contains(queryLower + " "))
                score = Math.Min(1.0, score + 0.2);

            if (score >= threshold)
            {
                matches.Add(new FuzzyMatch
                {
                    Text = candidate,
                    Score = score,
                    MatchIndices = FindMatchIndices(queryLower, candidateLower)
                });
            }
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Text.Length) // Prefer shorter matches when scores are equal
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Calculates similarity between two strings using normalized Levenshtein distance.
    /// Returns a value between 0 (no match) and 1 (exact match).
    /// </summary>
    private double CalculateSimilarity(string source, string target)
    {
        if (source == target)
            return 1.0;

        var distance = LevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);
        
        if (maxLength == 0)
            return 1.0;

        return 1.0 - ((double)distance / maxLength);
    }

    /// <summary>
    /// Calculates the Levenshtein distance (edit distance) between two strings.
    /// This is the minimum number of single-character edits (insertions, deletions, substitutions)
    /// required to change one string into the other.
    /// </summary>
    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;
        
        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var matrix = new int[sourceLength + 1, targetLength + 1];

        // Initialize first column and row
        for (var i = 0; i <= sourceLength; i++)
            matrix[i, 0] = i;
        
        for (var j = 0; j <= targetLength; j++)
            matrix[0, j] = j;

        // Fill the matrix
        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                
                matrix[i, j] = Math.Min(
                    Math.Min(
                        matrix[i - 1, j] + 1,      // deletion
                        matrix[i, j - 1] + 1),     // insertion
                    matrix[i - 1, j - 1] + cost);  // substitution
            }
        }

        return matrix[sourceLength, targetLength];
    }

    /// <summary>
    /// Finds the character indices in the target string that match characters from the query.
    /// Used for highlighting matched characters in the UI.
    /// </summary>
    private List<int> FindMatchIndices(string query, string target)
    {
        var indices = new List<int>();
        var queryIndex = 0;
        
        for (var i = 0; i < target.Length && queryIndex < query.Length; i++)
        {
            if (target[i] == query[queryIndex])
            {
                indices.Add(i);
                queryIndex++;
            }
        }

        return indices;
    }
}

/// <summary>
/// Represents a fuzzy match result with similarity scoring and highlighting information.
/// </summary>
public class FuzzyMatch
{
    /// <summary>
    /// The matched text from the candidate list.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Similarity score between 0 and 1, where 1 is an exact match.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Indices of characters in the text that match the query, used for highlighting.
    /// </summary>
    public List<int> MatchIndices { get; set; } = new();

    /// <summary>
    /// Formats the match percentage for display (e.g., "85%").
    /// </summary>
    public string ScorePercent => $"{Score * 100:F0}%";
}
