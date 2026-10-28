namespace AmazonCopyDb.Data.Models;

public class Product{
	public int Id {get;set;}
	public int SellerId {get;set;}
	public User Seller {get;set;} = null!;
	public string Name {get;set;} = null!;
	public string? Description {get;set;}
	public decimal Price {get;set;}
	public ProductStatus Status {get;set;} = ProductStatus.Pending;
	public bool IsDeleted {get;set;}
	public int? ModeratedById {get;set;} 
	public User? ModeratedBy {get;set;}
	public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
	public DateTime? ArchivedAt {get;set;} 
}
