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
            TypeAdapterConfig<RegistroAsociacionDto, TbRepresentanteLegal>
                .NewConfig()
                .Map(dest => dest.RepLegalId, src => src.RepLegalId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroAsociacionDto, TbApoderadoLegal>
                .NewConfig()
                .Map(dest => dest.ApoAbogadoId, src => src.ApoAbogadoId)
                .Map(dest => dest.ApoderadoFirmaId, src => src.ApoderadoFirmaId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroAsociacionDto, TbApoderadoFirma>
                .NewConfig()
                .Map(dest => dest.FirmaId, src => src.ApoderadoFirmaId)
                .IgnoreNullValues(true)
                .Condition((src, dest) => !src.UsaFirmaExistente && !string.IsNullOrEmpty(src.NombreFirma));

            TypeAdapterConfig<RegistroAsociacionDto, TbAsociacion>
                .NewConfig()
                .Map(dest => dest.AsociacionId, src => src.AsociacionId)
                .Map(dest => dest.RepresentanteLegalId, src => src.RepLegalId)
                .Map(dest => dest.ApoderadoLegalId, src => src.ApoAbogadoId)
                .IgnoreNullValues(true);

            // 🔹 Comité
            TypeAdapterConfig<RegistroComiteDto, TbDatosComite>
                .NewConfig()
                .Map(dest => dest.DcomiteId, src => src.DComiteId)
                .IgnoreNullValues(true);

            TypeAdapterConfig<RegistroComiteDto, TbDetalleRegComite>
                .NewConfig()
                .Map(dest => dest.ComiteId, src => src.ComiteId)
                .Map(dest => dest.TipoTramiteId, src => src.TipoTramiteId)
                .Map(dest => dest.CreadaEn, src => DateTime.Now)
                .Map(dest => dest.CreadaPor, src => "UsuarioActual")
                .IgnoreNullValues(true);

            TypeAdapterConfig<MiembroComiteDTO, TbDatosMiembros>
                .NewConfig()
                .Map(dest => dest.DmiembroId, src => src.DMiembroId)
                .Map(dest => dest.NombreMiembro, src => src.Nombre)
                .Map(dest => dest.CedulaMiembro, src => src.Cedula)
                .IgnoreNullValues(true);
        }
    }
}