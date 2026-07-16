using FrenteCaixa.BuildingBlocks.Inbox;
using FrenteCaixa.BuildingBlocks.Mensageria;
using FrenteCaixa.BuildingBlocks.Outbox;
using Microsoft.Extensions.Logging.Abstractions;

namespace FrenteCaixa.BuildingBlocks.Tests;

public sealed class OutboxInboxTests
{
    [Fact]
    public async Task outbox_message_marks_processed_after_publish()
    {
        var mensagem = RegistroOutbox.Criar(
            "catalogo.produto-criado.v1",
            "ProdutoCriado",
            1,
            "{\"id\":\"123\"}",
            DateTimeOffset.UtcNow);
        var repositorio = new RepositorioOutboxEmMemoria(mensagem);
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero));
        var processador = new ProcessadorOutbox(
            repositorio,
            new PublicadorFake(),
            relogio,
            NullLogger<ProcessadorOutbox>.Instance);

        var publicadas = await processador.ProcessarPendentesAsync(10, CancellationToken.None);

        Assert.Equal(1, publicadas);
        Assert.Equal(relogio.Agora, mensagem.ProcessadoEm);
        Assert.Equal(0, mensagem.Tentativas);
        Assert.True(repositorio.AlteracoesSalvas);
    }

    [Fact]
    public async Task outbox_message_records_failure_without_losing_payload()
    {
        const string payload = "{\"id\":\"123\"}";
        var mensagem = RegistroOutbox.Criar(
            "catalogo.produto-criado.v1",
            "ProdutoCriado",
            1,
            payload,
            DateTimeOffset.UtcNow);
        var repositorio = new RepositorioOutboxEmMemoria(mensagem);
        var processador = new ProcessadorOutbox(
            repositorio,
            new PublicadorFake(falhar: true),
            new RelogioFixo(DateTimeOffset.UtcNow),
            NullLogger<ProcessadorOutbox>.Instance);

        var publicadas = await processador.ProcessarPendentesAsync(10, CancellationToken.None);

        Assert.Equal(0, publicadas);
        Assert.Null(mensagem.ProcessadoEm);
        Assert.Equal(1, mensagem.Tentativas);
        Assert.Equal(payload, mensagem.PayloadJson);
        Assert.Contains("falha simulada", mensagem.UltimoErro);
        Assert.True(repositorio.AlteracoesSalvas);
    }

    [Fact]
    public async Task inbox_rejects_duplicate_message()
    {
        var repositorio = new RepositorioInboxEmMemoria();
        var mensagemId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        await repositorio.RegistrarProcessamentoAsync(
            RegistroInbox.Criar(mensagemId, "vendas.venda-concluida.v1", DateTimeOffset.UtcNow),
            CancellationToken.None);

        var duplicada = await repositorio.JaFoiProcessadaAsync(mensagemId, CancellationToken.None);

        Assert.True(duplicada);
    }

    private sealed class PublicadorFake : IPublicadorEventos
    {
        private readonly bool _falhar;

        public PublicadorFake(bool falhar = false)
        {
            _falhar = falhar;
        }

        public Task PublicarAsync(EnvelopeEventoIntegracao evento, CancellationToken cancellationToken)
        {
            if (_falhar)
            {
                throw new InvalidOperationException("falha simulada no broker");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RepositorioOutboxEmMemoria : IRepositorioOutbox
    {
        private readonly IReadOnlyCollection<RegistroOutbox> _mensagens;

        public RepositorioOutboxEmMemoria(params RegistroOutbox[] mensagens)
        {
            _mensagens = mensagens;
        }

        public bool AlteracoesSalvas { get; private set; }

        public Task<IReadOnlyCollection<RegistroOutbox>> ObterPendentesAsync(int quantidade, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<RegistroOutbox>>(
                _mensagens.Where(mensagem => mensagem.EstaPendente).Take(quantidade).ToArray());
        }

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
        {
            AlteracoesSalvas = true;

            return Task.CompletedTask;
        }
    }

    private sealed class RepositorioInboxEmMemoria : IRepositorioInbox
    {
        private readonly HashSet<Guid> _mensagens = new();

        public Task<bool> JaFoiProcessadaAsync(Guid mensagemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mensagens.Contains(mensagemId));
        }

        public Task RegistrarProcessamentoAsync(RegistroInbox registro, CancellationToken cancellationToken)
        {
            _mensagens.Add(registro.MensagemId);

            return Task.CompletedTask;
        }
    }

    private sealed class RelogioFixo : IRelogio
    {
        public RelogioFixo(DateTimeOffset agora)
        {
            Agora = agora;
        }

        public DateTimeOffset Agora { get; }
    }
}
