using REGIVA_CR.AB.LogicaDeNegocio.Blog;
using REGIVA_CR.AB.ModelosParaUI.Blog;
using REGIVA_CR.AB.ModelosParaUI.General;
using REGIVA_CR.AB.Services;
using REGIVA_CR.LN.Blog;

public class DocsService : IDocsService
{
    private readonly IBlogLN _blogLN;

    private readonly List<HelpArticleDto> _staticArticles = new List<HelpArticleDto>
        {
            new HelpArticleDto {
                Id = "renovar-llave-criptografica",
                Category = "Primeros Pasos",
                Title = "Cómo cargar o renovar la Llave Criptográfica",
                Author = "REGIVA",
                Summary = "Guía para subir el archivo .p12 generado en el ATV de Hacienda.",
                IconClass = "bi-key-fill",
                ContentHtml = @"<p>Para facturar en Costa Rica, necesitas la Llave Criptográfica del ATV.</p>
                                <h4>Pasos:</h4>
                                <ol>
                                    <li>Ingresa a tu perfil en REGIVA.</li>
                                    <li>Ve a <strong>Configuración > Hacienda</strong>.</li>
                                    <li>Sube tu archivo .p12 y digita el PIN de 4 dígitos.</li>
                                </ol>
                                <p>Si la llave está vencida, debes generar una nueva en el sitio web de Hacienda ATV.</p>"
            },

            new HelpArticleDto {
                Id = "crear-factura-electronica",
                Category = "Facturación",
                Title = "Emitir mi primera Factura Electrónica",
                Author = "REGIVA",
                Summary = "Aprende a crear una factura, agregar clientes y enviarla a Hacienda.",
                IconClass = "bi-receipt",
                ContentHtml = @"<p>Emitir facturas en REGIVA es muy sencillo.</p>
                                <p>Dirígete al botón <strong>Nueva Factura</strong>. Selecciona el cliente (o crea uno nuevo con su cédula). Agrega las líneas de detalle y haz clic en <strong>Firmar y Enviar</strong>.</p>"
            },

            new HelpArticleDto {
                Id = "error-hacienda-rechazo",
                Category = "Solución de Errores",
                Title = "Hacienda rechazó mi factura: ¿Qué hago?",
                Author = "REGIVA",
                Summary = "Entendiendo los mensajes de error comunes y cómo corregirlos.",
                IconClass = "bi-exclamation-triangle-fill",
                ContentHtml = @"<p>Si Hacienda rechaza un documento, generalmente es por:</p>
                                <ul>
                                    <li>El cliente no existe o su cédula es incorrecta.</li>
                                    <li>Tu llave criptográfica venció.</li>
                                    <li>Hubo un error de cálculo en los impuestos.</li>
                                </ul>
                                <p>REGIVA te indicará el motivo exacto en el detalle de la factura rechazada.</p>"
            },

            new HelpArticleDto {
                Id = "predicciones-flujo-caja",
                Category = "Inteligencia Artificial",
                Title = "Cómo funciona la predicción de Flujo de Caja",
                Author = "REGIVA",
                Summary = "Nuestra IA analiza tus gastos históricos para predecir el próximo mes.",
                IconClass = "bi-graph-up-arrow",
                ContentHtml = @"<p>El módulo de IA analiza tus últimos 6 meses de facturación y gastos para proyectar cuánto dinero tendrás disponible la próxima semana.</p>"
            },

            new HelpArticleDto {
                Id = "integrar-whatsapp",
                Category = "Pagos y Cuenta",
                Title = "Integrar WhatsApp para cobros automáticos",
                Author = "REGIVA",
                Summary = "Envía las facturas directamente al WhatsApp de tus clientes.",
                IconClass = "bi-whatsapp",
                ContentHtml = @"<p>Activa la integración en <strong>Configuración > Notificaciones</strong>. Tus clientes recibirán un PDF y un enlace de pago sinpe móvil.</p>"
            },

             new HelpArticleDto {
                Id = "busqueda-cabys",
                Category = "Facturación",
                Title = "Buscador de Códigos CABYS",
                Author = "REGIVA",
                Summary = "Cómo encontrar el código correcto para tus productos o servicios.",
                IconClass = "bi-search",
                ContentHtml = @"<p>El catálogo CABYS es obligatorio. Usa nuestro buscador integrado en la pantalla de facturación para encontrar tu servicio por nombre.</p>"

            }
        };

    public DocsService(IBlogLN blogLN)
    {
        _blogLN = blogLN;
    }

    public List<HelpArticleDto> GetFeaturedArticles()
    {
        List<HelpArticleDto> featured = new List<HelpArticleDto>(_staticArticles.Take(3));

        List<BlogDto> recentBlogs = _blogLN.GetAllAsync(false).Result.Take(2).ToList();

        foreach (var blog in recentBlogs)
        {
            featured.Add(MapBlogToArticle(blog));
        }

        return featured;
    }

    public HelpArticleDto? GetArticleById(string id)
    {
        HelpArticleDto? staticArticle = _staticArticles.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (staticArticle != null) return staticArticle;

        if (id.StartsWith("blog:"))
        {
            string slug = id.Substring(5);

            BlogDto? blog = _blogLN.GetBySlugAsync(slug).Result;

            if (blog != null) return MapBlogToArticle(blog);
        }

        return null;
    }

    public List<HelpArticleDto> SearchArticles(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<HelpArticleDto>();

        query = query.ToLower().Trim();

        List<HelpArticleDto> results = _staticArticles
            .Where(a => a.Title.ToLower().Contains(query) ||
                        a.Author.ToLower().Contains(query) ||
                        a.Summary.ToLower().Contains(query) ||
                        a.Category.ToLower().Contains(query))
            .ToList();

        List<BlogDto> blogResults = _blogLN.SearchAsync(query).Result;

        foreach (var b in blogResults)
        {
            results.Add(MapBlogToArticle(b));
        }

        return results;
    }

    private HelpArticleDto MapBlogToArticle(BlogDto b)
    {
        return new HelpArticleDto
        {
            Id = "blog:" + b.Slug,
            Title = b.Title,
            Author = b.Author,
            Category = "Blog Oficial",
            Summary = b.Summary,
            ContentHtml = b.ContentHtml,
            LastUpdated = b.CreatedAt,
            IconClass = "bi-newspaper"
        };
    }
}