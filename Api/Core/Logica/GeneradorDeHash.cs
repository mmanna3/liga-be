using Api.Core.Otros;

namespace Api.Core.Logica;

public static class GeneradorDeHash
	{
		public static string GenerarAlfanumerico7Digitos(int semilla)
		{
			if (semilla <= 0 || semilla >= 10000)
				throw new ExcepcionControlada("Semilla tiene que estar entre 0 y 10000");

			var semillaTransformada = TransformarAplicandoAlgoritmo(semilla);

			var letras = ObtenerLetras(semillaTransformada);

			var numeroFinal = semilla.ToString().PadLeft(4, '0');

			return $"{letras}{numeroFinal}";
		}

		public static int ObtenerSemillaAPartirDeAlfanumerico7Digitos(string alfaNumerico7Digitos)
		{
			if (alfaNumerico7Digitos.Length != 7)
				throw new ExcepcionControlada("El código debe ser de 7 dígitos");

			var numeroString = $"{alfaNumerico7Digitos[3]}{alfaNumerico7Digitos[4]}{alfaNumerico7Digitos[5]}{alfaNumerico7Digitos[6]}";
			var letrasQueLlegaron = $"{alfaNumerico7Digitos[0]}{alfaNumerico7Digitos[1]}{alfaNumerico7Digitos[2]}".ToUpper();

			if (!int.TryParse(numeroString, out int numero))
				throw new ExcepcionControlada("El código no tiene el formato correcto");

			if (letrasQueLlegaron != ObtenerLetras(TransformarAplicandoAlgoritmo(numero)))
				throw new ExcepcionControlada("El código es incorrecto");			

			return numero;
		}

		public static string ObtenerLetras(int semilla4Digitos)
		{
			var primeraLetra = ObtenerLetra(ObtenerNumeroEnteroMenorOIgualQue25(semilla4Digitos));
			var segundaLetra = ObtenerLetra(ObtenerNumeroEnteroMenorOIgualQue25(semilla4Digitos * semilla4Digitos));
			var terceraLetra = ObtenerLetra(ObtenerNumeroEnteroMenorOIgualQue25(semilla4Digitos * semilla4Digitos * semilla4Digitos));

			return $"{primeraLetra}{segundaLetra}{terceraLetra}";
		}

		public static int TransformarAplicandoAlgoritmo(int semilla)
		{
			int ultimoDigito = semilla % 10;

			var ultimoDigito4Veces = $"{ultimoDigito}{ultimoDigito}{ultimoDigito}{ultimoDigito}";

			var ultimoDigito4VecesInt = int.Parse(ultimoDigito4Veces);

			return (semilla + ultimoDigito4VecesInt) % 10000;
		}

		public static int ObtenerNumeroEnteroMenorOIgualQue25(int numero)
		{
			var resultado = Math.Abs(numero % 100);
									
			while (resultado > 25)
			{
				resultado -= 25;
			}
			
			return resultado;
		}

		private static char ObtenerLetra(int indice)
		{
			if (indice < 0 || indice > 25)
				throw new ExcepcionControlada("Índice fuera de rango");

			const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Son 26

			return letras[indice];
		}


	}