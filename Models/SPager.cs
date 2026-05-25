namespace WebBanHang.Models
{
    public class SPager
    {
        public int TotalItems { get; set; }
        public int Currentpage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public SPager() { }
        public SPager(int totalItems, int page, int pageSize = 10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;
            int startPage = currentPage - 5;
            int endPage = currentPage + 4;
            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;

            }
            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 19;
                }
            }
            TotalItems = totalItems;
            Currentpage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
