using System.ComponentModel.DataAnnotations;

namespace MarcenariaVersatto.ViewModels
{
    public class UsuarioCreateViewModel
    {
        [Display(Name = "Funcionário")]
        [Required(ErrorMessage = "Selecione o funcionário.")]
        public int FuncionarioId { get; set; }

        [Display(Name = "Login")]
        [Required(ErrorMessage = "Informe o login.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O login deve ter entre {2} e {1} caracteres.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Use apenas letras, números, ponto, hífen e sublinhado.")]
        public string Login { get; set; }

        [Display(Name = "Senha")]
        [Required(ErrorMessage = "Informe a senha.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [Display(Name = "Confirmar senha")]
        [Required(ErrorMessage = "Confirme a senha.")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
        [DataType(DataType.Password)]
        public string ConfirmarSenha { get; set; }

        [Display(Name = "Perfil de acesso")]
        [Required(ErrorMessage = "Selecione o perfil.")]
        [RegularExpression("^(admin|funcionario)$", ErrorMessage = "Perfil inválido.")]
        public string Perfil { get; set; } = "funcionario";
    }

    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Login")]
        public string Login { get; set; }

        [Display(Name = "Funcionário")]
        public string NomeFuncionario { get; set; }

        [Display(Name = "Perfil de acesso")]
        [Required(ErrorMessage = "Selecione o perfil.")]
        [RegularExpression("^(admin|funcionario)$", ErrorMessage = "Perfil inválido.")]
        public string Perfil { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; }

        [Display(Name = "Nova senha")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [Display(Name = "Confirmar nova senha")]
        [Compare("NovaSenha", ErrorMessage = "As senhas não conferem.")]
        [DataType(DataType.Password)]
        public string ConfirmarNovaSenha { get; set; }
    }

    public class PrimeiroAcessoViewModel
    {
        [Display(Name = "Seu nome")]
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
        public string Nome { get; set; }

        [Display(Name = "Login")]
        [Required(ErrorMessage = "Informe o login.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O login deve ter entre {2} e {1} caracteres.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Use apenas letras, números, ponto, hífen e sublinhado.")]
        public string Login { get; set; }

        [Display(Name = "Senha")]
        [Required(ErrorMessage = "Informe a senha.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo {2} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [Display(Name = "Confirmar senha")]
        [Required(ErrorMessage = "Confirme a senha.")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem.")]
        [DataType(DataType.Password)]
        public string ConfirmarSenha { get; set; }
    }
}
