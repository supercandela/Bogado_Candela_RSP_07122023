using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Xml.Linq;
using Entidades.Excepciones;
using Entidades.Exceptions;
using Entidades.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Entidades.Files;

namespace Entidades.DataBase
{
    static class DataBaseManager
    {
        private static SqlConnection connection;
        private static string stringConnection;

        /// <summary>
        /// Inicializa el string connection.
        /// </summary>
        static DataBaseManager()
        {
            DataBaseManager.stringConnection = "Server=.;Database=20230622SP;Trusted_Connection=True;";
            DataBaseManager.connection = new SqlConnection(stringConnection);
        }

        /// <summary>
        /// Recibe el tipo de comida. Usa el string para filtrar en la base de datos.
        /// En caso de que no exista el tipo de comida se lanzara una excepción ComidaInvalidaException.
        /// </summary>
        /// <param name="tipo">Tipo de comida</param>
        /// <returns>URL de la imagen almacenada en la BD</returns>
        /// <exception cref="ComidaInvalidaExeption"></exception>
        public static string GetImagenComida(string tipo)
        {
            string urlImagen = "";

            try
            {
                string query = "SELECT imagen FROM comidas WHERE tipo_comida LIKE @0";
                SqlCommand cmd = new SqlCommand(query, connection) ;
                cmd.Parameters.AddWithValue("@0", "%" + tipo + "%");
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    urlImagen = reader.GetString(0);
                }
                else
                {
                    //Se genera la excepción. 
                    Exception ex = new ComidaInvalidaExeption("Comida inválida.");

                    //Seteo el nombre del archivo
                    string nombreArchivo = "dataBase_logs.txt";

                    //Convierto a string la información a guardar
                    string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

                    //Llamo a FileManager para hacer el log de la excepción obtenida
                    FileManager.Guardar(dataAGuardar, nombreArchivo, true);
                }
            }
            catch (Exception ex)
            {
                //Se genera la excepción. 
                Exception exPrincipal = new DataBaseManagerException("Error de Lectura en la base de datos.", ex);

                //Seteo el nombre del archivo
                string nombreArchivo = "dataBase_logs.txt";

                //Convierto a string la información a guardar
                string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {exPrincipal.GetType()} - Mensaje: {exPrincipal.Message}{Environment.NewLine}";
                dataAGuardar += $"=>{DateTime.Now} Excepción Interna: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

                //Llamo a FileManager para hacer el log de la excepción obtenida
                FileManager.Guardar(dataAGuardar, nombreArchivo, true);
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return urlImagen;
        }

        /// <summary>
        /// Almacena en la tabla tickets el nombre del empleado y el ticket de la comida.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nombreEmpleado">Nombre del empleado</param>
        /// <param name="comida">Comida</param>
        /// <returns></returns>
        public static bool GuardarTicket<T> (string nombreEmpleado, T comida) where T : IComestible, new()
        {
            bool ticketCreado = false;
            try
            {
                 using (connection)
                {
                    string query =
                        "INSERT INTO tickets " +
                        "(empleado, ticket) " +
                        "VALUES(@0, @1)";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@0", nombreEmpleado);
                    cmd.Parameters.AddWithValue("@1", comida.ToString());
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.RecordsAffected > 0)
                    {
                        ticketCreado = true;
                    }
                    else
                    {
                        //Se genera la excepción. 
                        Exception ex = new DataBaseManagerException("No se pudo crear el ticket.");

                        //Seteo el nombre del archivo
                        string nombreArchivo = "dataBase_logs.txt";

                        //Convierto a string la información a guardar
                        string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

                        //Llamo a FileManager para hacer el log de la excepción obtenida
                        FileManager.Guardar(dataAGuardar, nombreArchivo, true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Se genera la excepción. 
                Exception exPrincipal = new DataBaseManagerException("Error de Escritura en la base de datos.", ex);

                //Seteo el nombre del archivo
                string nombreArchivo = "dataBase_logs.txt";

                //Convierto a string la información a guardar
                string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {exPrincipal.GetType()} - Mensaje: {exPrincipal.Message}{Environment.NewLine}";
                dataAGuardar += $"=>{DateTime.Now} Excepción Interna: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

                //Llamo a FileManager para hacer el log de la excepción obtenida
                FileManager.Guardar(dataAGuardar, nombreArchivo, true);
            }
            return ticketCreado;
        }
    }
}
