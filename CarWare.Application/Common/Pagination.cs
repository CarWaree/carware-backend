namespace CarWare.Application.Common;
    using System.Collections.Generic;
using System;

public  class Pagination<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize == 0) return 0;
                return (int)Math.Ceiling((double)TotalCount / PageSize);
            }
        }
    }

