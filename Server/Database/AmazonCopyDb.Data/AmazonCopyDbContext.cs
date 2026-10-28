using Microsoft.EntityFrameworkCore;
using AmazonCopyDb.Data.Models;

namespace AmazonCopyDb.Data;

public class AmazonCopyDbContext : DbContext
{
	public AmazonCopyDbContext(DbContextOptions<AmazonCopyDbContext> options) : base(options) {}
	
	public DbSet<User> Users => Set<User>();
	public DbSet<Product> Products => Set<Product>();
	public DbSet<CartItem> CartItems => Set<CartItem>();
	public DbSet<Order> Orders  => Set<Order>();
	public DbSet<OrderItem> OrderItems  => Set<OrderItem>();
	public DbSet<Payment> Payments  => Set<Payment>();
	public DbSet<ProductReturn> ProductReturns => Set<ProductReturn>();
	public DbSet<ModerationLog> ModerationLogs => Set<ModerationLog>();

	protected override void OnModelCreating(ModelBuilder b){
		b.Entity<User>(e =>
		{
			
			e.HasIndex(u => u.Email).IsUnique();
			e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
			e.HasQueryFilter(u => !u.IsDeleted);
		});
		b.Entity<Product>(e =>
		{
			e.Property(p => p.Price).HasPrecision(18,2);
			e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
			e.ToTable(t => t.HasCheckConstraint("CK_Product_Price", "\"Price\" > 0"));
			e.HasOne(p => p.Seller).WithMany(u => u.Products)
				.HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Restrict);
			e.HasOne(p => p.ModeratedBy).WithMany()
				.HasForeignKey(p => p.ModeratedById).OnDelete(DeleteBehavior.Restrict);
			e.HasQueryFilter(p => !p.IsDeleted);
		});
		b.Entity<CartItem>(e =>
		{
			e.HasIndex(c => new {c.UserId, c.ProductId}).IsUnique();
			e.HasOne(c => c.User).WithMany(u => u.CartItems)
				.HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
			e.HasOne(c => c.Product).WithMany()
				.HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
		});
		b.Entity<Order>()
			.HasOne(o => o.Buyer).WithMany(u => u.Orders)
			.HasForeignKey(o => o.BuyerId).OnDelete(DeleteBehavior.Restrict);
		b.Entity<OrderItem>(e =>
		{
			e.Property(i => i.PriceAtPurchase).HasPrecision(18,2);
			e.HasOne(i => i.Product).WithMany()
				.HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
		});
		b.Entity<Payment>(e =>
		{
			e.Property(p => p.Amount).HasPrecision(18,2);
			e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
			e.HasOne(p => p.Order).WithOne(o => o.Payment)
				.HasForeignKey<Payment>(p => p.OrderId);
			e.HasOne(p => p.VerifiedBy).WithMany()
				.HasForeignKey(p => p.VerifiedById).OnDelete(DeleteBehavior.Restrict);
		});
		b.Entity<ProductReturn>().HasOne(r => r.OrderItem).WithOne(i => i.Return).HasForeignKey<ProductReturn>(r => r.OrderItemId);
		b.Entity<ModerationLog>(e =>
		{
			e.Property(l => l.Action).HasConversion<string>().HasMaxLength(30);
			e.HasOne(l => l.Actor).WithMany()
				.HasForeignKey(l => l.ActorId).OnDelete(DeleteBehavior.Restrict);
		});
	
	} 
}
