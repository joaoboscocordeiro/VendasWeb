using System.Text.RegularExpressions;
using FrenteCaixa.Identidade.Application.Autenticacao;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Application.Usuarios.Contratos;
using FrenteCaixa.Identidade.Application.Usuarios.Interfaces;
using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Application.Usuarios;

public sealed class ServicoUsuarios : IServicoUsuarios
{
    private static readonly Regex EmailRegex = new(
        "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IUsuarioRepositorio _usuarios;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoUsuarios(
        IUsuarioRepositorio usuarios,
        ISenhaHasher senhaHasher,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _usuarios = usuarios;
        _senhaHasher = senhaHasher;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<ResultadoOperacao<UsuarioResponse>> CadastrarAsync(
        CadastrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarDados(request.Nome, request.Email, request.Perfil, request.Senha);

        if (validacao is not null)
        {
            return ResultadoOperacao<UsuarioResponse>.FalhaValidacao(validacao);
        }

        var email = NormalizarEmail(request.Email);
        var usuarioExistente = await _usuarios.ObterPorEmailAsync(email, cancellationToken);

        if (usuarioExistente is not null)
        {
            return ResultadoOperacao<UsuarioResponse>.Conflito("Ja existe usuario com este e-mail.");
        }

        var agora = _relogio.Agora;
        var usuario = Usuario.Criar(
            request.Nome,
            email,
            _senhaHasher.GerarHash(request.Senha),
            ParsePerfil(request.Perfil)!.Value,
            agora);

        await _usuarios.AdicionarAsync(usuario, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<UsuarioResponse>.Ok(Mapear(usuario));
    }

    public async Task<IReadOnlyCollection<UsuarioResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _usuarios.ListarAsync(cancellationToken);

        return usuarios.Select(Mapear).ToArray();
    }

    public async Task<ResultadoOperacao<UsuarioResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObterPorIdAsync(id, cancellationToken);

        return usuario is null
            ? ResultadoOperacao<UsuarioResponse>.NaoEncontrado("Usuario nao encontrado.")
            : ResultadoOperacao<UsuarioResponse>.Ok(Mapear(usuario));
    }

    public async Task<ResultadoOperacao<UsuarioResponse>> AtualizarAsync(
        Guid id,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var validacao = ValidarDados(request.Nome, request.Email, request.Perfil);

        if (validacao is not null)
        {
            return ResultadoOperacao<UsuarioResponse>.FalhaValidacao(validacao);
        }

        var usuario = await _usuarios.ObterPorIdAsync(id, cancellationToken);

        if (usuario is null)
        {
            return ResultadoOperacao<UsuarioResponse>.NaoEncontrado("Usuario nao encontrado.");
        }

        var email = NormalizarEmail(request.Email);
        var usuarioComEmail = await _usuarios.ObterPorEmailAsync(email, cancellationToken);

        if (usuarioComEmail is not null && usuarioComEmail.Id != id)
        {
            return ResultadoOperacao<UsuarioResponse>.Conflito("Ja existe usuario com este e-mail.");
        }

        usuario.AtualizarDados(request.Nome, email, ParsePerfil(request.Perfil)!.Value, _relogio.Agora);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<UsuarioResponse>.Ok(Mapear(usuario));
    }

    public async Task<ResultadoOperacao<bool>> InativarAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObterPorIdAsync(id, cancellationToken);

        if (usuario is null)
        {
            return ResultadoOperacao<bool>.NaoEncontrado("Usuario nao encontrado.");
        }

        if (usuario.Ativo)
        {
            usuario.Inativar(_relogio.Agora);
            await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        }

        return ResultadoOperacao<bool>.Ok(true);
    }

    private static string? ValidarDados(string nome, string email, string perfil, string? senha = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return "Nome e obrigatorio.";
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email.Trim()))
        {
            return "E-mail invalido.";
        }

        if (ParsePerfil(perfil) is null)
        {
            return "Perfil invalido.";
        }

        if (senha is not null && senha.Length < 8)
        {
            return "Senha deve possuir pelo menos 8 caracteres.";
        }

        return null;
    }

    private static PerfilUsuario? ParsePerfil(string perfil)
    {
        return Enum.TryParse<PerfilUsuario>(perfil, ignoreCase: true, out var perfilUsuario)
            && Enum.IsDefined(perfilUsuario)
                ? perfilUsuario
                : null;
    }

    private static UsuarioResponse Mapear(Usuario usuario)
    {
        return new UsuarioResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            usuario.Ativo,
            usuario.CriadoEm,
            usuario.AtualizadoEm);
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
