using AutoMapper;
using YX.Models;
using YX.Models.Dto;

namespace YX.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity -> DTO/ViewModel mappings (for now use same types)
            CreateMap<MotorModel, MotorModel>().ReverseMap();
            CreateMap<MotorDataPoint, MotorDataPoint>().ReverseMap();
            // Entity <-> DTO
            CreateMap<MotorModel, MotorDto>().ReverseMap();
            CreateMap<MotorDataPoint, MotorDataPointDto>().ReverseMap();
        }
    }
}
