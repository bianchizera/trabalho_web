namespace MarcenariaVersatto.Models
{
    /// <summary>
    /// Perfis de acesso (valor gravado em usuarios.perfil e usado como Role no cookie de login).
    /// admin       = "Administrador Master" da documentação: acesso total.
    /// funcionario = acesso básico: consultar, dar entrada/saída de estoque.
    /// </summary>
    public static class Perfis
    {
        public const string Admin = "admin";
        public const string Funcionario = "funcionario";

        public static readonly string[] Todos = { Admin, Funcionario };

        public static string Rotulo(string perfil) =>
            perfil == Admin ? "Administrador" : "Funcionário";
    }
}
