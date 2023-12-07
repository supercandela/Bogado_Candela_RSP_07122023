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
    public delegate void DelegadoPedidoEnCurso(IComestible pedidoEnPreparacion);

    public class Cocinero <T> where T : IComestible, new()
    {
        private CancellationTokenSource cancellation;
        private int cantPedidosFinalizados;
        private double demoraPreparacionTotal;
        private Mozo<T> mozo;
        private string nombre;
        private T pedidoEnPreparacion;
        private Queue<T> pedidos;
        private Task tarea;

        public event DelegadoDemoraAtencion OnDemora;
        public event DelegadoPedidoEnCurso OnPedido;


        public int CantPedidosFinalizados { get => cantPedidosFinalizados; }

        /// <summary>
        /// En el set bloque verdadero:
        /// a.Poner a trabajar al Mozo.
        /// b. Empezar a cocinar.
        /// 
        /// En el bloque falso adicionalmente a lo que ya hacía, ahora cambiara el estado de empezar a trabajar de mozo.
        /// </summary>
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
                    this.mozo.EmpezarATrabajar = true;
                    this.cancellation = new CancellationTokenSource();
                    this.EmpezarACocinar();
                }
                else
                {
                    this.cancellation.Cancel();
                    this.mozo.EmpezarATrabajar = !(this.mozo.EmpezarATrabajar);
                }
            }
        }

        public string Nombre { get => nombre; }

        public Queue<T> Pedidos { get => pedidos; }


        /// <summary>
        /// Retorna el resultante de dividir la demora en preparación total sobre la cantidad de pedidos finalizados
        /// </summary>
        public double TiempoMedioDePreparacion { get => this.cantPedidosFinalizados == 0 ? 0 : this.demoraPreparacionTotal / this.cantPedidosFinalizados; }

        /// <summary>
        /// En el constructor de Cocinero:
        /// 1.Instanciar el mozo.
        /// 2.Instanciar la cola de pedidos.
        /// 3. Agregar el manejador a OnPedido.
        /// </summary>
        /// <param name="nombre"></param>
        public Cocinero(string nombre)
        {
            this.nombre = nombre;
            this.mozo = new Mozo<T> { };
            this.pedidos = new Queue<T> { };
            this.mozo.OnPedido += this.TomarNuevoPedido;
        }

        /// <summary>
        /// Renombrar el método InciarIngreso por EmpezarACocinar, el cual realizara:
        /// Mientras no se requiera cancelación:
        /// a.Verificar que haya pedidos en la cola para luego:
        /// i.Asignar a pedido en preparación el primer pedido de la lista de pedidos.
        /// ii. Notificara el pedido.
        /// iii. Llamará a esperar próximo ingreso.
        /// iv. Incrementará los pedidos finalizados en 1.
        /// v. Guardara el Ticket en la BD.
        /// </summary>
        private void EmpezarACocinar()
        {
            this.tarea = Task.Run(() =>
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    if (this.Pedidos.Count > 0)
                    {
                        this.pedidoEnPreparacion = this.Pedidos.First();
                        this.OnPedido(this.pedidoEnPreparacion);
                        Thread.Sleep(1000);
                        this.EsperarProximoIngreso();
                        this.cantPedidosFinalizados++;
                        DataBaseManager.GuardarTicket(this.Nombre, this.pedidoEnPreparacion);
                    }
                }
            }, this.cancellation.Token);
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

            if (this.OnDemora != null)
            {
                while (!this.cancellation.IsCancellationRequested && !this.pedidoEnPreparacion.Estado)
                {
                    this.OnDemora.Invoke(tiempoEspera);
                    tiempoEspera++;
                    Thread.Sleep(1000);
                }
            }
            this.demoraPreparacionTotal += tiempoEspera;
        }

        /// <summary>
        /// El método tomar nuevo pedido, agregara el pedido a la cola de pedidos si OnPedido posee subscriptores.
        /// </summary>
        /// <param name="menu"></param>
        private void TomarNuevoPedido(T menu)
        {
            if (this.OnPedido is not null)
            {
                this.pedidos.Enqueue(menu);
            }
        }
    }
}