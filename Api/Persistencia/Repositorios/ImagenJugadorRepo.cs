using Api.Core.DTOs;
using Api.Core.Logica;
using Api.Core.Repositorios;
using SkiaSharp;

namespace Api.Persistencia.Repositorios
{
	public class ImagenJugadorRepo : IImagenJugadorRepo
	{
		private static AppPaths _paths = null!;

		public ImagenJugadorRepo(AppPaths appPaths)
		{
			_paths = appPaths;
		}

		// public void GuardarFotoDeJugadorDesdeArchivo(JugadorDTO vm)
		// {
		// 	var path = $"{_paths.ImagenesJugadoresAbsolute}/{vm.DNI}.jpg";
		//
		// 	if (File.Exists(path))
		// 		File.Delete(path);
		//
		// 	Directory.CreateDirectory(_paths.ImagenesJugadoresAbsolute);
		// 	vm.Foto.SaveAs(path);
		// 	
		// 	GuardarImagenEnDisco(path, imagen);
		// }

		public string PathFotoTemporalCarnet(string dni)
		{
			return $"{_paths.ImagenesTemporalesCarnetRelative}/{dni}.jpg";
		}

		public string PathFotoTemporalDNIFrente(string dni)
		{
			return $"{_paths.ImagenesTemporalesDNIFrenteRelative}/{dni}.jpg";
		}

		public string PathFotoTemporalDNIDorso(string dni)
		{
			return $"{_paths.ImagenesTemporalesDNIDorsoRelative}/{dni}.jpg";
		}

		public void FicharJugadorTemporal(string dni)
		{
			Directory.CreateDirectory(_paths.ImagenesJugadoresAbsolute);

			var archivosTemporales = Directory.GetFiles(_paths.ImagenesTemporalesCarnetAbsolute, $"{dni}.*");
			if (archivosTemporales.Length != 1)
				throw new Exception("No se encontró una foto temporal del carnet para el jugador");

			var pathTemporal = archivosTemporales[0];
			var extensionTemporal = System.IO.Path.GetExtension(pathTemporal).ToLowerInvariant();

			var pathDestino = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";

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
				throw new Exception($"Error al procesar la imagen temporal del jugador {dni}: {ex.Message}", ex);
			}

			BorrarDniFrenteYDorsoTemporales(dni);
		}

		// ReSharper disable once InconsistentNaming
		private static void ConvertirAJPGyGuardar(string origen, string destino)
		{
			using var bitmap = SKBitmap.Decode(origen);
			using var image = SKImage.FromBitmap(bitmap);
			using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
			using var fs = new FileStream(destino, FileMode.Create);
			data.SaveTo(fs);
		}

		private static void BorrarDniFrenteYDorsoTemporales(string dni)
		{
			var extensiones = new[] { ".jpg", ".jpeg", ".png" };
			foreach (var ext in extensiones)
			{
				var frente = $"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}{ext}";
				if (File.Exists(frente))
					File.Delete(frente);

				var dorso = $"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}{ext}";
				if (File.Exists(dorso))
					File.Delete(dorso);
			}
		}

		public void GuardarFotosTemporalesDeJugadorAutofichado(JugadorDTO vm)
		{
			GuardarFotoCarnetTemporal(new JugadorDTO {DNI = vm.DNI, FotoCarnet = vm.FotoCarnet });
			GuardarFotoDNIFrenteTemporal(vm);
			GuardarFotoDNIDorsoTemporal(vm);
		}

		public void GuardarFotosTemporalesDeJugadorAutofichadoSiendoEditado(JugadorDTO vm)
		{
			GuardarFotoCarnetTemporal(new JugadorDTO { DNI = vm.DNI, FotoCarnet = vm.FotoCarnet });
			
			GuardarFotoDNIFrenteTemporal(vm);

			GuardarFotoDNIDorsoTemporal(vm);
		}

		private static void GuardarFotoCarnetTemporal(JugadorDTO vm)
		{
			var path = $"{_paths.ImagenesTemporalesCarnetAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(_paths.ImagenesTemporalesCarnetAbsolute);

			var imagen = ImagenUtility.ConvertirABitMapYATamanio240X240(vm.FotoCarnet);
			GuardarImagenEnDisco(path, imagen);
		}

		private static void GuardarFotoDNIFrenteTemporal(JugadorDTO vm)
		{
			var imagePath = $"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(imagePath))
				File.Delete(imagePath);

			Directory.CreateDirectory(_paths.ImagenesTemporalesDNIFrenteAbsolute);
			var imagen = ImagenUtility.Comprimir(vm.FotoDNIFrente);
			GuardarImagenEnDisco(imagePath, imagen);
		}

		private static void GuardarFotoDNIDorsoTemporal(JugadorDTO vm)
		{
			var path = $"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(_paths.ImagenesTemporalesDNIDorsoAbsolute);
			var imagen = ImagenUtility.Comprimir(vm.FotoDNIDorso);
			
			GuardarImagenEnDisco(path, imagen);
		}

		private static void GuardarImagenEnDisco(string path, SKBitmap imagen)
		{
			using var stream = new FileStream(path, FileMode.Create);
			using var image = SKImage.FromBitmap(imagen);
			using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
			data.SaveTo(stream);
		}

		//private static void GuardarFotoDNIFrenteTemporal(JugadorFichadoPorDelegadoVM vm)
		//{
		//	if (vm.FotoDNIFrente != null)
		//	{
		//		var imagePath = $"{Paths.ImagenesTemporalesDNIFrenteAbsolute}/{vm.DNI}.jpg";

		//		if (File.Exists(imagePath))
		//			File.Delete(imagePath);

		//		Directory.CreateDirectory(Paths.ImagenesTemporalesDNIFrenteAbsolute);
		//		var result = ImagenUtility.RotarAHorizontalYComprimir(vm.FotoDNIFrente.InputStream);
		//		result.Save(imagePath);
		//	}
		//}

		public string GetFotoCarnetEnBase64(string dni)
		{
			var imagePath = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";

			if (!File.Exists(imagePath))
			{
				imagePath = $"{_paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";
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
		
		public string Path(string dni)
		{
			return $"{_paths.ImagenesJugadoresRelative}/{dni}.jpg";
		}

		//No testeado
		// public void GuardarImagenJugadorImportado(string dni, byte[] fotoByteArray)
		// {
		// 	var imagePath = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";
		//
		// 	if (File.Exists(imagePath))
		// 		File.Delete(imagePath);
		//
		// 	Directory.CreateDirectory(_paths.ImagenesJugadoresAbsolute);
		//
		// 	using (var image = Image.FromStream(new MemoryStream(fotoByteArray)))
		// 	{
		// 		image.Save(imagePath);
		// 	}
		// }

		public void EliminarLista(IList<string> dnis)
		{
			foreach (var dni in dnis)
				Eliminar(dni);
		}

		public void Eliminar(string dni)
		{
			var imagePath = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";

			if (File.Exists(imagePath))
				File.Delete(imagePath);
		}

		public void CambiarDNI(string dniAnterior, string dniNuevo)
		{
			var pathAnterior = $"{_paths.ImagenesJugadoresAbsolute}/{dniAnterior}.jpg";
			var pathNuevo = $"{_paths.ImagenesJugadoresAbsolute}/{dniNuevo}.jpg";

			if (File.Exists(pathAnterior))
				File.Move(pathAnterior, pathNuevo);
		}

		public void RenombrarFotosTemporalesPorCambioDeDNI(string dniAnterior, string dniNuevo)
		{
			RenombrarFoto($"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{_paths.ImagenesTemporalesCarnetAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesCarnetAbsolute}/{dniNuevo}.jpg");
		}

		private static void RenombrarFoto(string pathAnterior, string pathNuevo)
		{
			if (File.Exists(pathNuevo))
				File.Delete(pathNuevo);

			if (File.Exists(pathAnterior))
				File.Move(pathAnterior, pathNuevo);
		}

		public void EliminarFotosDelJugador(string dni)
		{
			var pathJugador = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";
			if (File.Exists(pathJugador))
				File.Delete(pathJugador);
			
			var pathCarnet = $"{_paths.ImagenesTemporalesCarnetAbsolute}/{dni}.jpg";
			if (File.Exists(pathCarnet))
				File.Delete(pathCarnet);

			var pathDNIFrente = $"{_paths.ImagenesTemporalesDNIFrenteAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIFrente))
				File.Delete(pathDNIFrente);

			var pathDNIDorso = $"{_paths.ImagenesTemporalesDNIDorsoAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIDorso))
				File.Delete(pathDNIDorso);
		}
	}
}