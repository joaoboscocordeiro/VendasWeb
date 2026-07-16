SELECT 'CREATE DATABASE frente_caixa_identidade'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_identidade')\gexec

SELECT 'CREATE DATABASE frente_caixa_catalogo'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_catalogo')\gexec

SELECT 'CREATE DATABASE frente_caixa_estoque'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_estoque')\gexec

SELECT 'CREATE DATABASE frente_caixa_caixa'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_caixa')\gexec

SELECT 'CREATE DATABASE frente_caixa_vendas'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_vendas')\gexec

SELECT 'CREATE DATABASE frente_caixa_pagamentos'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_pagamentos')\gexec

SELECT 'CREATE DATABASE frente_caixa_relatorios'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'frente_caixa_relatorios')\gexec
