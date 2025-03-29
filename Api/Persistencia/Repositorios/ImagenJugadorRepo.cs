using System.Drawing;
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
			return $"{_paths.ImagenesTemporalesJugadorCarnetRelative}/{dni}.jpg";
		}

		public string PathFotoTemporalDNIFrente(string dni)
		{
			return $"{_paths.ImagenesTemporalesJugadorDNIFrenteRelative}/{dni}.jpg";
		}

		public string PathFotoTemporalDNIDorso(string dni)
		{
			return $"{_paths.ImagenesTemporalesJugadorDNIDorsoRelative}/{dni}.jpg";
		}

		//No testeado
		public void FicharJugadorTemporal(string dni)
		{
			var pathTemporal = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dni}.jpg";
			var pathJugadores = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";

			Directory.CreateDirectory(_paths.ImagenesJugadoresAbsolute);
			
			// Si por algo quedó una foto de este jugador en el disco, aunque el jugador no figure fichado
			// (Quizás porque se borró el jugador pero no la foto)
			// De más está decir que esto no debería pasar, pero pasó, entonces puse este bonito IF
			if (File.Exists(pathJugadores))
				File.Delete(pathJugadores);

			if (File.Exists(pathTemporal))
				File.Move(pathTemporal, pathJugadores);

			var pathDNIFrente = $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{dni}.jpg";

			if (File.Exists(pathDNIFrente))
				File.Delete(pathDNIFrente);

			var pathDNIDorso = $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{dni}.jpg";

			if (File.Exists(pathDNIDorso))
				File.Delete(pathDNIDorso);
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
			var path = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(_paths.ImagenesTemporalesJugadorCarnetAbsolute);

			var imagen = ImagenUtility.ConvertirABitMapYATamanio240X240(vm.FotoCarnet);
			GuardarImagenEnDisco(path, imagen);
		}

		private static void GuardarFotoDNIFrenteTemporal(JugadorDTO vm)
		{
			var imagePath = $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(imagePath))
				File.Delete(imagePath);

			Directory.CreateDirectory(_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute);
			var imagen = ImagenUtility.Comprimir(vm.FotoDNIFrente);
			GuardarImagenEnDisco(imagePath, imagen);
		}

		private static void GuardarFotoDNIDorsoTemporal(JugadorDTO vm)
		{
			var path = $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{vm.DNI}.jpg";

			if (File.Exists(path))
				File.Delete(path);

			Directory.CreateDirectory(_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute);
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
		//		var imagePath = $"{Paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{vm.DNI}.jpg";

		//		if (File.Exists(imagePath))
		//			File.Delete(imagePath);

		//		Directory.CreateDirectory(Paths.ImagenesTemporalesJugadorDNIFrenteAbsolute);
		//		var result = ImagenUtility.RotarAHorizontalYComprimir(vm.FotoDNIFrente.InputStream);
		//		result.Save(imagePath);
		//	}
		//}

		public string GetFotoCarnetEnBase64(string dni)
		{
			var imagePath = $"{_paths.ImagenesJugadoresAbsolute}/{dni}.jpg";

			if (!File.Exists(imagePath))
			{
				imagePath = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dni}.jpg";
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
			RenombrarFoto($"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{dniNuevo}.jpg");
			RenombrarFoto($"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dniAnterior}.jpg", $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dniNuevo}.jpg");
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
			
			var pathCarnet = $"{_paths.ImagenesTemporalesJugadorCarnetAbsolute}/{dni}.jpg";
			if (File.Exists(pathCarnet))
				File.Delete(pathCarnet);

			var pathDNIFrente = $"{_paths.ImagenesTemporalesJugadorDNIFrenteAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIFrente))
				File.Delete(pathDNIFrente);

			var pathDNIDorso = $"{_paths.ImagenesTemporalesJugadorDNIDorsoAbsolute}/{dni}.jpg";
			if (File.Exists(pathDNIDorso))
				File.Delete(pathDNIDorso);
		}
	}
}