// Mappings/MappingConfig.cs

using Mapster;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Models.Entities.BdSisLegal;

namespace REGISTROLEGAL.Mapper
{
    public static class MappingConfig
    {
        public static void RegisterMappings()
        {
            // 🔹 Asociación
            TypeAdapterConfig<RegistroDto, TbRepresentanteLegal>
                .NewConfig()
                .Map(dest => dest.RepLegalId, src => src.RepLegalId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroDto, TbApoderadoLegal>
                .NewConfig()
                .Map(dest => dest.ApoAbogadoId, src => src.ApoAbogadoId)
                .Map(dest => dest.ApoderadoFirmaId, src => src.ApoderadoFirmaId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroDto, TbApoderadoFirma>
                .NewConfig()
                .Map(dest => dest.FirmaId, src => src.ApoderadoFirmaId)
                .IgnoreNullValues(true);


            TypeAdapterConfig<RegistroDto, TbAsociacion>
                .NewConfig()
                .Map(dest => dest.AsociacionId, src => src.AsociacionId)
                .Map(dest => dest.RepresentanteLegalId, src => src.RepLegalId)
                .Map(dest => dest.ApoderadoLegalId, src => src.ApoAbogadoId)
                .IgnoreNullValues(true);

            // 🔹 Comité
            TypeAdapterConfig<RegistroDto, TbDatosComite>
                .NewConfig()
                .Map(dest => dest.DcomiteId, src => src.DComiteId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroDto, TbDetalleRegComite>
                .NewConfig()
                .Map(dest => dest.ComiteId, src => src.ComiteId) // Asegúrate que 'ComiteId' existe en el DTO
                .Map(dest => dest.TipoTramiteId, src => src.TipoTramiteId)
                .Map(dest => dest.CreadaEn, src => DateTime.Now)
                .Map(dest => dest.CreadaPor, src => "UsuarioActual")
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroDto, TbDatosMiembros>
                .NewConfig()
                .Map(dest => dest.DmiembroId, src => src.DMiembroId) // Asegúrate que 'DMiembroId' existe en el DTO
                .Map(dest => dest.NombreMiembro, src => src.Nombre)
                .Map(dest => dest.CedulaMiembro, src => src.Cedula)
                .IgnoreNullValues(true);
        }
    }
}