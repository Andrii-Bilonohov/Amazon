namespace AmazonCopyDb.Data.Models;


public enum UserRole {Buyer , Seller , Moderator, Admin }
public enum ProductStatus { Pending , Approved , Rejected , Archived }
public enum PaymentStatus {Pending, Verified, Rejcted }
public enum ModerationActionType {
	
	ProductApproved,ProductRejected,UserBlocked,UserDeleted,
	PaymentVerified,PaymentRejected, CartItemRemoved,
	ModeratorAdded, ModeratorRemoved 

}
