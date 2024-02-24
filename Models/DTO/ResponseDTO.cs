using AutoWrapper;
using AutoWrapper.Wrappers;

namespace IngBackend.Models.DTO
{
    public class ResponseDTO<R> : ApiResponse
    {
        public R? Result { get; set; }
    }


}