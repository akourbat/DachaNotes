using Microsoft.AspNetCore.JsonPatch;
using SharedModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorCientApp.Services
{
    public interface IRefitBookService
    {
        [Patch("/api/books/{id}")]
        Task<Book> PatchBookAsync([Body]JsonPatchDocument<Book> patch, string id);

        [Get("/api/books")]
        Task<ApiResponse<Book[]>> GetBooksAsync();
    }
}
