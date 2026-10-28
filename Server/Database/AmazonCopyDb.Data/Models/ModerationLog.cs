namespace AmazonCopyDb.Data.Models;

public class ModerationLog{
	public int Id  {get;set;}
	public int ActorId {get;set;}
	public User Actor {get;set;} = null!;
	public ModerationActionType Action {get;set;} 
	public int? TargetUserId {get;set;}
	public int? TargetProductId {get;set;}
	public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
}
