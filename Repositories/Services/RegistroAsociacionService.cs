using Microsoft.AspNetCore.Components.Forms;
using REGISTROLEGAL.DTOs;
using REGISTROLEGAL.Repositories.Interfaces;

namespace REGISTROLEGAL.Repositories.Services
{
    public class RegistroAsociacionService : IRegistroAsociacionService
    {
        private readonly HttpClient _http;

        public RegistroAsociacionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<RegistroAsociacionDTO> ObtenerPorIdAsync(int id)
        {
            var response = await _http.GetAsync($"/api/registroasociacion/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"No se pudo cargar el registro. Código: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<RegistroAsociacionDTO>();
            if (result == null)
                throw new Exception("Respuesta vacía del servidor.");

            return result;
        }

        public async Task ActualizarAsync(RegistroAsociacionDTO model, List<IBrowserFile> nuevosArchivos)
        {
            var formData = new MultipartFormDataContent();

            // Agregar todos los campos del modelo
            var properties = typeof(RegistroAsociacionDTO).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.Name == nameof(model.Documentos) || prop.Name == nameof(model.DocumentoSubida))
                    continue; // Saltar listas de archivos

                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                formData.Add(new StringContent(value), prop.Name);
            }

            // Subir nuevos archivos
            foreach (var file in nuevosArchivos)
            {
                var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10_000_000)); // 10 MB
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                formData.Add(streamContent, "ArchivosSubida", file.Name);
            }

            var response = await _http.PutAsync("/api/registroasociacion", formData);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al actualizar: {error}");
            }
        }
    }
}