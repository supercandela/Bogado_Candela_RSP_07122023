using Entidades.Enumerados;
using System.Runtime.CompilerServices;

namespace Entidades.MetodosDeExtension
{
    public static class IngredientesExtension
    {
        /// <summary>
        /// Toma el costo inicial e incrementa su valor porcentualmente en base a los valores de la lista de Eingredientes.
        /// </summary>
        /// <param name="ingredientes">Extiende a Lista de Ingredientes</param>
        /// <param name="costoInicial">Costo inicial del producto</param>
        /// <returns>Costo inicial incrementado según la lista de ingredientes</returns>
        public static double CalcularCostoIngredientes (this List<EIngrediente> ingredientes, int costoInicial)
        {
            double costoIncrementado = 0;
            foreach (EIngrediente item in ingredientes)
            {
                double porcentaje = (int)item;
                costoIncrementado += costoInicial * (porcentaje / 100);
            }
            return costoIncrementado;
        }

        /// <summary>
        /// Genera una lista de ingredientes aleatorios.
        /// </summary>
        /// <param name="rand">Extiende a Random</param>
        /// <returns>La lista de ingredientes generada</returns>
        public static List<EIngrediente> IngredientesAleatorios(this Random rand)
        {
            Random random = new Random();
            List<EIngrediente> ingredientes = new List<EIngrediente>()
            {
                EIngrediente.QUESO,
                EIngrediente.PANCETA,
                EIngrediente.ADHERESO,
                EIngrediente.HUEVO,
                EIngrediente.JAMON,
            };
            int numeroDeIngredientes = random.Next(1, ingredientes.Count + 1);
            return ingredientes.Take(numeroDeIngredientes).ToList();
        }
    }
}
