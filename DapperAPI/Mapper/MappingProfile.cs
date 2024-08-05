using AutoMapper;
using DapperAPI.EntityModel;

namespace DapperAPI.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<OM_ITEM, OM_ITEM>();
            CreateMap<OM_ITEM_UOM, OM_ITEM_UOM>();
        }
    }
}
