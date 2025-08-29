using System.ComponentModel.DataAnnotations;

namespace active_directory_rest_api.Models.DTOs
{
    public class OrganizationalUnitDto
    {
        public string? DistinguishedName { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? ManagedBy { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Telephone { get; set; }
        public string? Fax { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? ParentOU { get; set; }
        public string[]? ChildOUs { get; set; }
        public string[]? Users { get; set; }
        public string[]? Groups { get; set; }
    }

    public class CreateOrganizationalUnitDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? ManagedBy { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Telephone { get; set; }
        public string? Fax { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? ParentOU { get; set; }
    }

    public class UpdateOrganizationalUnitDto
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? ManagedBy { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Telephone { get; set; }
        public string? Fax { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
    }
}
