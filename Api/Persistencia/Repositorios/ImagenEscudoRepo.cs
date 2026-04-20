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

		public string GetEscudoEnBase64(int clubId)
		{
			try
			{
				var pathCustom = Path.Combine(_paths.ImagenesEscudosAbsolute, $"{clubId}.jpg");
				var pathUsar = File.Exists(pathCustom) ? pathCustom : _paths.EscudoDefaultFileAbsolute;

				if (!File.Exists(pathUsar))
					return string.Empty;

				using var stream = new FileStream(pathUsar, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var img = SKImage.FromEncodedData(stream);
				if (img is null)
					return string.Empty;

				return ImagenUtility.ImageToBase64(img);
			}
			catch (IOException)
			{
				return string.Empty;
			}
			catch (UnauthorizedAccessException)
			{
				return string.Empty;
			}
		}

		public string GetRutaRelativaEscudo(int clubId)
		{
			var pathCustom = $"{_paths.ImagenesEscudosAbsolute}/{clubId}.jpg";
			var baseRel = _paths.ImagenesEscudosRelative.TrimEnd('/');
			if (File.Exists(pathCustom))
				return $"{baseRel}/{clubId}.jpg";
			return _paths.EscudoDefaultRelative;
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

		public void GuardarEscudoPorDefecto(string imagenBase64)
		{
			Directory.CreateDirectory(_paths.ImagenesEscudosAbsolute);
			var imagen = ImagenUtility.ConvertirABitMapYATamanio240X240(imagenBase64);
			GuardarImagenEnDisco(_paths.EscudoDefaultFileAbsolute, imagen);
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
