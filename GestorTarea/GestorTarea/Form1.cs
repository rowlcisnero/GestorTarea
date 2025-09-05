using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestorTareas
{
    public partial class Form1 : Form
    {
        private List<Tarea> listaTareas = new List<Tarea>();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (cmbEstado.Items.Count == 0)
                cmbEstado.Items.AddRange(new object[] { "No realizada", "En proceso", "Terminada" });
            if (cmbEstado.SelectedIndex < 0) cmbEstado.SelectedIndex = 0;

            if (cmbFiltroEstado.Items.Count == 0)
                cmbFiltroEstado.Items.AddRange(new object[] { "Todos", "No realizada", "En proceso", "Terminada" });
            cmbFiltroEstado.SelectedIndex = 0;

            dtpDesde.ShowCheckBox = true;
            dtpHasta.ShowCheckBox = true;

            txtBuscarCodigo.TextChanged -= TxtBuscarCodigo_TextChanged;
            txtBuscarCodigo.TextChanged += TxtBuscarCodigo_TextChanged;

            cmbFiltroEstado.SelectedIndexChanged -= CmbFiltroEstado_SelectedIndexChanged;
            cmbFiltroEstado.SelectedIndexChanged += CmbFiltroEstado_SelectedIndexChanged;

            dtpDesde.ValueChanged -= DtpFiltro_ValueChanged;
            dtpDesde.ValueChanged += DtpFiltro_ValueChanged;
            dtpHasta.ValueChanged -= DtpFiltro_ValueChanged;
            dtpHasta.ValueChanged += DtpFiltro_ValueChanged;

            btnLimpiarFiltros.Click -= BtnLimpiarFiltros_Click;
            btnLimpiarFiltros.Click += BtnLimpiarFiltros_Click;

            dgvTareas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTareas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTareas.MultiSelect = false;
            dgvTareas.ReadOnly = true;

            btnAgregar.Click -= btnAgregar_Click;
            btnAgregar.Click += btnAgregar_Click;

            ActualizarGrid();
        }

        private void TxtBuscarCodigo_TextChanged(object s, EventArgs e) => AplicarFiltros();
        private void CmbFiltroEstado_SelectedIndexChanged(object s, EventArgs e) => AplicarFiltros();
        private void DtpFiltro_ValueChanged(object s, EventArgs e) => AplicarFiltros();
        private void BtnLimpiarFiltros_Click(object s, EventArgs e) => LimpiarFiltros();

        private void ActualizarGrid(IEnumerable<Tarea> datos = null)
        {
            var fuente = (datos ?? listaTareas).ToList();
            dgvTareas.DataSource = null;
            dgvTareas.DataSource = fuente;

            if (dgvTareas.Columns.Contains("Fecha"))
                dgvTareas.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void AplicarFiltros()
        {
            IEnumerable<Tarea> query = listaTareas;

            var code = txtBuscarCodigo.Text?.Trim();
            if (!string.IsNullOrEmpty(code))
                query = query.Where(t => (t.Codigo ?? "").IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0);

            if (cmbFiltroEstado.SelectedIndex > 0)
            {
                var estado = cmbFiltroEstado.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(estado))
                    query = query.Where(t => string.Equals(t.Estado, estado, StringComparison.OrdinalIgnoreCase));
            }

            if (dtpDesde.Checked)
            {
                var desde = dtpDesde.Value.Date;
                query = query.Where(t => t.Fecha.Date >= desde);
            }
            if (dtpHasta.Checked)
            {
                var hasta = dtpHasta.Value.Date;
                query = query.Where(t => t.Fecha.Date <= hasta);
            }

            ActualizarGrid(query);
        }

        private void LimpiarFiltros()
        {
            txtBuscarCodigo.Clear();
            cmbFiltroEstado.SelectedIndex = 0;
            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
            ActualizarGrid();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            var codigo = txtCodigo.Text?.Trim();
            var nombre = txtNombre.Text?.Trim();
            var estado = cmbEstado.SelectedItem?.ToString() ?? cmbEstado.Text?.Trim() ?? "No realizada";

            if (string.IsNullOrEmpty(codigo))
            {
                MessageBox.Show("El código es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (listaTareas.Any(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ya existe una tarea con ese código.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nueva = new Tarea
            {
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = txtDescripcion.Text?.Trim(),
                Fecha = dtpFecha.Value,
                Lugar = txtLugar.Text?.Trim(),
                Estado = estado
            };

            listaTareas.Add(nueva);

            MessageBox.Show($"Tarea agregada. Total en memoria: {listaTareas.Count}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AplicarFiltros();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvTareas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una tarea para editar.");
                return;
            }

            string codigoFila = dgvTareas.SelectedRows[0].Cells["Codigo"]?.Value?.ToString();
            var tarea = listaTareas.FirstOrDefault(t => t.Codigo == codigoFila);
            if (tarea == null) return;

            var nuevoCodigo = txtCodigo.Text?.Trim();
            if (string.IsNullOrEmpty(nuevoCodigo))
            {
                MessageBox.Show("El código es obligatorio.");
                return;
            }
            if (!string.Equals(tarea.Codigo, nuevoCodigo, StringComparison.OrdinalIgnoreCase) &&
                listaTareas.Any(t => string.Equals(t.Codigo, nuevoCodigo, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ya existe una tarea con ese código.");
                return;
            }

            tarea.Codigo = nuevoCodigo;
            tarea.Nombre = txtNombre.Text?.Trim();
            tarea.Descripcion = txtDescripcion.Text?.Trim();
            tarea.Fecha = dtpFecha.Value;
            tarea.Lugar = txtLugar.Text?.Trim();
            tarea.Estado = cmbEstado.SelectedItem?.ToString() ?? cmbEstado.Text?.Trim() ?? tarea.Estado;

            AplicarFiltros();
            MessageBox.Show("Tarea editada correctamente.");
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvTareas.SelectedRows.Count == 0) return;

            string codigoFila = dgvTareas.SelectedRows[0].Cells["Codigo"]?.Value?.ToString();
            var tarea = listaTareas.FirstOrDefault(t => t.Codigo == codigoFila);
            if (tarea == null) return;

            listaTareas.Remove(tarea);
            AplicarFiltros();
            MessageBox.Show("Tarea eliminada correctamente.");
        }

        private void dgvTareas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            txtCodigo.Text = dgvTareas.Rows[e.RowIndex].Cells["Codigo"].Value?.ToString();
            txtNombre.Text = dgvTareas.Rows[e.RowIndex].Cells["Nombre"].Value?.ToString();
            txtDescripcion.Text = dgvTareas.Rows[e.RowIndex].Cells["Descripcion"].Value?.ToString();
            if (DateTime.TryParse(dgvTareas.Rows[e.RowIndex].Cells["Fecha"].Value?.ToString(), out var f))
                dtpFecha.Value = f;
            txtLugar.Text = dgvTareas.Rows[e.RowIndex].Cells["Lugar"].Value?.ToString();
            var est = dgvTareas.Rows[e.RowIndex].Cells["Estado"].Value?.ToString();
            if (!string.IsNullOrEmpty(est)) cmbEstado.SelectedItem = est;

            if (e.RowIndex >= 0) // asegura que no se hizo clic en encabezado
            {
                DataGridViewRow fila = dgvTareas.Rows[e.RowIndex];

                txtCodigo.Text = fila.Cells["Codigo"].Value?.ToString();
                txtNombre.Text = fila.Cells["Nombre"].Value?.ToString();
                txtDescripcion.Text = fila.Cells["Descripcion"].Value?.ToString();

                if (fila.Cells["Fecha"].Value is DateTime fecha)
                    dtpFecha.Value = fecha;

                txtLugar.Text = fila.Cells["Lugar"].Value?.ToString();
                cmbEstado.SelectedItem = fila.Cells["Estado"].Value?.ToString();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}