namespace Archie.Infrastructure.AzureSearch.Models;

public class SearchQuery
{
    public string Query { get; private set; } = string.Empty;
    public SearchType SearchType { get; private set; } = SearchType.Hybrid;
    public List<SearchFilter> Filters { get; private set; } = new();
    public int Top { get; private set; } = 50;
    public int Skip { get; private set; } = 0;

    public static SearchQuery Create(string query, SearchType searchType = SearchType.Hybrid)
    {
        return new SearchQuery
        {
            Query = query,
            SearchType = searchType
        };
    }

    public SearchQuery WithFilters(params SearchFilter[] filters)
    {
        Filters.AddRange(filters);
        return this;
    }

    public SearchQuery WithPaging(int top, int skip = 0)
    {
        Top = top;
        Skip = skip;
        return this;
    }
}

public enum SearchType
{
    Semantic,      // Vector search only
    Keyword,       // Text search only  
    Hybrid         // Combined vector + text search
}

public class SearchFilter
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "eq"; // eq, ne, gt, lt, contains
    public object Value { get; set; } = string.Empty;

    public static SearchFilter Equal(string field, object value)
    {
        return new SearchFilter { Field = field, Operator = "eq", Value = value };
    }

    public static SearchFilter Contains(string field, string value)
    {
        return new SearchFilter { Field = field, Operator = "contains", Value = value };
    }

    public static SearchFilter GreaterThan(string field, object value)
    {
        return new SearchFilter { Field = field, Operator = "gt", Value = value };
    }

    public static SearchFilter LessThan(string field, object value)
    {
        return new SearchFilter { Field = field, Operator = "lt", Value = value };
    }
}

public class SearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public double Score { get; set; }
    public SearchableDocument Document { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
}

public class SearchResults
{
    public long TotalCount { get; set; }
    public List<SearchResult> Results { get; set; } = new();
    public Dictionary<string, List<FacetResult>> Facets { get; set; } = new();
    public TimeSpan SearchDuration { get; set; }
}

public class FacetResult
{
    public string Value { get; set; } = string.Empty;
    public long Count { get; set; }
}