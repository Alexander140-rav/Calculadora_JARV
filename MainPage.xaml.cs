using System.Data;

namespace Calculadora_JARV;

public partial class MainPage : ContentPage
{
    private string expresionActual = "";
    private bool parentesisAbierto = false;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnNumeroClicked(object sender, EventArgs e)
    {
        var boton = (Button)sender;
        expresionActual += boton.Text;
        ActualizarPantalla();
    }

    private void OnOperadorClicked(object sender, EventArgs e)
    {
        var boton = (Button)sender;
        string op = boton.Text;

        if (string.IsNullOrEmpty(expresionActual) && op != "-")
            return;

        if (!string.IsNullOrEmpty(expresionActual))
        {
            char ultimo = expresionActual[^1];
            if ("+-×÷%".Contains(ultimo))
            {
                expresionActual = expresionActual[..^1] + op;
                ActualizarPantalla();
                return;
            }
        }

        expresionActual += op;
        ActualizarPantalla();
    }

    private void OnPuntoClicked(object sender, EventArgs e)
    {
        string ultimoNumero = ObtenerUltimoNumero(expresionActual);
        if (!ultimoNumero.Contains('.'))
        {
            if (string.IsNullOrEmpty(ultimoNumero) || "+-×÷%(".Contains(expresionActual[^1]))
            {
                expresionActual += "0.";
            }
            else
            {
                expresionActual += ".";
            }
            ActualizarPantalla();
        }
    }

    private void OnParentesisClicked(object sender, EventArgs e)
    {
        if (!parentesisAbierto)
        {
            expresionActual += "(";
            parentesisAbierto = true;
        }
        else
        {
            expresionActual += ")";
            parentesisAbierto = false;
        }
        ActualizarPantalla();
    }

    private void OnBorrarUltimoClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(expresionActual))
        {
            char borrado = expresionActual[^1];
            if (borrado == '(') parentesisAbierto = false;
            if (borrado == ')') parentesisAbierto = true;

            expresionActual = expresionActual.Remove(expresionActual.Length - 1);
            ActualizarPantalla();
        }
    }

    private void OnLimpiarClicked(object sender, EventArgs e)
    {
        expresionActual = "";
        parentesisAbierto = false;
        TxtHistorial.Text = "";
        TxtPantalla.Text = "0";
    }

    private void OnCalcularClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(expresionActual))
            return;

        try
        {
            TxtHistorial.Text = expresionActual + " =";

            // Reemplazar símbolos visuales por sintaxis matemática estándar
            string formula = expresionActual.Replace("×", "*").Replace("÷", "/");

            // Convertir porcentajes (ej: 50% -> (50/100))
            formula = System.Text.RegularExpressions.Regex.Replace(formula, @"(\d+(\.\d+)?)%", "($1/100)");

            // Validar división entre cero explícita
            if (System.Text.RegularExpressions.Regex.IsMatch(formula, @"/\s*0(\.0+)?(?!\d)"))
            {
                TxtPantalla.Text = "Error: Div / 0";
                expresionActual = "";
                return;
            }

            var dt = new DataTable();
            var resultado = dt.Compute(formula, null);

            double valor = Convert.ToDouble(resultado);

            if (double.IsInfinity(valor) || double.IsNaN(valor))
            {
                TxtPantalla.Text = "Error: Div / 0";
                expresionActual = "";
            }
            else
            {
                TxtPantalla.Text = valor.ToString("G15");
                expresionActual = valor.ToString();
            }
        }
        catch
        {
            TxtPantalla.Text = "Error";
            expresionActual = "";
        }
    }

    private void ActualizarPantalla()
    {
        TxtPantalla.Text = string.IsNullOrEmpty(expresionActual) ? "0" : expresionActual;
    }

    private string ObtenerUltimoNumero(string expr)
    {
        char[] delimitadores = { '+', '-', '×', '÷', '%', '(', ')' };
        string[] partes = expr.Split(delimitadores);
        return partes.Length > 0 ? partes[^1] : "";
    }
}