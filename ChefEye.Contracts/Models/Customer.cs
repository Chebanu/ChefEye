using System.ComponentModel.DataAnnotations;

namespace ChefEye.Contracts.Models;

public class Customer
{
    [Key]
    public string Username { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
}