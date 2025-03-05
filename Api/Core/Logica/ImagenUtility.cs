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
            var image = ConvertirAImageYQuitarMimeType(imagenBase64);
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
            // Calcular la altura proporcional basada en el ancho fijo
            var ratio = (float)anchoFijo / image.Width;
            int nuevaAltura = (int)(image.Height * ratio);

            // Redimensionar la imagen
            return image.Resize(new SKImageInfo(anchoFijo, nuevaAltura), SKFilterQuality.Medium);
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
            return HacerFotoCuadrada(bitmap, 240);
        }

        private static SKBitmap Base64ToBitmap(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            using var stream = new SKMemoryStream(imageBytes);
            return SKBitmap.Decode(stream);
        }

        private static SKBitmap HacerFotoCuadrada(SKBitmap bitmap, int size)
        {
            var cuadrada = new SKBitmap(size, size);
            using var canvas = new SKCanvas(cuadrada);
            var srcRect = new SKRect((bitmap.Width - size) / 2, (bitmap.Height - size) / 2, (bitmap.Width + size) / 2,
                (bitmap.Height + size) / 2);
            canvas.DrawBitmap(bitmap, srcRect, new SKRect(0, 0, size, size));
            return cuadrada;
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

    }
}
