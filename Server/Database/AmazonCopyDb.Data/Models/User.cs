namespace AmazonCopyDb.Data.Models;

public class User{
	public int Id {get;set;}
	public string  Email {get;set;} = null;
	public string  PasswordHash {get;set;} = null;
	public UserRole Role {get;set;}
	public bool IsBlocked {get;set;}
	public bool IsDeleted {get;set;}
	public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

	public List<Product> Products {get;set;} = new();
	public List<CartItem> CartItems {get;set;} = new();
	public List<Order> Orders {get;set;} = new();
}
