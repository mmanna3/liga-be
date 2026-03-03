using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using SkiaSharp;

namespace Api.Persistencia.Repositorios
{
	public class ImagenEscudoRepo : IImagenEscudoRepo
	{
		private readonly AppPaths _paths;

		public ImagenEscudoRepo(AppPaths paths)
		{
			_paths = paths;
		}

		public string PathRelativo(int clubId)
		{
			var pathAbsoluto = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
			return File.Exists(pathAbsoluto)
				? $"{_paths.ImagenesEscudosRelative}/{clubId}.jpg"
				: _paths.EscudoDefaultRelative;
		}

		public void Guardar(int clubId, string imagenBase64)
		{
			Directory.CreateDirectory(_paths.ImagenesEscudosAbsolute);
			var pathDestino = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
			var imagen = ImagenUtility.ConvertirABitMapYATamanio240X240(imagenBase64);
			GuardarImagenEnDisco(pathDestino, imagen);
		}

		public void Eliminar(int clubId)
		{
			var path = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
			if (File.Exists(path))
				File.Delete(path);
		}

		private static void GuardarImagenEnDisco(string path, SKBitmap imagen)
		{
			using var stream = new FileStream(path, FileMode.Create);
			using var image = SKImage.FromBitmap(imagen);
			using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
			data.SaveTo(stream);
		}
	}

}
