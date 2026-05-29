using Api.Core.Otros;
using SkiaSharp;

namespace Api.Core.Logica
{
    public static class ImagenUtility
    {
        public static SKBitmap ConvertirABitMapYATamanio240X240(string fotoBase64)
        {
            fotoBase64 = QuitarMimeType(fotoBase64);
            var fotoCuadrada = HacerFotoCuadrada240X240(fotoBase64);
            return fotoCuadrada;
        }

        public static SKBitmap ConvertirABitMapYATamanio240X240YEspejar(string fotoBase64)
        {
            var result = ConvertirABitMapYATamanio240X240(fotoBase64);
            result = EspejarImagen(result);
            return result;
        }

        public static SKBitmap ConvertirAImageYQuitarMimeType(string imagenBase64ConMimeType)
        {
            imagenBase64ConMimeType = QuitarMimeType(imagenBase64ConMimeType);
            return Base64ToBitmap(imagenBase64ConMimeType);
        }

        public static SKBitmap RotarAHorizontalYComprimir(Stream stream)
        {
            using var image = SKBitmap.Decode(stream);
            var resultado = RotarImagen(image);
            return Comprimir(resultado);
        }
        
        private static SKBitmap RotarImagen(SKBitmap bitmap)
        {
            var rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
            using var canvas = new SKCanvas(rotatedBitmap);
            canvas.Translate(rotatedBitmap.Width, 0);
            canvas.RotateDegrees(90);
            canvas.DrawBitmap(bitmap, 0, 0);
            return rotatedBitmap;
        }
        
        public static SKBitmap Comprimir(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
            return SKBitmap.Decode(data);
        }
        
        public static SKBitmap Comprimir(string imagenBase64)
        {
            if (string.IsNullOrWhiteSpace(imagenBase64))
                throw new ExcepcionControlada("La imagen está vacía. Verificá que hayas seleccionado una foto válida.");
            var image = ConvertirAImageYQuitarMimeType(imagenBase64);
            if (image == null)
                throw new ExcepcionControlada("La imagen no pudo procesarse. Verificá que sea una foto válida e intentá de nuevo.");
            return RedimensionarImagenConAnchoFijo(image, 500);
        }

        // private static SKBitmap ConvertirAImageYQuitarMimeType(string imagenBase64)
        // {
        //     // Primero, quitar el prefijo 'data:image/png;base64,' o similar
        //     var base64String = imagenBase64.Split(',')[1];
        //     byte[] imageBytes = Convert.FromBase64String(base64String);
        //
        //     // Convertir el array de bytes a SKBitmap
        //     using (var ms = new System.IO.MemoryStream(imageBytes))
        //     {
        //         return SKBitmap.Decode(ms);
        //     }
        // }

        private static SKBitmap RedimensionarImagenConAnchoFijo(SKBitmap image, int anchoFijo)
        {
            if (image == null)
                throw new ExcepcionControlada("La imagen no pudo procesarse. Verificá que sea una foto válida e intentá de nuevo.");
            if (image.Width <= 0 || image.Height <= 0)
                throw new ExcepcionControlada("La imagen tiene dimensiones inválidas. Probá con otra foto.");
            // Calcular la altura proporcional basada en el ancho fijo
            var ratio = (float)anchoFijo / image.Width;
            int nuevaAltura = (int)(image.Height * ratio);

            // Redimensionar la imagen
            return image.Resize(new SKImageInfo(anchoFijo, nuevaAltura), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }

        public static string StreamToBase64(Stream stream)
        {
            using var bitmap = SKBitmap.Decode(stream);
            return BitmapToBase64(bitmap);
        }
        
        public static string BitmapToBase64(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return Convert.ToBase64String(data.ToArray());
        }

        public static string ImageToBase64(SKImage img)
        {
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return Convert.ToBase64String(data.ToArray());
        }

        private static bool EsVertical(SKBitmap bitmap)
        {
            return bitmap.Height > bitmap.Width;
        }

        private static string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        private static SKBitmap EspejarImagen(SKBitmap bitmap)
        {
            var result = new SKBitmap(bitmap.Width, bitmap.Height);
            using var canvas = new SKCanvas(result);
            canvas.Scale(-1, 1, bitmap.Width / 2f, 0);
            canvas.DrawBitmap(bitmap, 0, 0);
            return result;
        }

        private static SKBitmap HacerFotoCuadrada240X240(string base64)
        {
            var bitmap = Base64ToBitmap(base64);
            return RedimensionarFoto(bitmap, 240);
        }

        private static SKBitmap Base64ToBitmap(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ExcepcionControlada("La imagen está vacía. Verificá que hayas seleccionado una foto válida.");
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                throw new ExcepcionControlada("La imagen tiene un formato inválido. Probá seleccionar la foto de nuevo.");
            }
            if (imageBytes.Length == 0)
                throw new ExcepcionControlada("La imagen está vacía. Verificá que hayas seleccionado una foto válida.");
            using var stream = new SKMemoryStream(imageBytes);
            var bitmap = SKBitmap.Decode(stream);
            if (bitmap == null)
                throw new ExcepcionControlada("La imagen no pudo procesarse. Verificá que sea una foto válida (JPG o PNG) e intentá de nuevo.");
            return bitmap;
        }

        private static SKBitmap RedimensionarFoto(SKBitmap foto, int tamanioEnPixeles)
        {
            if (foto == null) throw new ArgumentNullException(nameof(foto));
            if (tamanioEnPixeles <= 0) throw new ArgumentException("El tamaño debe ser mayor a 0", nameof(tamanioEnPixeles));

            // Crear un nuevo bitmap con el tamaño deseado
            SKBitmap bitmapRedimensionado = new SKBitmap(tamanioEnPixeles, tamanioEnPixeles);
    
            using (SKCanvas canvas = new SKCanvas(bitmapRedimensionado))
            {
                canvas.DrawBitmap(foto, new SKRect(0, 0, tamanioEnPixeles, tamanioEnPixeles));
            }
    
            return bitmapRedimensionado;
        }

        private static string QuitarMimeType(string base64)
        {
            return base64.Contains(',') ? base64.Split(',')[1] : base64;
        }
        
        public static string AgregarMimeType(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return string.Empty;

            return $"data:image/jpeg;base64,{base64String}";
        }

        public static string AgregarMimeType(string base64String, string mimeType)
        {
            if (string.IsNullOrEmpty(base64String))
                return string.Empty;

            if (base64String.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return base64String;

            return $"data:{mimeType};base64,{base64String}";
        }

        public static string ObtenerMimeTypeDesdeExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".svg" => "image/svg+xml",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }

        public static SponsorImagenFormato DetectarFormatoSponsor(string imagenBase64)
        {
            if (string.IsNullOrWhiteSpace(imagenBase64))
                throw new ExcepcionControlada("La imagen está vacía. Verificá que hayas seleccionado una foto válida.");

            if (imagenBase64.Contains("data:image/svg+xml", StringComparison.OrdinalIgnoreCase))
                return SponsorImagenFormato.Svg;
            if (imagenBase64.Contains("data:image/png", StringComparison.OrdinalIgnoreCase))
                return SponsorImagenFormato.Png;
            if (imagenBase64.Contains("data:image/jpeg", StringComparison.OrdinalIgnoreCase)
                || imagenBase64.Contains("data:image/jpg", StringComparison.OrdinalIgnoreCase))
                return SponsorImagenFormato.Jpeg;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(QuitarMimeType(imagenBase64));
            }
            catch (FormatException)
            {
                throw new ExcepcionControlada("La imagen tiene un formato inválido. Probá seleccionar la foto de nuevo.");
            }

            if (bytes.Length == 0)
                throw new ExcepcionControlada("La imagen está vacía. Verificá que hayas seleccionado una foto válida.");

            if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return SponsorImagenFormato.Png;
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
                return SponsorImagenFormato.Jpeg;

            var texto = System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
            if (texto.StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
                || texto.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                return SponsorImagenFormato.Svg;

            throw new ExcepcionControlada("Formato no soportado. Usá JPG, PNG o SVG.");
        }

        public static SKBitmap RedimensionarPng(string imagenBase64, int anchoMaximo = 500)
        {
            var image = ConvertirAImageYQuitarMimeType(imagenBase64);
            if (image == null)
                throw new ExcepcionControlada("La imagen no pudo procesarse. Verificá que sea una foto válida e intentá de nuevo.");
            if (image.Width <= anchoMaximo)
                return image;
            return RedimensionarImagenConAnchoFijo(image, anchoMaximo);
        }

        public static byte[] ObtenerBytesSvg(string imagenBase64)
        {
            var bytes = Convert.FromBase64String(QuitarMimeType(imagenBase64));
            var texto = System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
            if (!texto.Contains("<svg", StringComparison.OrdinalIgnoreCase))
                throw new ExcepcionControlada("El archivo SVG no es válido.");
            if (texto.Contains("<script", StringComparison.OrdinalIgnoreCase))
                throw new ExcepcionControlada("El SVG contiene contenido no permitido.");
            return System.Text.Encoding.UTF8.GetBytes(texto);
        }

        public static string ExtensionSponsor(SponsorImagenFormato formato) =>
            formato switch
            {
                SponsorImagenFormato.Svg => ".svg",
                SponsorImagenFormato.Png => ".png",
                _ => ".jpg"
            };

    }

    public enum SponsorImagenFormato
    {
        Jpeg,
        Png,
        Svg
    }
}
