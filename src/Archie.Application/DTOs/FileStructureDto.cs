namespace Archie.Application.DTOs;

public class FileStructureDto
{
    public IEnumerable<FolderDto> Folders { get; init; } = new List<FolderDto>();
    public IEnumerable<FileDto> Files { get; init; } = new List<FileDto>();
}

public class FolderDto
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public IList<FileDto> Files { get; init; } = new List<FileDto>();
    public IList<SubfolderDto> Subfolders { get; init; } = new List<SubfolderDto>();
}

public class FileDto
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Language { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
}

public class SubfolderDto
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
}