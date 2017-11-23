using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace MovieService
{
    public interface IMovieService : IService
    {
        Task<Movie> AddMovieAsync(Movie movie);
        Task<Movie> GetMovieAsync(long id);
        Task<IEnumerable<Movie>> GetMoviesAsync();
    }
}