using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Api.Core.DTOs;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Core.Servicios;

public class LogsCore : ILogsCore
{
    private const int TextoMinimo = 3;
    private const int MaxResultadosDefault = 200;
    private const int MaxResultadosTope = 1000;
    private const int DiasDefault = 14;
    private const int DiasTope = 90;
    private const long MaxBytesPorArchivo = 20L * 1024 * 1024;

    private static readonly Regex FechaEnNombreNlog = new(
        @"^nlog-(?:all|api)-(\d{4}-\d{2}-\d{2})\.log$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PrefijoFechaNlog = new(
        @"^(?<fecha>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d+)",
        RegexOptions.Compiled);

    private readonly string _logsDirectory;

    [ActivatorUtilitiesConstructor]
    public LogsCore(IWebHostEnvironment env)
        : this(Path.Combine(env.ContentRootPath, "logs"))
    {
    }

    /// <summary>Constructor para tests con carpeta de logs controlada.</summary>
    public LogsCore(string logsDirectory)
    {
        _logsDirectory = Path.GetFullPath(logsDirectory);
    }

    public BusquedaLogsDTO Buscar(string texto, int dias = DiasDefault, int maxResultados = MaxResultadosDefault)
    {
        var textoBusqueda = (texto ?? string.Empty).Trim();
        if (textoBusqueda.Length < TextoMinimo)
            throw new ExcepcionControlada($"El texto de búsqueda debe tener al menos {TextoMinimo} caracteres.");

        dias = NormalizarDias(dias);
        maxResultados = Math.Clamp(maxResultados, 1, MaxResultadosTope);

        var resultado = new BusquedaLogsDTO
        {
            Texto = textoBusqueda,
            Dias = dias,
            MaxResultados = maxResultados
        };

        if (!Directory.Exists(_logsDirectory))
        {
            resultado.Advertencias.Add("La carpeta de logs no existe en el servidor.");
            return resultado;
        }

        var desde = DateTime.Today.AddDays(-(dias - 1));
        var archivos = EnumerarArchivosDeLog()
            .Where(f => ArchivoDentroDeRango(f, desde))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        foreach (var archivo in archivos)
        {
            if (resultado.Resultados.Count >= maxResultados)
            {
                resultado.Truncado = true;
                break;
            }

            BuscarEnArchivo(archivo, textoBusqueda, maxResultados, resultado);
        }

        return resultado;
    }

    public IReadOnlyList<LogArchivoDTO> ListarArchivos(int? dias = null)
    {
        if (!Directory.Exists(_logsDirectory))
            return [];

        DateTime? desde = dias.HasValue
            ? DateTime.Today.AddDays(-(NormalizarDias(dias.Value) - 1))
            : null;

        return EnumerarArchivosDeLog()
            .Where(f => !desde.HasValue || ArchivoDentroDeRango(f, desde.Value))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(f => new LogArchivoDTO
            {
                Nombre = f.Name,
                TamanioBytes = f.Length,
                UltimaModificacion = f.LastWriteTime
            })
            .ToList();
    }

    private IEnumerable<FileInfo> EnumerarArchivosDeLog()
    {
        var dir = new DirectoryInfo(_logsDirectory);
        if (!dir.Exists)
            yield break;

        foreach (var archivo in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
        {
            if (!EsNombreDeLogPermitido(archivo.Name))
                continue;

            var fullPath = Path.GetFullPath(archivo.FullName);
            if (!fullPath.StartsWith(_logsDirectory, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return archivo;
        }
    }

    private static bool EsNombreDeLogPermitido(string nombre)
    {
        return nombre.StartsWith("nlog-", StringComparison.OrdinalIgnoreCase)
               && nombre.EndsWith(".log", StringComparison.OrdinalIgnoreCase)
               || nombre.StartsWith("stdout_", StringComparison.OrdinalIgnoreCase)
               && nombre.EndsWith(".log", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ArchivoDentroDeRango(FileInfo archivo, DateTime desde)
    {
        var match = FechaEnNombreNlog.Match(archivo.Name);
        if (match.Success
            && DateTime.TryParseExact(match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fechaNombre))
        {
            return fechaNombre.Date >= desde.Date;
        }

        return archivo.LastWriteTime.Date >= desde.Date;
    }

    private void BuscarEnArchivo(FileInfo archivo, string texto, int maxResultados, BusquedaLogsDTO resultado)
    {
        try
        {
            using var stream = new FileStream(archivo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long offset = 0;
            if (stream.Length > MaxBytesPorArchivo)
            {
                offset = stream.Length - MaxBytesPorArchivo;
                stream.Seek(offset, SeekOrigin.Begin);
                resultado.Advertencias.Add(
                    $"Se leyó solo el final de '{archivo.Name}' (archivo > {MaxBytesPorArchivo / (1024 * 1024)} MB).");
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024 * 64, leaveOpen: false);

            // Si arrancamos a mitad de archivo, descartamos la primera línea parcial.
            var numeroLinea = 0;
            if (offset > 0)
            {
                reader.ReadLine();
                numeroLinea = EstimarLineasOmitidas(offset);
            }

            string? linea;
            while ((linea = reader.ReadLine()) != null)
            {
                numeroLinea++;
                if (linea.IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                resultado.Resultados.Add(new LogHitDTO
                {
                    Archivo = archivo.Name,
                    Linea = numeroLinea,
                    Contenido = linea,
                    Fecha = ParsearFechaDeLinea(linea)
                });

                if (resultado.Resultados.Count >= maxResultados)
                {
                    resultado.Truncado = true;
                    return;
                }
            }
        }
        catch (IOException ex)
        {
            resultado.Advertencias.Add($"No se pudo leer '{archivo.Name}': {ex.Message}");
        }
    }

    private static int EstimarLineasOmitidas(long offsetBytes)
    {
        // Número de línea aproximado (no exacto) cuando leemos solo el final del archivo.
        return (int)Math.Min(int.MaxValue / 2, offsetBytes / 120);
    }

    private static DateTime? ParsearFechaDeLinea(string linea)
    {
        var match = PrefijoFechaNlog.Match(linea);
        if (!match.Success)
            return null;

        if (DateTime.TryParse(match.Groups["fecha"].Value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var fecha))
            return fecha;

        return null;
    }

    private static int NormalizarDias(int dias)
    {
        if (dias < 1)
            return DiasDefault;
        return Math.Min(dias, DiasTope);
    }
}
