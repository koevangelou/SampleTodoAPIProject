using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace TodoApi.Models;

public class TodoItem
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public bool IsComplete { get; set; }

    public string UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; }
}