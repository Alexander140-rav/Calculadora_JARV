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

            // Asegurar que comas se conviertan a puntos decimales
            string formula = expresionActual.Replace(",", ".").Replace("×", "*").Replace("÷", "/");

            // Reemplazar porcentajes (ej. 50% -> (50/100))
            formula = System.Text.RegularExpressions.Regex.Replace(formula, @"(\d+(\.\d+)?)%", "($1/100)");

            // Validar división entre cero
            if (System.Text.RegularExpressions.Regex.IsMatch(formula, @"/\s*0(\.0+)?(?!\d)"))
            {
                TxtPantalla.Text = "Error: Div / 0";
                expresionActual = "";
                return;
            }

            double resultado = EvaluadorMatematico.Evaluar(formula);

            if (double.IsInfinity(resultado) || double.IsNaN(resultado))
            {
                TxtPantalla.Text = "Error: Div / 0";
                expresionActual = "";
            }
            else
            {
                string resStr = resultado.ToString(System.Globalization.CultureInfo.InvariantCulture);
                TxtPantalla.Text = resStr;
                expresionActual = resStr;
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

// Evaluador matemático propio independiente de la configuración regional
public static class EvaluadorMatematico
{
    public static double Evaluar(string expresion)
    {
        int pos = 0;

        double ParsearTermino()
        {
            double valor = ParsearFactor();
            while (pos < expresion.Length)
            {
                char op = expresion[pos];
                if (op != '*' && op != '/') break;
                pos++;
                double siguiente = ParsearFactor();
                if (op == '*') valor *= siguiente;
                else
                {
                    if (siguiente == 0) throw new DivideByZeroException();
                    valor /= siguiente;
                }
            }
            return valor;
        }

        double ParsearFactor()
        {
            if (pos >= expresion.Length) return 0;

            if (expresion[pos] == '+') { pos++; return ParsearFactor(); }
            if (expresion[pos] == '-') { pos++; return -ParsearFactor(); }

            if (expresion[pos] == '(')
            {
                pos++; // Saltar '('
                double res = ParsearExpresion();
                if (pos < expresion.Length && expresion[pos] == ')') pos++; // Saltar ')'
                return res;
            }

            int inicio = pos;
            while (pos < expresion.Length && (char.IsDigit(expresion[pos]) || expresion[pos] == '.'))
            {
                pos++;
            }

            string numStr = expresion.Substring(inicio, pos - inicio);
            if (double.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num))
            {
                return num;
            }
            return 0;
        }

        double ParsearExpresion()
        {
            double valor = ParsearTermino();
            while (pos < expresion.Length)
            {
                char op = expresion[pos];
                if (op != '+' && op != '-') break;
                pos++;
                double siguiente = ParsearTermino();
                if (op == '+') valor += siguiente;
                else valor -= siguiente;
            }
            return valor;
        }

        return ParsearExpresion();
    }
}