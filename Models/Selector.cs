
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MachineTrading.Enum;

namespace MachineTrading.Models;

public class Selector
{
    [Key]
    public int Id { get; set; }

    [StringLength(250)]
    public required string Value { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public SelectorType Type { get; set; }
}