using System.ComponentModel.DataAnnotations;

namespace ABCRetail.StorageWeb.Models;

public sealed class CustomerProfileInputModel
{
    [Display(Name = "Segment / Partition Key")]
    [StringLength(64, ErrorMessage = "Please keep the segment under 64 characters.")]
    public string Segment { get; set; } = string.Empty;

    [Display(Name = "Customer ID (Row Key)")]
    [StringLength(128, ErrorMessage = "Row keys must be under 128 characters.")]
    public string? CustomerId { get; set; }

    [Required(ErrorMessage = "Please capture the customer's first name.")]
    [StringLength(100, ErrorMessage = "First names need to stay under 100 characters.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Please capture the customer's last name.")]
    [StringLength(100, ErrorMessage = "Last names need to stay under 100 characters.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Please capture the customer's email address.")]
    [EmailAddress(ErrorMessage = "Please capture a valid email address.")]
    public string? Email { get; set; }

    [Display(Name = "Loyalty Tier")]
    [StringLength(50, ErrorMessage = "Loyalty tier labels must be under 50 characters.")]
    public string? LoyaltyTier { get; set; }

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please capture a valid contact number.")]
    public string? PhoneNumber { get; set; }
}
