using Appacitive.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Data.Provider.Appacitive.Extensions
{
    public static class AppacitiveExtensions
    {
        public static List<Article> GetAllConnectedArticles(this Article article, string relation, string query = null, string label = null, IEnumerable<string> fields = null)
        {
            List<Article> articles = new List<Article>();
            bool isFirstPage = true;
            PagedList<Article> page = null;
            do
            {
                if( isFirstPage == true )
                {
                    page = article.GetConnectedArticlesAsync(relation, query, label, fields, 1, 200).Result;
                    isFirstPage = false;
                }
                else 
                {
                    page = page.NextPageAsync().Result;
                }
                articles.AddRange( page );
            }while( page.IsLastPage == false );
            return articles;
        }

        public static List<Article> GetAllArticles(string schema, string query = null, IEnumerable<string> fields = null, string orderBy = null, SortOrder order = SortOrder.Descending)
        {
            List<Article> articles = new List<Article>();
            bool isFirstPage = true;
            PagedList<Article> page = null;
            do
            {
                if (isFirstPage == true)
                {
                    page = Articles.FindAllAsync(schema, query, fields, 1, 200, orderBy, order).Result;
                    isFirstPage = false;
                }
                else
                {
                    page = page.NextPageAsync().Result;
                }
                articles.AddRange(page);
            } while (page.IsLastPage == false);
            return articles;
        }
    }
}
