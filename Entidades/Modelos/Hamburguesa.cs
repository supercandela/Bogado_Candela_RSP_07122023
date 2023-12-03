using Entidades.Enumerados;
using Entidades.Exceptions;
using Entidades.Files;
using Entidades.Interfaces;
using Entidades.MetodosDeExtension;
using System.Text;
using Entidades.DataBase;

namespace Entidades.Modelos
{
    public class Hamburguesa : IComestible
    {
        private double costo;
        private static int costoBase;
        private bool esDoble;
        private bool estado;
        private string imagen;
        List<EIngrediente> ingredientes;
        Random random;

        public bool Estado { get => estado;}

        public string Imagen { get => imagen; }

        public string Ticket => $"{this}\nTotal a pagar:{this.costo}";

        /// <summary>
        /// Agrega los ingredientes de forma aleatoria.
        /// </summary>
        private void AgregarIngredientes()
        {
            this.ingredientes = this.random.IngredientesAleatorios();
        }

        /// <summary>
        /// Asigna el costo a la hamburguesa, este será en relación al costo base y los ingredientes de la hamburguesa.
        /// Luego cambia el estado de la hamburguesa.
        /// </summary>
        /// <param name="cocinero"></param>
        public void FinalizarPreparacion(string cocinero)
        {
            this.costo = this.ingredientes.CalcularCostoIngredientes(costoBase);
            this.estado = !this.Estado;
        }
        static Hamburguesa() => Hamburguesa.costoBase = 1500;
        public Hamburguesa() : this(false) { }
        public Hamburguesa(bool esDoble)
        {
            this.esDoble = esDoble;
            this.random = new Random();
        }

        /// <summary>
        /// Chequea el estado del producto. 
        /// Si el estado es false, genera un numero aleatorio de 1 hasta 9 y asignara la imagen de la hamburguesa, para ello se le deberá enviar al método que obtiene la imagen el siguiente string: $"Hamburguesa_{“Acá va el numero aleatorio”}".
        /// Luego llamara a agregar ingredientes.
        /// </summary>
        public void IniciarPreparacion()
        {
            if (!this.Estado)
            {
                int tipoHamburguesa = this.random.Next(1, 10);
                this.imagen = DataBaseManager.GetImagenComida($"Hamburguesa_{tipoHamburguesa}");
                this.AgregarIngredientes();
                this.estado = true;
            }
        }

        private string MostrarDatos()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Hamburguesa {(this.esDoble ? "Doble" : "Simple")}");
            stringBuilder.AppendLine("Ingredientes: ");
            this.ingredientes.ForEach(i => stringBuilder.AppendLine(i.ToString()));
            return stringBuilder.ToString();

        }

        public override string ToString() => this.MostrarDatos();
    }
}