using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MovieService;

namespace MoviePlatform.Controllers
{
    [Route("api/[controller]")]
    public class MoviesController : Controller
    {
        
        // GET api/movies
        [HttpGet]
        public async Task<IEnumerable<Movie>> Get()
        {
            var proxy = CreateServiceProxy();
            return await proxy.GetMoviesAsync();
        }

        // GET api/movies/5
        [HttpGet("{id}")]
        public async Task<Movie> Get(long id)
        {
            var proxy = CreateServiceProxy();
            return await proxy.GetMovieAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task<Movie> Post([FromBody]Movie movie)
        {
            var proxy = CreateServiceProxy();
            return await proxy.AddMovieAsync(movie);
        }

        private static IMovieService CreateServiceProxy()
        {
            var proxy = ServiceProxy.Create<IMovieService>(new Uri("fabric:/MovieDemo/MovieService"),
                new ServicePartitionKey(0));
            return proxy;
        }

        //ToDo: Implement Put, Delete
    }
}