namespace Api.Core.Repositorios
{
	public interface IImagenEscudoRepo
	{
		/// <summary>
		/// Ruta relativa del escudo del club. Si tiene escudo propio: /Imagenes/Escudos/{clubId}.jpg.
		/// Si no: /Imagenes/Escudos/default.jpg.
		/// </summary>
		string PathRelativo(int clubId);

		/// <summary>
		/// Guarda o reemplaza el escudo del club con la imagen en base64.
		/// </summary>
		void Guardar(int clubId, string imagenBase64);

		/// <summary>
		/// Elimina el archivo del escudo del club (si existe).
		/// </summary>
		void Eliminar(int clubId);
	}
}
