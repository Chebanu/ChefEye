using System.ComponentModel.DataAnnotations;

namespace ChefEye.Contracts.Models;

public class Customer
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Phone { get; set; }
}