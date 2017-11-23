using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace MovieService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MovieService : StatefulService, IMovieService
    {
        private const string DictionaryName = "Movies";
        [DataContract]
        private class MovieContract
        {
            public MovieContract(long id, string title)
            {
                this.Id = id;
                this.Title = title;
            }
            [DataMember]
            public string Title { get; private set; }

            [DataMember]
            public long Id { get; private set; }
        }

        public MovieService(StatefulServiceContext context)
            : base(context)
        { }

        
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(
                    this.CreateServiceRemotingListener
                )
            };
        }

        public async Task<Movie> AddMovieAsync(Movie movie)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var dic = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, MovieContract>>(DictionaryName);
                await dic.AddAsync(tx, movie.Id, new MovieContract(movie.Id, movie.Title));
                await tx.CommitAsync();
                return movie;
            }
        }

        public async Task<Movie> GetMovieAsync(long id)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var dic = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, MovieContract>>(DictionaryName);
                var movie = await dic.TryGetValueAsync(tx, id);
                return movie.HasValue ? new Movie() {Id = movie.Value.Id, Title = movie.Value.Title} : null;
            }
        }

        public async Task<IEnumerable<Movie>> GetMoviesAsync()
        {
            var list = new List<Movie>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var dic = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, MovieContract>>(DictionaryName);
                var asyncEnumerable = await dic.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var contract = asyncEnumerator.Current.Value;
                        list.Add(new Movie() { Id = contract.Id, Title = contract.Title });
                    }
                }
                return list;
            }
        }
    }
}
