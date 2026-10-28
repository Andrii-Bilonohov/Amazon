namespace AmazonCopyDb.Data.Models;

public class Order{
	public int Id {get;set;} 
	public int BuyerId {get;set;}
	public User Buyer {get;set;} = null!;
	public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
	public List<OrderItem> Items {get;set;} = new();
	public Payment? Payment {get;set;}
}
public class OrderItem{
	public int Id {get;set;} 
	public int OrderId {get;set;}
	public Order Order {get;set;} = null!;
	public int ProductId {get;set;}
	public Product Product {get;set;} = null!;
	public int Quantity {get;set;} 
	public decimal PriceAtPurchase {get;set;}
	public ProductReturn? Return {get;set;}	

}
public class Payment{

	public int Id {get;set;} 
	public int OrderId {get;set;}
	public Order Order {get;set;} = null!;
	public PaymentStatus Status {get;set;} = PaymentStatus.Pending;
	public decimal Amount {get;set;}	
	public int? VerifiedById {get;set;}
	public User? VerifiedBy {get;set;}
}
public class ProductReturn {
	
	public int Id {get;set;} 
	public int OrderItemId {get;set;}
	public OrderItem OrderItem {get;set;} = null!;
	public DateTime ReturnedAt {get;set;} = DateTime.UtcNow;
}
