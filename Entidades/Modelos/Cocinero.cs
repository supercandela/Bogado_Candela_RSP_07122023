using Entidades.Exceptions;
using Entidades.Files;
using Entidades.Interfaces;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Entidades.DataBase;

namespace Entidades.Modelos
{
    public delegate void DelegadoDemoraAtencion(double demora);
    public delegate void DelegadoNuevoIngreso(IComestible menu);

    public class Cocinero <T> where T : IComestible, new()
    {
        private CancellationTokenSource cancellation;
        private int cantPedidosFinalizados;
        private double demoraPreparacionTotal;
        private T menu;
        private string nombre;
        private Task tarea;
        public event DelegadoNuevoIngreso OnIngreso;
        public event DelegadoDemoraAtencion OnDemora;

        //No hacer nada
        public bool HabilitarCocina
        {
            get
            {
                return this.tarea is not null && (this.tarea.Status == TaskStatus.Running ||
                    this.tarea.Status == TaskStatus.WaitingToRun ||
                    this.tarea.Status == TaskStatus.WaitingForActivation);
            }
            set
            {
                if (value && !this.HabilitarCocina)
                {
                    this.cancellation = new CancellationTokenSource();
                    this.IniciarIngreso();
                }
                else
                {
                    this.cancellation.Cancel();
                }
            }
        }

        public string Nombre { get => nombre; }

        /// <summary>
        /// Retorna el resultante de dividir la demora en preparación total sobre la cantidad de pedidos finalizados
        /// </summary>
        public double TiempoMedioDePreparacion { get => this.cantPedidosFinalizados == 0 ? 0 : this.demoraPreparacionTotal / this.cantPedidosFinalizados; }
        
        public int CantPedidosFinalizados { get => cantPedidosFinalizados; }

        public Cocinero(string nombre)
        {
            this.nombre = nombre;
        }

        /// <summary>
        /// Ejecuta en un hilo secundario la acción de que: 
        /// Mientras no se requiera cancelación de la tarea de: 
        /// Invocara al mensaje NotificarNuevoIngreso y EsperarProximoIngreso. 
        /// Incrementar cantidad de pedidos finalizados en 1.
        /// Guardar ticket en la BD.
        /// </summary>
        private void IniciarIngreso()
        {
            CancellationToken cancellationToken = this.cancellation.Token;
            //this.tarea = Task.Run(() => this.NotificarNuevoIngreso(), cancellationToken);
            this.tarea = new Task(this.NotificarNuevoIngreso);
            
            while (!this.cancellation.IsCancellationRequested)
            {
                tarea.Start();
                this.EsperarProximoIngreso();
                this.cantPedidosFinalizados ++;
                DataBaseManager.GuardarTicket(this.Nombre, this.menu);
            }
            
        }

        /// <summary>
        /// El método NotificarNuevoIngreso, verificara si el evento OnIngreso posee suscriptores y en caso exitoso realizara:
        /// Instanciara un nuevo menú
        /// Iniciar la preparación del menú. 
        /// Notificara el menú.
        /// </summary>
        private void NotificarNuevoIngreso()
        {
            if (this.OnIngreso is not null)
            {
                this.menu = new();
                this.menu.IniciarPreparacion();
                this.menu.ToString();
                this.OnIngreso.Invoke(this.menu);
            }
        }

        /// <summary>
        /// Si OnDemora posee un suscriptor notificara los segundos transcurridos mientras que 
        /// (Utilizar Thread.Sleep para dormir el hilo 1 segundos antes de ir decrementando):
        /// El hilo secundario no requiera cancelación.
        /// El estado del pedido false.
        /// Al finalizar incrementar el valor de demoraPreparacionTotal en base al tiempo transcurrido.
        /// </summary>
        private void EsperarProximoIngreso()
        {
            int tiempoEspera = 0;

            if (this.OnDemora is not null)
            {
                this.OnDemora.Invoke(this.demoraPreparacionTotal);
                while (!this.cancellation.IsCancellationRequested && !this.menu.Estado)
                {
                    Thread.Sleep(1000);
                    tiempoEspera++;
                }
            }
            this.demoraPreparacionTotal = tiempoEspera;
        }
    }
}
