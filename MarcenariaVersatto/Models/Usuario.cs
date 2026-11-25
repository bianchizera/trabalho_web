using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarcenariaVersatto.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("funcionario_id")]
        public int FuncionarioId { get; set; }

        [Column("login")]
        public string Login { get; set; } = string.Empty;

        [Column("senha_hash")]
        public string SenhaHash { get; set; } = string.Empty;

        [Column("perfil")]
        public string Perfil { get; set; } = "padrao";

        [Column("ativo")]
        public bool Ativo { get; set; }

        [Column("criado_em")]
        public DateTime CriadoEm { get; set; }

        [Column("ultimo_acesso")]
        public DateTime? UltimoAcesso { get; set; }

        public Funcionario? Funcionario { get; set; }
    }
}