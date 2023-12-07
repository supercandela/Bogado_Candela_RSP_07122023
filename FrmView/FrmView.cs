using Entidades.Exceptions;
using Entidades.Files;
using Entidades.Interfaces;
using Entidades.Modelos;
using static System.Net.Mime.MediaTypeNames;
using System;


namespace FrmView
{
    public partial class FrmView : Form
    {
        //i.Eliminar la lista de comidas(Queue), y asignar un atributo llamado comida de tipo IComestible.
        Cocinero<Hamburguesa> hamburguesero;
        private IComestible? comida;

        public FrmView()
        {
            InitializeComponent();
            this.hamburguesero = new Cocinero<Hamburguesa>("Ramon");
            //Alumno - agregar manejadores al cocinero
            this.hamburguesero.OnPedido += this.MostrarComida;
            this.hamburguesero.OnDemora += this.MostrarConteo;
        }


        //ii.MostrarComida: remplazar la acci�n de agregar la comida a la queue por la asignaci�n de la comida al atributo de la clase.
        private void MostrarComida(IComestible comida)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(() => this.MostrarComida(comida));
            }
            else
            {
                this.comida = comida;
                this.pcbComida.Load(comida.Imagen);
                this.rchElaborando.Text = comida.ToString();
            }
        }


        //Alumno: Realizar los cambios necesarios sobre MostrarConteo de manera que se refleje en el fomrulario el tiempo transucurrido
        private void MostrarConteo(double tiempo)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(() => this.MostrarConteo(tiempo));
            }
            else
            {
                this.lblTiempo.Text = $"{tiempo} segundos";
                this.lblTmp.Text = $"{this.hamburguesero.TiempoMedioDePreparacion.ToString("00.0")} segundos";
            }
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            if (!this.hamburguesero.HabilitarCocina)
            {
                this.hamburguesero.HabilitarCocina = true;
                this.btnAbrir.Image = Properties.Resources.close_icon;
            }
            else
            {
                this.hamburguesero.HabilitarCocina = false;
                this.btnAbrir.Image = Properties.Resources.open_icon;
            }
        }

        //iii.btnSiguienteClick:
        //1. remplazar condici�n del if, donde se eval�a por la cantidad de elementos de la lista, se evaluar� si comida no es null.
        //2. Dentro del bloque verdadero, se eliminar� del dequeue.
        //3. Luego de finalizar la preparacion se agregar� el Ticket de la comida al richt text box de finalizados.Ej this.rchFinalizados.Text += "\n" + comida.Ticket;
        //4. Asignar null a la comida de la clase.
        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (this.comida is not null)
            {
                IComestible comida = this.hamburguesero.Pedidos.Dequeue();
                comida.FinalizarPreparacion(this.hamburguesero.Nombre);
                this.rchFinalizados.Text += "\n" + comida.Ticket;
                this.comida = null;
            }
            else
            {
                MessageBox.Show("El Cocinero no posee comidas", "Atencion", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void FrmView_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Alumno: Serializar el cocinero antes de cerrar el formulario
            string nombreArchivo = "cocinero.json";
            if (FileManager.Serializar(this.hamburguesero, nombreArchivo))
            {
                MessageBox.Show("Informaci�n guardada con �xito.", "Informaci�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Informaci�n no guardada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}