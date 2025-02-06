
using System.ComponentModel.DataAnnotations;

namespace MachineTrading.Models;

public class Article
{
    [Key]
    [StringLength(450)]
    public required string Link { get; set; }

    [StringLength(maximumLength: 100)]
    public string? ParentLink { get; set; }

    [StringLength(maximumLength: 25)]
    public required string Time { get; set; }

    public string? Text { get; set; }

    [StringLength(maximumLength: 150)]
    public required string UserTitle { get; set; }

    [StringLength(maximumLength: 5)]
    public string? CommentCount { get; set; }

    [StringLength(maximumLength: 5)]
    public string? ReShareCount { get; set; }

    [StringLength(maximumLength: 5)]
    public string? LikeCount { get; set; }

    public DateTime CreateDate { get; set; }
}