using Api.Core.DTOs;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using SkiaSharp;

namespace Api.Persistencia.Repositorios
{
	public abstract class ImagenPersonaFichadaRepoBase : IImagenPersonaFichadaRepo
	{
		protected readonly AppPaths Paths;
		protected readonly string ImagenesDefinitivasAbsolute;
		protected readonly string ImagenesDefinitivasRelative;

		protected ImagenPersonaFichadaRepoBase(AppPaths paths, string imagenesDefinitivasAbsolute, string imagenesDefinitivasRelative)
		{
			Paths = paths;
			ImagenesDefinitivasAbsolute = imagenesDefinitivasAbsolute;
			ImagenesDefinitivasRelative = imagenesDefinitivasRelative;
		}

		public string PathFotoTemporalCarnet(string dni) =>
			$"{Paths.ImagenesTemporalesCarnetRelative}/{dni}.jpg";

		public string PathFotoTemporalDNIFrente(string dni) =>
			$"{Paths.ImagenesTemporalesDNIFrenteRelative}/{dni}.jpg";

		public string PathFotoTemporalDNIDorso(string dni) =>
			$"{Paths.ImagenesTemporalesDNIDorsoRelative}/{dni}.jpg";

		public void FicharPersonaTemporal(string dni)
		{
			var archivosTemporales = Directory.GetFiles(Paths.ImagenesTemporalesCarnetAbsolute, $"{dni}.*");
			if (archivosTemporales.Length != 1)
				throw new ExcepcionControlada("No se encontró una foto temporal del carnet para la persona");

			Directory.CreateDirectory(ImagenesDefinitivasAbsolute);

			var pathTemporal = archivosTemporales[0];
			var extensionTemporal = System.IO.Path.GetExtension(pathTemporal).ToLowerInvariant();
			var pathDestino = $"{ImagenesDefinitivasAbsolute}/{dni}.jpg";

			try
			{
				if (extensionTemporal is ".jpg" or ".jpeg")
					File.Move(pathTemporal, pathDestino);
				else
				{
					ConvertirAJPGyGuardar(pathTemporal, pathDestino);
					File.Delete(pathTemporal);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al procesar la imagen temporal de {dni}: {ex.Message}", ex);
			}

			BorrarDniFrenteYDorsoTemporales(dni);
		}

		public void GuardarFotosTemporalesDePersonaFichada(string dni, IFotosDTO fotos)
		{
			GuardarFotoCarnetTemporal(dni, fotos);
			GuardarFotoDNIFrenteTemporal(dni, fotos);
			GuardarFotoDNIDorsoTemporal(dni, fotos);
		}

		public void GuardarFotosTemporalesDePersonaFichadaSiendoEditada(string dni, IFotosDTO fotos)
		{
			GuardarFotoCarnetTemporal(dni, fotos);
			GuardarFotoDNIFrenteTemporal(dni, fotos);
			GuardarFotoDNIDorsoTemporal(dni, fotos);
		}

		public string GetFotoCarnetEnBase64(string dni)
		{
			var imagePath = $"{ImagenesDefinitivasAbsolute}/{dni}.jpg";

			if (!File.Exists(imagePath))
			{
				imagePath = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";
				if (!File.Exists(imagePath))
					return string.Empty;
			}

			using var stream = new FileStream(imagePath, FileMode.Open);
			using var img = SKImage.FromEncodedData(stream);
			return ImagenUtility.ImageToBase64(img);
		}

		public string GetFotoEnBase64ConPathAbsoluto(string pathAbsoluto)
		{
			if (!File.Exists(pathAbsoluto))
				return string.Empty;

			using var stream = new FileStream(pathAbsoluto, FileMode.Open);
			using var img = SKImage.FromEncodedData(stream);
			return ImagenUtility.ImageToBase64(img);
		}

		public string Path(string dni) => $"{ImagenesDefinitivasRelative}/{dni}.jpg";

		public void Eliminar(string dni)
		{
			var imagePath = $"{ImagenesDefinitivasAbsolute}/{dni}.jpg";
			if (File.Exists(imagePath))
				File.Delete(imagePath);
		}

		public void EliminarLista(IList<string> dnis)
		{
			foreach (var dni in dnis)
				Eliminar(dni);
		}

		public void CambiarDNI(string dniAnterior, string dniNuevo)
		{
			var pathAnterior = $"{ImagenesDefinitivasAbsolute}/{dniAnterior}.jpg";
			var pathNuevo = $"{ImagenesDefinitivasAbsolute}/{dniNuevo}.jpg";

			if (File.Exists(pathAnterior))
				File.Move(pathAnterior, pathNuevo);
		}

		public void RenombrarFotosTemporalesPorCambioDeDNI(string dniAnterior, string dniNuevo)
		{
			RenombrarFoto($"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dniAnterior}.jpg", $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dniAnterior}.jpg", $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{Paths.ImagenesTemporalesCarnetAbsolute}/{dniAnterior}.jpg", $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dniNuevo}.jpg");
		}

		public void EliminarTodasLasFotos(string dni)
		{
			var pathDefinitivo = $"{ImagenesDefinitivasAbsolute}/{dni}.jpg";
			if (File.Exists(pathDefinitivo))
				File.Delete(pathDefinitivo);

			var pathCarnet = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";
			if (File.Exists(pathCarnet))
				File.Delete(pathCarnet);

			var pathDNIFrente = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIFrente))
				File.Delete(pathDNIFrente);

			var pathDNIDorso = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIDorso))
				File.Delete(pathDNIDorso);
		}

		private void BorrarDniFrenteYDorsoTemporales(string dni)
		{
			var extensiones = new[] { ".jpg", ".jpeg", ".png" };
			foreach (var ext in extensiones)
			{
				var frente = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}{ext}";
				if (File.Exists(frente))
					File.Delete(frente);

				var dorso = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}{ext}";
				if (File.Exists(dorso))
					File.Delete(dorso);
			}
		}

		protected void GuardarFotoCarnetTemporal(string dni, IFotosDTO fotos)
		{
			var path = $"{Paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(Paths.ImagenesTemporalesCarnetAbsolute);

			var imagen = ImagenUtility.ConvertirABitMapYATamanio240X240(fotos.FotoCarnet);
			GuardarImagenEnDisco(path, imagen);
		}

		protected void GuardarFotoDNIFrenteTemporal(string dni, IFotosDTO fotos)
		{
			var imagePath = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}.jpg";

			if (File.Exists(imagePath))
				File.Delete(imagePath);

			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIFrenteAbsolute);
			var imagen = ImagenUtility.Comprimir(fotos.FotoDNIFrente);
			GuardarImagenEnDisco(imagePath, imagen);
		}

		protected void GuardarFotoDNIDorsoTemporal(string dni, IFotosDTO fotos)
		{
			var path = $"{Paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(Paths.ImagenesTemporalesDNIDorsoAbsolute);
			var imagen = ImagenUtility.Comprimir(fotos.FotoDNIDorso);
			GuardarImagenEnDisco(path, imagen);
		}

		private static void ConvertirAJPGyGuardar(string origen, string destino)
		{
			using var bitmap = SKBitmap.Decode(origen);
			using var image = SKImage.FromBitmap(bitmap);
			using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
			using var fs = new FileStream(destino, FileMode.Create);
			data.SaveTo(fs);
		}

		private static void RenombrarFoto(string pathAnterior, string pathNuevo)
		{
			if (File.Exists(pathNuevo))
				File.Delete(pathNuevo);

			if (File.Exists(pathAnterior))
				File.Move(pathAnterior, pathNuevo);
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
