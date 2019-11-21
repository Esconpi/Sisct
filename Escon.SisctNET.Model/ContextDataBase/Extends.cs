using System;
using System.Linq;

namespace Escon.SisctNET.Model.ContextDataBase
{
    public static class Extends
    {
        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize) where T : EntityBase
        {
            var result = new PagedResult<T>();
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.RowCount = query.Count();

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            result.Results = query.OrderBy(o => o.Id).Skip(skip).Take(pageSize).ToList();

            return result;
        }
    }
}
