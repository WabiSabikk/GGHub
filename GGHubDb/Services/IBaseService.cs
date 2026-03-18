using GGHubShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGHubDb.Services
{
    public interface IBaseService<T, TDto, TCreateDto, TUpdateDto>
      where T : class
      where TDto : class
      where TCreateDto : class
      where TUpdateDto : class
    {
        Task<ApiResponse<TDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<TDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<ApiResponse<TDto>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<TDto>> UpdateAsync(Guid id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
