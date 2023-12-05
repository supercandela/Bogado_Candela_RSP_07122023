using Entidades.Exceptions;
using Entidades.Files;
using Entidades.Modelos;

namespace MisTest
{
    [TestClass]
    public class TestCocina
    {
        [TestMethod]
        [ExpectedException(typeof(FileManagerException))]
        public void AlGuardarUnArchivo_ConNombreInvalido_TengoUnaExcepcion()
        {
            //arrange
            string data = "Error al guardar desde test";
            string nombreArchivo = "¡?#¡&%=&(=%?¡";
            bool append = true;

            //act
            FileManager.Guardar(data, nombreArchivo, append);
            
            //assert
            //Lanza la excepción

        }

        [TestMethod]
        //Al instanciar un nuevo cocinero, la cantidad de pedidos finalizados debe ser igual a 0 (cero).

        public void AlInstanciarUnCocinero_SeEspera_PedidosCero()
        {
            //arrange
            int cantidadDePedidosFinalizadosEsperados = 0;

            //act
            Cocinero<Hamburguesa> jorge = new Cocinero<Hamburguesa>("Jorge");

            //assert
            Assert.AreEqual(cantidadDePedidosFinalizadosEsperados, jorge.CantPedidosFinalizados);
        }
    }
}