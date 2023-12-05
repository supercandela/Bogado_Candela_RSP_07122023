using Entidades.Excepciones;
using Entidades.Exceptions;
using Entidades.Interfaces;
using Entidades.Modelos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Entidades.Files
{    
    public static class FileManager
    {
        private static string path;

        static FileManager ()
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\20231207_Alumna_Bogado_Candela\\";
            FileManager.ValidaExistenciaDeDirectorio();
        }

        /// <summary>
        /// Sera el método para poder generar archivos de texto.El mismo se podrá usar para agregar información a un archivo ya existente o sobre escribirlo.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nombreArchivo"></param>
        /// <param name="append"></param>
        /// <exception cref="FileManagerException"></exception>
        public static void Guardar (string data, string nombreArchivo, bool append)
        {
            try
            {
                //Agrego el nombre del archivo al Path
                string rutaCompleta = Path.Combine(path, nombreArchivo);

                //Reviso si el archivo existe para setear el append en False
                if (!File.Exists(rutaCompleta))
                {
                    append = false;
                }

                using (StreamWriter sw = new StreamWriter(rutaCompleta, append))
                {
                    sw.WriteLine(data);
                }
            }
            catch (Exception ex)
            {
                throw new FileManagerException("Error al guardar el archivo.", ex);
            }
            //catch (Exception ex)
            //{

            //    //Se genera la excepción.
            //    Exception exPrincipal = new FileManagerException("Error al guardar el archivo.", ex);

            //    //Seteo el nombre del archivo
            //    string nombreArchivoAGuardar = "logs.txt";

            //    //Convierto a string la información a guardar
            //    string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {exPrincipal.GetType()} - Mensaje: {exPrincipal.Message}{Environment.NewLine}";
            //    dataAGuardar += $"=>{DateTime.Now} Excepción Interna: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

            //    //Llamo a FileManager para hacer el log de la excepción obtenida
            //    FileManager.Guardar(dataAGuardar, nombreArchivoAGuardar, true);
            //}
        }

        /// <summary>
        /// Serializar:
        /// Sera genérico y solo aceptara tipos por referencia.
        /// Sera el método encargado de serializar en json.
        /// Retornara true al terminar la serialización;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elemento"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        public static bool Serializar<T> (T elemento, string nombreArchivo) where T : class
        {
            bool serializado = false;
            if (elemento is not null && nombreArchivo.Trim().Length > 0)
            {
                //Agrego el nombre del archivo al Path
                string rutaCompleta = Path.Combine(path, nombreArchivo);

                JsonSerializerOptions opciones = new JsonSerializerOptions();
                opciones.WriteIndented = true;

                using (StreamWriter sw =  new StreamWriter(rutaCompleta))
                {
                    string elementoData = JsonSerializer.Serialize(elemento, opciones);
                    sw.WriteLine(elementoData);
                }
                serializado = true;
            }
            return serializado;
        }

        /// <summary>
        /// Revisa si existe el directorio, caso contrario lo crea.
        /// De existir un error, lanza la excepción FileManagerException
        /// </summary>
        /// <exception cref="FileManagerException"></exception>
        private static void ValidaExistenciaDeDirectorio ()
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                //Se genera la excepción.
                Exception exPrincipal =  new FileManagerException("Error al crear el directorio", ex);

                //Seteo el nombre del archivo
                string nombreArchivo = "logs.txt";

                //Convierto a string la información a guardar
                string dataAGuardar = $"=>{DateTime.Now} Se produjo una excepción: {exPrincipal.GetType()} - Mensaje: {exPrincipal.Message}{Environment.NewLine}";
                dataAGuardar += $"=>{DateTime.Now} Excepción Interna: {ex.GetType()} - Mensaje: {ex.Message}{Environment.NewLine}";

                //Llamo a FileManager para hacer el log de la excepción obtenida
                FileManager.Guardar(dataAGuardar, nombreArchivo, true);
            }
        }
    }
}
