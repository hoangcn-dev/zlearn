using System.IO;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IDocumentImportService
    {
        Task<string> ExtractTextFromFileAsync(Stream fileStream, string contentType);
    }
}
