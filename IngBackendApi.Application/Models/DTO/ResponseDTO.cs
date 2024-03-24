using AutoWrapper;
using AutoWrapper.Wrappers;

namespace IngBackendApi.Models.DTO
{
    public class ResponseDTO<R> : ApiResponse
    {
        public R? Result { get; set; }
    }


}