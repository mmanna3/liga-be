namespace Api.Core.Repositorios
{
	public interface IImagenEscudoRepo
	{
		/// <summary>
		/// Obtiene el escudo del club en base64 (sin prefijo data:image). Si tiene escudo propio lo devuelve; si no, el default.
		/// </summary>
		string GetEscudoEnBase64(int clubId);

		/// <summary>
		/// Guarda o reemplaza el escudo del club con la imagen en base64.
		/// </summary>
		void Guardar(int clubId, string imagenBase64);

		/// <summary>
		/// Elimina el archivo del escudo del club (si existe).
		/// </summary>
		void Eliminar(int clubId);

		/// <summary>
		/// Guarda o reemplaza el escudo por defecto (<c>Imagenes/Escudos/_pordefecto.jpg</c>), con el mismo tratamiento que <see cref="Guardar"/>.
		/// </summary>
		void GuardarEscudoPorDefecto(string imagenBase64);
	}
}
