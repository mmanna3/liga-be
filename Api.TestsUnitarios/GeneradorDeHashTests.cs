using Api.Core.Logica;
using Api.Core.Otros;

namespace Api.TestsUnitarios;

public class GeneradorDeHashTest
	{
		[Fact]
		public void ObtieneSemilla_ApartirDeHash_Correctamente()
		{
			var semilla = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("MTD0001");
			Assert.Equal(1, semilla);

			var semilla2 = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("WJT2456");
			Assert.Equal(2456, semilla2);

			var semilla3 = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("WJT8011");
			Assert.Equal(8011, semilla3);
		}

		[Fact]
		public void ObtieneSemilla_ApartirDeHash_SoportandoMinusculas()
		{
			var semilla = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("mtd0001");
			Assert.Equal(1, semilla);

			var semilla2 = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("WjT2456");
			Assert.Equal(2456, semilla2);
		}

		[Fact]
		public void ObtieneSemilla_ApartirDeHash_Falla()
		{
			var ex1 = Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("0MTD001A"));
			Assert.Equal("El código debe ser de 7 dígitos", ex1.Message);

			var ex2 = Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("MTD00M1"));
			Assert.Equal("El código no tiene el formato correcto", ex2.Message);

			var ex3 = Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos("XXX0001"));
			Assert.Equal("El código es incorrecto", ex3.Message);
		}

		[Fact]
		public void Genera_Correctamente()
		{
			var hash = GeneradorDeHash.GenerarAlfanumerico7Digitos(1);
			Assert.Equal("MTD0001", hash);

			var hash2 = GeneradorDeHash.GenerarAlfanumerico7Digitos(2);
			Assert.Equal("YBO0002", hash2);

			var hash22 = GeneradorDeHash.GenerarAlfanumerico7Digitos(22);
			Assert.Equal("TLE0022", hash22);

			var hash3 = GeneradorDeHash.GenerarAlfanumerico7Digitos(23);
			Assert.Equal("GLX0023", hash3);

			var hash4 = GeneradorDeHash.GenerarAlfanumerico7Digitos(10);
			Assert.Equal("KAA0010", hash4);

			var hash5 = GeneradorDeHash.GenerarAlfanumerico7Digitos(100);
			Assert.Equal("AAA0100", hash5);

			var hash6 = GeneradorDeHash.GenerarAlfanumerico7Digitos(2456);
			Assert.Equal("WJT2456", hash6);

			var hash8011 = GeneradorDeHash.GenerarAlfanumerico7Digitos(8011);
			Assert.Equal("WJT8011", hash8011);
		}

		[Fact]
		public void Semilla_TieneQueSer_MayorQue0_Y_MenorQue10000()
		{
			Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.GenerarAlfanumerico7Digitos(-1));
			Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.GenerarAlfanumerico7Digitos(0));
			Assert.Throws<ExcepcionControlada>(() => GeneradorDeHash.GenerarAlfanumerico7Digitos(10000));
		}

		[Fact]
		public void TransformarSemilla_En_NumeroDe4Digitos()
		{
			var hash = GeneradorDeHash.TransformarAplicandoAlgoritmo(2);
			Assert.Equal(2224, hash);

			var hash2 = GeneradorDeHash.TransformarAplicandoAlgoritmo(1);
			Assert.Equal(1112, hash2);

			var hash3 = GeneradorDeHash.TransformarAplicandoAlgoritmo(1000);
			Assert.Equal(1000, hash3);

			var hash4 = GeneradorDeHash.TransformarAplicandoAlgoritmo(7779);
			Assert.Equal(7778, hash4);
		}

		[Fact]
		public void Obtener_NumeroEntero_MenorOIgualQue25()
		{
			var resultado1 = GeneradorDeHash.ObtenerNumeroEnteroMenorOIgualQue25(14);
			Assert.Equal(14, resultado1);

			var resultado2 = GeneradorDeHash.ObtenerNumeroEnteroMenorOIgualQue25(100);
			Assert.Equal(00, resultado2);
			
			var resultado3 = GeneradorDeHash.ObtenerNumeroEnteroMenorOIgualQue25(26);
			Assert.Equal(1, resultado3);

			var resultado4 = GeneradorDeHash.ObtenerNumeroEnteroMenorOIgualQue25(99);
			Assert.Equal(24, resultado4);
		}

		[Fact]
		public void Obtener_Letras()
		{
			var resultado1 = GeneradorDeHash.ObtenerLetras(1112);
			Assert.Equal("MTD", resultado1);

			var resultado2 = GeneradorDeHash.ObtenerLetras(2224);
			Assert.Equal("YBO", resultado2);

			var resultado3 = GeneradorDeHash.ObtenerLetras(3336);
			Assert.Equal("LVI", resultado3);

			var resultado4 = GeneradorDeHash.ObtenerLetras(1245);
			Assert.Equal("UZZ", resultado4);

			var resultado5 = GeneradorDeHash.ObtenerLetras(9999);
			Assert.Equal("YBT", resultado5);
		}

	}