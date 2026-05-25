namespace WebBanHang.Models
{
	public class Comment
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public int ProductID { get; set; }
		public DateTime CommentDate { get; set; }

        public int? ParentCommentId { get; set; } // Nullable ParentCommentId for replies

        public virtual Product Product { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
		
	}
}
