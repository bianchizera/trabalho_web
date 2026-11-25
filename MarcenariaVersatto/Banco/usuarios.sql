-- Tabela de usuários (login) do sistema. Rode no banco Versatto se ainda não existir.
-- perfil: 'admin' (acesso total) ou 'funcionario' (acesso básico: consultar e dar entrada/saída).
CREATE TABLE IF NOT EXISTS usuarios (
    id            SERIAL PRIMARY KEY,
    funcionario_id INT NOT NULL UNIQUE REFERENCES funcionarios(id) ON DELETE CASCADE,
    login         VARCHAR(50) NOT NULL UNIQUE,
    senha_hash    VARCHAR(100) NOT NULL,
    perfil        VARCHAR(20) NOT NULL DEFAULT 'funcionario' CHECK (perfil IN ('admin','funcionario')),
    ativo         BOOLEAN NOT NULL DEFAULT true,
    criado_em     TIMESTAMP NOT NULL DEFAULT now(),
    ultimo_acesso TIMESTAMP
);
