using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.ModelosParaUI.General;

namespace REGIVA_CR.AB.Services
{
    public interface IDocsService
    {
        List<HelpArticleDto> SearchArticles(string query);
        HelpArticleDto? GetArticleById(string id);
        List<HelpArticleDto> GetFeaturedArticles();
    }
}
