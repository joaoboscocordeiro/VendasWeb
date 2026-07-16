using FrenteCaixa.Identidade.Application.Autenticacao.Contratos;
using FrenteCaixa.Identidade.Application.Autenticacao.Interfaces;
using FrenteCaixa.Identidade.Application.Autenticacao.Repositorios;
using FrenteCaixa.Identidade.Domain.Usuarios;

namespace FrenteCaixa.Identidade.Application.Autenticacao;

public sealed class ServicoAutenticacao : IServicoAutenticacao
{
    private static readonly TimeSpan DuracaoRefreshToken = TimeSpan.FromDays(7);

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRefreshTokenRepositorio _refreshTokens;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IGeradorJwt _geradorJwt;
    private readonly IGeradorRefreshToken _geradorRefreshToken;
    private readonly IRelogio _relogio;
    private readonly IUnidadeTrabalho _unidadeTrabalho;

    public ServicoAutenticacao(
        IUsuarioRepositorio usuarios,
        IRefreshTokenRepositorio refreshTokens,
        ISenhaHasher senhaHasher,
        IRefreshTokenHasher refreshTokenHasher,
        IGeradorJwt geradorJwt,
        IGeradorRefreshToken geradorRefreshToken,
        IRelogio relogio,
        IUnidadeTrabalho unidadeTrabalho)
    {
        _usuarios = usuarios;
        _refreshTokens = refreshTokens;
        _senhaHasher = senhaHasher;
        _refreshTokenHasher = refreshTokenHasher;
        _geradorJwt = geradorJwt;
        _geradorRefreshToken = geradorRefreshToken;
        _relogio = relogio;
        _unidadeTrabalho = unidadeTrabalho;
    }

    public async Task<ResultadoOperacao<AutenticacaoResponse>> EntrarAsync(
        LoginRequest request,
        string? enderecoIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return ResultadoOperacao<AutenticacaoResponse>.NaoAutorizado("Credenciais invalidas.");
        }

        var email = NormalizarEmail(request.Email);
        var usuario = await _usuarios.ObterPorEmailAsync(email, cancellationToken);

        if (usuario is null || !usuario.Ativo || !_senhaHasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            return ResultadoOperacao<AutenticacaoResponse>.NaoAutorizado("Credenciais invalidas.");
        }

        return await CriarSessaoAsync(usuario, enderecoIp, cancellationToken);
    }

    public async Task<ResultadoOperacao<AutenticacaoResponse>> RenovarAsync(
        RenovarTokenRequest request,
        string? enderecoIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ResultadoOperacao<AutenticacaoResponse>.NaoAutorizado("Refresh token invalido.");
        }

        var tokenHash = _refreshTokenHasher.GerarHash(request.RefreshToken);
        var tokenAtual = await _refreshTokens.ObterPorHashAsync(tokenHash, cancellationToken);
        var agora = _relogio.Agora;
        var usuario = tokenAtual is null
            ? null
            : await _usuarios.ObterPorIdAsync(tokenAtual.UsuarioId, cancellationToken);

        if (tokenAtual is null || usuario is null || !tokenAtual.EstaAtivo(agora) || !usuario.Ativo)
        {
            return ResultadoOperacao<AutenticacaoResponse>.NaoAutorizado("Refresh token invalido.");
        }

        var novoTokenValor = _geradorRefreshToken.Gerar();
        var novoTokenHash = _refreshTokenHasher.GerarHash(novoTokenValor);
        var novoToken = RefreshToken.Criar(
            tokenAtual.UsuarioId,
            novoTokenHash,
            agora,
            agora.Add(DuracaoRefreshToken),
            enderecoIp);

        await _refreshTokens.AdicionarAsync(novoToken, cancellationToken);
        tokenAtual.Revogar(agora, enderecoIp, novoToken.Id);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<AutenticacaoResponse>.Ok(CriarResposta(usuario, novoTokenValor, novoToken.ExpiraEm));
    }

    public async Task<ResultadoOperacao<bool>> SairAsync(
        LogoutRequest request,
        string? enderecoIp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ResultadoOperacao<bool>.NaoAutorizado("Refresh token invalido.");
        }

        var tokenHash = _refreshTokenHasher.GerarHash(request.RefreshToken);
        var refreshToken = await _refreshTokens.ObterPorHashAsync(tokenHash, cancellationToken);

        if (refreshToken is null)
        {
            return ResultadoOperacao<bool>.NaoAutorizado("Refresh token invalido.");
        }

        if (refreshToken.RevogadoEm is null)
        {
            refreshToken.Revogar(_relogio.Agora, enderecoIp);
            await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        }

        return ResultadoOperacao<bool>.Ok(true);
    }

    public async Task<ResultadoOperacao<UsuarioAutenticadoResponse>> ObterUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            return ResultadoOperacao<UsuarioAutenticadoResponse>.NaoAutorizado("Usuario nao encontrado.");
        }

        return ResultadoOperacao<UsuarioAutenticadoResponse>.Ok(MapearUsuario(usuario));
    }

    private async Task<ResultadoOperacao<AutenticacaoResponse>> CriarSessaoAsync(
        Usuario usuario,
        string? enderecoIp,
        CancellationToken cancellationToken)
    {
        var agora = _relogio.Agora;
        var refreshTokenValor = _geradorRefreshToken.Gerar();
        var refreshToken = RefreshToken.Criar(
            usuario.Id,
            _refreshTokenHasher.GerarHash(refreshTokenValor),
            agora,
            agora.Add(DuracaoRefreshToken),
            enderecoIp);

        await _refreshTokens.AdicionarAsync(refreshToken, cancellationToken);
        await _unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<AutenticacaoResponse>.Ok(CriarResposta(usuario, refreshTokenValor, refreshToken.ExpiraEm));
    }

    private AutenticacaoResponse CriarResposta(Usuario usuario, string refreshToken, DateTimeOffset refreshTokenExpiraEm)
    {
        var tokenJwt = _geradorJwt.Gerar(usuario);

        return new AutenticacaoResponse(
            tokenJwt.Valor,
            tokenJwt.ExpiraEm,
            refreshToken,
            refreshTokenExpiraEm,
            MapearUsuario(usuario));
    }

    private static UsuarioAutenticadoResponse MapearUsuario(Usuario usuario)
    {
        return new UsuarioAutenticadoResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString());
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
