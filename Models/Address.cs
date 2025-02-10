using System.ComponentModel.DataAnnotations;

namespace MachineTrading.Models;

public class Address
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public required string Url { get; set; }

    public virtual ICollection<Article>? Articles { get; set; }
}