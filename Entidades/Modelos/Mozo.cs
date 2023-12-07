using Entidades.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Entidades.Exceptions;
using Entidades.Files;
using Entidades.DataBase;
using System.Drawing;

namespace Entidades.Modelos
{
    //enviara el tipo genérico denominado menú.
    public delegate void DelegadoNuevoPedido<T>(T menu);

    public class Mozo<T> where T : IComestible, new()
    {
        private CancellationTokenSource? cancellation;
        private T? menu;
        private Task? tarea;
        public event DelegadoNuevoPedido<T>? OnPedido;

        /// <summary>
        /// En el GET retornara True, si la tares no es nula y estado de la tarea es Running o WaitingToRun o WaitingForActivation.
        /// En el SET, si el valor recibido es TRUE y la tarea es nula o su estado no es Running o no es WaitingToRun o no es WaitingForActivation, se instanciará un nuevo CancelationTokenSource y se llamará a TomarPedidos.
        /// De lo contrario se llamará al método Cancel de cancellation.
        /// </summary>
        public bool EmpezarATrabajar
        {
            get
            {
                return this.tarea is not null && (this.tarea.Status == TaskStatus.Running ||
                   this.tarea.Status == TaskStatus.WaitingToRun ||
                   this.tarea.Status == TaskStatus.WaitingForActivation);
            }
            set
            {
                if (value && (this.tarea is null || this.tarea.Status != TaskStatus.Running ||
                   this.tarea.Status != TaskStatus.WaitingToRun ||
                   this.tarea.Status != TaskStatus.WaitingForActivation))
                {
                    this.cancellation = new CancellationTokenSource();
                    this.TomarPedidos();
                }
                else
                {
                    this.cancellation.Cancel();
                }
            }
        }

        /// <summary>
        /// Se ejecutará en un hilo secundario la tarea de notificar nuevo pedido, mientras no se requiera cancelación.Esta acción la realizara cada 5 segundos (usar Thread sleep)
        /// </summary>
        private void TomarPedidos()
        {
            this.tarea = Task.Run(() =>
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    this.NotificarNuevoPedido();
                    Thread.Sleep(5000);
                }
            }, this.cancellation.Token);
        }

        /// <summary>
        /// Si posee subscriptores:
        /// 1. Instanciara un nuevo menú.
        /// 2. Iniciará la preparación del menú.
        /// 3. Notificara el nuevo pedido.
        /// </summary>
        private void NotificarNuevoPedido()
        {
            if (this.OnPedido != null)
            {
                this.menu = new();
                this.menu.IniciarPreparacion();
                this.OnPedido.Invoke(this.menu);
            }
        }
    }
}